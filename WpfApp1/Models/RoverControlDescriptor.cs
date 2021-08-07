using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class RoverControlDescriptor
    {
        public string WaypointName { get; set; }
        public float MaxSpeed { get; set; }
        public float SteeringSpeed { get; set; }
        public float MinTargetDistance { get; set; }
        public float MaxAngleDiff { get; set; }

        public RoverControlDescriptor()
        {

        }

        public RoverControlDescriptor(string waypointName, float maxSpeed, float steeringSpeed, float minTargetDistance, float maxAngleDiff)
        {
            WaypointName = waypointName;
            MaxSpeed = maxSpeed;
            SteeringSpeed = steeringSpeed;
            MinTargetDistance = minTargetDistance;
            MaxAngleDiff = maxAngleDiff;
        }
    }

    public class RoverData
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }

        public RoverData()
        {

        }

        public RoverData(float latitude, float longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }

    public class Coordinates
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }

        public Coordinates()
        {

        }

        public Coordinates(float latitude, float longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
