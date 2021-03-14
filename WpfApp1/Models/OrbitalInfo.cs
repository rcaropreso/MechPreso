using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class OrbitalInfo
    {
        public static List<TakeOffDescriptor> TakeOffDescriptors = new List<TakeOffDescriptor>();
        static OrbitalInfo()
        {
            TakeOffDescriptors.Add(new TakeOffDescriptor("eve", 90, 200, 42000, 75000, 100000, 90000));
            TakeOffDescriptors.Add(new TakeOffDescriptor("duna",     90, 100, 1000, 30000, 65000, 50000));
            TakeOffDescriptors.Add(new TakeOffDescriptor("kerbin",   90, 200, 5000, 50000, 100000, 70000));
            TakeOffDescriptors.Add(new TakeOffDescriptor("kerbin_2", 90, 300, 1000, 55000, 75000, 70000));
            TakeOffDescriptors.Add(new TakeOffDescriptor("mun",      90, 30, 500, 15000, 40000, 0));
            TakeOffDescriptors.Add(new TakeOffDescriptor("minmus",   90, 30, 500, 3500, 40000, 0));
            TakeOffDescriptors.Add(new TakeOffDescriptor("ike",      90, 30, 500, 4500, 40000, 0));
            TakeOffDescriptors.Add(new TakeOffDescriptor("gilly",    90, 30, 200, 2000, 20000, 0));
            TakeOffDescriptors.Add(new TakeOffDescriptor("dres",     90, 30, 500, 4500, 20000, 0));
            TakeOffDescriptors.Add(new TakeOffDescriptor("moho",     90, 30, 500, 20000, 40000, 0));
            TakeOffDescriptors.Add(new TakeOffDescriptor("eeloo",    90, 30, 500, 15000, 40000, 0));
            TakeOffDescriptors.Add(new TakeOffDescriptor("pol",      90, 30, 200, 4000, 20000, 0));
            TakeOffDescriptors.Add(new TakeOffDescriptor("bop",      90, 30, 200, 3500, 30000, 0));
        }

        public static TakeOffDescriptor GetOrbitalInfo(string name)
        {
            return TakeOffDescriptors.Find(x => x.Name.ToUpper().Equals(name.ToUpper()));
        }
    }
}
