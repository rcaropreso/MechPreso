using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;
using WpfApp1.Services;
using WpfApp1.Utils;
using WpfApp1.Models;

namespace WpfApp1
{
    public class FlightTelemetry : IAsyncMessageUpdate
    {
        public enum TelemetryInfo
        {
            Apoapsis_Altitude=0,
            Periapsis_Altitude,
            Surface_Altitude,
            Mean_Altitude,
            UT,
            SRB_Fuel,
            Remaining_DeltaV,
            Terminal_Velocity,
            VesselPitch,
            VesselHeading,
            NodeTimeTo,
            CurrentSpeed,
            HorizontalSpeed,
            VerticalSpeed,
            Latitude,
            Longitude
        };

        public ReferenceFrame CurrentRefFrame { get => _CurrentRefFrame; }

        private Connection m_conn = null;
        private StreamProxy m_streamManager;
        private ReferenceFrame _CurrentRefFrame;


        private bool m_bIsBaseTelemetryOn;
        private bool m_bIsNodeTelemetryOn; 
        private bool m_bIsSRBTelemetryOn;
        private bool m_bIsRoverTelemetryOn;

        #region Public Methods
        public FlightTelemetry(in Connection conn)
        {
            m_conn = conn;

            if (m_conn != null)
            {
                //TODO: Verificar qual é a GameScene para pegar o ActiveVessel
                Vessel currentVessel = m_conn?.SpaceCenter().ActiveVessel;
                _CurrentRefFrame = ReferenceFrame.CreateHybrid(m_conn, currentVessel.Orbit.Body.ReferenceFrame, currentVessel.SurfaceReferenceFrame);
                m_streamManager = new StreamProxy(m_conn, CurrentRefFrame);
            }
        }
        
        public double GetInfo(TelemetryInfo infoType)
        {
            double? returnValue = -1.0d;

            try
            {
                switch (infoType)
                {
                    case TelemetryInfo.Apoapsis_Altitude:
                        if (m_bIsBaseTelemetryOn)
                        {
                            returnValue = m_streamManager?.ApoapsisAltitudeStream?.Get();
                        }
                        break;

                    case TelemetryInfo.Periapsis_Altitude:
                        if (m_bIsBaseTelemetryOn)
                        {
                            returnValue = m_streamManager?.PeriapsisAltitudeStream?.Get();
                        }
                        break;

                    case TelemetryInfo.Surface_Altitude:
                        if (m_bIsBaseTelemetryOn)
                        {
                            returnValue = m_streamManager?.SurfaceAltitudeStream?.Get();
                        }
                        break;

                    case TelemetryInfo.Mean_Altitude:
                        if (m_bIsBaseTelemetryOn)
                        {
                            returnValue = m_streamManager?.MeanAltitudeStream?.Get();
                        }
                        break;

                    case TelemetryInfo.Terminal_Velocity:
                        if (m_bIsBaseTelemetryOn)
                        {
                            returnValue = m_streamManager?.TerminalVelocityStream?.Get();
                        }
                        break;

                    case TelemetryInfo.VesselHeading:
                        if (m_bIsBaseTelemetryOn)
                        {
                            returnValue = m_streamManager?.VesselHeadingStream?.Get();
                        }
                        break;

                    case TelemetryInfo.VesselPitch:
                        if (m_bIsBaseTelemetryOn)
                        {
                            returnValue = m_streamManager?.VesselPitchStream?.Get();
                        }
                        break;

                    case TelemetryInfo.CurrentSpeed:
                        if (m_bIsBaseTelemetryOn)
                        {
                            returnValue = m_streamManager?.CurrentSpeedStream?.Get();
                        }
                        break;

                    case TelemetryInfo.HorizontalSpeed:
                        if (m_bIsBaseTelemetryOn)
                        {
                            returnValue = m_streamManager?.HorizontalSpeedStream?.Get();
                        }
                        break;

                    case TelemetryInfo.VerticalSpeed:
                        if (m_bIsBaseTelemetryOn)
                        {
                            returnValue = m_streamManager?.VerticalSpeedStream?.Get();
                        }
                        break;

                    case TelemetryInfo.UT:
                        if (m_bIsNodeTelemetryOn)
                        {
                            returnValue = m_streamManager?.UTStream?.Get();
                        }
                        break;

                    case TelemetryInfo.Remaining_DeltaV:
                        if (m_bIsNodeTelemetryOn)
                        {
                            returnValue = m_streamManager?.RemainingDeltaVStream?.Get();
                        }
                        break;

                    case TelemetryInfo.NodeTimeTo:
                        if (m_bIsNodeTelemetryOn)
                        {
                            returnValue = m_streamManager?.NodeTimeTo?.Get();
                        }
                        break;

                    case TelemetryInfo.SRB_Fuel:
                        if (m_bIsSRBTelemetryOn)
                        {
                            returnValue = m_streamManager?.SRBFuelStream?.Get();
                        }
                        break;

                    case TelemetryInfo.Latitude:
                        if(m_bIsRoverTelemetryOn)
                        {
                            returnValue = m_streamManager?.LatitudeStream?.Get();
                        }
                        break;

                    case TelemetryInfo.Longitude:
                        if (m_bIsRoverTelemetryOn)
                        {
                            returnValue = m_streamManager?.LongitudeStream?.Get();
                        }
                        break;

                    default:
                        returnValue = -1.0d;
                        break;
                }
            }
            catch(Exception ex) when (ex is System.IO.IOException ||
                                      ex is System.InvalidOperationException ||
                                      ex is KRPC.Client.RPCException)
            {
                MethodBase m = MethodBase.GetCurrentMethod();
                StringBuilder strMessage = new StringBuilder();
                strMessage.AppendFormat("Exception on {0}.{1}:{2}", m.ReflectedType.Name, m.Name, ex.Message);
                SendMessage(strMessage.ToString());
                return 0.0d;
            }

            return returnValue ?? -1.0d;
        }

