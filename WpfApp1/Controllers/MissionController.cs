using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WpfApp1.Services;
using WpfApp1.Utils;

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
            ShipControl = new ShipFlighter(Connector?._connection);
        }

        //This method is synchronous
        public void ExecuteTakeOff(TakeOffDescriptor _tod)
        {
            //Restart Telemetry
            RestartTelemetry();

            //Fazer contagem regressiva e ligar a nave
            for (int i = 3; i >= 0; i--)
            {
                Thread.Sleep(1000);
                SendMessage("Launching in " + i + " seconds...");
            }

            SendMessage("Launch!");
            ShipControl.Launch(_tod);

            SendMessage("Waiting takeoff to end...");
            while (ShipControl.TakeOffStatus != ShipFlighter.VesselState.Finished)
            {
                if (ReturnToManualControl())
                    return;

                Thread.Sleep(500);
            }

            //Execute
            PlanCircularization();

            //Execute Maneuver
            ExecuteManeuverNode();

            //Wait
            while (ShipControl.ManeuverStatus != ShipFlighter.VesselState.Finished)
            {
                if (ReturnToManualControl())
                    return;

                Thread.Sleep(500);
            }
            SendMessage("Orbital Maneuver has ended.");
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

        public void RestartTelemetry()
        {
            ShipControl.RestartTelemetry();
        }

        public void SetManualControl()
        {
            ShipControl.SetManualControl();

            lock (lock_gate_manual)
            {
                _manualControl = true;
            }
        }

        public void PlanCircularization(bool bReduce = false)
        {
            ShipControl.PlanCircularization(bReduce);
        }

        //This method is synchronous
        public void ExecuteSuicideBurn(SuicideBurnSetup suicideBurnSetup)
        {
            RestartTelemetry();
            Thread.Sleep(1000);

            SendMessage("Executing Suicide Burn...");

            ShipControl.ExecuteSuicideBurn(suicideBurnSetup);

            //Wait
            while (ShipControl.SuicideBurnStatus != ShipFlighter.VesselState.Finished)
            {
                if (ReturnToManualControl())
                    return;

                Thread.Sleep(500);
            }

            SendMessage("Suicide Burn has ended.");
        }

        //This method is synchronous
        public void ExecuteManeuverNode()
        {
            RestartTelemetry();
            Thread.Sleep(1000);

            SendMessage("Executing Maneuver Node...");

            bool bRet = ShipControl.ExecuteManeuverNode();

            SendMessage("Waiting Maneuver Node to end...");
            while (ShipControl.ManeuverStatus != ShipFlighter.VesselState.Finished)
            {
                if (ReturnToManualControl())
                    return;

                Thread.Sleep(1000);
            }
            SendMessage("Maneuver Node has ended.");
        }

        public void SendMessage(string strMessage)
        {
            Console.WriteLine(strMessage);
            Mediator.Notify("SendMessage", strMessage);
        }
    }
}
