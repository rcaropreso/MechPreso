using System;
using System.Threading.Tasks;

using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;
using WpfApp1.Models;
using System.Threading;

namespace WpfApp1.Controllers
{
    /// <summary>
    /// Classe que gerencia a decolagem da nave
    /// </summary>
    public class TakeOffController : BaseShipController
    {
        /// <summary>
        /// Describes the controls of a given ship
        /// </summary>
        private TakeOffDescriptor _tod;

        private readonly object lock_gate_takeoff = new object();

        public TakeOffController(in Connection conn, in FlightTelemetry _flightTel) : base(conn, _flightTel)
        {
        }

        public void StartSRBTelemetry(int iSRBStage = 0)
        {
            _flightTelemetry.StartSRBTelemetry(iSRBStage);
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
            this.SetPitchAndHeading(90, 0.0f, false, true); //primeiro fique na vertical
            this.SetPitchAndHeading(90, _tod.ShipHeadingAngle, true, false); //agora rotaciona
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
    }
}
