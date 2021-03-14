using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfApp1.Models;
using WpfApp1.Utils;

namespace WpfApp1.ViewModel
{
    class ManeuverViewModel : BaseViewModel, IPageViewModel
    {
        private string _nodeTimeTo;
        private string _remainingDeltaV;
        private string _startBurn;

        private ICommand _circularize;
        private ICommand _executeManeuver;
        private ICommand _circularizeSelection;
        //private string   _selectedCircularization; //nome da manobra selecionada (mesmo nome do Radio Button da view)
        private bool     _reduceOrbit;

        private delegate void UpdateManeuverLabelCallback(ManeuverData _data);

        public string NodeTimeTo
        {
            get { return _nodeTimeTo; }
            set
            {
                _nodeTimeTo = value;
                OnPropertyChanged(nameof(NodeTimeTo));
            }
        }
        public string RemainingDeltaV
        {
            get { return _remainingDeltaV; }
            set
            {
                _remainingDeltaV = value;
                OnPropertyChanged(nameof(RemainingDeltaV));
            }
        }

        public string StartBurn
        {
            get { return _startBurn; }
            set
            {
                _startBurn = value;
                OnPropertyChanged(nameof(StartBurn));
            }
        }

        public ICommand Circularize
        {
            get
            {
                return _circularize ?? (_circularize = new RelayCommand(x =>
                {                    
                    Mediator.Notify("ClearScreen", "");
                    Mediator.Notify("StartTimers", "");
                    Mediator.Notify("Circularize", _reduceOrbit);
                }));
            }
        }

        public ICommand ExecuteManeuver
        {
            get
            {
                return _executeManeuver ?? (_executeManeuver = new RelayCommand(x =>
                {                    
                    Mediator.Notify("ClearScreen", "");
                    Mediator.Notify("StartTimers", "");
                    Mediator.Notify("ExecuteManeuver", "");
                }));
            }
        }

        public ICommand CircularizeSelection //associado aos Radio Buttons da main window
        {
            get
            {
                return _circularizeSelection ?? (_circularizeSelection = new RelayCommand(x =>
                {
                    switch (x)
                    {
                        case "Reduce Orbit":
                            _reduceOrbit = true;
                            break;
                        case "Enlarge Orbit":
                        default:
                            _reduceOrbit = false;
                            break;
                    }
                    //SelectedCircularization = (string)x;
                }));
            }
        }

        public bool ReduceOrbit
        {
            get { return _reduceOrbit; }
        }

        /*
        public string SelectedCircularization //based upon https://www.c-sharpcorner.com/article/explain-radio-button-binding-in-mvvm-wpf/
        {
            get { return _selectedCircularization; }
            set
            {
                _selectedCircularization = value;
                switch (value)
                {
                    case "rdReduceOrbit":
                        _reduceOrbit = true;
                        break;
                    case "rdEnlargeOrbit":
                    default:
                        _reduceOrbit = false;
                        break;
                }
                OnPropertyChanged(nameof(SelectedCircularization));
            }
        }
        */

        public void SafeUpdateManeuverText(ManeuverData _data)
        {
            App.Current.Dispatcher.Invoke(new UpdateManeuverLabelCallback(this.UpdateManeuverText),
                              new object[] { _data });
        }

        public void SendMessage(string strMessage)
        {
            throw new NotImplementedException();
        }

        //Método chamado atraves de invoke para atualizar a GUI
        private void UpdateManeuverText(ManeuverData _data)
        {
            NodeTimeTo      = String.Format("{0:0.##}", _data.NodeTimeTo);
            RemainingDeltaV = String.Format("{0:0.##}", _data.RemainingDeltaV);
            StartBurn       = String.Format("{0:0.##}", (_data.NodeTimeTo - _data.BurnTime / 2.0d));
        }
    }
}
