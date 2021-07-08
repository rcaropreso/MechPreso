using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using KRPC.Client;
using KRPC.Client.Services.KRPC;
using System.Timers;
using System.Threading;
using KRPC.Client.Services.SpaceCenter;
using WpfApp1.Utils;

namespace WpfApp1
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private ConnectionProxy _connector = null;
        private ListCollectionView _view;
        private System.Timers.Timer _telemetryTimer = null;
        private System.Timers.Timer _maneuverTimer = null;
        private System.Timers.Timer _suicideBurnTimer = null;
        private float _maneuverBurnTime = 0;
        private MissionController _missionController = null;
        private string OrbitalInfoName;
        private int _iSRBStage;
        private SuicideBurnSetup _suicideBurnSetup;


        /*
        private delegate void UpdateTelemetryLabelCallback(double vessel_heading, double vessel_pitch,
                                                         double surface_altitude, double srb_fuel,
                                                         double apoapsis_altitude, double terminal_velocity);

        private delegate void UpdateManeuverLabelCallback(double? NodeTimeTo, double? RemainingDeltaV, double? UT);

        private delegate void UpdateSuicideBurnLabelCallback(SuicideBurnData data);
        */

        //private delegate void UpdateStatusBarText(string message);

        AsyncMessageCallback updateStatusBarCallBack;

        public MainWindow()
        {
            InitializeComponent();
            cbTakeOffList.ItemsSource = OrbitalInfo.TakeOffDescriptors; //data binding
            this.DataContext = OrbitalInfo.TakeOffDescriptors; //data binding to textboxes
            _view = (ListCollectionView)CollectionViewSource.GetDefaultView(this.DataContext);
            cbTakeOffList.SelectedIndex = 0;

            //updateStatusBarCallBack = new AsyncMessageCallback();
            //updateStatusBarCallBack.NotifyEvent += UpdateStatusBarCallBack_NotifyEvent; //event handler for mission control and ship classes 
        }

        //private void UpdateStatusBarCallBack_NotifyEvent(object sender, EventArgs e)
        //{
        //    NotifyEventArgs ee = (NotifyEventArgs) e;
        //    SetStatusMessageAsync(ee.message);
        //}

        #region Mission_Control
        //thread (task) asynchronous
        private void ExecuteTakeOff()
        {
            /*
            _missionController?.ExecuteTakeOff(OrbitalInfoName, _iSRBStage);
            _maneuverBurnTime = 0;
            StopTimers();
            */
        }

        private void ExecuteSuicideBurn()
        {
            /*
            _missionController?.ExecuteSuicideBurn(_suicideBurnSetup);
            _maneuverBurnTime = 0;
            StopTimers();
            */
        }

        //thread (task) asynchronous
        private void ExecuteManeuverNode()
        {
            /*
            _missionController?.ExecuteManeuverNode();
            _maneuverBurnTime = 0;
            StopTimers();
            */
        }

        private void StopTimers()
        {
            /*
            SetStatusMessageAsync("Stopping Telemetry Timer...");
            _telemetryTimer?.Stop();

            SetStatusMessageAsync("Stopping Maneuver Timer...");
            _maneuverTimer?.Stop();

            SetStatusMessageAsync("Stopping Suicide Burn Timer...");
            _suicideBurnTimer?.Stop();
            */
        }

        private void StartTimers()
        {
            //_missionController.ResetVesselStates();//tem que ser chamado antes dos timers

            //SetStatusMessage("Starting Telemetry Timer...");
            //SetTelemetryTimer();

            //SetStatusMessage("Starting Maneuver Timer...");
            //SetManeuverTimer();

            //SetStatusMessage("Starting Suicide Burn Timer...");
            //SetSuicideBurnTimer();
        }
        #endregion

        #region GUI Update Methods
        private void SetSuicideBurnTimer()
        {
            //if(_suicideBurnTimer == null)
            //{
            //    _suicideBurnTimer = new System.Timers.Timer(250);
            //    _suicideBurnTimer.Elapsed += OnTimedEventSuicideBurnTelemetry;
            //}
            //
            // Hook up the Elapsed event for the timer.             
            //_suicideBurnTimer.AutoReset = true;
            //_suicideBurnTimer.Enabled = true;
        }

        private void OnTimedEventSuicideBurnTelemetry(Object source, ElapsedEventArgs e)
        {
            /*
            if (_missionController?.ShipControl.SuicideBurnStatus == ShipFlighter.VesselState.Finished)
            {
                return;
            }

            //Update GUI
            SuicideBurnData data = _missionController?.ShipControl?._flightTelemetry?.GetSuicideBurnTelemetryInfo();

            if (App.Current == null || data == null)//avoid crashes on application close by now
            {
                return;
            }

            App.Current.Dispatcher.Invoke(new UpdateSuicideBurnLabelCallback(this.UpdateSuicideBurnTelemetryText), new object[] { data });
            */
        }

        private void UpdateSuicideBurnTelemetryText(SuicideBurnData data)
        {
            /*
            txtCurrentSpeed.Text       = String.Format("{0:0.##}", data.CurrentSpeed);
            txtVerticalSpeed.Text      = String.Format("{0:0.##}", data.VerticalVelocity);
            txtHorizontalSpeed.Text    = String.Format("{0:0.##}", data.HorizontalVelocity);
            txtEngineAcc.Text          = String.Format("{0:0.##}", data.EngineAcceleration);
            txtVertBurnStartAlt.Text   = String.Format("{0:0.##}", data.VerticalBurnStartAltitude);
            txtHorzBurnStartAlt.Text   = String.Format("{0:0.##}", data.HorizontalBurnStartAltitude);       
            */
        }

        private void SetTelemetryTimer()
        {
            /*
            if (_telemetryTimer == null)
            {
                _telemetryTimer = new System.Timers.Timer(250);
                _telemetryTimer.Elapsed += OnTimedEventTelemetry;
            }

            // Hook up the Elapsed event for the timer. 
            _telemetryTimer.AutoReset = true;
            _telemetryTimer.Enabled   = true;
            */
        }

        private void OnTimedEventTelemetry(Object source, ElapsedEventArgs e)
        {
            /*
            if(_missionController?.ShipControl.TakeOffStatus == ShipFlighter.VesselState.Finished)
            {
                _telemetryTimer.Stop();
                return;
            }

            //Update GUI
            double vessel_heading = _missionController.ShipControl._flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.VesselHeading);
            double vessel_pitch = _missionController.ShipControl._flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.VesselPitch);
            double surface_altitude = _missionController.ShipControl._flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Surface_Altitude);
            double srb_fuel = _missionController.ShipControl._flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.SRB_Fuel);
            double apoapsis_altitude = _missionController.ShipControl._flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Apoapsis_Altitude);
            double terminal_velocity = _missionController.ShipControl._flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Terminal_Velocity);

            if (App.Current == null)//avoid crashes on application close by now
            {
                _telemetryTimer.Enabled = false;
                return;
            }
            App.Current.Dispatcher.Invoke(new UpdateTelemetryLabelCallback(this.UpdateTelemetryText), new object[] { vessel_heading, vessel_pitch,
                surface_altitude, srb_fuel, apoapsis_altitude, terminal_velocity});
                */
        }

        private void UpdateTelemetryText(double vessel_heading, double vessel_pitch, double surface_altitude, double srb_fuel, double apoapsis_altitude, double terminal_velocity)
        {
            /*
            txtCurrentHeading.Text      = String.Format("{0:0.##}", vessel_heading);
            txtCurrentPitch.Text        = String.Format("{0:0.##}", vessel_pitch);
            txtCurrentAltitude.Text     = String.Format("{0:0.##}", surface_altitude);
            txtCurrentSRB.Text          = String.Format("{0:0.##}", srb_fuel);
            txtCurrentTermVelocity.Text = String.Format("{0:0.##}", terminal_velocity);
            txtCurrentApoapsis.Text     = String.Format("{0:0.##}", apoapsis_altitude);
            txtGravity.Text             = String.Format("{0:0.##}", _missionController?.ShipControl._flightTelemetry.GetGravity());
            */
        }

        private void SetManeuverTimer()
        {
            /*
            if(_maneuverTimer == null)
            {
                _maneuverTimer = new System.Timers.Timer(250);
                _maneuverTimer.Elapsed += OnTimedEventManeuver;
            }

            // Hook up the Elapsed event for the timer. 
            _maneuverTimer.AutoReset = true;
            _maneuverTimer.Enabled   = true;
            */
        }

        private void OnTimedEventManeuver(Object source, ElapsedEventArgs e)
        {
            /*
            if(_missionController == null || _missionController.ShipControl == null)
            {
                return;
            }

            switch (_missionController.ShipControl.ManeuverStatus)
            {
                case ShipFlighter.VesselState.Preparation:
                case ShipFlighter.VesselState.Executing:
                    if (_maneuverBurnTime == 0)
                    {
                        _maneuverBurnTime = _missionController.ShipControl.CalculateBurnTime();
                    }
                    break;

                case ShipFlighter.VesselState.Finished:
                    _maneuverTimer.Stop();
                    _maneuverBurnTime = 0;
                    return;
            }

            //Update GUI
            double NodeTimeTo      = _missionController.ShipControl._flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.NodeTimeTo);
            double RemainingDeltaV = _missionController.ShipControl._flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.Remaining_DeltaV);
            double UT              = _missionController.ShipControl._flightTelemetry.GetInfo(FlightTelemetry.TelemetryInfo.UT);

            if (App.Current == null)//avoid crashes on application close by now
            {
                _maneuverTimer.Enabled = false;
                return;
            }

            App.Current.Dispatcher.Invoke(new UpdateManeuverLabelCallback(this.UpdateManeuverText),
                                          new object[] { NodeTimeTo, RemainingDeltaV, UT });
                                          */
        }

        private void UpdateManeuverText(double? NodeTimeTo, double? RemainingDeltaV, double? UT)
        {
            /*
            double nodeTime = NodeTimeTo ?? 0.0d;
            double remainingDV = RemainingDeltaV ?? 0;

            txtNodeTimeTo.Text      = String.Format("{0:0.##}", nodeTime);
            txtRemainingDeltaV.Text = String.Format("{0:0.##}", remainingDV);
            txtStartBurn.Text       = String.Format("{0:0.##}", (nodeTime - _maneuverBurnTime / 2.0d));
            */
        }

        //private void SetStatusMessageAsync(string message)
        //{
        //    App.Current.Dispatcher.Invoke(new UpdateStatusBarText(this.SetStatusMessage), new object[] { message });
        //}

        public void SetStatusMessage(string message)
        {
            /*
            txtEventLog.AppendText(message);
            txtEventLog.AppendText(Environment.NewLine);

            //Scroll to last texbox row
            txtEventLog.ScrollToEnd();

            lblStatusBar.Content = message;
            */
        }
        #endregion

        #region GUI Events        
        private bool HasValidData()
        {
            /*
            bool bRet = true;

            if (_connector == null || !_connector.IsConnected())
            {
                SetStatusMessage("No connection available.");
                bRet = false;
            }
            else if (_missionController == null)
            {
                SetStatusMessage("No mission controller available");
                bRet = false;
            }
            
            return bRet;
            */
            return true;
        }

        private void ClearScreen()
        {
            /*
            txtEventLog.Clear();
            txtNodeTimeTo.Text = "";
            txtRemainingDeltaV.Text = "";
            txtStartBurn.Text = "";

            cbTakeOffList.SelectedIndex = 0;

            txtCurrentAltitude.Text = "";
            txtCurrentHeading.Text = "";
            txtCurrentPitch.Text = "";
            txtCurrentSRB.Text = "";
            txtCurrentTermVelocity.Text = "";
            txtCurrentApoapsis.Text = "";
            txtGravity.Text = "";

            txtRollAngle.Text = "";

            txtCurrentSpeed.Text = "";
            txtVerticalSpeed.Text = "";
            txtHorizontalSpeed.Text = "";
            txtEngineAcc.Text = "";
            txtVertBurnStartAlt.Text = "";
            txtHorzBurnStartAlt.Text = "";

            txtDeorbitAltitude.Text = "0";

            lblStatusBar.Content = "";
            */
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            /*
            ClearScreen();

            _connector = new ConnectionProxy( name: "My Connection", address: txtIPAddress.Text, 
                port: int.Parse(txtPort.Text), streamport: int.Parse(txtPort.Text) + 1);

            if (_connector.IsConnected())
            {
                SetStatusMessage("Connected on KRPC version: " + _connector.GetVersion());
                _missionController = _missionController ?? new MissionController(_connector);
            }
            */
        }

        private void BtnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (!HasValidData())
            {
                return;
            }

            _missionController?.ShipControl?._flightTelemetry?.StopAllTelemetry();
            StopTimers();

            _connector?.CloseConnection();

            _connector = null;
            _missionController = null;

            GC.Collect();

            SetStatusMessage("Disconnected.");
            */
        }

        private void CbTakeOffList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*
            _view.MoveCurrentTo(cbTakeOffList.SelectedItem);
            OrbitalInfoName = cbTakeOffList.SelectedItem.ToString();
            */
        }

        private void BtnLaunch_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (!HasValidData())
            {
                return;
            }

            //Clear log
            _iSRBStage = int.Parse(txtSRB_Stage.Text);

            txtEventLog.Clear();
            StartTimers();
            _missionController?.ResetManualControl();
            Task.Run(() => ExecuteTakeOff());
            */
        }
        
        private void BtnExecManeuver_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (!HasValidData())
            {
                return;
            }

            txtEventLog.Clear();

            StartTimers();
            _maneuverBurnTime = (_missionController?.ShipControl?.CalculateBurnTime()) ?? 0.0f;
            _missionController?.ResetManualControl();
            Task.Run(() => ExecuteManeuverNode());
            */
        }
        
        private void BtnCircularization_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (!HasValidData())
            {
                return;
            }

            bool bReduce = rdReduceOrbit.IsChecked ?? false;
            _missionController?.PlanCircularization( bReduce );
            */
        }
        
        private void BtnSuicideBurn_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (!HasValidData())
            {
                return;
            }

            txtEventLog.Clear();
            StartTimers();
            SetStatusMessage("Starting Landing Maneuver...");
            
            if(_missionController != null && _missionController.ShipControl != null)
                _missionController.ShipControl.DeorbitTargetAltitude = int.Parse(txtDeorbitAltitude.Text);

            _suicideBurnSetup = new SuicideBurnSetup {
                DoCancelVerticalVelocity = (bool)chkCancelVertical.IsChecked,
                DoCancelHorizontalVelocity = (bool) chkCancelVelocity.IsChecked,
                DoDeorbitBody = (bool) chkDeorbit.IsChecked,
                DoFineTunning = (bool) chkFineTunning.IsChecked,
                DoStopBurn = (bool) chkStopBurn.IsChecked,
            };

            _missionController?.ResetManualControl();
            Task.Run(() => ExecuteSuicideBurn());
            */
        }
        #endregion

        private void BtnRoll_Click(object sender, RoutedEventArgs e)
        {
            if (!HasValidData())
            {
                return;
            }

            if (_missionController is null || _missionController.ShipControl is null ||
                _missionController.ShipControl.Telemetry is null)
                return;

            ReferenceFrame CurrentRefFrame = _missionController.ShipControl.Telemetry.CurrentRefFrame;
            Flight flight = _missionController.ShipControl.CurrentVessel.Flight(CurrentRefFrame);

            var h = flight.Heading;
            var p = flight.Pitch;

            {
                _missionController.ShipControl.TurnOnAutoPilot();
                _missionController.ShipControl.SetupRoll();
                _missionController.ShipControl.TurnOffAutoPilot();
                Thread.Sleep(1000);
                
                _missionController.ShipControl.TurnOnAutoPilot();
                _missionController.ShipControl.SetPitchAndHeading(90, 45);
                _missionController.ShipControl.TurnOffAutoPilot();
                Thread.Sleep(1000);
               

                //Teste
                _missionController.ShipControl.SetShipPosition(SASMode.Normal);
                Thread.Sleep(1000);
                _missionController.ShipControl.SetShipPosition(SASMode.AntiNormal);
                Thread.Sleep(1000);
                _missionController.ShipControl.SetShipPosition(SASMode.Prograde);
                Thread.Sleep(1000);
                _missionController.ShipControl.SetShipPosition(SASMode.Retrograde);

                return;
            }



            {
                _missionController.ShipControl.CurrentVessel.AutoPilot.Disengage();

                _missionController.ShipControl.CurrentVessel.Control.RCS = true;

                //Para setar o flag AutoPilot.SAS, somente com AutoPilot Disengaged
                //Ao que parece, setar esse flag vai dar engage novamente
                _missionController.ShipControl.CurrentVessel.AutoPilot.SAS = true;
                Thread.Sleep(1000); //Esse sleep é obrigatorio

                //Para setar o SASMODE do AutoPilot somente se der sleep acima para dar tempo de ligar o AP
                //Senão ele não atualiza corretamente
                _missionController.ShipControl.CurrentVessel.AutoPilot.SASMode = SASMode.Retrograde;
                //Se não colocar o sleep exatamente ali em cima, este wait nao funciona, porque ele precisa do AP engaged
                _missionController.ShipControl.CurrentVessel.AutoPilot.Wait(); 
                

                Thread.Sleep(3000);

                //Adjust rotation

                //Amanha podemos testar esse trecho de codigo sem o engage e ver se funciona
                _missionController.ShipControl.CurrentVessel.AutoPilot.Engage();
                //Thread.Sleep(500);//precisa deste delay senao nao funciona                
                _missionController.ShipControl.CurrentVessel.AutoPilot.TargetPitchAndHeading(90, h);
                return;
            }
            
            
            
            //Vessel currentVessel = _missionController.ShipControl.CurrentVessel;
            //ReferenceFrame CurrentRefFrame = ReferenceFrame.CreateHybrid(_connector._connection, currentVessel.Orbit.Body.ReferenceFrame, currentVessel.SurfaceReferenceFrame);


            //Adjust rotation
            _missionController.ShipControl.CurrentVessel.AutoPilot.Engage();
            Thread.Sleep(500);//precisa deste delay senao nao funciona
            _missionController.ShipControl.CurrentVessel.AutoPilot.TargetPitchAndHeading(p, h);
            _missionController.ShipControl.CurrentVessel.AutoPilot.Wait();
            _missionController.ShipControl.CurrentVessel.AutoPilot.Disengage();

            //Daria pra tentar fazer um PID que controla a rotação da nave(pitch)
            //o setpoint seria manter a velocidade vertical igual ou proxima a zero 


            if(!String.IsNullOrEmpty( txtRollAngle.Text ))
            {
                var newPitch = float.Parse(txtRollAngle.Text);
                _missionController.ShipControl.CurrentVessel.AutoPilot.Engage();
                Thread.Sleep(1000);
                _missionController.ShipControl.CurrentVessel.AutoPilot.TargetPitchAndHeading(newPitch, h);
                _missionController.ShipControl.CurrentVessel.AutoPilot.Wait();
                _missionController.ShipControl.CurrentVessel.AutoPilot.Disengage();
            }
            
            //txtRollAngle.Text = _missionController.ShipControl.CurrentVessel.AutoPilot.TargetRoll.ToString(); // = float.Parse(txtRollAngle.Text);
            //txtRollAngle.Text = _missionController.ShipControl.CurrentVessel.AutoPilot.TargetPitch.ToString(); //= float.Parse(txtRollAngle.Text);
            //txtRollAngle.Text = _missionController.ShipControl.CurrentVessel.AutoPilot.TargetHeading.ToString(); //= float.Parse(txtRollAngle.Text);
            //_missionController.ShipControl.CurrentVessel.AutoPilot.TargetPitch = float.Parse(txtRollAngle.Text);
        }

        private void BtnManualControl_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (!HasValidData())
            {
                return;
            }

            //Stop everything and give manual control
            _missionController?.SetManualControl();
            */
        }

        private void BtnCopyLog_Click(object sender, RoutedEventArgs e)
        {
            /*
            txtEventLog.SelectAll();
            txtEventLog.Copy();
            SetStatusMessage("Log copied to Clipboard");
            */
        }
    }
}
