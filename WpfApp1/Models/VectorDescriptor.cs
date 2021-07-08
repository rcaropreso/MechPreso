using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vector3 = System.Tuple<double, double, double>;

namespace WpfApp1.Models
{
    public static class VectorDescriptor
    {
        public static Vector3 CrossProduct(Vector3 u, Vector3 v)
        {
            return new Vector3(
                u.Item2 * v.Item3 - u.Item3 * v.Item2,
                u.Item3 * v.Item1 - u.Item1 * v.Item3,
                u.Item1 * v.Item2 - u.Item2 * v.Item1
            );
        }

        public static double DotProduct(Vector3 u, Vector3 v)
        {
            return u.Item1 * v.Item1 + u.Item2 * v.Item2 + u.Item3 * v.Item3;
        }

        public static double Magnitude(Vector3 v)
        {
            return Math.Sqrt(DotProduct(v, v));
        }

        // Compute the angle between vector x and y
        public static double AngleBetweenVectors(Vector3 u, Vector3 v)
        {
            double dp = DotProduct(u, v);
            if (dp == 0)
                return 0;
            double um = Magnitude(u);
            double vm = Magnitude(v);
            return Math.Acos(dp / (um * vm)) * (180f / Math.PI);
        }
    }
}
