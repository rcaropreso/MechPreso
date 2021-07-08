using KRPC.Client;
using System;
using System.Threading.Tasks;

using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;
using WpfApp1.Models;
using System.Threading;

namespace WpfApp1.Controllers
{
    public class ManeuverController : BaseShipController
    {
        private readonly object lock_gate_maneuver = new object();

        private float _maneuverBurnTime;
        public float ManeuverBurnTime
        {
            get
            {
                return _maneuverBurnTime;
            }
        }

        public ManeuverController(in Connection conn, in FlightTelemetry _flightTel) : base(conn, _flightTel)
        {
        }

        //DEVE IR PARA A CLASSE ManeuverController
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

        //REFATORAR PARA UTILIZAR A CLASSE _maneuverController
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

        //DEVE IR PARA A CLASSE ManeuverController
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

        //DEVE IR PARA A CLASSE ManeuverController
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

        //DEVE IR PARA A CLASSE ManeuverController
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
