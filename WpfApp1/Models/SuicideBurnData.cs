using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    /// <summary>
    /// This class describes suicide burn flight telemetry parameters
    /// </summary>
    public class SuicideBurnData
    {
        public double CurrentSpeed { get; set; }
        public double EngineAcceleration { get; set; }
        public double VerticalVelocity { get; set; }
        public double HorizontalVelocity { get; set; }
        public double SurfaceAltitude { get; set; }
        public double Theta { get; set; }
        public double VerticalBurnStartAltitude { get; set; }
        public double HorizontalBurnStartAltitude { get; set; }
        public double Gravity { get; set; }

        public SuicideBurnData(double _currentSpeed, double _engineAcc, double _verticalSpeed, 
            double _horizontalSpeed, double _surfaceAltitude, double _theta, double _verticalStartAltitude,
            double _horizontalStartAltitude, double _gravity)
        {
            CurrentSpeed = _currentSpeed;
            EngineAcceleration = _engineAcc;
            VerticalVelocity = _verticalSpeed;
            HorizontalVelocity = _horizontalSpeed;
            SurfaceAltitude = _surfaceAltitude;
            Theta = _theta;
            VerticalBurnStartAltitude = _verticalStartAltitude;
            HorizontalBurnStartAltitude = _horizontalStartAltitude;
            Gravity = _gravity;
        }
    }

    public class SuicideBurnSetup
    {
        public float DeorbitTargetAltitude;
        public bool DoDeorbitBody;
        public bool DoCancelVerticalVelocity;
        public bool DoCancelHorizontalVelocity;
        public bool DoStopBurn;
        public bool DoFineTunning;
        public float MinVerticalVelocity;
        public float MinHorizontalVelocity;
    }
}
