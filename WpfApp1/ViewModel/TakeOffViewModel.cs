using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfApp1.Utils;
using WpfApp1.Models;

namespace WpfApp1.ViewModel
{
    class TakeOffViewModel : BaseViewModel, IPageViewModel
    {
        private ObservableCollection<TakeOffDescriptor> _takeOffDescriptors;

        private string _shipHeadingAngle;
        private string _initialRotationAltitude;
        private string _startTurnAltitude;
        private string _endTurnAltitude;
        private string _targetAltitude;
        private string _atmosphereAltitude;
        private string _srbStage;

        private ICommand _launch;

        public string ShipHeadingAngle
        {
            get { return _shipHeadingAngle; }
            set
            {
                _shipHeadingAngle = value;
                OnPropertyChanged(nameof(ShipHeadingAngle));
            }
        }
        public string InitialRotationAltitude
        {
            get { return _initialRotationAltitude; }
            set
            {
                _initialRotationAltitude = value;
                OnPropertyChanged(nameof(InitialRotationAltitude));
            }
        }
        public string StartTurnAltitude
        {
            get { return _startTurnAltitude; }
            set
            {
                _startTurnAltitude = value;
                OnPropertyChanged(nameof(StartTurnAltitude));
            }
        }
        public string EndTurnAltitude
        {
            get { return _endTurnAltitude; }
            set
            {
                _endTurnAltitude = value;
                OnPropertyChanged(nameof(EndTurnAltitude));
            }
        }
        public string TargetAltitude
        {
            get { return _targetAltitude; }
            set
            {
                _targetAltitude = value;
                OnPropertyChanged(nameof(TargetAltitude));
            }
        }
        public string AtmosphereAltitude
        {
            get { return _atmosphereAltitude; }
            set
            {
                _atmosphereAltitude = value;
                OnPropertyChanged(nameof(AtmosphereAltitude));
            }
        }
        public string SRBStage
        {
            get { return _srbStage; }
            set
            {
                _srbStage = value;
                OnPropertyChanged(nameof(SRBStage));
            }
        }

        public ObservableCollection<TakeOffDescriptor> TakeOffDescriptors
        {
            get { return _takeOffDescriptors; }
            set
            {
                _takeOffDescriptors = value;
                OnPropertyChanged(nameof(TakeOffDescriptors));
            }
        }

        private TakeOffDescriptor _selectedTakeoff;

        public TakeOffDescriptor SelectedTakeOff
        {
            get
            {
                return _selectedTakeoff;
            }
            set
            {
                _selectedTakeoff = value;
                OnPropertyChanged(nameof(SelectedTakeOff));

                //New item has been selected
                ShipHeadingAngle        = _selectedTakeoff.ShipHeadingAngle.ToString();
                InitialRotationAltitude = _selectedTakeoff.InitialRotationAltitude.ToString();
                StartTurnAltitude       = _selectedTakeoff.StartTurnAltitude.ToString();
                EndTurnAltitude         = _selectedTakeoff.EndTurnAltitude.ToString();
                TargetAltitude          = _selectedTakeoff.TargetAltitude.ToString();
                AtmosphereAltitude      = _selectedTakeoff.AtmosphereAltitude.ToString();
            }
        }
        public ICommand Launch
        {
            get
            {
                return _launch ?? (_launch = new RelayCommand(x =>
                {
                    _selectedTakeoff.SRBStage = int.Parse(SRBStage ?? "0");
                    Mediator.Notify(CommonDefs.MSG_CLEAR_SCREEN, "");
                    Mediator.Notify(CommonDefs.MSG_START_TIMERS, "");
                    Mediator.Notify(CommonDefs.MSG_LAUNCH, _selectedTakeoff);
                }));
            }
        }

        public TakeOffViewModel()
        {
            _takeOffDescriptors = new ObservableCollection<TakeOffDescriptor>();

            _takeOffDescriptors.Add(new TakeOffDescriptor("eve", 90, 200, 42000, 90000, 100000, 90000));
            _takeOffDescriptors.Add(new TakeOffDescriptor("duna", 90, 100, 1000, 30000, 65000, 50000));
            _takeOffDescriptors.Add(new TakeOffDescriptor("kerbin", 90, 200, 5000, 50000, 100000, 70000));
            _takeOffDescriptors.Add(new TakeOffDescriptor("kerbin_2", 90, 300, 1000, 55000, 75000, 70000));
            _takeOffDescriptors.Add(new TakeOffDescriptor("mun", 90, 30, 500, 15000, 40000, 0));
            _takeOffDescriptors.Add(new TakeOffDescriptor("minmus", 90, 30, 500, 3500, 40000, 0));
            _takeOffDescriptors.Add(new TakeOffDescriptor("ike", 90, 30, 500, 4500, 40000, 0));
            _takeOffDescriptors.Add(new TakeOffDescriptor("gilly", 90, 30, 200, 2000, 20000, 0));
            _takeOffDescriptors.Add(new TakeOffDescriptor("dres", 90, 30, 500, 4500, 20000, 0));
            _takeOffDescriptors.Add(new TakeOffDescriptor("moho", 90, 30, 500, 20000, 40000, 0));
            _takeOffDescriptors.Add(new TakeOffDescriptor("eeloo", 90, 30, 500, 15000, 40000, 0));
            _takeOffDescriptors.Add(new TakeOffDescriptor("pol", 90, 30, 200, 4000, 20000, 0));
            _takeOffDescriptors.Add(new TakeOffDescriptor("bop", 90, 30, 200, 3500, 30000, 0));
        }

        public void SendMessage(string strMessage)
        {
            throw new NotImplementedException();
        }
    }
}
