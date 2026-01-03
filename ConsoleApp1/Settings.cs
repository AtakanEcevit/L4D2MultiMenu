using System.Numerics;

namespace L4D2MultiMenu
{
    internal class Settings
    {
        public bool EnableEsp { get; set; } = true;
        public bool EnableAutoShove { get; set; } = false;
        public bool RequireAutoShoveKey { get; set; } = false;
        public bool EnableHealthText { get; set; } = false;

        public bool SurvivorLineEnable { get; set; } = true;
        public bool SurvivorBoxEnable { get; set; } = true;
        public bool SurvivorDotEnable { get; set; } = true;

        public bool SpecialInfectedLineEnable { get; set; } = true;
        public bool SpecialInfectedBoxEnable { get; set; } = true;
        public bool SpecialInfectedDotEnable { get; set; } = true;

        public bool CommonInfectedLineEnable { get; set; } = true;
        public bool CommonInfectedBoxEnable { get; set; } = true;
        public bool CommonInfectedDotEnable { get; set; } = true;

        public Vector4 SurvivorColor { get; set; } = new Vector4(0, 0, 1, 1);
        public Vector4 SpecialInfectedColor { get; set; } = new Vector4(1, 0, 1, 1);
        public Vector4 CommonInfectedColor { get; set; } = new Vector4(1, 0, 0, 1);
    }
}
