using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfApp1.Utils;
using WpfApp1.Models;

namespace WpfApp1.ViewModel
{
    class SuicideBurnViewModel : BaseViewModel, IPageViewModel
    {
        private string _verticalBurnStart;
        private string _horizontalBurnStart;
        private string _highestPeak;
        private string _deorbitAltitude;
        
        private float _minVVel;
        private float _minHVel;
        private float _safetyMargin;

        private ICommand _executeSB;
        private ICommand _cancelVVel;
        private ICommand _cancelHVel;
        private ICommand _deorbitBody;
        private ICommand _stopBurn;
        private ICommand _fineTunning;

        private delegate void UpdateSuicideBurnLabelCallback(SuicideBurnData data);

        public string VerticalBurnStart
        {
            get { return _verticalBurnStart; }
            set
            {
                _verticalBurnStart = value;
                OnPropertyChanged(nameof(VerticalBurnStart));
            }
        }
        public string HorizontalBurnStart
        {
            get { return _horizontalBurnStart; }
            set
            {
                _horizontalBurnStart = value;
                OnPropertyChanged(nameof(HorizontalBurnStart));
            }
        }

        public string HighestPeak
        {
            get { return _highestPeak; }
            set
            {
                _highestPeak = value;
                OnPropertyChanged(nameof(HighestPeak));
            }
        }

        public string DeorbitAltitude
        {
            get { return _deorbitAltitude; }
            set
            {
                _deorbitAltitude = value;
                OnPropertyChanged(nameof(DeorbitAltitude));
            }
        }

        public float MinVerticalVelocity
        {
            get { return _minVVel; }
            set
            {
                _minVVel = value;
                OnPropertyChanged(nameof(MinVerticalVelocity));
            }
        }

        public float MinHorizontalVelocity
        {
            get { return _minHVel; }
            set
            {
                _minHVel = value;
                OnPropertyChanged(nameof(MinHorizontalVelocity));
            }
        }

        public float SafetyMargin
        {
            get { return _safetyMargin; }
            set
            {
                _safetyMargin = value;
                OnPropertyChanged(nameof(SafetyMargin));
            }
        }

        public ICommand ExecuteSB
        {
            get
            {
                return _executeSB ?? (_executeSB = new RelayCommand(x =>
                {
                    SuicideBurnSetup _sbSetup = new SuicideBurnSetup
                    {
                        DeorbitTargetAltitude      = float.Parse(DeorbitAltitude ?? "0"),
                        MinVerticalVelocity        = MinVerticalVelocity == 0.0f? 10.0f : MinVerticalVelocity,
                        MinHorizontalVelocity      = MinHorizontalVelocity == 0.0f ? 5.0f : MinHorizontalVelocity,
                        SafetyMargin               = SafetyMargin == 0.0f ? 0.10f : SafetyMargin, //nao dá pra colocar zero, perigoso
                    };

                    Mediator.Notify(CommonDefs.MSG_CLEAR_SCREEN, "");
                    Mediator.Notify(CommonDefs.MSG_START_TIMERS, "");
                    Mediator.Notify(CommonDefs.MSG_EXECUTE_SUICIDE_BURN, _sbSetup);
                }));
            }
        }

        public ICommand DeorbitBody
        {
            get
            {
                return _deorbitBody ?? (_deorbitBody = new RelayCommand(x =>
                {
                    SuicideBurnSetup _sbSetup = new SuicideBurnSetup
                    {
                        DeorbitTargetAltitude = float.Parse(DeorbitAltitude ?? "0"),
                        MinVerticalVelocity = MinVerticalVelocity == 0.0f ? 10.0f : MinVerticalVelocity,
                        MinHorizontalVelocity = MinHorizontalVelocity == 0.0f ? 5.0f : MinHorizontalVelocity,
                        SafetyMargin = SafetyMargin == 0.0f ? 0.10f : SafetyMargin, //nao dá pra colocar zero, perigoso
                    };

                    Mediator.Notify(CommonDefs.MSG_CLEAR_SCREEN, "");
                    Mediator.Notify(CommonDefs.MSG_START_TIMERS, "");
                    Mediator.Notify(CommonDefs.MSG_EXECUTE_DEORBIT_BODY, _sbSetup);
                }));
            }
        }

