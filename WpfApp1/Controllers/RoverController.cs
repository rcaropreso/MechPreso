using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading.Tasks;

using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;
using WpfApp1.Models;
using System.Threading;


namespace WpfApp1.Controllers
{
    public class RoverController : BaseShipController
    {
        /// <summary>
        /// Describes the controls of a given rover
        /// </summary>
        
        private readonly object lock_gate_rover = new object();

        private bool m_StopRoverThread;
        private bool m_bRoverIsMoving;

        private Task taskMoveRover;

        public RoverController(in Connection conn, in FlightTelemetry _flightTel) : base(conn, _flightTel)
        {
        }

        private Coordinates GetWayPointCoordinates(in string wpName)
        {
            var wm = this._conn.SpaceCenter().WaypointManager;

            Coordinates retCoord = new Coordinates(999.0f, 999.0f);

            foreach(var item in wm.Waypoints)
            {
                if(item.Name.Equals(wpName) || string.IsNullOrEmpty(wpName))
                {
                    SendMessage("Planet: " + item.Body.Name);

                    StringBuilder strMessage = new StringBuilder("");
                    strMessage.AppendFormat("Name: {0}, Latitude: {1}, Longintude: {2}", item.Name, item.Latitude, item.Longitude);
                    SendMessage(strMessage.ToString());

                    retCoord.Latitude = (float) item.Latitude;
                    retCoord.Longitude = (float) item.Longitude;
                }
            }

            return retCoord;
        }

        private void RemoveWaypoint(string wpName)
        {
            var wm = this._conn.SpaceCenter().WaypointManager;

            foreach (var item in wm.Waypoints)
            {
                if (item.Name.Equals(wpName))
                {
                    SendMessage("Removing: " + item.Name);
                    item.Remove();

                    break;
                }
            }
        }

        private float ToRadians(float v)
        {
            return v * (float)Math.PI / 180.0f;
        }