        public void RestartTelemetry(bool basicTelemetryOn, bool nodeTelemetryOn, bool roverTelemetryOn)
        {
            StopAllTelemetry();

            Vessel currentVessel = m_conn?.SpaceCenter().ActiveVessel;
            _CurrentRefFrame = ReferenceFrame.CreateHybrid(m_conn, currentVessel.Orbit.Body.ReferenceFrame, currentVessel.SurfaceReferenceFrame);
            m_streamManager = new StreamProxy(m_conn, CurrentRefFrame);

            if (basicTelemetryOn)
            {
                StartBasicTelemetry();
            }

            if (nodeTelemetryOn)
            {
                StartNodeTelemetry();
            }

            if(roverTelemetryOn)
            {
                StartRoverTelemetry();
            }

            //Por enquanto o evento de timer nao está separado para cada tipo de timer ou telemetria, entao temos que ligar todos
            Thread.Sleep(1000);
            Mediator.Notify(CommonDefs.MSG_START_TIMERS, "");//envia mensagem para a GUI voltar a monitorar o stream
        }

        public void StopAllTelemetry()
        {
            m_bIsBaseTelemetryOn = false;
            m_bIsNodeTelemetryOn = false;
            m_bIsSRBTelemetryOn = false;
            m_bIsRoverTelemetryOn = false;
            
            Mediator.Notify(CommonDefs.MSG_STOP_TIMERS, "");//envia mensagem para a GUI parar de monitorar o stream
            Thread.Sleep(250);
            
            m_streamManager.RemoveStreams();
        }

        public void StartBasicTelemetry()
        {
            m_streamManager.CreateApoapsisAltitudeStream();
            m_streamManager.CreatePeriapsisAltitudeStream();
            m_streamManager.CreateSurfaceAltitudeStream();
            m_streamManager.CreateMeanAltitudeStream();
            m_streamManager.CreateTerminalVelocityStream();
            m_streamManager.CreateVesselPitchStream();
            m_streamManager.CreateVesselHeadingStream();
            m_streamManager.CreateCurrentSpeedStream();
            m_streamManager.CreateHorizontalSpeedStream();
            m_streamManager.CreateVerticalSpeedStream();

            m_bIsBaseTelemetryOn = true;
        }

        public void StartSRBTelemetry(int iSRBStage)
        {
            m_streamManager.CreateSolidFuelStream(iSRBStage);
            
            m_bIsSRBTelemetryOn = true;
        }

        public void StartNodeTelemetry()
        {
            m_streamManager.CreateRemainingDeltaVStream();
            m_streamManager.CreateNodeTimeTo();
            m_streamManager.CreateUTStream();
            
            m_bIsNodeTelemetryOn = true;
        }

        public void StartRoverTelemetry()
        {
            m_streamManager.CreateLatitudeStream();
            m_streamManager.CreateLongitudeStream();

            m_bIsRoverTelemetryOn = true;
        }

        public RoverData GetRoverTelemetryInfo()
        {
            var lat = (float) GetInfo(TelemetryInfo.Latitude);
            var lon = (float) GetInfo(TelemetryInfo.Longitude);

            RoverData retData = new RoverData(lat, lon);

            return retData;
        }

