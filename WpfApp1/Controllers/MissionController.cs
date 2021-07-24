using System;
using System.Threading;
using WpfApp1.Services;
using WpfApp1.Utils;

using WpfApp1.Models;

namespace WpfApp1
{
    class MissionController : IAsyncMessageUpdate
    {
        public readonly ShipFlighter ShipControl;
        public readonly ConnectionProxy Connector;

        private bool _manualControl = false;
        private object lock_gate_manual = new object();

        public MissionController(ConnectionProxy connector)
        {           
            Connector = connector;
            ShipControl = new ShipFlighter(Connector._connection);
        }

        private bool ReturnToManualControl()
        {
            return _manualControl;
        }

        public void ResetVesselStates()
        {
            ShipControl.ResetStates();
        }

        public void ResetManualControl()
        {
            ShipControl.ResetManualControl();

            lock (lock_gate_manual)
            {
                _manualControl = false;
            }
        }

        public void RestartTelemetry(bool basicTelemetryOn, bool nodeTelemetryOn)
        {
            ShipControl.RestartTelemetry(basicTelemetryOn, nodeTelemetryOn);
        }

        public void StopAllTelemetry(bool bClearScreen=true)
        {
            ShipControl.StopAllTelemetry();

            if (bClearScreen)
            {
                ClearScreen();
            }
        }

        public void SetManualControl()
        {
            SendMessage("Setting control to manual...");

            ShipControl.SetManualControl();

            lock (lock_gate_manual)
            {
                _manualControl = true;
            }

            SendMessage("Control was set to manual.");

            StopAllTelemetry(false);
        }

        public void PlanCircularization(bool bReduce = false)
        {
            //Restart Telemetry
            RestartTelemetry(false, true);

            ShipControl.PlanCircularization(bReduce);
        }

        //This method is synchronous
        public void ExecuteManeuverNode()
        {
            RestartTelemetry(false, true);
            Thread.Sleep(1000);

            SendMessage("Executing Maneuver Node...");

            bool bRet = ShipControl.ExecuteManeuverNode();

            SendMessage("Waiting Maneuver Node to end...");
            while (ShipControl.ManeuverStatus != CommonDefs.VesselState.Finished)
            {
                if (ReturnToManualControl())
                    return;

                Thread.Sleep(1000);
            }
            SendMessage("Maneuver Node has ended.");

            StopAllTelemetry(false);
        }

        //This method is synchronous
        public void ExecuteTakeOff(TakeOffDescriptor _tod)
        {
            //Restart Telemetry
            RestartTelemetry(true, true);

            //Fazer contagem regressiva e ligar a nave
            for (int i = 3; i >= 0; i--)
            {
                Thread.Sleep(1000);
                SendMessage("Launching in " + i + " seconds...");
            }

            SendMessage("Launch!");
            ShipControl.Launch(_tod);

            SendMessage("Waiting takeoff to end...");
            while (ShipControl.TakeOffStatus != CommonDefs.VesselState.Finished)
            {
                if (ReturnToManualControl())
                    return;

                Thread.Sleep(500);
            }

            //Execute
            PlanCircularization();

            //Execute Maneuver
            ExecuteManeuverNode();

            SendMessage("Orbital Maneuver has ended.");
            StopAllTelemetry(false);
        }

        //This method is synchronous
        public void ExecuteSuicideBurn(SuicideBurnSetup suicideBurnSetup)
        {
            RestartTelemetry(true, false);
            Thread.Sleep(1000);

            SendMessage("Executing Suicide Burn...");

            ShipControl.ExecuteSuicideBurn(suicideBurnSetup);

            //Wait
            while (ShipControl.SuicideBurnStatus != CommonDefs.VesselState.Finished)
            {
                if (ReturnToManualControl())
                    return;

                Thread.Sleep(500);
            }

            SendMessage("Suicide Burn has ended.");
            StopAllTelemetry(false);
        }

        public void SendMessage(string strMessage)
        {
            Console.WriteLine(strMessage);
            Mediator.Notify(CommonDefs.MSG_SEND_MESSAGE, strMessage);
        }

        public void ClearScreen()
        {
            Mediator.Notify(CommonDefs.MSG_CLEAR_SCREEN);
        }
    }
}