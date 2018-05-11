using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Gravity_Sim
{
    static class RNG
    {
        private static int seed;
        public static int Seed
        {
            get
            {
                return seed;
            }
            set
            {
                seed = value;
                rand = new Random(seed); // Update rand
            }
        }
        private static Random rand = new Random();

        /// <summary>
        /// Return a uniformly distributed random number between a and b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Uniform(float a, float b)
        {
            // Transform a random float from U[0,1] into a float from U[a, b]
            float x = (float)rand.NextDouble();
            return (b - a) * x + a;
        }

        public static float Uniform(float a)
        {
            return Uniform(-a, a);
        }

        /// <summary>
        /// Return a normally distributed random variable using the 
        /// Box-Muller tranform.
        /// </summary>
        /// <param name="mean">Mean</param>
        /// <param name="stddev">Standard deviation</param>
        /// <returns></returns>
        public static float Normal(float mean, float stddev)
        {
            // Two independent
            double u1 = rand.NextDouble();
            double u2 = rand.NextDouble();

            // Transform to a standard normal distributed variable
            // Only need one number from Box-Muller (which provides two)
            double z = Math.Sqrt(- 2 * Math.Log(u1)) * Math.Sin(2 * Math.PI * u2);
            
            // Transform from standard normal to parameterised normal 
            return (float)z * stddev + mean;
        }
    }
}
