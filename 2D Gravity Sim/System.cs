using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;


namespace _2D_Gravity_Sim
{
    static class System
    {
        const float GravConst = 1;

        public static List<Body> Bodies { get; private set; }

        public static void Initialise(int maxBodies)
        {
            Bodies = new List<Body>();

            for (uint i = 0; i < maxBodies; i++)
            {
                float maxPos = 100.0f;
                float maxMom = 50;

                Vector2 r = new Vector2(RNG.Normal(0, maxPos), RNG.Normal(0, maxPos));
                // Vector2 r = new Vector2(RNG.Uniform(maxPos), RNG.Uniform(maxPos));
                Vector2 p = new Vector2(RNG.Uniform(maxMom), RNG.Uniform(maxMom));
                float m;
                do
                {
                    m = RNG.Uniform(10, 100);
                } while (m < 0);
                Body newBody = new Body(i, m, r, p);
                Bodies.Add(newBody);
            }
            return;
        }

        // Initialise without angular momentum
        public static void Initialise(int maxBodies, float minMass, float maxMass, float maxPos, float maxMom)
        {
            Bodies = new List<Body>();

            for (uint i = 0; i < maxBodies; i++)
            {
                // Random mass
                float m = RNG.Uniform(minMass, maxMass);

                // Pick random position using polar coordinates
                float R = RNG.Uniform(0, maxPos);
                float thetaR = RNG.Uniform(0, 2 * (float)Math.PI);
                Vector2 r = new Vector2(R * (float)Math.Cos(thetaR), R * (float)Math.Sin(thetaR));

                // Pick random momentum
                float P = Math.Abs(RNG.Normal(0, maxMom)); 
                float thetaP = RNG.Uniform(0, 2* (float)Math.PI);
                Vector2 p = new Vector2(P * (float)Math.Cos(thetaP), P * (float)Math.Sin(thetaP));

                Body newBody = new Body(i, m, r, p);
                Bodies.Add(newBody);
            }
            return;
        }

        // Initialise with angular momentum
        public static void Initialise(int maxBodies, float minMass, float maxMass, float maxPos, float maxMom, float angMomMean, float angMomStdDev)
        {
            Bodies = new List<Body>();

            for (uint i = 0; i < maxBodies; i++)
            {
                // Random mass
                float m = RNG.Uniform(minMass, maxMass);

                // Pick random position using polar coordinates
                float R = RNG.Uniform(0, maxPos);
                float thetaR = RNG.Uniform(0, 2 * (float)Math.PI);

                Vector2 r = new Vector2(R * (float)Math.Cos(thetaR), R * (float)Math.Sin(thetaR));

                float L = RNG.Normal(angMomMean, angMomStdDev); // Random angular momentum according to distribution
                float P;

                // 
                if (Math.Abs(L) / R > maxMom)
                {
                    P = Math.Abs(L) / R;
                }
                else
                {
                    P = RNG.Uniform(Math.Abs(L) / R, maxMom);
                }


                float sinTheta = L / (R * P);
                float theta = (float)Math.Asin(sinTheta);
                float thetaP = thetaR - theta;

                Vector2 p = new Vector2(P * (float)Math.Cos(thetaP), P * (float)Math.Sin(thetaP));

                Body newBody = new Body(i, m, r, p);
                Bodies.Add(newBody);
            }
            return;
        }

        /// <summary>
        /// Advance the system by 'time' in "system units" 
        /// (to be scaled externally before being passed)
        /// </summary>
        /// <param name="time"></param>
        public static void Update(float time)
        {
            UpdatePositions(time);
            UpdateMomenta(time);
            return;
        }


        private static void UpdatePositions(float time)
        {
            foreach (Body body in Bodies)
            {
                if (body == null)
                    continue;

                // x_new = x_old + dx
                // dx = p dt / m
                body.Pos += body.Mom * time / body.Mass;
            }
        }
        
        /// <summary>
        /// Calculate net gravitational forces and update momenta.
        /// Also check if bodies collided and merge them.
        /// </summary>
        /// <param name="time"></param>
        private static void UpdateMomenta(float time)
        {
            int bodyCount = (int)Bodies.Count;
            for (int i = 0; i < bodyCount; i++)
            {
                if (Bodies[i] == null)
                    // If i was absorbed
                    continue;

                Vector2 force_i = new Vector2(0);       // Net force on body i
                for (int j = 0; j < bodyCount; j++)
                {
                    if (Bodies[j] == null || i == j)
                    {
                        // If j was absorbed or if j and i are the same
                        continue;
                    }
                    Vector2 r_ij = Bodies[i].Pos - Bodies[j].Pos;   // Relative position vector

                    // Check if i & j have collided and, if so, merge them
                    if (r_ij.Length() < Bodies[i].Radius + Bodies[j].Radius)
                    {

                        if (Bodies[i].Mass > Bodies[j].Mass)
                        {
                            MergeBodies(Bodies[i], Bodies[j]);
                        }
                        else
                        {
                            MergeBodies(Bodies[j], Bodies[i]);
                        }
                        continue;
                    }

                    // Force on i due to j
                    Vector2 force_ij = -(GravConst * Bodies[i].Mass * Bodies[j].Mass / (float)Math.Pow(r_ij.Length(), 3)) * r_ij;

                    // Add to the net force on i
                    force_i += force_ij;
                }
                
                // Update body i's momentum
                Bodies[i].Mom += force_i * time;
            }
        }

        /// <summary>
        /// Merge two bodies, with larger "absorbing" smaller, which is set to null.
        /// </summary>
        /// <param name="larger"></param>
        /// <param name="smaller"></param>
        private static void MergeBodies(Body larger, Body smaller)
        {
            float newMass = larger.Mass + smaller.Mass;
            Vector2 newMom = larger.Mom + smaller.Mom;

            larger = new Body(larger.BodyIndex, newMass, larger.Pos, newMom);
            smaller = null;
        }
        
    }
}
