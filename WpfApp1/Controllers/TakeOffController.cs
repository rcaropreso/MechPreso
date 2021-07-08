using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;
using System.Reflection;
using WpfApp1.Services;
using WpfApp1.Utils;
using WpfApp1.Models;
//using Vector3 = System.Tuple<double, double, double>;
using System.Threading;

namespace WpfApp1.Controllers
{
    /// <summary>
    /// Classe que gerencia a decolagem da nave
    /// </summary>
    public class TakeOffController : IAsyncMessageUpdate
    {
        /// <summary>
        /// Describes the controls of a given ship
        /// </summary>
        /// 
        private Connection _conn = null;
        private FlightTelemetry _flightTelemetry;
        private TakeOffDescriptor _tod;

        public Vessel CurrentVessel { get => _conn.SpaceCenter().ActiveVessel; }
        public string ShipName { get => CurrentVessel.Name; }

        private object lock_gate_takeoff = new object();
        private object lock_gate_maneuver = new object();

        private bool _manualControl = false;
        private object lock_gate_manual = new object();

        private float _maneuverBurnTime;
        public float ManeuverBurnTime
        {
            get
            {
                return _maneuverBurnTime;
            }
        }

        private CommonDefs.VesselState _TakeOffStatus;
        private CommonDefs.VesselState _ManeuverStatus;

        public CommonDefs.VesselState TakeOffStatus { get => _TakeOffStatus; }
        public CommonDefs.VesselState ManeuverStatus { get => _ManeuverStatus; }


        public TakeOffController(in Connection conn, in FlightTelemetry _flightTel)
        {
            _conn = conn;
            _flightTelemetry = _flightTel;
        }

        private bool ReturnToManualControl()
        {
            return _manualControl;
        }

        public void ResetManualControl()
        {
            lock (lock_gate_manual)
            {
                _manualControl = false;
            }
        }

        public void SetManualControl()
        {
            lock (lock_gate_manual)
            {
                _manualControl = true;
            }
        }

        public void ResetStates()
        {
            _TakeOffStatus = CommonDefs.VesselState.NotStarted;
            _ManeuverStatus = CommonDefs.VesselState.NotStarted;
        }

        public void SetPitchAndHeading(float pitch, float heading,
            bool bKeepCurrentPitch = false, bool bKeepCurrentHeading = false, bool bWait = true)
        {
            MethodBase m = MethodBase.GetCurrentMethod();
            StringBuilder strMessage = new StringBuilder();

            if (pitch > 90.0f || pitch < -90.0f)
            {
                strMessage.AppendFormat("{0}.{1}: Invalid pitch range", m.ReflectedType.Name, m.Name);
                SendMessage(strMessage.ToString());
                return;
            }

            //Se bKeepCurrentPitch=true, ignora o valor de pitch
            //Se bKeepCurrentHeading=true, ignora o valor de heading
            ReferenceFrame CurrentRefFrame = _flightTelemetry.CurrentRefFrame;
            Flight flight = CurrentVessel.Flight(CurrentRefFrame);

            float h;
            float p;

            if (bKeepCurrentHeading)
                h = flight.Heading;
            else
                h = heading;

            if (bKeepCurrentPitch)
                p = flight.Pitch;
            else
                p = pitch;

            CurrentVessel.AutoPilot.TargetPitchAndHeading(p, h);

            if (bWait)
            {
                Thread.Sleep(500); //precisa deste delay para dar tempo da nave se mover e o wait() detectar
                CurrentVessel.AutoPilot.Wait();
            }
        }

        public void TurnOffAutoPilot()
        {
            CurrentVessel.AutoPilot.Disengage();
            CurrentVessel.Control.SAS = true;
            CurrentVessel.Control.RCS = true;

            System.Threading.Thread.Sleep(500);

            CurrentVessel.Control.SASMode = SASMode.StabilityAssist;
        }

        public void TurnOnAutoPilot()
        {
            CurrentVessel.AutoPilot.Engage();
            CurrentVessel.Control.SAS = false;
            CurrentVessel.Control.RCS = true;

            System.Threading.Thread.Sleep(500);

            //CurrentVessel.Control.SASMode = SASMode.StabilityAssist;
        }

        public void StartSRBTelemetry(int iSRBStage = 0)
        {
            _flightTelemetry.StartSRBTelemetry(iSRBStage);
        }

