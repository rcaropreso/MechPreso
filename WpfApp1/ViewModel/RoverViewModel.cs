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
    class RoverViewModel : BaseViewModel, IPageViewModel
    {
        private string _latitude;
        private string _longitude;
        private string _waypointName;

        private float _maxSpeed;        
        private float _steeringSpeed;
        private float _minTargetDist;
        private float _maxAngleDiff;

        private ICommand _executeGoRover;
        
        private delegate void UpdateRoverLabelCallback(RoverData _data);

        public RoverViewModel()
        {
            //Valores default
            WaypointName = "Waypoint1";
            MaxSpeed = 10.0f;
            SteeringSpeed = 3.0f;
            MinTargetDistance = 50.0f;
            MaxAngleDiff = 5.0f;
        }

        #region Labels
        public string Latitude
        {
            get { return _latitude; }
            set
            {
                _latitude = value;
                OnPropertyChanged(nameof(Latitude));
            }
        }

        public string Longitude
        {
            get { return _longitude; }
            set
            {
                _longitude = value;
                OnPropertyChanged(nameof(Longitude));
            }
        }
        #endregion

        #region Parameters
        public string WaypointName
        {
            get { return _waypointName; }
            set
            {
                _waypointName = value;
                OnPropertyChanged(nameof(WaypointName));
            }
        }

        public float MaxSpeed
        {
            get { return _maxSpeed; }
            set
            {
                _maxSpeed = value;
                OnPropertyChanged(nameof(MaxSpeed));
            }
        }

        public float SteeringSpeed
        {
            get { return _steeringSpeed; }
            set
            {
                _steeringSpeed = value;
                OnPropertyChanged(nameof(SteeringSpeed));
            }
        }

        public float MinTargetDistance
        {
            get { return _minTargetDist; }
            set
            {
                _minTargetDist = value;
                OnPropertyChanged(nameof(MinTargetDistance));
            }
        }

        public float MaxAngleDiff
        {
            get { return _maxAngleDiff; }
            set
            {
                _maxAngleDiff = value;
                OnPropertyChanged(nameof(MaxAngleDiff));
            }
        }
        #endregion

        public ICommand ExecuteGoRover
        {
            get
            {
                return _executeGoRover ?? (_executeGoRover = new RelayCommand(x =>
                {
                    RoverControlDescriptor _roverSetup = new RoverControlDescriptor
                    {
                        WaypointName = this.WaypointName,
                        MaxSpeed = this.MaxSpeed,
                        SteeringSpeed = this.SteeringSpeed,
                        MinTargetDistance = this.MinTargetDistance,
                        MaxAngleDiff = this.MaxAngleDiff,
                    };

                    Mediator.Notify(CommonDefs.MSG_CLEAR_SCREEN, "");
                    //Mediator.Notify(CommonDefs.MSG_START_TIMERS, "");
                    Mediator.Notify(CommonDefs.MSG_EXECUTE_GO_ROVER, _roverSetup);
                }));
            }
        }
       
        public void SendMessage(string strMessage)
        {
            throw new NotImplementedException();
        }

        public void SafeUpdateRoverText(RoverData _data)
        {
            App.Current.Dispatcher.Invoke(new UpdateRoverLabelCallback(this.UpdateRoverText),
                              new object[] { _data });
        }

        private void UpdateRoverText(RoverData _data)
        {
            Latitude   = String.Format("{0:0.##}", _data.Latitude);
            Longitude  = String.Format("{0:0.##}", _data.Longitude);
        }
    }
}
