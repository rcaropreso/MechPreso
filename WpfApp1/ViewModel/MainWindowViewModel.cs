using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using WpfApp1.Models;
using WpfApp1.Utils;

namespace WpfApp1.ViewModel
{
    public class MainWindowViewModel : BaseViewModel
    {
        //Parametros especificos
        private IPageViewModel _connectionViewModel;
        private IPageViewModel _maneuverViewModel;
        private IPageViewModel _telemetryViewModel;
        private IPageViewModel _suicideBurnViewModel;
        private IPageViewModel _takeoffViewModel;

        private string _eventLog;
        private System.Timers.Timer _telemetryTimer = null;
        private System.Timers.Timer _maneuverTimer = null;
        private System.Timers.Timer _suicideBurnTimer = null;
        private float _maneuverBurnTime = 0;


        private MissionController _missionController;

        private string _selectedView; //nome da view selecionada (mesmo nome do Radio Button da mainWindow)
        private ICommand _closeWindowCommand;
        private ICommand _viewSelector;
        private ICommand _copyLog;
        private ICommand _manualControl;


        private delegate void UpdateEventLogText(string message);

        #region MVVM Pattern Attributes
        private IPageViewModel _currentPageViewModel;
        private List<IPageViewModel> _pageViewModels;

        public List<IPageViewModel> PageViewModels
        {
            get
            {
                if (_pageViewModels == null)
                    _pageViewModels = new List<IPageViewModel>();

                return _pageViewModels;
            }
        }

        public IPageViewModel CurrentPageViewModel
        {
            get
            {
                return _currentPageViewModel;
            }

            set
            {
                _currentPageViewModel = value;
                OnPropertyChanged("CurrentPageViewModel");
            }
        }


        public string SelectedView //based upon https://www.c-sharpcorner.com/article/explain-radio-button-binding-in-mvvm-wpf/
        {
            get { return _selectedView; }
            set
            {
                _selectedView = value;
                //OnPropertyChanged(nameof(SelectedView));
                OnPropertyChanged(nameof(ViewSelector));

                switch (_selectedView)
                {
                    case "Landing":
                        ChangeViewModel(PageViewModels[1]);
                        break;
                    case "Takeoff":
                    default:
                        ChangeViewModel(PageViewModels[0]);
                        break;
                }
            }
        }

        public ICommand CloseWindowCommand //associado ao close da janela (botao "X")
        {
            get
            {
                return _closeWindowCommand ?? (_closeWindowCommand = new RelayCommand(x =>
                {
                    this.OnCloseWindow();
                }));
            }
        }

        private void OnCloseWindow()
        {
            ((ConnectionViewModel)_connectionViewModel).OnDisconnect();
        }

        public ICommand ViewSelector //associado aos Radio Buttons da main window
        {
            get
            {
                return _viewSelector ?? (_viewSelector = new RelayCommand( x =>
                {
                    SelectedView = (string)x;
                }));
            }           
        }

        public ICommand CopyLog
        {
            get
            {
                return _copyLog ?? (_copyLog = new RelayCommand(x =>
                {
                    System.Windows.Clipboard.SetText(EventLog);
                    SetEventLogMessage("Log copied to Clipboard");
                }));
            }
        }

        public ICommand ManualControl
        {
            get
            {
                return _manualControl ?? (_manualControl = new RelayCommand(x =>
                {
                    if (((ConnectionViewModel)_connectionViewModel).HasValidData())
                    {
                        _missionController.SetManualControl();
                    }
                }));
            }
        }
        #endregion

        #region MVVM Pattern Methods

        private void ChangeViewModel(IPageViewModel viewModel)
        {
            if (!PageViewModels.Contains(viewModel))
            {
                PageViewModels.Add(viewModel);
            }

            CurrentPageViewModel = PageViewModels.FirstOrDefault(vm => vm == viewModel);
        }
        #endregion

        #region Additional MVVM Attributes (Fixed Views)
        public IPageViewModel ConnectionViewModel
        {
            get
            {
                return _connectionViewModel;
            }
        }

        public IPageViewModel ManeuverViewModel
        {
            get
            {
                return _maneuverViewModel;
            }
        }

