using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class ManeuverData
    {
        public double NodeTimeTo;
        public double RemainingDeltaV;
        public double BurnTime;

        public ManeuverData(double _nodeTimeTo, double _remainingDeltaV, double _burnTime)
        {
            NodeTimeTo      = _nodeTimeTo;
            RemainingDeltaV = _remainingDeltaV;
            BurnTime        = _burnTime;
        }
    }
}
