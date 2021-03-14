using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfApp1.Utils;

namespace WpfApp1.ViewModel
{
    class SuicideBurnViewModel : BaseViewModel, IPageViewModel
    {
        private string _verticalBurnStart;
        private string _horizontalBurnStart;
        private string _deorbitAltitude;
        private bool _deorbitBody;
        private bool _cancelVVel;
        private bool _cancelHVel;
        private bool _stopBurn;
        private bool _fineTunning;
        private float _minVVel;
        private float _minHVel;

        private ICommand _executeSB;

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
        public bool DeorbitBody
        {
            get { return _deorbitBody; }
            set
            {
                _deorbitBody = value;
                OnPropertyChanged(nameof(DeorbitBody));
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

        public bool CancelVVel
        {
            get { return _cancelVVel; }
            set
            {
                _cancelVVel = value;
                OnPropertyChanged(nameof(CancelVVel));
            }
        }
        public bool CancelHVel
        {
            get { return _cancelHVel; }
            set
            {
                _cancelHVel = value;
                OnPropertyChanged(nameof(CancelHVel));
            }
        }
        public bool StopBurn
        {
            get { return _stopBurn; }
            set
            {
                _stopBurn = value;
                OnPropertyChanged(nameof(StopBurn));
            }
        }
        public bool FineTunning
        {
            get { return _fineTunning; }
            set
            {
                _fineTunning = value;
                OnPropertyChanged(nameof(FineTunning));
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
                        DoCancelVerticalVelocity   = CancelVVel,
                        DoCancelHorizontalVelocity = CancelHVel,
                        DoDeorbitBody              = DeorbitBody,
                        DoFineTunning              = FineTunning,
                        DoStopBurn                 = StopBurn,
                        MinVerticalVelocity        = this.MinVerticalVelocity,
                        MinHorizontalVelocity      = this.MinHorizontalVelocity,
                    };

                    Mediator.Notify("ClearScreen", "");
                    Mediator.Notify("StartTimers", "");
                    Mediator.Notify("ExecuteSuicideBurn", _sbSetup);
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
        }
    }
}
