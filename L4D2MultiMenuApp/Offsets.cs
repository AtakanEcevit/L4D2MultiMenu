using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L4D2MultiMenu
{
    internal class Offsets
    {
        public int engineAngles = 0x04268EC;
        public int engineAnglesOffset = 0x4AAC;
        public int viewMatrix = 0x601FDC;
        public int viewMatrixOffset = 0x2E4;



        public int entityList = 0x73A574 + 0x10;
        public int localPlayer = 0x726BD8;

        public int health = 0xEC;
        public int lifeState = 0x144;
        public int jumpFlag = 0xF0;
        public int viewOffset = 0xF4;
        public int origin = 0x124;
        public int teamNum = 0xE4;
        public int forceShove = 0x759F30;


    }
}
