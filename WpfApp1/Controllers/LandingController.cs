using System;
using System.Text;
using System.Threading.Tasks;

using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;
using WpfApp1.Models;
using Vector3 = System.Tuple<double, double, double>;
using System.Threading;

namespace WpfApp1.Controllers
{
    /// <summary>
    /// Classe que controla o pouso da nave (suicide burn)
    /// </summary>
    public class LandingController : BaseShipController
    {
        /// <summary>
        /// Describes the controls of a given ship
        /// </summary>
        /// 
        private readonly object lock_gate_suicide_burn = new object();

        public LandingController(in Connection conn, in FlightTelemetry _flightTel) : base(conn, _flightTel)
        {
        }

        //Suicide Burn method
        private bool IsLandingOnSea()
        {
            bool bRet;

            if (CurrentVessel.Orbit.Body.Name.Contains("Kerbin, Eve, Laythe"))
            {
                var surfaceAltitude = _flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Surface_Altitude);
                var meanAltitude = _flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Mean_Altitude);

                bRet = (Math.Abs(meanAltitude - surfaceAltitude) < 50);
            }
            else
            {
                bRet = false;
            }
            return bRet;
        }

        private void CheckCancelVVel(SuicideBurnSetup suicideBurnSetup)
        {
            SendMessage("Starting cancelling vertical velocity thread...");

            lock (lock_gate_suicide_burn)
            {
                _SuicideBurnStatus = CommonDefs.VesselState.Executing;
            }

            //Cancela velocidade vertical
            DoBurn(CommonDefs.BurnType.Vertical, suicideBurnSetup.MinVerticalVelocity, CommonDefs.WhenStartBurn.Now, 0.0f, 0.10f);

            lock (lock_gate_suicide_burn)
            {
                _SuicideBurnStatus = CommonDefs.VesselState.Finished;
            }

            SendMessage("Cancelling vertical velocity thread has ended.");
        }

        private void CheckCancelHVel(SuicideBurnSetup suicideBurnSetup)
        {
            SendMessage("Starting cancelling horizontal velocity thread...");

            lock (lock_gate_suicide_burn)
            {
                _SuicideBurnStatus = CommonDefs.VesselState.Executing;
            }

            //Ajusta a posição da nave
            SetShipPosition(SASMode.Retrograde, true);
            SetupRoll(false);

            //Cancela velocidade horizontal mantendo a ultima velocidade vertical constante (quase um hover)            
            DoBurn(CommonDefs.BurnType.Diagonal, suicideBurnSetup.MinHorizontalVelocity, CommonDefs.WhenStartBurn.Now, 0.0f, 0.0f);

            lock (lock_gate_suicide_burn)
            {
                _SuicideBurnStatus = CommonDefs.VesselState.Finished;
            }

            SendMessage("Cancelling horizontal velocity thread has ended.");
        }

        private void CheckStopBurn(SuicideBurnSetup suicideBurnSetup)
        {
            SendMessage("Starting stop burn thread...");

            lock (lock_gate_suicide_burn)
            {
                _SuicideBurnStatus = CommonDefs.VesselState.Executing;
            }

            SetEnginesGimball();
            DoBurn(CommonDefs.BurnType.Retrograde, 30.0f, CommonDefs.WhenStartBurn.WaitVerticalAltitude, 0.0f, 0.10f);
            SetEnginesGimball(false);

            lock (lock_gate_suicide_burn)
            {
                _SuicideBurnStatus = CommonDefs.VesselState.Finished;
            }

            SendMessage("Stop burn thread has ended.");
        }

        private void CheckFineTunning(SuicideBurnSetup suicideBurnSetup)
        {
            SendMessage("Starting fine tunning thread...");

            lock (lock_gate_suicide_burn)
            {
                _SuicideBurnStatus = CommonDefs.VesselState.Executing;
            }

            //Ajusta a posição da nave
            SetShipPosition(SASMode.Retrograde, true);
            SetupRoll(false);

            DoFinalBurn(suicideBurnSetup);

            lock (lock_gate_suicide_burn)
            {
                _SuicideBurnStatus = CommonDefs.VesselState.Finished;
            }

            SendMessage("Fine tunning thread has ended.");
        }

        private void CheckDeorbitBody(SuicideBurnSetup suicideBurnSetup)
        {
            SendMessage("Starting deorbit body thread...");

            lock (lock_gate_suicide_burn)
            {
                _SuicideBurnStatus = CommonDefs.VesselState.Executing;
            }

            TurnOnAutoPilot();
            SendMessage("Positioning ship...");
            SetShipPosition(SASMode.Retrograde, false);

            CurrentVessel.Control.SpeedMode = SpeedMode.Orbit;
            DeorbitBody(suicideBurnSetup.DeorbitTargetAltitude);

            lock (lock_gate_suicide_burn)
            {
                _SuicideBurnStatus = CommonDefs.VesselState.Finished;
            }

            SendMessage("Deorbit body thread has ended.");
        }

        private void CheckSuicideBurn(SuicideBurnSetup suicideBurnSetup)
        {
            SendMessage("Starting suicide burn thread...");

            lock (lock_gate_suicide_burn)
            {
                _SuicideBurnStatus = CommonDefs.VesselState.Executing;
            }

            //Validation
            SuicideBurnData data = _flightTelemetry.GetSuicideBurnTelemetryInfo();

            if (data.EngineAcceleration < data.Gravity)
            {
                SendMessage("Suicide burn is not possible!");
                return;
            }

            //Get current fuel amount
            var fuelBegin = CurrentVessel.Resources.Amount("LiquidFuel");
            if (suicideBurnSetup.DeorbitBody)
            {
                SendMessage("Deorbiting body...");
                SendMessage("Positioning ship...");
                CurrentVessel.Control.SpeedMode = SpeedMode.Orbit;

                TurnOnAutoPilot();
                SetShipPosition(SASMode.Retrograde, false);
                //prepareDeorbitKerbin();

                DeorbitBody(suicideBurnSetup.DeorbitTargetAltitude);
                SendMessage("Deorbit body has ended.");

                SendMessage("Adjusting ship position aand roll...");
                TurnOnAutoPilot();
                SetShipPosition(SASMode.Retrograde, false);
                SetupRoll(false);
            }

            CurrentVessel.Control.SpeedMode = SpeedMode.Surface;

            //CurrentVessel.Control.Brakes = true; //aciona aerofreios se houver
            //CurrentVessel.Control.SetActionGroup(9, true); //aciona escudo de calor configurado em 9, se houver

            //Check for landing on "Land" or "Sea"
            bool bLandingOnSea = IsLandingOnSea();

            if (!bLandingOnSea)
            {
                if (suicideBurnSetup.CancelVVel)
                {
                    //Cancela velocidade vertical, levando em conta o ponto de maior altitude do planeta em questão
                    SendMessage("Cancelling Vertical Velocity...");
                    DoBurn(CommonDefs.BurnType.Vertical, suicideBurnSetup.MinVerticalVelocity, CommonDefs.WhenStartBurn.WaitVerticalAltitude, (float)data.HighestPeak, 0.10f);
                    SendMessage("Cancelling Vertical Velocity has ended.");
                }

                if (suicideBurnSetup.CancelHVel)
                {
                    //Ajusta a posição da nave
                    SetShipPosition(SASMode.Retrograde, true);
                    SetupRoll(false);

                    //Cancela velocidade horizontal mantendo a ultima velocidade vertical constante (quase um hover)
                    //suicideBurnSetup.MinHorizontalVelocity = 5.0f;
                    SendMessage("Cancelling Horizontal Velocity...");
                    DoBurn(CommonDefs.BurnType.Diagonal, suicideBurnSetup.MinHorizontalVelocity, CommonDefs.WhenStartBurn.Now, 0.0f, 0.0f);
                    SendMessage("Cancelling Horizontal Velocity has ended.");
                }
            }

            //A partir deste ponto, deveriamos ter um suicide burn "quase" vertical
            if (suicideBurnSetup.StopBurn)
            {
                //Parada total
                float limitAltitude = 500.0f; //vamos garantir que a nave chega a 30 m/s acima de 500m de altitude
                SendMessage("Performing stop burn...");
                SetEnginesGimball();
                DoBurn(CommonDefs.BurnType.Retrograde, 30.0f, CommonDefs.WhenStartBurn.WaitVerticalAltitude, limitAltitude, 0.10f);
                SetEnginesGimball(false);
                SendMessage("Stop burn has ended.");
            }

            if (bLandingOnSea)
            {
                CurrentVessel.Control.SetActionGroup(7, true); //destrava articulação
                CurrentVessel.Control.SetActionGroup(8, true); //move articulação

                foreach (Parachute chute in CurrentVessel.Parts.Parachutes)
                {
                    chute.Deploy();
                }

                SendMessage("Landing over sea, deploying parachutes...");
            }
            else
            {
                SendMessage("Landing on land...");

                if (suicideBurnSetup.FinalBurn)
                {
                    SendMessage("Final burn...");
                    //Melhor colocar a checagem de aerofreios, trem de pouso e paraquedas em uma thread separada
                    CurrentVessel.Control.Gear = true; //aciona trem de pouso
                    DoFinalBurn(suicideBurnSetup);
                    SendMessage("Final burn has ended.");
                }
            }

            CurrentVessel.Control.Throttle = 0.0f;
            TurnOffAutoPilot();

            lock (lock_gate_suicide_burn)
            {
                _SuicideBurnStatus = CommonDefs.VesselState.Finished;
            }

            //Get current fuel amount
            var fuelEnd = CurrentVessel.Resources.Amount("LiquidFuel");

            StringBuilder strMessage = new StringBuilder();
            strMessage.AppendFormat("Ship has landed. Liquid Fuel spent = {0}.", fuelBegin - fuelEnd);
            SendMessage(strMessage.ToString());
        }

        private void DoFinalBurn(SuicideBurnSetup suicideBurnSetup)
        {
            SendMessage("Starting final burn control...");
            Task t1 = Task.Run(() => CheckShipPosition(100.0f));
            SuicideBurnData data;

            SendMessage("Locking engine gimball...");
            SetEnginesGimball();

            DoSoftBurn(300, 30, 0);      //final burn                    
            DoSoftBurn(60.0f, 0, 10); //Desce na proporção de 1 décimo da velocidade em relação a altitude ate 60m

            //Estamos abaixo de 60, temos que garantir neste momento a velocidade de 2m/s
            //Estamos proximos de 6m/s neste momento
            //(tem cenarios em que a nave termina o stop burn ali em cima abaixo desta altitude)
            data = _flightTelemetry.GetSuicideBurnTelemetryInfo();
            if (data.VerticalVelocity < -5)
            {
                CurrentVessel.Control.Throttle = CalcThrustFactor(1.0f); //aceleração de 1m/s^2 para cima;
                while (true)
                {
                    data = _flightTelemetry.GetSuicideBurnTelemetryInfo();
                    if (data.VerticalVelocity >= -2)
                        break;
                    Thread.Sleep(50);
                }
            }
            CurrentVessel.Control.Throttle = 0.0f;

            //Definimos aceleração que cancela g, mantendo a velocidade constante (proxima aos 2m/s)
            data = _flightTelemetry.GetSuicideBurnTelemetryInfo();
            var max_acc = (float)(data.Gravity / data.EngineAcceleration);
            DoSoftBurn(0, 2, 0, max_acc);      //final burn                    

            CurrentVessel.Control.Throttle = 0.0f;
            t1.Wait();

            SendMessage("Unlocking engine gimball...");
            SetEnginesGimball(false);

            SendMessage("Final burn control has ended.");
        }

        private void CheckShipPosition(float minAltitude)
        {
            SendMessage("Starting position check...");
            SuicideBurnData data;
            while (true)
            {
                if (ReturnToManualControl())
                    break;

                data = _flightTelemetry.GetSuicideBurnTelemetryInfo();
                if (data.HorizontalVelocity < 2)
                {
                    SetPitchAndHeading(90, 0, false, true, false);
                }
                else
                {
                    SetShipPosition(SASMode.Retrograde, false);
                }

                if (data.SurfaceAltitude < minAltitude)
                {
                    SetPitchAndHeading(90, 0, false, true, false);
                    break;
                }

                if (CurrentVessel.Situation == VesselSituation.Landed || CurrentVessel.Situation == VesselSituation.Splashed)
                {
                    break;
                }
            }
            SendMessage("Position check has ended.");
        }


        //Suicide Burn method
        private float CalcThrustFactor(float maxAcceleration)
        {
            //Define a porcentagem de empuxo da nave para manter uma dada aceleração (no caso, a vertical)
            /*
             * Aceleração procurada ax corresponde a uma posição de throttle
             acc -> 1
             ax  -> x

             //Ax é a resultante vertical para uma dada  aceleração desejada ad (que é o parametro da função)
             ax - g = ad

             acc -> 1
             ad + g  -> x

            x = (ad + g) / acc
             */

            float f_thrust;

            var data = _flightTelemetry.GetSuicideBurnTelemetryInfo();
            f_thrust = (float)Math.Abs((maxAcceleration + data.Gravity) / data.EngineAcceleration);

            return f_thrust > 1.0f ? 1.0f : f_thrust;
        }

        private void DoBurnPreparation(CommonDefs.BurnType burnType = CommonDefs.BurnType.Diagonal)
        {
            StringBuilder strMessage = new StringBuilder();
            strMessage.AppendFormat("Burn preparation. type = {0}", CommonDefs.BurnTypeToString(burnType));
            SendMessage(strMessage.ToString());

            //Faz a espera e já ajusta a posição da nave previamente
            //Ajuste de posição
            TurnOnAutoPilot();

            SuicideBurnData data = _flightTelemetry.GetSuicideBurnTelemetryInfo();
            if (burnType == CommonDefs.BurnType.Vertical)
            {
                this.SetPitchAndHeading(90, 0.0f, false, true);
            }
            else if (burnType == CommonDefs.BurnType.Horizontal)
            {
                this.SetPitchAndHeading(0.0f, 0.0f, false, true);
            }
            else if (burnType == CommonDefs.BurnType.Diagonal)
            {
                //Vira na direção aproximada do cancelamento da componente vertical
                float thetaV = (float)Math.Asin(data.Gravity / data.EngineAcceleration);
                this.SetPitchAndHeading(thetaV, 0.0f, false, true);
            }
            else if (burnType == CommonDefs.BurnType.Retrograde)
            {
                this.SetShipPosition(SASMode.Retrograde, true);
            }

            strMessage.Clear();
            strMessage.AppendFormat("Burn preparation has ended. type = {0}", CommonDefs.BurnTypeToString(burnType));
            SendMessage(strMessage.ToString());
        }

        private void DoBurn(CommonDefs.BurnType burnType = CommonDefs.BurnType.Diagonal, float finalSpeed = 10.0f,
            CommonDefs.WhenStartBurn altType = CommonDefs.WhenStartBurn.WaitVerticalAltitude, float offsetAltitude = 0.0f, float safetyMargin = 0.10f)
        {
            StringBuilder strMessage = new StringBuilder();
            strMessage.AppendFormat("Landing burn. type: {0}, finalSpeed = {1}, when = {2}, limitAltitude = {3}, safetyMargin = {4}",
                CommonDefs.BurnTypeToString(burnType), finalSpeed, CommonDefs.AltitudeTypeToString(altType), offsetAltitude, safetyMargin);
            SendMessage(strMessage.ToString());

            DoBurnPreparation(burnType);

            if (altType != CommonDefs.WhenStartBurn.Now)
            {
                WaitBurnAltitude(altType, offsetAltitude, safetyMargin);
            }

            SuicideBurnData data = _flightTelemetry.GetSuicideBurnTelemetryInfo();

            //Esta classe deve ser criada aqui sempre (por enquanto) pois pode ser usada no while 
            //O set point é a ultima velocidade vertical obtida, que deverá ser mantida
            float timeStep = 0.050f; //500 ms
            PID _controller = new PID(_SP: (float)data.VerticalVelocity, _P: 0.5f, _I: 0.05f, _D: 0.05f,
                _TimeStep: timeStep, _MinValue: 0.0f, _MaxValue: 90.0f);
            float output;

            SendMessage("Starting Landing Burn...");
            CurrentVessel.Control.Throttle = 1.0f;

            float h = GetHeading();

            double prevHorzV = data.HorizontalVelocity; //para evitar sair na primeira iteraçao
            int nSamples = 30; //calcula a cada nSamples a diferença entre velocidades
            int iSampleCounter = 0;

            while (true)
            {
                if (ReturnToManualControl())
                    return;

                if (burnType == CommonDefs.BurnType.Vertical)
                {
                    if (Math.Abs(data.VerticalVelocity) <= Math.Abs(finalSpeed))
                    {
                        SendMessage("Vertical burn finished. Vertical Velocity is lower than final speed.");
                        break;
                    }
                    if (data.VerticalVelocity >= 0.0d)
                    {
                        SendMessage("Vertical burn finished. Vertical Velocity has changed its signal.");
                        break;
                    }
                }

                if (burnType == CommonDefs.BurnType.Horizontal)
                {
                    if (Math.Abs(data.HorizontalVelocity) <= Math.Abs(finalSpeed))
                    {
                        SendMessage("Horizontal burn finished. Horizontal Velocity is lower than final speed.");
                        break;
                    }

                    if (iSampleCounter > nSamples && (prevHorzV - data.HorizontalVelocity) < 0.0d)
                    {
                        strMessage.Clear();
                        strMessage.AppendFormat("Changed signal. VH_PREV={0}, VH_NOW={1}", prevHorzV, data.HorizontalVelocity);
                        SendMessage(strMessage.ToString());

                        //SendMessage("Horizontal burn finished. Horizontal Velocity has changed its signal.");
                        break;
                    }
                }

                if (burnType == CommonDefs.BurnType.Diagonal)
                {
                    if (Math.Abs(data.HorizontalVelocity) <= Math.Abs(finalSpeed))
                    {
                        SendMessage("Diagonal burn finished. Horizontal Velocity is lower than final speed.");
                        break;
                    }
                    if (iSampleCounter > nSamples && (prevHorzV - data.HorizontalVelocity) < 0.0d)
                    {
                        strMessage.Clear();
                        strMessage.AppendFormat("Changed signal. VH_PREV={0}, VH_NOW={1}", prevHorzV, data.HorizontalVelocity);
                        SendMessage(strMessage.ToString());
                        //SendMessage("Diagonal burn finished. Horizontal Velocity has changed its signal.");
                        break; //no modo diagonal também estamos interessados em cancelar a comopnente horizontal da velocidade
                    }
                }

                if (burnType == CommonDefs.BurnType.Retrograde)
                {
                    if (Math.Abs(data.CurrentSpeed) <= Math.Abs(finalSpeed))
                    {
                        SendMessage("Retrograde burn finished. Vertical Velocity is lower than final speed.");
                        break;
                    }
                    if (data.VerticalVelocity >= 0.0d)
                    {
                        SendMessage("Retrograde burn finished. Vertical Velocity has changed its signal.");
                        break;
                    }
                }

                //Em diagonal, a inclinação sera controlada pelo PID
                if (burnType == CommonDefs.BurnType.Diagonal)
                {
                    output = _controller.Output((float)data.VerticalVelocity);

                    //aqui é melhor chamar direto, senao o sleep dentro do metodo SetPitchAndHeading vai atrapalhar
                    CurrentVessel.AutoPilot.TargetPitchAndHeading(output, h);

                    //É necessario calcular o HEADING QUE MUDA AO LONGO DO TEMPO, SE A QUEIMA FOR LONGA.
                    //RESULTADO: O FOGUETE VAI ANDAR DE LADO E A QUEIMA FICARA DESALINHADA
                    ReferenceFrame CurrentRefFrame = _flightTelemetry.CurrentRefFrame;
                    var velocityDirection = CurrentVessel.Velocity(CurrentRefFrame);

                    // Direção do veiculo no plano do horizonte
                    //como a nave está indo "de ré" pois estamos na posição retrograda, vamos inverter o sinal do vetor de velocidade
                    var horizonVelocityDirection = new Vector3(0, velocityDirection.Item2 * (-1), velocityDirection.Item3 * (-1));

                    // Calcula o 'heading' - angulo entre o norte e a direção no plano do horizonte
                    var north = new Vector3(0, 1, 0);

                    h = (float)VectorDescriptor.AngleBetweenVectors(north, horizonVelocityDirection);

                    if (horizonVelocityDirection.Item3 < 0)
                        h = 360 - h;
                }

                if (iSampleCounter % nSamples == 0)//a cada nSamples tira uma amostra e calcula pra ver se a velocidade mudou de sinal
                {
                    prevHorzV = Math.Min(data.HorizontalVelocity, prevHorzV);//para garantir que a prevHorzV só abaixa e qualquer aumento, sai do loop
                }

                data = _flightTelemetry.GetSuicideBurnTelemetryInfo();
                iSampleCounter++;

                Thread.Sleep((int)(1000 * timeStep));
            }

            CurrentVessel.Control.Throttle = 0.0f;

            //Wait vertical velocity be negative (down)
            while (data.VerticalVelocity >= 0)
            {
                if (ReturnToManualControl())
                    return;

                data = _flightTelemetry.GetSuicideBurnTelemetryInfo();
                Thread.Sleep(50);
            }

            strMessage.Clear();
            strMessage.AppendFormat("Landing Burn has ended.");
            SendMessage(strMessage.ToString());
        }

        //Suicide Burn method
        private void DoSoftBurn(float minAltitude, float FixedVelocity = 0, float AltitudeFactor = 100, float max_thrust = 1.0f)
        {
            //FixedVelocity : se for diferente de zero, utiliza como valor fixo, caso contrario utiliza o proporcional de Altitude/AltitudeFactor
            //FixedVelocity tem precedencia sobre AltitudeFactor, se FixedVelocity for diferente de zero (não vai importar o valor de AltitudeFactor)
            //Velocidade deve ser considerada positiva para BAIXO e negativa para cima
            //Set Point deve ser zero e outputSys = (target - current) /Max( abs(target), abs(current))
            StringBuilder strMessage = new StringBuilder();
            strMessage.AppendFormat("Starting SOFT burn for Alt = {0}m, FixedVel = {1}m/s and Factor={2}.", minAltitude, FixedVelocity, AltitudeFactor);
            SendMessage(strMessage.ToString());

            var data = _flightTelemetry.GetSuicideBurnTelemetryInfo();

            if (data.SurfaceAltitude < minAltitude)
                return;

            float targetSpeed;
            if (FixedVelocity == 0)
                targetSpeed = (float)data.SurfaceAltitude / AltitudeFactor; //velocidade positiva é para baixo
            else
                targetSpeed = Math.Abs(FixedVelocity);

            float timeStep = 0.050f; //50 ms

            while (true)
            {
                if (ReturnToManualControl())
                    return;

                if (CurrentVessel.Situation == VesselSituation.Landed || CurrentVessel.Situation == VesselSituation.Splashed
                    || data.SurfaceAltitude < minAltitude)
                {
                    break;
                }

                data = _flightTelemetry.GetSuicideBurnTelemetryInfo();

                if (FixedVelocity == 0) //velocidade proporcional a altitude
                    targetSpeed = (float)data.SurfaceAltitude / AltitudeFactor; //velocidade positiva é para baixo

                if (data.VerticalVelocity >= 0)
                    break;

                if (data.CurrentSpeed > targetSpeed)
                {
                    CurrentVessel.Control.Throttle = max_thrust;
                }
                else
                {
                    CurrentVessel.Control.Throttle = 0.0f;
                }

                Thread.Sleep((int)timeStep * 1000);
            }

            CurrentVessel.Control.Throttle = 0.0f;

            strMessage.Clear();
            strMessage.AppendFormat("SOFT burn for Alt = {0}m, FixedVel = {1}m/s and Factor={2} has ended.", minAltitude, FixedVelocity, AltitudeFactor);
            SendMessage(strMessage.ToString());
        }

        private void WaitBurnAltitude(CommonDefs.WhenStartBurn altType, float offsetAltitude = 0.0f, float safetyMargin = 0.10f)
        {
            StringBuilder strMessage = new StringBuilder();
            strMessage.AppendFormat("Waiting altitude: when = {0}, limitAlt = {1}, safetyMargin = {2}.", CommonDefs.AltitudeTypeToString(altType), offsetAltitude, safetyMargin);
            SendMessage(strMessage.ToString());

            SuicideBurnData data = _flightTelemetry.GetSuicideBurnTelemetryInfo();
            while (true)
            {
                if (ReturnToManualControl())
                    return;

                var totalHAltitude = data.HorizontalBurnStartAltitude + offsetAltitude;
                var totalVAltitude = data.VerticalBurnStartAltitude + offsetAltitude;

                if (altType == CommonDefs.WhenStartBurn.WaitHorizontalAltitude && data.SurfaceAltitude <= totalHAltitude * (1 + safetyMargin))
                    break;

                if (altType == CommonDefs.WhenStartBurn.WaitVerticalAltitude && data.SurfaceAltitude <= totalVAltitude * (1 + safetyMargin))
                    break;

                if (data.SurfaceAltitude <= offsetAltitude)
                    break;

                data = _flightTelemetry.GetSuicideBurnTelemetryInfo();
                Thread.Sleep(100);
            }
        }

        //Suicide Burn method
        private void DeorbitBody(float targetAltitude)
        {
            float periapsis = (float)_flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Periapsis_Altitude);

            if (periapsis <= 0 || periapsis <= targetAltitude)
                return;

            //Count down
            for (int counter = 3; counter >= 0; counter--)
            {
                StringBuilder sMessage = new StringBuilder();
                sMessage.AppendFormat("Start deorbiting in {0} seconds", counter);
                SendMessage(sMessage.ToString());
                Thread.Sleep(1000);
            }

            //Controle on-off
            float timeStep = 0.1f;
            PID _controller = new PID(_SP: 0.0f, _P: 1.0f, _I: 0.0f, _D: 0.00f, _TimeStep: timeStep);
            float output;

            while (true)
            {

                if (ReturnToManualControl())
                    return;

                periapsis = (float)_flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Periapsis_Altitude);

                output = _controller.Output((targetAltitude - periapsis) / Math.Abs(periapsis));
                CurrentVessel.Control.Throttle = output;
                Thread.Sleep((int)timeStep * 1000);

                if (output < 0.1)
                    break;
            }

            CurrentVessel.Control.Throttle = 0.0f;
        }

        //ESTE AQUI TEM QUE SER THREAD TAMBÉM SENAO TRAVA A TELA
        //VAMOS TER QUE CRIAR UMA THREAD DE MONITORAMENTO E VAZAR DAQUI
        public void ExecuteSuicideBurn(SuicideBurnSetup suicideBurnSetup)
        {
            _SuicideBurnStatus = CommonDefs.VesselState.Preparation;

            Console.WriteLine("Executing Suicide burn...");

            Task t0 = Task.Run(() => CheckSuicideBurn(suicideBurnSetup));
        }

        private void ExecuteDeorbitBody(SuicideBurnSetup suicideBurnSetup)
        {
            _SuicideBurnStatus = CommonDefs.VesselState.Preparation;

            Console.WriteLine("Executing deorbitation...");

            Task t0 = Task.Run(() => CheckDeorbitBody(suicideBurnSetup));
        }

        private void ExecuteCancelVVel(SuicideBurnSetup suicideBurnSetup)
        {
            _SuicideBurnStatus = CommonDefs.VesselState.Preparation;

            Console.WriteLine("Cancelling Vertical Velocity...");

            Task t0 = Task.Run(() => CheckCancelVVel(suicideBurnSetup));
        }

        private void ExecuteCancelHVel(SuicideBurnSetup suicideBurnSetup)
        {
            _SuicideBurnStatus = CommonDefs.VesselState.Preparation;

            Console.WriteLine("Cancelling Horizontal Velocity...");

            Task t0 = Task.Run(() => CheckCancelHVel(suicideBurnSetup));
        }

        private void ExecuteStopBurn(SuicideBurnSetup suicideBurnSetup)
        {
            _SuicideBurnStatus = CommonDefs.VesselState.Preparation;

            Console.WriteLine("Executing stop burn...");

            Task t0 = Task.Run(() => CheckStopBurn(suicideBurnSetup));
        }

        private void ExecuteFineTunning(SuicideBurnSetup suicideBurnSetup)
        {
            _SuicideBurnStatus = CommonDefs.VesselState.Preparation;

            Console.WriteLine("Executing fine tunning...");

            Task t0 = Task.Run(() => CheckFineTunning(suicideBurnSetup));
        }
    }
}