        public void SendMessage(string strMessage)
        {
            Console.WriteLine(strMessage);
            Mediator.Notify(CommonDefs.MSG_SEND_MESSAGE, strMessage);
        }

        //Thread
        private void CheckAcceleration()
        {
            SendMessage("Acceleration check has started.");
            while (true)
            {
                if (ReturnToManualControl())
                    return;

                var apoapsis_altitude = _flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Apoapsis_Altitude);
                if (apoapsis_altitude > _tod.TargetAltitude * 0.9)
                    break;

                Thread.Sleep(200);
            }

            //We have reached between 90% and 100% of apoapsis
            SendMessage("Approaching target apoapsis");
            //Reduce Vessel throttle
            CurrentVessel.Control.Throttle = 0.25f;

            while (true)
            {
                if (ReturnToManualControl())
                    return;

                var apoapsis_altitude = _flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Apoapsis_Altitude);
                if (apoapsis_altitude >= _tod.TargetAltitude)
                    break;

                Thread.Sleep(200);
            }

            SendMessage("Target apoapsis has been reached!");
            CurrentVessel.Control.Throttle = 0.0f;
            SendMessage("Acceleration check has ended.");
        }

        //Thread
        private void CheckGravityTurn()
        {
            SendMessage("Gravity turn check has started.");
            double turn_angle = 0.0;
            float current_angle = 0.0f;

            while (true)
            {
                if (ReturnToManualControl())
                    return;

                var altitude = _flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Surface_Altitude);
                //Gravity turn
                if (altitude > _tod.StartTurnAltitude)
                {
                    double frac = (altitude - _tod.StartTurnAltitude) / (_tod.EndTurnAltitude - _tod.StartTurnAltitude);
                    double new_turn_angle = frac * 90;

                    if (Math.Abs(new_turn_angle - turn_angle) > 0.5)
                    {
                        turn_angle = new_turn_angle;
                        CurrentVessel.AutoPilot.Engage();

                        current_angle = (float)(90 - turn_angle);
                        CurrentVessel.AutoPilot.TargetPitchAndHeading(current_angle, _tod.ShipHeadingAngle);
                    }
                }

                if (altitude > _tod.EndTurnAltitude)
                {
                    SendMessage("Gravity end turn altitude has been reached!");
                    break;
                }

                Thread.Sleep(200);
            }//while(true)

            //if current angle is not zero, finish to turn the ship
            CurrentVessel.AutoPilot.Engage();
            Thread.Sleep(500);
            CurrentVessel.Control.RCS = true;
            CurrentVessel.AutoPilot.TargetPitchAndHeading(0, _tod.ShipHeadingAngle);

            //Wait until pitch be lower than 5 degrees
            //while(_flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.VesselPitch) > 5.0d)
            //{
            //    Thread.Sleep(250);
            //}

            SendMessage("Gravity turn check has ended.");
        }

        //Thread
        private void CheckSRBs(int iSRBStage)
        {
            SendMessage("Solid fuel check has started.");
            this.StartSRBTelemetry(iSRBStage);
            Thread.Sleep(1000);

            while (true)
            {
                if (ReturnToManualControl())
                    return;

                var solid_fuel = _flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.SRB_Fuel);
                //SendMessage("SRB Fuel =" + solid_fuel.ToString());

                if (solid_fuel <= 300)
                {
                    for (int i = 5; i > 0; i--)
                    {
                        Thread.Sleep(1000);
                        SendMessage("Separating Booster in " + i + " seconds...");
                    }

                    CurrentVessel.Control.ActivateNextStage();
                    SendMessage("Boosters separated!");
                    break;
                }

                Thread.Sleep(1000);
            }
            SendMessage("Solid fuel check has ended.");
        }

        private bool IsBelowRotationAltitude()
        {
            var altitude = _flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Surface_Altitude);
            return (altitude < _tod.InitialRotationAltitude);
        }

        private bool IsBelowLimitAltitude()
        {
            var altitude = _flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Surface_Altitude);
            return (altitude < _tod.AtmosphereAltitude + 500);
        }

        //Thread
        private void CheckInitialRotation()
        {
            //Initial Rotation
            while (IsBelowRotationAltitude())
            {
                if (ReturnToManualControl())
                    return;

                Thread.Sleep(200);
            }

            SendMessage("Starting Initial Rotation...");
            TurnOnAutoPilot();
            //this.SetPitchAndHeading(90, 0.0f, false, true);
            this.SetPitchAndHeading(90, _tod.ShipHeadingAngle, true, false);

            //this.SetPitchAndHeading(90, _tod.ShipHeadingAngle, false, false);
            //CurrentVessel.AutoPilot.Engage();
            //CurrentVessel.AutoPilot.TargetPitchAndHeading(90, _tod.ShipHeadingAngle);
            //CurrentVessel.AutoPilot.Wait();
            SendMessage("Initial Rotation has been completed.");
        }

        private void CheckTakeoff(int iSRBStage)
        {
            //Create threads
            //Acceleration
            //Gravity Turn
            //SRB
            Task t1 = Task.Run(() => CheckInitialRotation());
            Task t2 = Task.Run(() => CheckAcceleration());
            Task t3 = Task.Run(() => CheckGravityTurn());
            Task t4 = iSRBStage > 0 ? Task.Run(() => CheckSRBs(iSRBStage)) : null;

            _TakeOffStatus = CommonDefs.VesselState.Executing;

            //Wait threads
            t1.Wait();
            t2.Wait();
            t3.Wait();

            if (iSRBStage > 0)
                t4.Wait();

            SendMessage("Coasting out of atmosphere...");
            while (IsBelowLimitAltitude())
            {
                if (ReturnToManualControl())
                    return;

                Thread.Sleep(200);
            }

            lock (lock_gate_takeoff)
            {
                _TakeOffStatus = CommonDefs.VesselState.Finished;
            }
            SendMessage("Out of atmosphere.");
        }

        public void Launch(TakeOffDescriptor _tod)
        {
            _TakeOffStatus = CommonDefs.VesselState.Preparation;

            this._tod = _tod;
            Console.WriteLine("Launching...");
            CurrentVessel.Control.SAS = false;
            CurrentVessel.Control.RCS = false;
            CurrentVessel.Control.Throttle = 1;
            CurrentVessel.Control.ActivateNextStage();

            Task t0 = Task.Run(() => CheckTakeoff(_tod.SRBStage));//return control to GUI
        }

        public void PlanCircularization(bool bReduceOrbit = false)
        {
            //bReduceOrbit => reduce the orbit from periapsis if true
            //bReduceOrbit => enlarge the orbit from apoapsis if false

            //Plan circularization burn (using vis-viva equation)
            SendMessage("Planning Circularization");

            double currentUT = _flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.UT);
            double uTime;

            double r;
            if (bReduceOrbit)
            {
                r = CurrentVessel.Orbit.Periapsis; //distance between the two bodies, usuallly the planet is on elipsis focus
                uTime = currentUT + CurrentVessel.Orbit.TimeToPeriapsis;
            }
            else
            {
                r = CurrentVessel.Orbit.Apoapsis; //distance between the two bodies, usuallly the planet is on elipsis focus
                uTime = currentUT + CurrentVessel.Orbit.TimeToApoapsis;
            }

            float mu = CurrentVessel.Orbit.Body.GravitationalParameter;
            double a = CurrentVessel.Orbit.SemiMajorAxis;

            //Velocity on current orbit
            double v1 = Math.Sqrt(mu * ((2.0 / r) - (1.0 / a)));

            //Velocity on adjusted orbit (circular)
            a = r; //on circle the semi-major axis IS THE RADIUS
            double v2 = Math.Sqrt(mu * ((2.0 / r) - (1.0 / a)));

            //Delta-V for orbit change (the signal is correctly calculated here)
            float delta_v = (float)(v2 - v1);

            //Creating the maneuver node
            CurrentVessel.Control.AddNode(uTime, delta_v);
        }

        public bool ExecuteManeuverNode()
        {
            if (CurrentVessel.AvailableThrust == 0 || CurrentVessel.SpecificImpulse == 0)
            {
                SendMessage("The engines are off, activate them, aborting maneuver...");
                return false;
            }

            Node node = null;
            if (CurrentVessel.Control.Nodes.Count > 0)
            {
                node = CurrentVessel.Control.Nodes[0];
            }
            else
            {
                SendMessage("No maneuver nodes available!");
                return false;
            }

            _ManeuverStatus = CommonDefs.VesselState.Preparation;
            Task t0 = Task.Run(() => CheckManeuverExecution(node)); //return control to GUI

            return true;
        }

        public float CalculateBurnTime()
        {
            SendMessage("Calculating Burn time...");

            Node node;
            if (CurrentVessel.Control.Nodes.Count > 0)
            {
                node = CurrentVessel.Control.Nodes[0];
            }
            else
            {
                SendMessage("No maneuver nodes available!");
                return 0.0f;
            }

            float burn_time;
            double delta_v = node.DeltaV;

            float F = CurrentVessel.AvailableThrust;
            float Isp = CurrentVessel.SpecificImpulse * CurrentVessel.Orbit.Body.SurfaceGravity; //9.82 in kerbin
            float m0 = CurrentVessel.Mass;
            float m1 = m0 / (float)Math.Exp(delta_v / Isp);
            float flow_rate = F / Isp;

            burn_time = (m0 - m1) / flow_rate;

            SendMessage("Burn time is " + burn_time + " seconds");
            return burn_time;
        }

        //Thread
        private void CheckManeuverExecution(Node node)
        {
            SendMessage("Starting Node Telemetry...");

            _maneuverBurnTime = CalculateBurnTime();

            _flightTelemetry.StartNodeTelemetry();
            Thread.Sleep(1000);//just wait for streaming to start

            //Go to maneuver, position ship and execute it
            Task t1 = Task.Run(() => Warp(60.0f, _maneuverBurnTime, node));
            t1.Wait();

            SendMessage("Maneuver Preparation: positioning ship...");

            TurnOffAutoPilot();
            Thread.Sleep(1000);
            CurrentVessel.Control.SASMode = SASMode.Maneuver;

            //Wait until start time

            SendMessage("Waiting burn time...");
            _maneuverBurnTime = CalculateBurnTime(); //atualiza caso haja correção (normalmente quando orbita de um planeta para outro)
            //while (node.UT - _flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.UT) > burnTime / 2.0d)
            while (_flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.NodeTimeTo) > _maneuverBurnTime / 2.0d)
            {
                if (ReturnToManualControl())
                    return;

                Thread.Sleep(250);
            }

            SendMessage("Executing Maneuver...");
            _ManeuverStatus = CommonDefs.VesselState.Executing;

            CurrentVessel.Control.Throttle = 1.0f;
            CurrentVessel.Control.RCS = false;

            double delta_v = node.DeltaV;
            float acc_burn = (float)delta_v / _maneuverBurnTime;

            //burn until 2 seconds before burning end
            while (_flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Remaining_DeltaV) > acc_burn / 2.0d)
            {
                if (ReturnToManualControl())
                    return;

                Thread.Sleep(100);
            }

            SendMessage("Fine tunning...");
            CurrentVessel.Control.Throttle = 0.1f;
            CurrentVessel.Control.SASMode = SASMode.StabilityAssist;

            //Stop control
            var last_delta_v = _flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Remaining_DeltaV);
            while (true)
            {
                if (ReturnToManualControl())
                    return;

                var rdv = _flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Remaining_DeltaV);

                if (rdv < 0)
                    break;

                if (rdv <= last_delta_v)
                    last_delta_v = rdv;
                else
                    break;
            }

            CurrentVessel.Control.Throttle = 0.0f;
            lock (lock_gate_maneuver)
            {
                _ManeuverStatus = CommonDefs.VesselState.Finished;
            }

            _maneuverBurnTime = 0.0f;
            SendMessage("Maneuver completed!");
        }

        //Thread
        private void Warp(float fLeadTime, float fBurnTime, Node maneuverNode)
        {
            SendMessage("Warping to " + fLeadTime + " seconds before starting maneuver");

            double totalLeadTime = fLeadTime + fBurnTime / 2.0;

            _conn.SpaceCenter().WarpTo(maneuverNode.UT - totalLeadTime);

            while (maneuverNode.UT - _flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.UT) > totalLeadTime)
            {
                if (ReturnToManualControl())
                    return;

                Thread.Sleep(200); //just wait until correct time 
            }

            SendMessage("Approaching maneuver node...");
        }
    }
}
