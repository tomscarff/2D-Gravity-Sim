using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace _2D_Gravity_Sim
{
    class Body
    {
        public readonly uint BodyIndex;
        public readonly float Mass;
        public readonly float Radius; // initialised based on mass

        public readonly Color Colour;

        public Vector2 Pos { get; set; }
        public Vector2 Mom { get; set; }

        public Body(uint bodyIndex, float mass, Vector2 pos, Vector2 mom)
        {
            BodyIndex = bodyIndex;
            Mass = mass;
            Radius = GetRadius(mass);
            Pos = pos;
            Mom = mom;
        }

        /// <summary>
        /// Get the radius of the body for initialisation.
        /// Proportional to mass^1/3
        /// </summary>
        /// <returns></returns>
        private float GetRadius(float mass)
        {
            const float radiusConst = 1.0f;
            return radiusConst * (float)Math.Pow(mass, 1.0 / 3.0);
        }



    }
}