        public IPageViewModel TelemetryViewModel
        {
            get
            {
                return _telemetryViewModel;
            }
        }

        public IPageViewModel TakeoffViewModel
        {
            get
            {
                return _takeoffViewModel;
            }
        }

        public IPageViewModel SuicideBurnViewModel
        {
            get
            {
                return _suicideBurnViewModel;
            }
        }

        public string EventLog
        {
            get
            {
                return _eventLog;
            }
            set
            {
                _eventLog = value;
                //Scroll to last texbox row
                OnPropertyChanged(nameof(EventLog));
            }
        }
        #endregion

        public MainWindowViewModel()
        {
            _connectionViewModel  = new ConnectionViewModel();
            _maneuverViewModel    = new ManeuverViewModel();
            _telemetryViewModel   = new TelemetryViewModel();
            _suicideBurnViewModel = new SuicideBurnViewModel();
            _takeoffViewModel     = new TakeOffViewModel();
            
            //Cadastro das ViewModels usadas na aplicação
            PageViewModels.Add(_takeoffViewModel);
            PageViewModels.Add(_suicideBurnViewModel);

            CurrentPageViewModel = null;

            //Seleciona a View Takeoff como default
            SelectedView = "Takeoff"; //texto do radiobutton


            //Cadastro das mensagens e tópicos gerenciadas pelo Mediator
            Mediator.Subscribe(CommonDefs.MSG_CLEAR_SCREEN, OnClearScreen);
            Mediator.Subscribe(CommonDefs.MSG_SEND_MESSAGE, OnSendMessage);
            Mediator.Subscribe(CommonDefs.MSG_CONNECT,     OnConnect);
            Mediator.Subscribe(CommonDefs.MSG_DISCONNECT,  OnDisconnect);
            Mediator.Subscribe(CommonDefs.MSG_START_TIMERS, OnStartTimers); //Talvez nao precise inscrever este aqui
            Mediator.Subscribe(CommonDefs.MSG_STOP_TIMERS,  OnStopTimers);

            Mediator.Subscribe(CommonDefs.MSG_LAUNCH, OnExecuteTakeoff);

            Mediator.Subscribe(CommonDefs.MSG_EXECUTE_MANEUVER, OnExecuteManeuver);
            Mediator.Subscribe(CommonDefs.MSG_CIRCULARIZE,      OnCircularize);

            Mediator.Subscribe(CommonDefs.MSG_EXECUTE_SUICIDE_BURN, OnExecuteSuicideBurn);

            Mediator.Subscribe(CommonDefs.MSG_NONE_SCREEN,    OnNoneScreen);
            Mediator.Subscribe(CommonDefs.MSG_TAKEOFF_SCREEN, OnTakeoffScreen);
            Mediator.Subscribe(CommonDefs.MSG_LANDING_SCREEN, OnLandingScreen);
        }

        private void OnStartTimers(object obj)
        {
            _missionController.ResetVesselStates();//tem que ser chamado antes dos timers

            SetEventLogMessage("Starting Telemetry Timer...");
            if (_telemetryTimer == null)
            {
                _telemetryTimer = new System.Timers.Timer(250);
                _telemetryTimer.Elapsed += OnTimedEventTelemetry;
            }

            // Hook up the Elapsed event for the timer. 
            _telemetryTimer.AutoReset = true;
            _telemetryTimer.Enabled = true;

            SetEventLogMessage("Starting Maneuver Timer...");
            if (_maneuverTimer == null)
            {
                _maneuverTimer = new System.Timers.Timer(250);
                _maneuverTimer.Elapsed += OnTimedEventManeuver;
            }

            // Hook up the Elapsed event for the timer. 
            _maneuverTimer.AutoReset = true;
            _maneuverTimer.Enabled = true;

            SetEventLogMessage("Starting Suicide Burn Timer...");
            if (_suicideBurnTimer == null)
            {
                _suicideBurnTimer = new System.Timers.Timer(250);
                _suicideBurnTimer.Elapsed += OnTimedEventSuicideBurnTelemetry;
            }

            // Hook up the Elapsed event for the timer.             
            _suicideBurnTimer.AutoReset = true;
            _suicideBurnTimer.Enabled = true;
        }

