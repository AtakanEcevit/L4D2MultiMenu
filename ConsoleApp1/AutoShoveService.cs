using Swed32;
using System.Numerics;

namespace L4D2MultiMenu
{
    internal class AutoShoveService
    {
        private const int KEY_TOGGLE_AUTOSHOVE = 0x46;
        private const float SHOVE_RANGE = 95f;
        private const float SHOVE_FOV = 35f;
        private const float SMOOTH_FACTOR = 0.30f;
        private static readonly Random rng = new Random();

        private readonly Swed swed;
        private readonly Offsets offsets;

        private IntPtr client;
        private IntPtr engine;
        private DateTime nextShoveTime = DateTime.MinValue;

        private readonly Vector3 offsetVector = new Vector3(0, 0, 15);

        public AutoShoveService(Swed swed, Offsets offsets)
        {
            this.swed = swed;
            this.offsets = offsets;
        }

        public void Initialize()
        {
            client = swed.GetModuleBase("client.dll");
            engine = swed.GetModuleBase("engine.dll");
        }

        public void TryAutoShove(GameState state)
        {
            bool keyHeld = NativeMethods.GetAsyncKeyState(KEY_TOGGLE_AUTOSHOVE) < 0;

            if (!keyHeld)
            {
                swed.WriteInt(client, offsets.forceShove, 4);
                return;
            }

            if (DateTime.Now < nextShoveTime) return;

            Entity target = state.SpecialInfected
                .Where(e => e.health > 0 && e.lifeState > 1 && e.magnitude < SHOVE_RANGE)
                .OrderBy(e => e.magnitude)
                .FirstOrDefault();

            if (target == null) return;

            Vector3 desired = CalculateAngles(state.LocalPlayer.origin,
                                              target.origin - offsetVector);

            if (AngleDiffDeg(ReadLocalAngles(), desired) > SHOVE_FOV) return;

            Vector3 cur = ReadLocalAngles();
            Vector3 smooth = LerpAngles(cur, desired, SMOOTH_FACTOR);
            smooth = AddRandomOffset(smooth, 1.0f);
            WriteLocalAngles(smooth);

            swed.WriteInt(client, offsets.forceShove, 5);
            nextShoveTime = DateTime.Now.AddMilliseconds(rng.Next(180, 260));
        }

        private Vector3 ReadLocalAngles()
        {
            IntPtr engineAnglesBase = swed.ReadPointer(engine, offsets.engineAngles);
            float pitch = swed.ReadFloat(engineAnglesBase, offsets.engineAnglesOffset + 0x4);
            float yaw = swed.ReadFloat(engineAnglesBase, offsets.engineAnglesOffset);
            return new Vector3(yaw, pitch, 0);
        }

        private void WriteLocalAngles(Vector3 angles)
        {
            IntPtr engineAnglesBase = swed.ReadPointer(engine, offsets.engineAngles);
            swed.WriteFloat(engineAnglesBase, offsets.engineAnglesOffset, angles.X);
            swed.WriteFloat(engineAnglesBase, offsets.engineAnglesOffset + 0x4, angles.Y);
        }

        private Vector3 LerpAngles(Vector3 from, Vector3 to, float t)
        {
            return new Vector3(
                from.X + t * (to.X - from.X),
                from.Y + t * (to.Y - from.Y),
                0
            );
        }

        private Vector3 AddRandomOffset(Vector3 angles, float maxOffset = 1.5f)
        {
            float offsetX = (float)(rng.NextDouble() * maxOffset * 2 - maxOffset);
            float offsetY = (float)(rng.NextDouble() * maxOffset * 2 - maxOffset);

            angles.X += offsetX;
            angles.Y += offsetY;
            return angles;
        }

        private float AngleDiffDeg(Vector3 a, Vector3 b)
        {
            float dx = NormalizeDeg(a.X - b.X);
            float dy = NormalizeDeg(a.Y - b.Y);
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private float NormalizeDeg(float deg)
        {
            while (deg > 180) deg -= 360;
            while (deg < -180) deg += 360;
            return deg;
        }

        private Vector3 CalculateAngles(Vector3 from, Vector3 destination)
        {
            float deltaX = destination.X - from.X;
            float deltaY = destination.Y - from.Y;
            float deltaZ = destination.Z - from.Z;

            float yaw = (float)(Math.Atan2(deltaY, deltaX) * 180 / Math.PI);
            float distance = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            float pitch = -(float)(Math.Atan2(deltaZ, distance) * 180 / Math.PI);

            return new Vector3(yaw, pitch, 0);
        }
    }
}
