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

        private float _deorbitAltitude;        
        private float _minVVel;
        private float _minHVel;
        private float _safetyMargin;

        /*
         */
        private bool _deorbitBody;
        private bool _cancelVVel;
        private bool _cancelHVel;
        private bool _stopBurn;
        private bool _finalBurn;

        private ICommand _executeSB;
        
        private delegate void UpdateSuicideBurnLabelCallback(SuicideBurnData data);

        public SuicideBurnViewModel()
        {
            //Valores default
            DeorbitBody = true;
            CancelVVel = true;
            CancelHVel = true;
            StopBurn = true;
            FinalBurn = true;

            DeorbitAltitude = 0.0f;
            MinVerticalVelocity = 10.0f;
            MinHorizontalVelocity = 5.0f;
            SafetyMargin = 0.10f;
        }

        #region CheckBoxes
        public bool DeorbitBody
        {
            get { return _deorbitBody; }
            set
            {
                _deorbitBody = value;
                OnPropertyChanged(nameof(DeorbitBody));
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

        public bool FinalBurn
        {
            get { return _finalBurn; }
            set
            {
                _finalBurn = value;
                OnPropertyChanged(nameof(FinalBurn));
            }
        }
        #endregion

        #region Labels
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
        #endregion

        #region Parameters
        public float DeorbitAltitude
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
        #endregion

        public ICommand ExecuteSB
        {
            get
            {
                return _executeSB ?? (_executeSB = new RelayCommand(x =>
                {
                    SuicideBurnSetup _sbSetup = new SuicideBurnSetup
                    {
                        DeorbitTargetAltitude = DeorbitAltitude,
                        MinVerticalVelocity = MinVerticalVelocity == 0.0f ? 10.0f : MinVerticalVelocity,
                        MinHorizontalVelocity = MinHorizontalVelocity == 0.0f ? 5.0f : MinHorizontalVelocity,
                        SafetyMargin = SafetyMargin == 0.0f ? 0.10f : SafetyMargin, //nao dá pra colocar zero, perigoso

                        DeorbitBody = this.DeorbitBody,
                        CancelVVel  = this.CancelVVel,
                        CancelHVel = this.CancelHVel,
                        StopBurn = this.StopBurn,
                        FinalBurn = this.FinalBurn,
                    };

                    Mediator.Notify(CommonDefs.MSG_CLEAR_SCREEN, "");
                    //Mediator.Notify(CommonDefs.MSG_START_TIMERS, "");
                    Mediator.Notify(CommonDefs.MSG_EXECUTE_SUICIDE_BURN, _sbSetup);
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