        private void OnTimedEventSuicideBurnTelemetry(Object source, ElapsedEventArgs e)
        {
            if (_missionController?.ShipControl.SuicideBurnStatus == CommonDefs.VesselState.Finished)
            {
                _suicideBurnTimer.Stop();
                return;
            }

            //Update GUI
            SuicideBurnData _data = _missionController?.ShipControl?.Telemetry?.GetSuicideBurnTelemetryInfo();

            if (App.Current == null)//avoid crashes on application close by now
            {
                _suicideBurnTimer.Enabled = false;
                return;
            }

            ((SuicideBurnViewModel)_suicideBurnViewModel).SafeUpdateSuicideBurnText(_data);
        }       

        private void OnTimedEventTelemetry(Object source, ElapsedEventArgs e)
        {
            if (_missionController?.ShipControl.TakeOffStatus == CommonDefs.VesselState.Finished)
            {
                _telemetryTimer.Stop();
                return;
            }

            //Update GUI
            double _vesselHeading    = _missionController.ShipControl.Telemetry.GetInfo(FlightTelemetry.TelemetryInfo.VesselHeading);
            double _vesselPitch      = _missionController.ShipControl.Telemetry.GetInfo(FlightTelemetry.TelemetryInfo.VesselPitch);
            double _surfaceAltitude  = _missionController.ShipControl.Telemetry.GetInfo(FlightTelemetry.TelemetryInfo.Surface_Altitude);
            double _srbFuel          = _missionController.ShipControl.Telemetry.GetInfo(FlightTelemetry.TelemetryInfo.SRB_Fuel);
            double _apoapsisAltitude = _missionController.ShipControl.Telemetry.GetInfo(FlightTelemetry.TelemetryInfo.Apoapsis_Altitude);
            double _terminalVelocity = _missionController.ShipControl.Telemetry.GetInfo(FlightTelemetry.TelemetryInfo.Terminal_Velocity);

            double _currentSpeed    = _missionController.ShipControl.Telemetry.GetInfo(FlightTelemetry.TelemetryInfo.CurrentSpeed);
            double _verticalSpeed   = _missionController.ShipControl.Telemetry.GetInfo(FlightTelemetry.TelemetryInfo.VerticalSpeed);
            double _horizontalSpeed = _missionController.ShipControl.Telemetry.GetInfo(FlightTelemetry.TelemetryInfo.HorizontalSpeed);

            double _engineAcc = _missionController.ShipControl.Telemetry.GetEngineAcceleration();
            double _gravity   = _missionController.ShipControl.Telemetry.GetGravity();

            TelemetryData _data = new TelemetryData(_vesselHeading, _vesselPitch, _surfaceAltitude, _srbFuel,
                _apoapsisAltitude, _terminalVelocity, _currentSpeed, _verticalSpeed, 
                _horizontalSpeed, _engineAcc, _gravity);

            if (App.Current == null || _data == null)//avoid crashes on application close by now
            {
                _telemetryTimer.Enabled = false;
                return;
            }

            ((TelemetryViewModel)_telemetryViewModel).SafeUpdateTelemetryText(_data);
        }

        private void OnTimedEventManeuver(Object source, ElapsedEventArgs e)
        {
            if (_missionController == null || _missionController.ShipControl == null)
            {
                return;
            }

            switch (_missionController.ShipControl.ManeuverStatus)
            {
                case CommonDefs.VesselState.Preparation:
                case CommonDefs.VesselState.Executing:
                    _maneuverBurnTime = _missionController.ShipControl.ManeuverBurnTime;
                    break;

                case CommonDefs.VesselState.Finished:
                    _maneuverTimer.Stop();
                    _maneuverBurnTime = 0;
                    return;
            }

            //Update GUI
            double NodeTimeTo      = _missionController.ShipControl.Telemetry.GetInfo(FlightTelemetry.TelemetryInfo.NodeTimeTo);
            double RemainingDeltaV = _missionController.ShipControl.Telemetry.GetInfo(FlightTelemetry.TelemetryInfo.Remaining_DeltaV);

            if (App.Current == null)//avoid crashes on application close by now
            {
                _maneuverTimer.Enabled = false;
                return;
            }

            ManeuverData _data = new ManeuverData(NodeTimeTo, RemainingDeltaV, _maneuverBurnTime);

            ((ManeuverViewModel)_maneuverViewModel).SafeUpdateManeuverText(_data);           
        }

