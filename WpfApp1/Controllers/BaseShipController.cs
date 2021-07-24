using System;
using System.Reflection;
using System.Text;
using System.Threading;
using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.Utils;


namespace WpfApp1.Controllers
{
    /// <summary>
    /// Classe base de controle de nave
    /// </summary>
    public class BaseShipController: IAsyncMessageUpdate
    {
        protected object lock_gate_manual = new object();
        protected bool _manualControl = false;

        protected Connection _conn = null;
        protected FlightTelemetry _flightTelemetry;

        protected CommonDefs.VesselState _TakeOffStatus;
        protected CommonDefs.VesselState _ManeuverStatus;
        protected CommonDefs.VesselState _SuicideBurnStatus;

        public CommonDefs.VesselState TakeOffStatus { get => _TakeOffStatus; }
        public CommonDefs.VesselState ManeuverStatus { get => _ManeuverStatus; }
        public CommonDefs.VesselState SuicideBurnStatus { get => _SuicideBurnStatus; }

        public Vessel CurrentVessel { get => _conn.SpaceCenter().ActiveVessel; }
        public string ShipName { get => CurrentVessel.Name; }

        public BaseShipController(in Connection conn, in FlightTelemetry _flightTel)
        {
            _conn = conn;
            _flightTelemetry = _flightTel;
        }

        public void ResetStates()
        {
            _SuicideBurnStatus = CommonDefs.VesselState.NotStarted;
            _TakeOffStatus = CommonDefs.VesselState.NotStarted;
            _ManeuverStatus = CommonDefs.VesselState.NotStarted;
        }

        protected bool ReturnToManualControl()
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

        protected void SetPitchAndHeading(float pitch, float heading,
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

        protected void TurnOffAutoPilot()
        {
            CurrentVessel.AutoPilot.Disengage();
            CurrentVessel.Control.SAS = true;
            CurrentVessel.Control.RCS = true;

            System.Threading.Thread.Sleep(500);

            CurrentVessel.Control.SASMode = SASMode.StabilityAssist;
        }

        protected void TurnOnAutoPilot()
        {
            CurrentVessel.AutoPilot.Engage();
            CurrentVessel.Control.SAS = false;
            CurrentVessel.Control.RCS = true;

            System.Threading.Thread.Sleep(500);

            //CurrentVessel.Control.SASMode = SASMode.StabilityAssist;
        }

        protected float GetHeading()
        {
            ReferenceFrame CurrentRefFrame = _flightTelemetry.CurrentRefFrame;
            Flight flight = CurrentVessel.Flight(CurrentRefFrame);

            return flight.Heading;
        }

        protected void SetupRoll(bool bWait = true)
        {
            SetPitchAndHeading(0, 0, true, bWait);
        }

        protected void SetShipPosition(SASMode mode, bool bWait = true)
        {
            //Para setar o flag AutoPilot.SAS, somente com AutoPilot Disengaged
            //Ao que parece, setar o flag AutoPilot.SAS vai dar engage novamente
            //mas é obrigatório aguardar um tempo antes de setar o SASMode
            TurnOffAutoPilot();

            CurrentVessel.Control.RCS = true;
            CurrentVessel.AutoPilot.SAS = true;
            Thread.Sleep(500);//Esse sleep é obrigatorio
            CurrentVessel.AutoPilot.SASMode = mode;

            if (bWait)
            {
                CurrentVessel.AutoPilot.Wait();
            }
        }

        //DEVE IR PARA BaseShipController
        protected void SetEnginesGimball(bool bOn = true)
        {
            foreach (Part item in CurrentVessel.Parts.All)
            {
                if (item.Engine != null)
                {
                    try
                    {
                        item.Engine.GimbalLocked = bOn;
                    }
                    catch (System.InvalidOperationException e)
                    {
                    }
                }
            }
        }

        public void SendMessage(string strMessage)
        {
            Console.WriteLine(strMessage);
            Mediator.Notify(CommonDefs.MSG_SEND_MESSAGE, strMessage);
        }
    }
}
