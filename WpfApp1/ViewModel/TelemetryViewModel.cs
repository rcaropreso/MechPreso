using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Models;

namespace WpfApp1.ViewModel
{
    class TelemetryViewModel : BaseViewModel, IPageViewModel
    {
        private string _vesselHeading;
        private string _vesselPitch;
        private string _surfaceAltitude;
        private string _srbFuel;
        private string _apoapsisAltitude;
        private string _terminalVelocity;
        private string _currentSpeed;
        private string _verticalSpeed;
        private string _horizontalSpeed;
        private string _engineAcc;
        private string _gravity;

        private delegate void UpdateTelemetryTextCallback(TelemetryData _data);

        public string VesselHeading
        {
            get { return _vesselHeading; }
            set
            {
                _vesselHeading = value;
                OnPropertyChanged(nameof(VesselHeading));
            }
        }

        public string VesselPitch
        {
            get { return _vesselPitch; }
            set
            {
                _vesselPitch = value;
                OnPropertyChanged(nameof(VesselPitch));
            }
        }

        public string SurfaceAltitude
        {
            get { return _surfaceAltitude; }
            set
            {
                _surfaceAltitude = value;
                OnPropertyChanged(nameof(SurfaceAltitude));
            }
        }

        public string SrbFuel
        {
            get { return _srbFuel; }
            set
            {
                _srbFuel = value;
                OnPropertyChanged(nameof(SrbFuel));
            }
        }
        public string TerminalVelocity
        {
            get { return _terminalVelocity; }
            set
            {
                _terminalVelocity = value;
                OnPropertyChanged(nameof(TerminalVelocity));
            }
        }

        public string ApoapsisAltitude
        {
            get { return _apoapsisAltitude; }
            set
            {
                _apoapsisAltitude = value;
                OnPropertyChanged(nameof(ApoapsisAltitude));
            }
        }
        public string CurrentSpeed
        {
            get { return _currentSpeed; }
            set
            {
                _currentSpeed = value;
                OnPropertyChanged(nameof(CurrentSpeed));
            }
        }
        public string VerticalSpeed
        {
            get { return _verticalSpeed; }
            set
            {
                _verticalSpeed = value;
                OnPropertyChanged(nameof(VerticalSpeed));
            }
        }
        public string HorizontalSpeed
        {
            get { return _horizontalSpeed; }
            set
            {
                _horizontalSpeed = value;
                OnPropertyChanged(nameof(HorizontalSpeed));
            }
        }
        public string EngineAcc
        {
            get { return _engineAcc; }
            set
            {
                _engineAcc = value;
                OnPropertyChanged(nameof(EngineAcc));
            }
        }
        public string Gravity
        {
            get { return _gravity; }
            set
            {
                _gravity = value;
                OnPropertyChanged(nameof(Gravity));
            }
        }

        public void SafeUpdateTelemetryText(TelemetryData _data)
        {
            App.Current.Dispatcher.Invoke(new UpdateTelemetryTextCallback(this.UpdateTelemetryText),
                              new object[] { _data });
        }

        public void SendMessage(string strMessage)
        {
            throw new NotImplementedException();
        }

        //Método chamado atraves de invoke para atualizar a GUI
        private void UpdateTelemetryText(TelemetryData _data)
        {
            VesselHeading    = String.Format("{0:0.##}", _data.VesselHeading);
            VesselPitch      = String.Format("{0:0.##}", _data.VesselPitch);
            SurfaceAltitude  = String.Format("{0:0.##}", _data.SurfaceAltitude);
            SrbFuel          = String.Format("{0:0.##}", _data.SrbFuel);
            TerminalVelocity = String.Format("{0:0.##}", _data.TerminalVelocity);
            ApoapsisAltitude = String.Format("{0:0.##}", _data.ApoapsisAltitude);
            CurrentSpeed     = String.Format("{0:0.##}", _data.CurrentSpeed);
            VerticalSpeed    = String.Format("{0:0.##}", _data.VerticalSpeed);
            HorizontalSpeed  = String.Format("{0:0.##}", _data.HorizontalSpeed);
            EngineAcc        = String.Format("{0:0.##}", _data.EngineAcc);
            Gravity          = String.Format("{0:0.##}", _data.Gravity);
        }
    }
}