        private void OnStopTimers(object obj)
        {
            SetEventLogMessage("Stopping Telemetry Timer...");
            _telemetryTimer?.Stop();

            SetEventLogMessage("Stopping Maneuver Timer...");
            _maneuverTimer?.Stop();

            SetEventLogMessage("Stopping Suicide Burn Timer...");
            _suicideBurnTimer?.Stop();
        }

        private void OnConnect(object obj)
        {            
            _missionController = ((ConnectionViewModel)_connectionViewModel).OnConnect();
        }

        private void OnDisconnect(object obj)
        {
            ((ConnectionViewModel)_connectionViewModel).OnDisconnect();
        }

        private void OnClearScreen(object obj)
        {
            //Limpa campos da tela principal (todas as views)
            EventLog = "";

            ManeuverData _data1    = new ManeuverData(0, 0, 0);
            TelemetryData _data2   = new TelemetryData(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            SuicideBurnData _data3 = new SuicideBurnData(0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

            ((ManeuverViewModel)_maneuverViewModel).SafeUpdateManeuverText(_data1);
            ((TelemetryViewModel)_telemetryViewModel).SafeUpdateTelemetryText(_data2);
            ((SuicideBurnViewModel)_suicideBurnViewModel).SafeUpdateSuicideBurnText(_data3);
        }

        private void OnExecuteTakeoff(object obj)
        {
            TakeOffDescriptor _tod = (TakeOffDescriptor)obj;

            if (!((ConnectionViewModel)_connectionViewModel).HasValidData())
                return;

            _missionController?.ResetManualControl();

            Task.Run(() => AsyncExecuteTakeOff(_tod));
        }

        private void AsyncExecuteTakeOff(TakeOffDescriptor _tod)
        {
            _missionController?.ExecuteTakeOff(_tod);
            _maneuverBurnTime = 0;
            //OnStopTimers("");
        }

        private void OnExecuteManeuver(object obj)
        {
            if (!((ConnectionViewModel)_connectionViewModel).HasValidData())
                return;

            _missionController?.ResetManualControl();
            _maneuverBurnTime = (_missionController?.ShipControl?.CalculateBurnTime()) ?? 0.0f;

            Task.Run(() => AsyncExecuteManeuver());
        }

        private void AsyncExecuteManeuver()
        {
            _missionController?.ExecuteManeuverNode();
            _maneuverBurnTime = 0;
            //OnStopTimers("");
        }

        private void OnCircularize(object obj)
        {
            bool reduceOrbit = (bool)obj;

            if (!((ConnectionViewModel)_connectionViewModel).HasValidData())
                return;

            _missionController?.PlanCircularization(reduceOrbit);
        }

        private void OnExecuteSuicideBurn(object obj)
        {
            if (!((ConnectionViewModel)_connectionViewModel).HasValidData())
                return;

            SuicideBurnSetup _sbSetup = (SuicideBurnSetup)obj;

            OnSendMessage("Starting Landing Maneuver...");

            _missionController?.ResetManualControl();

            Task.Run(() => AsyncExecuteSuicideBurn(_sbSetup));
        }
        private void AsyncExecuteSuicideBurn(SuicideBurnSetup _sbSetup)
        {
            _missionController?.ExecuteSuicideBurn(_sbSetup);
            _maneuverBurnTime = 0;
            //OnStopTimers("");
        }

        private void OnTakeoffScreen(object obj)
        {
            ChangeViewModel(PageViewModels[0]);
        }

        private void OnLandingScreen(object obj)
        {
            ChangeViewModel(PageViewModels[1]);
        }

        private void OnNoneScreen(object obj)
        {
            CurrentPageViewModel = null;
        }

        private void OnSendMessage(object message)
        {
            //Console.WriteLine((string)message);
            App.Current.Dispatcher.Invoke(new UpdateEventLogText(this.SetEventLogMessage), new object[] { message });
        }

        public void SetEventLogMessage(string message)
        {
            EventLog += message;
            EventLog += Environment.NewLine;
        }
    }
}
