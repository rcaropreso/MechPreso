using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public static class CommonDefs
    {
        public static string MSG_CLEAR_SCREEN { get => "CrearScreen"; }
        public static string MSG_SEND_MESSAGE { get => "SendMessage"; }
        public static string MSG_CONNECT { get => "Connect"; }
        public static string MSG_DISCONNECT { get => "Disconnect"; }
        public static string MSG_START_TIMERS { get => "StartTimers"; }
        public static string MSG_STOP_TIMERS { get => "StopTimers"; }
        public static string MSG_LAUNCH { get => "Launch"; }
        public static string MSG_EXECUTE_MANEUVER { get => "ExecuteManeuver"; }
        public static string MSG_CIRCULARIZE { get => "Circularize"; }
        public static string MSG_EXECUTE_SUICIDE_BURN { get => "ExecuteSuicideBurn"; }
        public static string MSG_NONE_SCREEN { get => "NoneScreen"; }
        public static string MSG_TAKEOFF_SCREEN { get => "TakeoffScreen"; }
        public static string MSG_LANDING_SCREEN { get => "LandingScreen"; }
        public static string MSG_EXECUTE_CANCEL_VVEL { get => "CancelVVel"; }
        public static string MSG_EXECUTE_CANCEL_HVEL { get => "CancelHVel"; }
        public static string MSG_EXECUTE_DEORBIT_BODY { get => "DeorbitBody"; }
        public static string MSG_EXECUTE_STOP_BURN { get => "StopBurn"; }
        public static string MSG_EXECUTE_FINE_TUNNING { get => "FineTunning"; }

        public enum WhenStartBurn
        {
            WaitHorizontalAltitude,
            WaitVerticalAltitude,
            Now
        };

        public enum BurnType
        {
            Horizontal,
            Vertical,
            Diagonal,
            Retrograde
        };

        public enum VesselState
        {
            NotStarted,
            Preparation,
            Executing,
            Finished
        };

        public static string AltitudeTypeToString(CommonDefs.WhenStartBurn value)
        {
            switch (value)
            {
                case WhenStartBurn.WaitHorizontalAltitude:
                    return @"WaitHorizontalAltitude";
                case WhenStartBurn.WaitVerticalAltitude:
                    return @"WaitVerticalAltitude";
                case WhenStartBurn.Now:
                    return @"Now";
                default:
                    return string.Empty;
            }
        }

        public static string BurnTypeToString(BurnType value)
        {
            switch (value)
            {
                case BurnType.Horizontal:
                    return @"Horizontal";
                case BurnType.Vertical:
                    return @"Vertical";
                case BurnType.Diagonal:
                    return @"Diagonal";
                case BurnType.Retrograde:
                    return @"Retrograde";
                default:
                    return string.Empty;
            }
        }
    }
}
