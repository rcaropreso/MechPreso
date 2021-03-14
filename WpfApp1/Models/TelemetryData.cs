using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class TelemetryData
    {
        public double VesselHeading;
        public double VesselPitch;
        public double SurfaceAltitude;
        public double SrbFuel;
        public double ApoapsisAltitude;
        public double TerminalVelocity;

        public double CurrentSpeed;
        public double VerticalSpeed;
        public double HorizontalSpeed;
        public double EngineAcc;
        public double Gravity;

        public TelemetryData(double _vesselHeading, double _vesselPitch, double _surfaceAltitude, double _srbFuel,
            double _apoapsisAltitude, double _terminalVelocity, double _currentSpeed, double _verticalSpeed, 
            double _horizontalSpeed, double _engineAcc, double _gravity)
        {
            VesselHeading = _vesselHeading;
            VesselPitch = _vesselPitch;
            SurfaceAltitude = _surfaceAltitude;
            SrbFuel = _srbFuel;
            ApoapsisAltitude = _apoapsisAltitude;
            TerminalVelocity = _terminalVelocity;

            CurrentSpeed = _currentSpeed;
            VerticalSpeed = _verticalSpeed;
            HorizontalSpeed = _horizontalSpeed;
            EngineAcc = _engineAcc;
            Gravity = _gravity;
        }
    }
}