        public SuicideBurnData GetSuicideBurnTelemetryInfo() //poderia fazer um takeoffTelemetryInfo
        {
            if(!m_bIsBaseTelemetryOn) //telemetria desligada
            {
                return new SuicideBurnData(0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            }

            var g         = GetGravity();
            var a         = GetEngineAcceleration();
            var v         = GetInfo(TelemetryInfo.CurrentSpeed);
            var vv        = GetInfo(TelemetryInfo.VerticalSpeed);
            var vh        = GetInfo(TelemetryInfo.HorizontalSpeed);
            float theta   = (float)Math.Atan2(Math.Abs(vv), Math.Abs(vh));//angulo entre o vetor velocidade e a horizontal
            var altitude  = GetInfo(TelemetryInfo.Surface_Altitude);
            //var meanAltitude    = GetInfo(TelemetryInfo.Mean_Altitude);           

            //Calcula altura percorrida pelo veiculo durante o cancelamento da velocidade vertical (assumindo a nave na vertical)
            float av = (float)a - g;
            var verticalBurnHeight = Math.Pow(vv, 2) / (2 * av);

            //Calcula altura percorrida pelo veiculo durante o cancelamento da velocidade horizontal (assumindo a nave na horizontal)
            float ah = (float) (a * Math.Cos(theta));
            float t_burn = Math.Abs((float)vh / ah); //Tempo para zerar velocidade horizontal
            float horizontalBurnHeight = (float)( Math.Abs(vv) * t_burn + g * t_burn * t_burn * 0.5); //

            //Verificar os seguintes parametros: https://krpc.github.io/krpc/csharp/api/space-center/auto-pilot.html#property-KRPC.Client.Services.SpaceCenter.AutoPilot.SASMode
            //DecelerationTime { get; set; }
            //StoppingTime { get; set; }

            //Obtem maior altitude do corpo onde está pousando
            Vessel currentVessel = m_conn?.SpaceCenter().ActiveVessel;
            string bodyName = currentVessel.Orbit.Body.Name.ToLower();
            var planetData = PlanetData.DateFromBody[bodyName];

            SuicideBurnData data = new SuicideBurnData(v, a, vv, vh, altitude, theta * 180 / Math.PI, verticalBurnHeight, horizontalBurnHeight, planetData.MaxHeight, g);

            return data;
        }

        public float GetGravity()
        {
            try
            {
                var gravity = m_conn?.SpaceCenter()?.ActiveVessel?.Orbit.Body.SurfaceGravity;
                return gravity ?? 0.0f; //in m/s^2
            }
            catch (System.IO.IOException e)
            {
                MethodBase m = MethodBase.GetCurrentMethod();
                StringBuilder strMessage = new StringBuilder();
                strMessage.AppendFormat("Exception on {0}.{1}:{2}", m.ReflectedType.Name, m.Name, e.Message);
                SendMessage(strMessage.ToString());
                return 0.0f;
            } 
        }

        public float GetThrust()
        {
            try
            {
                Vessel vv = m_conn.SpaceCenter().ActiveVessel;

                var thrust = m_conn?.SpaceCenter()?.ActiveVessel?.AvailableThrust; //in Newtons
                return thrust ?? 0.0f;
            }
            catch (System.IO.IOException e)
            {
                MethodBase m = MethodBase.GetCurrentMethod();
                StringBuilder strMessage = new StringBuilder();
                strMessage.AppendFormat("Exception on {0}.{1}:{2}", m.ReflectedType.Name, m.Name, e.Message);
                SendMessage(strMessage.ToString());
                return 0.0f;
            }
        }

        public float GetMass()
        {
            try
            {
                var mass = m_conn?.SpaceCenter()?.ActiveVessel?.Mass; //in kg
                return mass ?? 1.0f; //evita divisao por zero
            }
            catch (System.IO.IOException e)
            {
                MethodBase m = MethodBase.GetCurrentMethod();
                StringBuilder strMessage = new StringBuilder();
                strMessage.AppendFormat("Exception on {0}.{1}:{2}", m.ReflectedType.Name, m.Name, e.Message);
                SendMessage(strMessage.ToString());
                return 0.0f;
            }            
        }

        #endregion

        #region Private Methods
        public float GetEngineAcceleration()
        {
            return GetThrust() / GetMass(); //in m/s^2
        }

        public double GetAverageAcceleration()
        {
            double v1;
            double v2;

            double sumAcc = 0.0d;

            int deltaT_ms = 3000;
            int deltaT_s = deltaT_ms / 1000;
            int nSamples = 3;

            m_conn.SpaceCenter().ActiveVessel.Control.Throttle = 1.0f;
            for (int i = 0; i < nSamples; i++)
            {
                v1 = GetInfo(TelemetryInfo.CurrentSpeed);
                
                Thread.Sleep(deltaT_ms);

                v2 = GetInfo(TelemetryInfo.CurrentSpeed);
                sumAcc += (v1 - v2) / deltaT_s;
            }
            m_conn.SpaceCenter().ActiveVessel.Control.Throttle = 0.0f;
            return sumAcc / nSamples;
        }

        public void SendMessage(string strMessage)
        {
            Console.WriteLine(strMessage);
            Mediator.Notify(CommonDefs.MSG_SEND_MESSAGE, strMessage);
        }
        #endregion
    }
}
