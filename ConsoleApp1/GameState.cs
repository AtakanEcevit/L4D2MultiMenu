using System.Numerics;

namespace L4D2MultiMenu
{
    internal class GameState
    {
        public Vector2 WindowSize { get; set; } = new Vector2(1920, 1080);
        public Vector2 WindowLocation { get; set; } = new Vector2(0, 0);
        public Vector2 WindowCenter { get; set; } = new Vector2(1920 / 2, 1080 / 2);
        public Vector2 LineOrigin { get; set; } = new Vector2(1920 / 2, 1080);

        public Entity LocalPlayer { get; } = new Entity();
        public List<Entity> CommonInfected { get; } = new List<Entity>();
        public List<Entity> SpecialInfected { get; } = new List<Entity>();
        public List<Entity> Survivors { get; } = new List<Entity>();
    }
}
