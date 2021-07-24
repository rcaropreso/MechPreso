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
        public double HighestPeak { get; set; }
        public double Gravity { get; set; }

        public SuicideBurnData(double _currentSpeed, double _engineAcc, double _verticalSpeed, 
            double _horizontalSpeed, double _surfaceAltitude, double _theta, double _verticalStartAltitude,
            double _horizontalStartAltitude, double _highestPeak, double _gravity)
        {
            CurrentSpeed = _currentSpeed;
            EngineAcceleration = _engineAcc;
            VerticalVelocity = _verticalSpeed;
            HorizontalVelocity = _horizontalSpeed;
            SurfaceAltitude = _surfaceAltitude;
            Theta = _theta;
            VerticalBurnStartAltitude = _verticalStartAltitude;
            HorizontalBurnStartAltitude = _horizontalStartAltitude;
            HighestPeak = _highestPeak;
            Gravity = _gravity;
        }
    }

    public class SuicideBurnSetup
    {
        public float DeorbitTargetAltitude;
        public float MinVerticalVelocity = 10.0f;
        public float MinHorizontalVelocity = 5.0f;
        public float SafetyMargin = 0.10f;

        public bool DeorbitBody = true; //cancela orbita
        public bool CancelVVel = true;  //cancela velocidade vertical
        public bool CancelHVel = true;  //cancela velocidade horizontal (hover)
        public bool StopBurn = true;    //Executa parada a certa altitude (reduz a 30m/s)
        public bool FinalBurn = true; //Ajustes finais de pouso a partir de 300m 
    }

    public class PlanetData
    {
        public static readonly Dictionary<string, PlanetDescriptor> DateFromBody = new Dictionary<string, PlanetDescriptor>()
        {
            {PlanetName.MOHO, new PlanetDescriptor(PlanetName.MOHO, 6817) },
            {PlanetName.EVE, new PlanetDescriptor(PlanetName.EVE, 7540) },
            {PlanetName.GILLY, new PlanetDescriptor(PlanetName.GILLY, 500) },
            {PlanetName.KERBIN, new PlanetDescriptor(PlanetName.KERBIN, 6764) },
            {PlanetName.MUN, new PlanetDescriptor(PlanetName.MUN, 7061) },
            {PlanetName.MINMUS, new PlanetDescriptor(PlanetName.MINMUS, 5724) },
            {PlanetName.DUNA, new PlanetDescriptor(PlanetName.DUNA, 8264) },
            {PlanetName.IKE, new PlanetDescriptor(PlanetName.IKE, 12750) },
            {PlanetName.DRES, new PlanetDescriptor(PlanetName.DRES, 5700) },
            { PlanetName.JOOL, new PlanetDescriptor(PlanetName.JOOL, 99999) },
            {PlanetName.LAYTHE, new PlanetDescriptor(PlanetName.LAYTHE, 6079) },
            {PlanetName.VALL, new PlanetDescriptor(PlanetName.VALL, 7985) },
            {PlanetName.TYLO, new PlanetDescriptor(PlanetName.TYLO, 12904) },
            {PlanetName.BOP, new PlanetDescriptor(PlanetName.BOP, 21757) },
            {PlanetName.POL, new PlanetDescriptor(PlanetName.JOOL, 4891) },
            {PlanetName.EELOO, new PlanetDescriptor(PlanetName.EELOO, 3900) }
        };
    }

    public class PlanetDescriptor
    {
        public string Name { get; set; }
        public int MaxHeight { get; set; } //maximum altitude in planet (mountain peak)

        public PlanetDescriptor(string name, int maxHeight)
        {
            Name = name;
            MaxHeight = maxHeight;
        }
    }

    public static class PlanetName 
    {
        public static string MOHO = "moho";
        public static string EVE = "eve";
        public static string GILLY = "gilly";

        public static string KERBIN = "kerbin";
        public static string MUN = "mun";
        public static string MINMUS = "minmus";

        public static string DUNA = "duna";
        public static string IKE = "ike";

        public static string DRES = "dres";

        public static string JOOL = "jool";

        public static string LAYTHE = "laythe";
        public static string VALL = "vall";
        public static string TYLO = "tylo";
        public static string BOP = "bop";
        public static string POL = "pol";

        public static string EELOO = "eeloo";
    }
}
