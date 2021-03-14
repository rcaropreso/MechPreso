using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;
using WpfApp1.Controllers;
using System.Reflection;
using WpfApp1.Services;
using WpfApp1.Utils;

namespace WpfApp1
{
    public class ShipFlighter : IAsyncMessageUpdate
    {
        /// <summary>
        /// Describes the controls of a given ship
        /// </summary>
        /// 
        private Connection _conn = null;
        public FlightTelemetry _flightTelemetry { get; }
        private TakeOffDescriptor _tod;

        public Vessel CurrentVessel { get => _conn.SpaceCenter().ActiveVessel; }        
        public string ShipName { get => CurrentVessel.Name; }

        private object lock_gate_takeoff = new object();
        private object lock_gate_maneuver = new object();
        private object lock_gate_suicide_burn = new object();

        private bool _manualControl = false;
        private object lock_gate_manual = new object();

        private float _maneuverBurnTime;
        public float ManeuverBurnTime
        {
            get
            {
                return _maneuverBurnTime;
            }
        }


        private VesselState _TakeOffStatus;
        private VesselState _ManeuverStatus;
        private VesselState _SuicideBurnStatus;

        public VesselState TakeOffStatus { get => _TakeOffStatus;  }
        public VesselState ManeuverStatus { get => _ManeuverStatus; }
        public VesselState SuicideBurnStatus { get => _SuicideBurnStatus; }

        public enum WhenStartBurn
        {
            WaitHorizontalAltitude,
            WaitVerticalAltitude,
            Now
        };

        public enum BurnType
        {
            Horizontal,
            Vertical,
            Diagonal,
            Retrograde
        };

        public enum VesselState
        {
            NotStarted,
            Preparation,
            Executing,
            Finished
        };

        private string AltitudeTypeToString(WhenStartBurn value)
        {
            switch (value)
            {
                case WhenStartBurn.WaitHorizontalAltitude:
                    return @"WaitHorizontalAltitude";
                case WhenStartBurn.WaitVerticalAltitude:
                    return @"WaitVerticalAltitude";
                case WhenStartBurn.Now:
                    return @"Now";
                default:
                    return string.Empty;
            }
        }

        private string BurnTypeToString(BurnType value)
        {
            switch (value)
            {
                case BurnType.Horizontal:
                    return @"Horizontal";
                case BurnType.Vertical:
                    return @"Vertical";
                case BurnType.Diagonal:
                    return @"Diagonal";
                case BurnType.Retrograde:
                    return @"Retrograde";
                default:
                    return string.Empty;
            }
        }
        
        public ShipFlighter(Connection conn)
        {
            _conn = conn;                
            _flightTelemetry = new FlightTelemetry(conn);

            SendMessage("Starting Telemetry...");
            _flightTelemetry?.StartAllTelemetry();
            Thread.Sleep(2000);
        }

        private bool ReturnToManualControl()
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

        public void RestartTelemetry()
        {
            _flightTelemetry.RestartTelemetry();
        }

        public void SetManualControl()
        {
            lock (lock_gate_manual)
            {
                _manualControl = true;
            }
        }

        public void ResetStates()
        {
            _TakeOffStatus = VesselState.NotStarted;
            _ManeuverStatus = VesselState.NotStarted;
            _SuicideBurnStatus = VesselState.NotStarted;
        }

        public void SetupRoll(bool bWait = true)
        {
            SetPitchAndHeading(0, 0, true, bWait);
        }

        public float GetHeading()
        {
            ReferenceFrame CurrentRefFrame = _flightTelemetry.CurrentRefFrame;
            Flight flight = CurrentVessel.Flight(CurrentRefFrame);

            return flight.Heading;
        }

        public void SetPitchAndHeading(float pitch, float heading,
            bool bKeepCurrentPitch = false, bool bKeepCurrentHeading = false, bool bWait=true)
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

        public void SetShipPosition(SASMode mode, bool bWait=true)
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

        public void TurnOffAutoPilot()
        {
            CurrentVessel.AutoPilot.Disengage();
            CurrentVessel.Control.SAS = true;
            CurrentVessel.Control.RCS = true;

            System.Threading.Thread.Sleep(500);

            CurrentVessel.Control.SASMode = SASMode.StabilityAssist;
        }

