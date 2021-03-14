using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    /// <summary>
    /// This class describes the takeoff parameters
    /// </summary>
    public class TakeOffDescriptor
    {
        public string Name { get; set; }
        public float ShipHeadingAngle { get; set; } //the flight angle, 90 degress means equatorial trajectory
        public float InitialRotationAltitude { get; set; }
        public float StartTurnAltitude { get; set; }
        public float EndTurnAltitude { get; set; }
        public float TargetAltitude { get; set; }
        public float AtmosphereAltitude { get; set; }       
        public int SRBStage { get; set; }
        public override string ToString()
        {
            return Name;
        }

        public TakeOffDescriptor(string name, float ship_heading_angle, float initial_rotation_altitude, 
                                 float start_turn_altitude, float end_turn_altitude, 
                                 float target_altitude, float atmosphere_altitude, int srb_stage=-1)
        {
            Name = name;
            ShipHeadingAngle        = ship_heading_angle;
            InitialRotationAltitude = initial_rotation_altitude;
            StartTurnAltitude       = start_turn_altitude;
            EndTurnAltitude         = end_turn_altitude;
            TargetAltitude          = target_altitude;
            AtmosphereAltitude      = atmosphere_altitude;
            SRBStage                = srb_stage;
        }
    }    
}