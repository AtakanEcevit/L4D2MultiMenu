using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Entity
    {
        public IntPtr address { get; set; }
        public string name { get; set; }
        public int health { get; set; }
        public int lifeState { get; set; }
        public int teamNum { get; set; }
        public int jumpFlag { get; set; }

        public int maxHealth { get; set; }

        public Vector3 origin { get; set; }
        public Vector3 abs { get; set; }
        public Vector3 viewOffset { get; set; }

        public Vector2 originScreenPosition { get; set; }

        public Vector2 absScreenPosition { get; set; }

        public float magnitude { get; set; }

        public string infectedType { get; set; }




    }
}