        public void TurnOnAutoPilot()
        {
            CurrentVessel.AutoPilot.Engage();
            CurrentVessel.Control.SAS = false;
            CurrentVessel.Control.RCS = true;

            System.Threading.Thread.Sleep(500);

            //CurrentVessel.Control.SASMode = SASMode.StabilityAssist;
        }

        public void StartSRBTelemetry(int iSRBStage = 0)
        {
            _flightTelemetry.SetupSRBTelemetry(iSRBStage);
        }

        public void SendMessage(string strMessage)
        {
            Console.WriteLine(strMessage);
            Mediator.Notify("SendMessage", strMessage);
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

            while(true)
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

            while(true)
            {
                if (ReturnToManualControl())
                    return;

                var altitude = _flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Surface_Altitude);
                //Gravity turn
                if( altitude > _tod.StartTurnAltitude )
                {
                    double frac = (altitude - _tod.StartTurnAltitude) / (_tod.EndTurnAltitude - _tod.StartTurnAltitude);
                    double new_turn_angle = frac * 90;

                    if( Math.Abs(new_turn_angle - turn_angle) > 0.5 )
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
            
            //Wait until pitch be lower than 5 degrees
            //while(_flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.VesselPitch) > 5.0d)
            //{
            //    Thread.Sleep(250);
            //}

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
                //SendMessage("SRB Fuel =" + solid_fuel.ToString());
                
                if(solid_fuel <= 300)
                {
                    for(int i=5; i>0; i--)
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
            //this.SetPitchAndHeading(90, 0.0f, false, true);
            this.SetPitchAndHeading(90, _tod.ShipHeadingAngle, true, false);

            this.SetPitchAndHeading(90, _tod.ShipHeadingAngle, false, false);
            //CurrentVessel.AutoPilot.Engage();
            //CurrentVessel.AutoPilot.TargetPitchAndHeading(90, _tod.ShipHeadingAngle);
            //CurrentVessel.AutoPilot.Wait();
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

            _TakeOffStatus = VesselState.Executing;

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
                _TakeOffStatus = VesselState.Finished;
            }
            SendMessage("Out of atmosphere.");
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

        private void CheckSuicideBurn(SuicideBurnSetup suicideBurnSetup)
        {
            SendMessage("Starting suicide burn thread...");

            lock (lock_gate_suicide_burn)
            {
                _SuicideBurnStatus = VesselState.Executing;
            }

            //Validation
            SuicideBurnData data = _flightTelemetry.GetSuicideBurnTelemetryInfo();

            if (data.EngineAcceleration < data.Gravity)
            {
                SendMessage("Suicide burn is not possible!");
                return;
            }

            TurnOnAutoPilot();
            SendMessage("Positioning ship...");
            SetShipPosition(SASMode.Retrograde, false);

            //Get current fuel amount
            var fuelBegin = CurrentVessel.Resources.Amount("LiquidFuel");

            if (suicideBurnSetup.DoDeorbitBody)
            {
                //prepareDeorbitKerbin();
                CurrentVessel.Control.SpeedMode = SpeedMode.Orbit;
                DeorbitBody(suicideBurnSetup.DeorbitTargetAltitude);
            }

            CurrentVessel.Control.SpeedMode = SpeedMode.Surface;

            TurnOnAutoPilot();            
            SetShipPosition(SASMode.Retrograde, false);
            SetupRoll(false);

            //CurrentVessel.Control.Brakes = true; //aciona aerofreios se houver
            //CurrentVessel.Control.SetActionGroup(9, true); //aciona escudo de calor configurado em 9, se houver

            //Check for landing on "Land" or "Sea"
            bool bLandingOnSea  = IsLandingOnSea();
            Task t1;

            if (!bLandingOnSea)
            {
                if (suicideBurnSetup.DoCancelVerticalVelocity)// cancela a velocidade vertical primeiro
                {
                    DoBurn(BurnType.Vertical, suicideBurnSetup.MinVerticalVelocity, WhenStartBurn.WaitHorizontalAltitude, 0.0f, 0.10f);
                    SetShipPosition(SASMode.Retrograde, true);
                    SetupRoll(false);
                    //cancela horizontal mantendo a ultima velocidade vertical constante
                    DoBurn(BurnType.Diagonal, suicideBurnSetup.MinHorizontalVelocity, WhenStartBurn.Now, 0.0f, 0.0f); 
                }
                else
                {
                    if (suicideBurnSetup.DoCancelHorizontalVelocity)
                    {
                        //cancela horizontal mantendo a ultima velocidade vertical constante
                        DoBurn(BurnType.Diagonal, suicideBurnSetup.MinHorizontalVelocity, WhenStartBurn.WaitHorizontalAltitude, 0.0f, 0.20f);
                    }
                }
            }

            //Parada total
            if (suicideBurnSetup.DoStopBurn)
            {
                SetEnginesGimball();
                DoBurn(BurnType.Retrograde, 10.0f, WhenStartBurn.WaitVerticalAltitude, 0.0f, 0.10f);
                SetEnginesGimball(false);
            }

            if (bLandingOnSea)
            {
                CurrentVessel.Control.SetActionGroup(7, true); //destrava articulação
                CurrentVessel.Control.SetActionGroup(8, true); //move articulação

                foreach(Parachute chute in CurrentVessel.Parts.Parachutes)
                {
                    chute.Deploy();
                }

                SendMessage("Landing over sea, deploying parachutes...");
            }
            else
            {
                SendMessage("Landing on land...");

                //Melhor colocar a checagem de aerofreios, trem de pouso e paraquedas em uma thread separada
                CurrentVessel.Control.Gear = true; //aciona trem de pouso
                DoFinalBurn(suicideBurnSetup);
            }

            CurrentVessel.Control.Throttle = 0.0f;
            TurnOffAutoPilot();

            lock (lock_gate_suicide_burn)
            {
                _SuicideBurnStatus = VesselState.Finished;
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

            if (suicideBurnSetup.DoFineTunning) //Nem deveria ter este flag aqui
            {
                SendMessage("Locking engine gimball...");
                SetEnginesGimball();

                DoSoftBurn(300, 30, 0);   //mantem velocidade fixa de 30 /s até a 300m 
                DoSoftBurn(50.0f, 0, 10); //Desce na proporção de 1 décimo da velocidade em relação a altitude ate 100m

                //Estamos abaixo de 30, temos que garantir neste momento a velocidade de 3m/s
                //Estamos proximos de 5m/s neste momento
                //(tem cenarios em que a nave termina o stop burn ali em cima abaixo desta altitude)
                data = _flightTelemetry.GetSuicideBurnTelemetryInfo();
                if (data.VerticalVelocity < -5)
                {
                    CurrentVessel.Control.Throttle = CalcThrustFactor(1.0f); //aceleração de 1m/s^2 para cima;
                    while (true)
                    {
                        data = _flightTelemetry.GetSuicideBurnTelemetryInfo();
                        if (data.VerticalVelocity >= -3)
                            break;
                        Thread.Sleep(50);
                    }
                }
                CurrentVessel.Control.Throttle = 0.0f;

                //Definimos aceleração que cancela g, mantendo a velocidade constante (proxima aos 3m/s)
                //data = _flightTelemetry.GetSuicideBurnTelemetryInfo();
                var max_acc = (float)(data.Gravity / data.EngineAcceleration);
                DoSoftBurn(0, 3, 0, max_acc);      //final burn                    
            }

            CurrentVessel.Control.Throttle = 0.0f;
            t1.Wait();

            SendMessage("Unlocking engine gimball...");
            SetEnginesGimball(false);

            SendMessage("Final burn control has ended.");
        }

        private void SetEnginesGimball(bool bOn=true)
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

        private void CheckShipPosition(float minAltitude)
        {
            SendMessage("Starting position check...");
            SuicideBurnData data;
            while (true)
            {
                if (ReturnToManualControl())
                    break;

                data = _flightTelemetry.GetSuicideBurnTelemetryInfo();
                if(data.HorizontalVelocity < 2)
                {
                    SetPitchAndHeading(90, 0, false, true, false);
                }
                else
                {
                    SetShipPosition(SASMode.Retrograde, false);
                }

                if(data.SurfaceAltitude < minAltitude)
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

            /*
             * var theta = data.Theta;
            float verticalAcceleration = (float) ((data.EngineAcceleration )* Math.Sin(theta));

            if( maxAcceleration >= verticalAcceleration )
            {
                f_thrust = 1.0f;
            }
            else
            {
                f_thrust = (float) Math.Abs( (maxAcceleration + data.Gravity) / verticalAcceleration);
            }
            */
            return f_thrust > 1.0f ? 1.0f : f_thrust ;
        }

        private void DoBurnPreparation(BurnType burnType = BurnType.Diagonal)
        {
            StringBuilder strMessage = new StringBuilder();
            strMessage.AppendFormat("Burn preparation. Type: {0}", BurnTypeToString(burnType));
            SendMessage(strMessage.ToString());

            //Faz a espera e já ajusta a posição da nave previamente
            //Ajuste de posição
            TurnOnAutoPilot();

            SuicideBurnData data = _flightTelemetry.GetSuicideBurnTelemetryInfo();
            if (burnType == BurnType.Vertical)
            {
                this.SetPitchAndHeading(90, 0.0f, false, true);
            }
            else if (burnType == BurnType.Horizontal)
            {
                this.SetPitchAndHeading(0.0f, 0.0f, false, true);
            }
            else if (burnType == BurnType.Diagonal)
            {
                //Vira na direção aproximada do cancelamento da componente vertical
                float thetaV = (float)Math.Asin(data.Gravity / data.EngineAcceleration);
                this.SetPitchAndHeading(thetaV, 0.0f, false, true);
            }
            else if (burnType == BurnType.Retrograde)
            {
                this.SetShipPosition(SASMode.Retrograde, true);
            }

            strMessage.Clear();
            strMessage.AppendFormat("Burn preparation has ended. Type: {0}", BurnTypeToString(burnType));
            SendMessage(strMessage.ToString());
        }

        private void DoBurn(BurnType burnType = BurnType.Diagonal, float finalSpeed = 10.0f,
            WhenStartBurn altType = WhenStartBurn.WaitHorizontalAltitude, float limitAltitude = 0.0f, float safetyMargin = 0.10f)
        {
            StringBuilder strMessage = new StringBuilder();
            strMessage.AppendFormat("Landing burn. Type: {0}, finalSpeed = {1}, Altitude={2}, limitAltitude={3}, safetyMargin={4}", 
                BurnTypeToString(burnType), finalSpeed, AltitudeTypeToString(altType), limitAltitude, safetyMargin);
            SendMessage(strMessage.ToString());

            DoBurnPreparation(burnType);

            if (altType != WhenStartBurn.Now)
            {
                WaitBurnAltitude(altType, limitAltitude, safetyMargin);
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

                if (burnType == BurnType.Vertical)
                {
                    if (Math.Abs(data.VerticalVelocity) <= Math.Abs(finalSpeed))
                    {
                        SendMessage("Vertical burn finished. Vertical Velocity is lower than final speed.");
                        break;
                    }
                    if(data.VerticalVelocity >= 0.0d)
                    {
                        SendMessage("Vertical burn finished. Vertical Velocity has changed its signal.");
                        break;
                    }                        
                }

                if (burnType == BurnType.Horizontal)
                {
                    if (Math.Abs(data.HorizontalVelocity) <= Math.Abs(finalSpeed))
                    {
                        SendMessage("Horizontal burn finished. Horizontal Velocity is lower than final speed.");
                        break;
                    }

                    if ( iSampleCounter > nSamples && (prevHorzV - data.HorizontalVelocity) < 0.0d)
                    {
                        strMessage.Clear();
                        strMessage.AppendFormat("Changed signal. VH_PREV={0}, VH_NOW={1}", prevHorzV, data.HorizontalVelocity);
                        SendMessage(strMessage.ToString());

                        //SendMessage("Horizontal burn finished. Horizontal Velocity has changed its signal.");
                        break;
                    }                        
                }

                if (burnType == BurnType.Diagonal) 
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

                if (burnType == BurnType.Retrograde)
                {
                    if (Math.Abs(data.CurrentSpeed) <= Math.Abs(finalSpeed))
                    {
                        SendMessage("Retrograde burn finished. Vertical Velocity is lower than final speed.");
                        break;
                    }
                    if(data.VerticalVelocity >= 0.0d)
                    {
                        SendMessage("Retrograde burn finished. Vertical Velocity has changed its signal.");
                        break;
                    }                        
                }

                //Em diagonal, o throtle sera controlado pelo PID
                if (burnType == BurnType.Diagonal)
                {
                    output = _controller.Output((float)data.VerticalVelocity);

                    //aqui é melhor chamar direto, senao o sleep dentro do metodo SetPitchAndHeading vai atrapalhar
                    CurrentVessel.AutoPilot.TargetPitchAndHeading(output, h);
                    //this.SetPitchAndHeading(output, h); //mantem o heading alinhado com o vetor velocidade (retrograde)
                }

                if (iSampleCounter % nSamples == 0)//a cada nSamples tira uma amostra e calcula pra ver se a velocidade mudou de sinal
                {
                    prevHorzV = Math.Min(data.HorizontalVelocity, prevHorzV);//para garantir que a prevHorzV só abaixa e qualquer aumento, sai do loop
                }

                data = _flightTelemetry.GetSuicideBurnTelemetryInfo();
                iSampleCounter++;

                Thread.Sleep((int) (1000 * timeStep));
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
        private void DoSoftBurn(float minAltitude, float FixedVelocity = 0, float AltitudeFactor=100, float max_thrust=1.0f)
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
                targetSpeed = (float) data.SurfaceAltitude / AltitudeFactor; //velocidade positiva é para baixo
            else
                targetSpeed = Math.Abs(FixedVelocity);

            float timeStep = 0.050f; //50 ms

            //O set point deveria ser -1 (se fosse a velocidade alvo) e deve ser exatamente 1 para as velocidades serem iguais levando em conta os sinais das duas
            //0.5 0.75 0.05
            PID _controller = new PID(_SP: 0.0f, _P: 0.50f, _I:0.010f, _D: 0.015f, 
                _TimeStep: timeStep, _MinValue: -1.0f, _MaxValue: 1.0f);
            float output;

            //Se a velocidade estiver muito abaixo do set point vai acumular erro negativo no termo integral e vai demorar 
            //para o PID entrar em ação
            //Espera velocidade atual igualar a velocidade alvo            
            while (data.CurrentSpeed < targetSpeed && data.VerticalVelocity < 0)
            {
                if (ReturnToManualControl())
                    return;

                data = _flightTelemetry.GetSuicideBurnTelemetryInfo();

                if (FixedVelocity == 0) //velocidade proporcional a altitude
                    targetSpeed = (float)data.SurfaceAltitude / AltitudeFactor; //velocidade positiva é para baixo

                Thread.Sleep(100);
            }

            //CurrentVessel.Control.Throttle = 1.0f;
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
                    targetSpeed = (float) data.SurfaceAltitude / AltitudeFactor; //velocidade positiva é para baixo

                //no jogo, velocidade pra baixo é negativo, mas para o PID deve ser positiva.
                //int iSignal = data.VerticalVelocity < 0 ? 1 : -1; //a mudança de sinal vai indicar se está subindo ou descendo

                //var currentSpeed = (data.CurrentSpeed) * iSignal;
                //double y = (targetSpeed - currentSpeed) / Math.Max( Math.Abs(targetSpeed), Math.Abs(currentSpeed));

                //output = _controller.Output((float) y);//targetSpeed / (float)data.CurrentSpeed); //*(iSignal) );
                //CurrentVessel.Control.Throttle = output;

                if (data.VerticalVelocity >= 0)
                    break;

                if(data.CurrentSpeed > targetSpeed)
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

        private void WaitBurnAltitude(WhenStartBurn altType, float limitAltitude=0.0f, float safetyMargin=0.10f)
        {
            StringBuilder strMessage = new StringBuilder();
            strMessage.AppendFormat("Waiting altitude: Type: {0}, limitAlt:{1}, safetyMargin:{2}.", AltitudeTypeToString(altType), limitAltitude, safetyMargin);
            SendMessage(strMessage.ToString());

            SuicideBurnData data = _flightTelemetry.GetSuicideBurnTelemetryInfo();
            while (true)
            {
                if (ReturnToManualControl())
                    return;

                if (altType == WhenStartBurn.WaitHorizontalAltitude && data.SurfaceAltitude <= data.HorizontalBurnStartAltitude * (1 + safetyMargin))
                    break;

                if (altType == WhenStartBurn.WaitVerticalAltitude && data.SurfaceAltitude <= data.VerticalBurnStartAltitude * (1 + safetyMargin))
                    break;

                if (data.SurfaceAltitude <= limitAltitude)
                    break;

                data = _flightTelemetry.GetSuicideBurnTelemetryInfo();
                Thread.Sleep(100);
            }
        }

        //Suicide Burn method
        private void DeorbitBody(float targetAltitude)
        {
            float periapsis = (float)_flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Periapsis_Altitude);

            if (periapsis <= 0)
                return;

            //Count down
            for ( int counter = 3; counter >= 0; counter--)
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

            //_CurrentVessel.Control.Throttle = 1.0f;
            while (true)
            {

                if (ReturnToManualControl())
                    return;

                periapsis = (float) _flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Periapsis_Altitude);

                output = _controller.Output( (targetAltitude - periapsis) / Math.Abs(periapsis));
                CurrentVessel.Control.Throttle = output;
                Thread.Sleep( (int)timeStep*1000);

                if (output < 0.1)
                    break;
            }

            CurrentVessel.Control.Throttle = 0.0f;
        }

        //ESTE AQUI TEM QUE SER THREAD TAMBÉM SENAO TRAVA A TELA
        //VAMOS TER QUE CRIAR UMA THREAD DE MONITORAMENTO E VAZAR DAQUI
        public void ExecuteSuicideBurn(SuicideBurnSetup suicideBurnSetup)
        {
            _SuicideBurnStatus = VesselState.Preparation;

            Console.WriteLine("Executing Suicide burn...");

            Task t0 = Task.Run(() => CheckSuicideBurn(suicideBurnSetup));
        }

        public void Launch(TakeOffDescriptor _tod)
        {
            _TakeOffStatus = VesselState.Preparation;

            this._tod = _tod;
            Console.WriteLine("Launching...");
            CurrentVessel.Control.SAS = false;
            CurrentVessel.Control.RCS = false;
            CurrentVessel.Control.Throttle = 1;
            CurrentVessel.Control.ActivateNextStage();

            Task t0 = Task.Run(() => CheckTakeoff(_tod.SRBStage));//return control to GUI
        }

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

            float mu  = CurrentVessel.Orbit.Body.GravitationalParameter;            
            double a = CurrentVessel.Orbit.SemiMajorAxis;

            //Velocity on current orbit
            double v1 = Math.Sqrt(mu * ((2.0 / r) - (1.0 / a)));

            //Velocity on adjusted orbit (circular)
            a = r; //on circle the semi-major axis IS THE RADIUS
            double v2 = Math.Sqrt(mu * ((2.0 / r) - (1.0 / a)));

            //Delta-V for orbit change (the signal is correctly calculated here)
            float delta_v = (float) (v2 - v1);

            //Creating the maneuver node
            CurrentVessel.Control.AddNode( uTime, delta_v );
        }

        public bool ExecuteManeuverNode()
        {
            if (CurrentVessel.AvailableThrust == 0 || CurrentVessel.SpecificImpulse == 0)
            {
                SendMessage("The engines are off, activate them, aborting maneuver...");
                return false;
            }

            Node node = null;
            if(CurrentVessel.Control.Nodes.Count > 0)
            {
                node = CurrentVessel.Control.Nodes[0];
            }
            else
            {
                SendMessage("No maneuver nodes available!");
                return false;
            }

            _ManeuverStatus = VesselState.Preparation;
            Task t0 = Task.Run(() => CheckManeuverExecution(node)); //return control to GUI

            return true;
        }
       
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
        
        //Thread
        private void CheckManeuverExecution(Node node)
        {
            SendMessage("Starting Node Telemetry...");

            _maneuverBurnTime = CalculateBurnTime();

            _flightTelemetry.SetupNodeTelemetry();
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
            //while (node.UT - _flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.UT) > burnTime / 2.0d)
            while (_flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.NodeTimeTo) > _maneuverBurnTime / 2.0d)
            {
                if (ReturnToManualControl())
                    return;

                Thread.Sleep(250);
            }

            SendMessage("Executing Maneuver...");
            _ManeuverStatus = VesselState.Executing;

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
            while(true)
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
                _ManeuverStatus = VesselState.Finished;
            }

            _maneuverBurnTime = 0.0f;
            SendMessage("Maneuver completed!");
        }

        //Thread
        private void Warp(float fLeadTime, float fBurnTime, Node maneuverNode)
        {
            SendMessage("Warping to " + fLeadTime + " seconds before starting maneuver");

            double totalLeadTime = fLeadTime + fBurnTime / 2.0;

            _conn.SpaceCenter().WarpTo(maneuverNode.UT - totalLeadTime);

            while(maneuverNode.UT - _flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.UT) > totalLeadTime)
            {
                if (ReturnToManualControl())
                    return;

                Thread.Sleep(200); //just wait until correct time 
            }

            SendMessage("Approaching maneuver node...");
        }
    }
}
