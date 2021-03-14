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
            VerticalSpeed
        };

        public ReferenceFrame CurrentRefFrame { get => _CurrentRefFrame; }

        private Connection m_conn = null;
        private StreamProxy m_streamManager;
        private ReferenceFrame _CurrentRefFrame;

        #region Public Methods
        public FlightTelemetry(Connection conn)
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
                        returnValue = m_streamManager?.ApoapsisAltitudeStream?.Get();
                        break;

                    case TelemetryInfo.Periapsis_Altitude:
                        returnValue = m_streamManager?.PeriapsisAltitudeStream?.Get();
                        break;

                    case TelemetryInfo.Surface_Altitude:
                        returnValue = m_streamManager?.SurfaceAltitudeStream?.Get();
                        break;

                    case TelemetryInfo.Mean_Altitude:
                        returnValue = m_streamManager?.MeanAltitudeStream?.Get();
                        break;

                    case TelemetryInfo.UT:
                        returnValue = m_streamManager?.UTStream?.Get();
                        break;

                    case TelemetryInfo.SRB_Fuel:
                        returnValue = m_streamManager?.SRBFuelStream?.Get();
                        break;

                    case TelemetryInfo.Remaining_DeltaV:
                        returnValue = m_streamManager?.RemainingDeltaVStream?.Get();
                        break;

                    case TelemetryInfo.Terminal_Velocity:
                        returnValue = m_streamManager?.TerminalVelocityStream?.Get();
                        break;

                    case TelemetryInfo.VesselHeading:
                        returnValue = m_streamManager?.VesselHeadingStream?.Get();
                        break;

                    case TelemetryInfo.VesselPitch:
                        returnValue = m_streamManager?.VesselPitchStream?.Get();
                        break;

                    case TelemetryInfo.NodeTimeTo:
                        returnValue = m_streamManager?.NodeTimeTo?.Get();
                        break;

                    case TelemetryInfo.CurrentSpeed:
                        returnValue = m_streamManager?.CurrentSpeedStream?.Get();
                        break;

                    case TelemetryInfo.HorizontalSpeed:
                        returnValue = m_streamManager?.HorizontalSpeedStream?.Get();
                        break;

                    case TelemetryInfo.VerticalSpeed:
                        returnValue = m_streamManager?.VerticalSpeedStream?.Get();
                        break;

                    default:
                        returnValue = -1.0d;
                        break;
                }
            }
            catch(Exception ex) when (ex is System.IO.IOException ||
                                      ex is System.InvalidOperationException)
            {
                MethodBase m = MethodBase.GetCurrentMethod();
                StringBuilder strMessage = new StringBuilder();
                strMessage.AppendFormat("Exception on {0}.{1}:{2}", m.ReflectedType.Name, m.Name, ex.Message);
                SendMessage(strMessage.ToString());
                return 0.0d;
            }

            return returnValue ?? -1.0d;
        }

        public void RestartTelemetry()
        {
            StopAllTelemetry();

            Vessel currentVessel = m_conn?.SpaceCenter().ActiveVessel;
            _CurrentRefFrame = ReferenceFrame.CreateHybrid(m_conn, currentVessel.Orbit.Body.ReferenceFrame, currentVessel.SurfaceReferenceFrame);
            m_streamManager = new StreamProxy(m_conn, CurrentRefFrame);

            StartAllTelemetry();
        }

        public void SetupSRBTelemetry(int iSRBStage)
        {
            m_streamManager.CreateSolidFuelStream(iSRBStage);
        }

        public void StartAllTelemetry()
        {
            m_streamManager.CreateVesselHeadingStream();
            m_streamManager.CreateVesselPitchStream();
            m_streamManager.CreateApoapsisAltitudeStream();
            m_streamManager.CreatePeriapsisAltitudeStream();
            m_streamManager.CreateSurfaceAltitudeStream();
            m_streamManager.CreateMeanAltitudeStream();
            m_streamManager.CreateTerminalVelocityStream();
            m_streamManager.CreateUTStream();
            m_streamManager.CreateCurrentSpeedStream();
            m_streamManager.CreateHorizontalSpeedStream();
            m_streamManager.CreateVerticalSpeedStream();
        }

        public void SetupNodeTelemetry()
        {
            m_streamManager.CreateRemainingDeltaVStream();
            m_streamManager.CreateNodeTimeTo();
        }

        public void StopAllTelemetry()
        {
            m_streamManager.RemoveStreams();
        }

        public SuicideBurnData GetSuicideBurnTelemetryInfo() //poderia fazer um takeoffTelemetryInfo
        {
            var g         = GetGravity();
            var a         = GetEngineAcceleration();
            var v         = GetInfo(TelemetryInfo.CurrentSpeed);
            var vv        = GetInfo(TelemetryInfo.VerticalSpeed);
            var vh        = GetInfo(TelemetryInfo.HorizontalSpeed);
            float theta   = (float)Math.Atan2(Math.Abs(vv), Math.Abs(vh));//angulo entre o vetor velocidade e a horizontal
            var alt       = GetInfo(TelemetryInfo.Surface_Altitude);
            //var meanAltitude    = GetInfo(TelemetryInfo.Mean_Altitude);           

            //Estimativa de altitude para inicio da queima vertical  
            //Considerando a nave na vertical
            double av = a - g;
            var verticalBurnStartAltitude = Math.Pow(vv, 2) / (2 * av);

            //Estimativa de altitude para inicio de queima de cancelamento de velocidade horizontal
            //Altura da queda enquanto cancela a velocidade horizontal
            float thetaEq = (float)Math.Asin(g / a);
            var th = vh / (a * Math.Cos(thetaEq));

            //O angulo thetaEq deveria cancelar a gravidade, portanto a queda é MU (Movimento Uniforme)
            //var v0 = Math.Abs(vv); //velocidade vertical apos th segundos, em thetaEq a vh nao deveria mudar
            //var dh = v0 * th;
            //var horizontalBurnStartAltitude = verticalBurnStartAltitude + dh;            

            //TESTE 28/08/2020 - BEGIN
            //Calcula altura de inicio da queima puramente horizontal
            //float v_htarget = 0.0f;
            //float v_hnow = (float)vh;
            //float v_vnow = (float)vv;
            //float ar = (float)a - g;
            //float ah = (float) a;
            //float t_burn = Math.Abs((v_htarget - v_hnow) / ah);
            //float y_now = (float)(Math.Pow((v_vnow + (-g) * t_burn), 2) / (2 * ar) - v_vnow * t_burn - 0.5 * (-g) * Math.Pow(t_burn, 2));

            //Calcula altura de inicio da queima mantendo velocidade vertical constante
            float v_htarget = 0.0f;//(float)vv;
            float v_hnow = (float)vh;
            float v_vnow = (float)vv;
            float ar = (float)a - g;
            float ah = (float) (a * Math.Cos(theta));

            float t_burn = Math.Abs((v_htarget - v_hnow) / ah);
            float y_now = (float)(Math.Pow(v_vnow, 2) / (2 * ar) - v_vnow * t_burn);
            //TESTE 28/08/2020 - END

            SuicideBurnData data = new SuicideBurnData(v, a, vv, vh, alt, theta * 180 / Math.PI, verticalBurnStartAltitude, y_now, g);

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
            Mediator.Notify("SendMessage", strMessage);
        }
        #endregion
    }
}
