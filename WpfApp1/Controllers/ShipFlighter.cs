using System;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;
using WpfApp1.Controllers;
using System.Reflection;
using WpfApp1.Services;
using WpfApp1.Utils;
using WpfApp1.Models;
using Vector3 = System.Tuple<double, double, double>;

namespace WpfApp1
{
    public class ShipFlighter : IAsyncMessageUpdate
    {
        /// <summary>
        /// Describes the controls of a given ship
        /// </summary>
        /// 
        private Connection _conn = null;

        private FlightTelemetry _flightTelemetry;
        public FlightTelemetry Telemetry { get => _flightTelemetry; }
        
        public Vessel CurrentVessel { get => _conn.SpaceCenter().ActiveVessel; }        
        public string ShipName { get => CurrentVessel.Name; }

        public CommonDefs.VesselState TakeOffStatus { get => _takeOffController.TakeOffStatus; }
        public CommonDefs.VesselState SuicideBurnStatus { get => _landingController.SuicideBurnStatus; }
        public CommonDefs.VesselState ManeuverStatus { get => _maneuverController.ManeuverStatus; }
        public float ManeuverBurnTime { get => _maneuverController.ManeuverBurnTime; }

        private LandingController _landingController;
        private TakeOffController _takeOffController;
        private ManeuverController _maneuverController;

        public ShipFlighter(in Connection conn)
        {
            _conn = conn;                
            _flightTelemetry = new FlightTelemetry(in conn);

            SendMessage("Starting Telemetry...");
            _flightTelemetry?.StartBasicTelemetry();
            Thread.Sleep(2000);
            SendMessage("Telemetry has started.");

            //Create controllers
            _landingController = new LandingController(in conn, in _flightTelemetry);
            _takeOffController = new TakeOffController(in conn, in _flightTelemetry);
            _maneuverController = new ManeuverController(in conn, in _flightTelemetry);
        }

        public void SetManualControl()
        {
            _landingController.SetManualControl();
            _takeOffController.SetManualControl();
            _maneuverController.SetManualControl();
        }

        public void ResetManualControl()
        {
            _landingController.ResetManualControl();
            _takeOffController.ResetManualControl();
            _maneuverController.ResetManualControl();
        }

        public void RestartTelemetry(bool basicTelemetryOn, bool nodeTelemetryOn)
        {
            _flightTelemetry.RestartTelemetry(basicTelemetryOn, nodeTelemetryOn);
        }

        public void StopAllTelemetry()
        {
            _flightTelemetry.StopAllTelemetry();
        }

        public void ResetStates()
        {
            _landingController.ResetStates();
            _takeOffController.ResetStates();
            _maneuverController.ResetStates();
        }

        public void SendMessage(string strMessage)
        {
            Console.WriteLine(strMessage);
            Mediator.Notify(CommonDefs.MSG_SEND_MESSAGE, strMessage);
        }

        //ESTE AQUI TEM QUE SER THREAD TAMBÉM SENAO TRAVA A TELA
        //VAMOS TER QUE CRIAR UMA THREAD DE MONITORAMENTO E VAZAR DAQUI
        public void ExecuteSuicideBurn(SuicideBurnSetup suicideBurnSetup)
        {
            _landingController.ExecuteSuicideBurn(suicideBurnSetup);
        }

        public void ExecuteDeorbitBody(SuicideBurnSetup suicideBurnSetup)
        {
            _landingController.ExecuteDeorbitBody(suicideBurnSetup);
        }

        public void ExecuteCancelVVel(SuicideBurnSetup suicideBurnSetup)
        {
            _landingController.ExecuteCancelVVel(suicideBurnSetup);
        }

        public void ExecuteCancelHVel(SuicideBurnSetup suicideBurnSetup)
        {
            _landingController.ExecuteCancelHVel(suicideBurnSetup);
        }

        public void ExecuteStopBurn(SuicideBurnSetup suicideBurnSetup)
        {
            _landingController.ExecuteStopBurn(suicideBurnSetup);
        }

        public void ExecuteFineTunning(SuicideBurnSetup suicideBurnSetup)
        {
            _landingController.ExecuteFineTunning(suicideBurnSetup);
        }

        public void Launch(TakeOffDescriptor _tod)
        {
            _takeOffController.Launch(_tod);
        }

        public void PlanCircularization(bool bReduceOrbit = false)
        {
            _maneuverController.PlanCircularization(bReduceOrbit);
        }

        public bool ExecuteManeuverNode()
        {
            return _maneuverController.ExecuteManeuverNode();
        }

        public float CalculateBurnTime()
        {
            return _maneuverController.CalculateBurnTime();
        }
    }
}