        private float CalcCoordDistance(Coordinates sourceCoord, Coordinates destCoord)
        {
            //Converte para radianos
            var sourceLatRad = ToRadians(sourceCoord.Latitude);
            var destLatRad = ToRadians(destCoord.Latitude);

            var sourceLonRad = ToRadians(sourceCoord.Longitude);
            var destLonRad = ToRadians(destCoord.Longitude);

            var delta_phi = Math.Abs(sourceLatRad - destLatRad);
            var delta_lambda = Math.Abs(sourceLonRad - destLonRad);
            var R = CurrentVessel.Orbit.Body.EquatorialRadius; //em metros

            var central_angle = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(delta_phi / 2), 2) + Math.Cos(sourceLatRad) * Math.Cos(destLatRad) * Math.Pow(Math.Sin(delta_lambda / 2), 2)));

            //formula alternativa para o angulo central
            //theta = np.arccos(np.sin(lat_1) * np.sin(lat_2) + np.cos(lat_1)*np.cos(lat_2)*np.cos(delta_lambda))

            float distance = (float)central_angle * R;

            //SendMessage("Central angle: " + central_angle);
            return distance;
        }

        //https://www.youtube.com/watch?v=24CZwy_Ww_k
        //#Lat = phi, Lon - L
        //#O angulo de rolamento (bearing = beta) é definido entre a posição atual do veiculo e o norte, medida no sentido anti-horario
        //#podendo variar da seguinte forma: 0o = norte, 90o = leste, 180o = sul e 270o (ou -90o) = oeste
        //#Neset caso temos que somar ou subtrair 360 para manter o angulo entre -180o e 180o
        private float CalcCoordDirection(Coordinates sourceCoord, Coordinates destCoord)
        {
            var R = CurrentVessel.Orbit.Body.EquatorialRadius; //em metros

            //Converte para radianos
            var sourceLatRad = ToRadians(sourceCoord.Latitude);
            var destLatRad = ToRadians(destCoord.Latitude);

            var sourceLonRad = ToRadians(sourceCoord.Longitude);
            var destLonRad = ToRadians(destCoord.Longitude);

            var delta_lambda = destLonRad - sourceLonRad; //o sinal faz diferença aqui

            var S = Math.Cos(destLatRad) * Math.Sin(delta_lambda);
            var C = Math.Cos(sourceLatRad) * Math.Sin(destLatRad) - Math.Sin(sourceLatRad) * Math.Cos(destLatRad) * Math.Cos(delta_lambda);

            var bearing_rad = Math.Atan2(S, C); //já retorna angulo no intervalo [-pi, pi]   
            var bearing_deg = bearing_rad * 180 / Math.PI;
            var bearing_compass = (bearing_deg + 360) % 360;

            return (float) bearing_compass;
        }

        private void StopRover(float minSpeed = 0.2f) //as vezes é dificil atingir o zero
        {
            SendMessage("Stopping rover...");

            //Cancela a thread de controle de movimento
            lock (lock_gate_rover)
            {
                m_StopRoverThread = true;
            }

            //Aguarda o final da thread
            taskMoveRover.Wait();

            //Ajusta controles
            TurnOffAutoPilot();
            CurrentVessel.Control.SAS = false;
            CurrentVessel.Control.RCS = false;
            CurrentVessel.Control.Brakes = true;
            CurrentVessel.Control.WheelThrottle = 0.0f;

            //Espera atingir a velocidade minima
            while (true)
            {
                var currSpeed = (float)_flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.CurrentSpeed);
                if (currSpeed <= minSpeed)
                {
                    break;
                }

                Thread.Sleep(250);
            }

            lock(lock_gate_rover)
            {
                m_bRoverIsMoving = false;
            }

            SendMessage("Rover has stopped.");
        }

        /// <summary>
        /// Malha de controle de velocidade de rover. Roda numa thread separada
        /// </summary>
        private void MoveRover(float maxSpeed)
        {
            //Este metodo tem que rodar em uma thread separada
            SendMessage("MoveRover begin. MaxSpeed: " + maxSpeed);

            TurnOffAutoPilot();
            CurrentVessel.Control.SAS = false;
            CurrentVessel.Control.RCS = false;
            CurrentVessel.Control.Brakes = false;

            float sp = maxSpeed; //SetPoint
            float kp = 0.5f;
            float ki = 0.15f;
            float kd = 0.5f;
            float t = 0.1f; //tempo em segundos
            float maxValue = 10;
            float minValue = -10;

            var pid1 = new PID(sp, kp, ki, kd, t, minValue, maxValue);

            float curr_speed = (float) _flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.CurrentSpeed);

            while(!m_StopRoverThread)
            {
                lock(lock_gate_rover)
                {
                    m_bRoverIsMoving = true;
                }

                if(CurrentVessel.Resources.Amount("ElectricCharge")  < 10 )
                {
                    StopRover();
                    break;
                }

                curr_speed = (float)_flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.CurrentSpeed);
                float control_signal = pid1.Output(curr_speed);

                if(control_signal > 1)
                {
                    control_signal = 1.0f;
                }

                if (control_signal < -1)
                {
                    control_signal = -1.0f;
                }

                CurrentVessel.Control.WheelThrottle = control_signal;
                Thread.Sleep((int)t * 1000); //converte para milissegundos
            }

            lock(lock_gate_rover)
            {
                m_StopRoverThread = false; //limpa o flag de controle da thread
            }

            SendMessage("MoveRover end.");
        }

        /// <summary>
        /// Malha de controle de direação do rover
        /// </summary>
        private void SteerRover(float steerAngleDeg, float maxSpeed, float steeringSpeed = 3.0f)
        {
            StringBuilder strMessage = new StringBuilder("");
            strMessage.AppendFormat("SteerRover begin. SteeringAngle: {0}, SteeringSpeed: {1}", steerAngleDeg, steeringSpeed);
            SendMessage(strMessage.ToString());

            float curr_speed = (float)_flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.CurrentSpeed);
            float curr_heading = (float)_flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.VesselHeading);

            //Freia para a velocidade permitida para fazer curvas
            if (m_bRoverIsMoving)
            {
                StopRover(steeringSpeed); //vai matar a thread de movimento
            }

            //Solta o freio
            CurrentVessel.Control.Brakes = false;

            steerAngleDeg = (steerAngleDeg + 360) % 360;
            SendMessage("SteeringAngle updated: " + steerAngleDeg);

            float sp = 0.0f; //SetPoint
            float kp = 0.50f;
            float ki = 0.001f;
            float kd = 0.50f;
            float t = 0.1f; //tempo em segundos
            float maxValue = 1;
            float minValue = -1;

            float zeroOffset = 0.017f;

            var pid1 = new PID(sp, kp, ki, kd, t, minValue, maxValue);

            //Criar uma thread para isso
            //Create thread:
            taskMoveRover = Task.Run(() => MoveRover(steeringSpeed));

            bool bStopSteering = false;
            while(!bStopSteering)
            {
                if (CurrentVessel.Resources.Amount("ElectricCharge") < 10)
                {
                    StopRover();
                    break;
                }

                curr_heading = (float)_flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.VesselHeading);

                float correction = ToRadians(steerAngleDeg - curr_heading);

                float control_signal = pid1.Output(correction);

                if(Math.Abs(correction) <= zeroOffset)
                {
                    control_signal = 0.0f;
                    SendMessage("Stopping steering...");
                    bStopSteering = true;
                }

                CurrentVessel.Control.WheelSteering = control_signal;
                Thread.Sleep((int)t * 1000); //tempo em milissegundos
            }

            //Precisa aguardar a thread de movimento terminar aqui
            if (m_bRoverIsMoving)
            {
                StopRover(steeringSpeed);
            }

            SendMessage("SteerRover has ended.");
        }

        public void ExecuteGoToWaypoint(RoverControlDescriptor roverSetup)
        {
            _SuicideBurnStatus = CommonDefs.VesselState.Preparation;

            Console.WriteLine("Moving rover to waypoint...");

            Task t0 = Task.Run(() => CheckGoToWaypoint(roverSetup));
        }

        //Este metodo roda em uma thread separada
        private void CheckGoToWaypoint(RoverControlDescriptor roverSetup)
        {
            StringBuilder strMessage = new StringBuilder("");
            strMessage.AppendFormat("CheckGoToWaypoint begin. Waypoint: {0}, MaxSpeed: {1}, MinTargetDistance: {2}, MaxAngleDiff: {3}", 
                roverSetup.WaypointName, roverSetup.MaxSpeed, roverSetup.MinTargetDistance, roverSetup.MaxAngleDiff);
            SendMessage(strMessage.ToString());

            Coordinates targetCoord = GetWayPointCoordinates(roverSetup.WaypointName);
            Coordinates vesselCoord = new Coordinates();

            vesselCoord.Latitude = (float) _flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Latitude);
            vesselCoord.Longitude = (float) _flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Longitude);
            var currHead = (float)  _flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.VesselHeading);

            strMessage.Clear();
            strMessage.AppendFormat("Current Latitude: {0}, Current Longitude: {1}", vesselCoord.Latitude, vesselCoord.Longitude);
            SendMessage(strMessage.ToString());

            m_bRoverIsMoving = false;
            m_StopRoverThread = false;

            float distanceToTarget = CalcCoordDistance(vesselCoord, targetCoord);
            while( distanceToTarget > roverSetup.MinTargetDistance )
            {
                //Distancia ate o alvo
                vesselCoord.Latitude = (float)_flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Latitude);
                vesselCoord.Longitude = (float)_flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Longitude);
                distanceToTarget = CalcCoordDistance(vesselCoord, targetCoord);

                strMessage.Clear();
                strMessage.AppendFormat("Distance to target: {0} meters", distanceToTarget);
                SendMessage(strMessage.ToString());

                //Direção até o alvo
                float bearingCompass = CalcCoordDirection(vesselCoord, targetCoord);

                //Diferença entre a direção atual e a direção ao alvo
                currHead = (float)_flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.VesselHeading);
                float angleDiff = Math.Abs(currHead - bearingCompass);

                if(angleDiff > 180.0f)
                {
                    angleDiff = Math.Abs(angleDiff - 360.0f); //considera a diferença entre 359o e 5o
                }

                strMessage.Clear();
                strMessage.AppendFormat("Angle difference: {0} degrees", angleDiff);
                SendMessage(strMessage.ToString());

                //Verifica se precisa fazer correção de rota (virar para algum lado)
                if (angleDiff > roverSetup.MaxAngleDiff) //diferença de angulo entre o heading e o alvo maior que o valor de threshold
                {
                    strMessage.Clear();
                    strMessage.AppendFormat("Prepare to steer: Current Heading = {0}, Target Angle = {1}", currHead, bearingCompass);
                    SendMessage(strMessage.ToString());

                    //Agora vira o rover (e nao precisa ser thread separada)
                    SteerRover(bearingCompass, roverSetup.MaxSpeed, roverSetup.SteeringSpeed);
                }

                //Terminou de virar? Volta a acelerar
                if (!m_bRoverIsMoving)
                {
                    m_bRoverIsMoving = true;
                    m_StopRoverThread = false;
                    taskMoveRover = Task.Run(() => MoveRover(roverSetup.MaxSpeed));
                }
            }

            //Neste ponto estamos proximos do alvo, pode parar
            StopRover();

            strMessage.Clear();
            strMessage.AppendFormat("CheckGoToWaypoint end. Rover is approximately near {0} meters away the target", roverSetup.MinTargetDistance);
            SendMessage(strMessage.ToString());
        }
    }
}