        public ICommand CancelVVel
        {
            get
            {
                return _cancelVVel ?? (_cancelVVel = new RelayCommand(x =>
                {
                    SuicideBurnSetup _sbSetup = new SuicideBurnSetup
                    {
                        DeorbitTargetAltitude = float.Parse(DeorbitAltitude ?? "0"),
                        MinVerticalVelocity = MinVerticalVelocity == 0.0f ? 10.0f : MinVerticalVelocity,
                        MinHorizontalVelocity = MinHorizontalVelocity == 0.0f ? 5.0f : MinHorizontalVelocity,
                        SafetyMargin = SafetyMargin == 0.0f ? 0.10f : SafetyMargin, //nao dá pra colocar zero, perigoso
                    };

                    Mediator.Notify(CommonDefs.MSG_CLEAR_SCREEN, "");
                    Mediator.Notify(CommonDefs.MSG_START_TIMERS, "");
                    Mediator.Notify(CommonDefs.MSG_EXECUTE_CANCEL_VVEL, _sbSetup);
                }));
            }
        }

        public ICommand CancelHVel
        {
            get
            {
                return _cancelHVel ?? (_cancelHVel = new RelayCommand(x =>
                {
                    SuicideBurnSetup _sbSetup = new SuicideBurnSetup
                    {
                        DeorbitTargetAltitude = float.Parse(DeorbitAltitude ?? "0"),
                        MinVerticalVelocity = MinVerticalVelocity == 0.0f ? 10.0f : MinVerticalVelocity,
                        MinHorizontalVelocity = MinHorizontalVelocity == 0.0f ? 5.0f : MinHorizontalVelocity,
                        SafetyMargin = SafetyMargin == 0.0f ? 0.10f : SafetyMargin, //nao dá pra colocar zero, perigoso
                    };

                    Mediator.Notify(CommonDefs.MSG_CLEAR_SCREEN, "");
                    Mediator.Notify(CommonDefs.MSG_START_TIMERS, "");
                    Mediator.Notify(CommonDefs.MSG_EXECUTE_CANCEL_HVEL, _sbSetup);
                }));
            }
        }

        public ICommand StopBurn
        {
            get
            {
                return _stopBurn ?? (_stopBurn = new RelayCommand(x =>
                {
                    SuicideBurnSetup _sbSetup = new SuicideBurnSetup
                    {
                        DeorbitTargetAltitude = float.Parse(DeorbitAltitude ?? "0"),
                        MinVerticalVelocity = MinVerticalVelocity == 0.0f ? 10.0f : MinVerticalVelocity,
                        MinHorizontalVelocity = MinHorizontalVelocity == 0.0f ? 5.0f : MinHorizontalVelocity,
                        SafetyMargin = SafetyMargin == 0.0f ? 0.10f : SafetyMargin, //nao dá pra colocar zero, perigoso
                    };

                    Mediator.Notify(CommonDefs.MSG_CLEAR_SCREEN, "");
                    Mediator.Notify(CommonDefs.MSG_START_TIMERS, "");
                    Mediator.Notify(CommonDefs.MSG_EXECUTE_STOP_BURN, _sbSetup);
                }));
            }
        }

        public ICommand FineTunning
        {
            get
            {
                return _fineTunning ?? (_fineTunning = new RelayCommand(x =>
                {
                    SuicideBurnSetup _sbSetup = new SuicideBurnSetup
                    {
                        DeorbitTargetAltitude = float.Parse(DeorbitAltitude ?? "0"),
                        MinVerticalVelocity = MinVerticalVelocity == 0.0f ? 10.0f : MinVerticalVelocity,
                        MinHorizontalVelocity = MinHorizontalVelocity == 0.0f ? 5.0f : MinHorizontalVelocity,
                        SafetyMargin = SafetyMargin == 0.0f ? 0.10f : SafetyMargin, //nao dá pra colocar zero, perigoso
                    };

                    Mediator.Notify(CommonDefs.MSG_CLEAR_SCREEN, "");
                    Mediator.Notify(CommonDefs.MSG_START_TIMERS, "");
                    Mediator.Notify(CommonDefs.MSG_EXECUTE_FINE_TUNNING, _sbSetup);
                }));
            }
        }

        public void SendMessage(string strMessage)
        {
            throw new NotImplementedException();
        }
        public void SafeUpdateSuicideBurnText(SuicideBurnData _data)
        {
            App.Current.Dispatcher.Invoke(new UpdateSuicideBurnLabelCallback(this.UpdateSuicideBurnText),
                              new object[] { _data });
        }
        private void UpdateSuicideBurnText(SuicideBurnData _data)
        {
            VerticalBurnStart   = String.Format("{0:0.##}", _data.VerticalBurnStartAltitude);
            HorizontalBurnStart = String.Format("{0:0.##}", _data.HorizontalBurnStartAltitude);
            HighestPeak         = String.Format("{0:0.##}", _data.HighestPeak);
        }
    }
}
