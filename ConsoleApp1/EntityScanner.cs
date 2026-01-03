using Swed32;
using System.Numerics;
using System.Text;

namespace L4D2MultiMenu
{
    internal class EntityScanner
    {
        private readonly Swed swed;
        private readonly Offsets offsets;
        private readonly Encoding encoding;

        private IntPtr client;
        private IntPtr engine;
        private IntPtr testViewMatrixAddress = IntPtr.Zero;

        public EntityScanner(Swed swed, Offsets offsets, Encoding encoding)
        {
            this.swed = swed;
            this.offsets = offsets;
            this.encoding = encoding;
        }

        public void Initialize()
        {
            client = swed.GetModuleBase("client.dll");
            engine = swed.GetModuleBase("engine.dll");
        }

        public void ReloadEntities(GameState state)
        {
            state.CommonInfected.Clear();
            state.SpecialInfected.Clear();
            state.Survivors.Clear();

            state.LocalPlayer.address = swed.ReadPointer(client, offsets.localPlayer);
            UpdateEntity(state.LocalPlayer, state);

            UpdateEntities(state);

            SortByDistance(state.CommonInfected);
            SortByDistance(state.SpecialInfected);
        }

        private void UpdateEntities(GameState state)
        {
            for (int i = 0; i < 850; i++)
            {
                Entity entity = new Entity
                {
                    address = swed.ReadPointer(client, offsets.entityList + i * 0x10)
                };

                if (entity.address == IntPtr.Zero) continue;
                UpdateEntity(entity, state);

                if (entity.lifeState < 1) continue;

                if (entity.teamNum == 2 && entity.health > 0)
                {
                    state.Survivors.Add(entity);
                }
                else if (entity.teamNum == 3)
                {
                    if (entity.name.Contains("inf", StringComparison.OrdinalIgnoreCase))
                    {
                        state.CommonInfected.Add(entity);
                    }
                    else
                    {
                        if (entity.health > 1)
                            state.SpecialInfected.Add(entity);
                    }
                }
            }
        }

        private void UpdateEntity(Entity entity, GameState state)
        {
            entity.lifeState = swed.ReadInt(entity.address, offsets.lifeState);

            entity.origin = swed.ReadVec(entity.address, offsets.origin);
            entity.viewOffset = swed.ReadVec(entity.address, offsets.viewOffset);
            entity.abs = entity.origin + entity.viewOffset;
            entity.health = swed.ReadInt(entity.address, offsets.health);
            entity.teamNum = swed.ReadInt(entity.address, offsets.teamNum);
            entity.jumpFlag = swed.ReadInt(entity.address, offsets.jumpFlag);
            entity.magnitude = CalculateMagnitude(entity.origin, state.LocalPlayer.origin);

            var currentViewMatrix = ReadMatrix();
            entity.originScreenPosition = World2Screen(currentViewMatrix, entity.origin, (int)state.WindowSize.X, (int)state.WindowSize.Y) + state.WindowLocation;
            entity.absScreenPosition = World2Screen(currentViewMatrix, entity.abs, (int)state.WindowSize.X, (int)state.WindowSize.Y) + state.WindowLocation;

            var entityStringPointer = swed.ReadPointer(entity.address, 0x10);
            var nameBytes = swed.ReadBytes(entityStringPointer, 32);
            entity.name = encoding.GetString(nameBytes).TrimEnd('\0');
            if (entity.maxHealth == 0)
            {
                entity.maxHealth = entity.health;
            }

            entity.infectedType = GetInfectedType(entity.name, entity.maxHealth);
        }

        private float CalculateMagnitude(Vector3 from, Vector3 destination)
        {
            return (float)Math.Sqrt(
                Math.Pow(destination.X - from.X, 2) +
                Math.Pow(destination.Y - from.Y, 2) +
                Math.Pow(destination.Z - from.Z, 2));
        }

        private Vector2 World2Screen(ViewMatrix matrix, Vector3 pos, int width, int height)
        {
            Vector2 screenCoordinates = new Vector2();

            float screenW = matrix.m41 * pos.X + matrix.m42 * pos.Y + matrix.m43 * pos.Z + matrix.m44;
            if (screenW < 0.001f) return new Vector2(-99, -99);

            float screenX = (matrix.m11 * pos.X) + (matrix.m12 * pos.Y) + (matrix.m13 * pos.Z) + matrix.m14;
            float screenY = (matrix.m21 * pos.X) + (matrix.m22 * pos.Y) + (matrix.m23 * pos.Z) + matrix.m24;
            float camX = width / 2f;
            float camY = height / 2f;

            screenCoordinates.X = camX + (camX * screenX / screenW);
            screenCoordinates.Y = camY - (camY * screenY / screenW);
            return screenCoordinates;
        }

        private ViewMatrix ReadMatrix()
        {
            var viewMatrix = new ViewMatrix();
            IntPtr matrixPointer = swed.ReadPointer(engine, offsets.viewMatrix);
            float[] matrix = swed.ReadMatrix(matrixPointer + offsets.viewMatrixOffset);

            if (testViewMatrixAddress != IntPtr.Zero)
            {
                matrix = swed.ReadMatrix(testViewMatrixAddress);
            }

            viewMatrix.m11 = matrix[0];
            viewMatrix.m12 = matrix[1];
            viewMatrix.m13 = matrix[2];
            viewMatrix.m14 = matrix[3];
            viewMatrix.m21 = matrix[4];
            viewMatrix.m22 = matrix[5];
            viewMatrix.m23 = matrix[6];
            viewMatrix.m24 = matrix[7];
            viewMatrix.m31 = matrix[8];
            viewMatrix.m32 = matrix[9];
            viewMatrix.m33 = matrix[10];
            viewMatrix.m34 = matrix[11];
            viewMatrix.m41 = matrix[12];
            viewMatrix.m42 = matrix[13];
            viewMatrix.m43 = matrix[14];
            viewMatrix.m44 = matrix[15];

            return viewMatrix;
        }

        private string GetInfectedType(string name, int maxHealth)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var lowerName = name.ToLowerInvariant();

                if (lowerName.Contains("tank")) return "Tank";
                if (lowerName.Contains("witch")) return "Witch";
                if (lowerName.Contains("charger")) return "Charger";
                if (lowerName.Contains("jockey")) return "Jockey";
                if (lowerName.Contains("hunter")) return "Hunter";
                if (lowerName.Contains("smoker")) return "Smoker";
                if (lowerName.Contains("spitter")) return "Spitter";
                if (lowerName.Contains("boomer")) return "Boomer";
                if (lowerName.Contains("infected") || lowerName.Contains("common")) return "Common Infected";
            }

            switch (maxHealth)
            {
                case 4000: return "Tank";
                case 1000: return "Witch";
                case 600: return "Charger";
                case 325: return "Jockey";
                case 250: return "Hunter/Smoker";
                case 100: return "Spitter";
                case 50: return "Boomer/Common Infected";
                default: return "Unknown";
            }
        }

        private void SortByDistance(List<Entity> entities)
        {
            var sorted = entities.OrderBy(x => x.magnitude).ToList();
            entities.Clear();
            entities.AddRange(sorted);
        }
    }
}
