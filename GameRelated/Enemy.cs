using System;
using System.Collections.Generic;
using InfiniTD_2.Framework;
using InfiniTD_2.Framework.Audio;

namespace InfiniTD_2.GameRelated
{
    [Serializable]
    public class Enemy
    {
        public float X, Y;
        public float Health;
        public float MaxHealth;
        public int PathIndex;
        public bool IsActive = true;
        public Vector2[] path;
        public int CurrentPathIndex; // От какого портала (индекс пути)

        public Enemy(Vector2[][] allPaths, int pathIndex, float healthMultiplier)
        {
            CurrentPathIndex = pathIndex;
            path = allPaths[pathIndex];

            if (path != null && path.Length > 0)
            {
                X = path[0].X * 50 + 25;
                Y = path[0].Y * 50 + 25;
            }

            Health = 100f * healthMultiplier;
            MaxHealth = Health;
            PathIndex = 0;
        }

        public void Update(float deltaTime)
        {
            if (!IsActive || path == null || path.Length == 0) return;

            if (PathIndex >= path.Length - 1)
            {
                IsActive = false;
                return;
            }

            Vector2 target = path[PathIndex + 1];
            float targetX = target.X * 50 + 25;
            float targetY = target.Y * 50 + 25;

            float dx = targetX - X;
            float dy = targetY - Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            if (distance < 5f)
            {
                PathIndex++;
            }
            else
            {
                float speed = 50f * deltaTime;
                X += (dx / distance) * speed;
                Y += (dy / distance) * speed;
            }
        }

        public void Draw(Camera camera)
        {
            if (!IsActive) return;

            float screenX = (X - camera.X) * camera.Zoom;
            float screenY = (Y - camera.Y) * camera.Zoom;
            float size = 15 * camera.Zoom;

            Primitives.DrawCircle(screenX, screenY, size, 0.1f, 0.5f, 0.1f, 1f, 32);
            // Тело врага (зелёный круг)
            Primitives.DrawCircle(screenX + 1 * camera.Zoom, screenY + 1 * camera.Zoom, size / 1.2f, 0.3f, 0.8f, 0.3f, 1f, 32);

            float healthWidth = 40 * camera.Zoom;
            float healthHeight = 4 * camera.Zoom;
            float healthX = screenX - healthWidth / 2.5f;
            float healthY = screenY - size - 5 * camera.Zoom;

            // Фон полоски
            Primitives.DrawQuad(healthX, healthY, healthWidth, healthHeight, 0.3f, 0.3f, 0.3f, 1f);

            // Здоровье
            float healthPercent = Health / MaxHealth;
            Primitives.DrawQuad(healthX, healthY, healthWidth * healthPercent, healthHeight, 0.2f, 0.8f, 0.2f, 1f);
        }

        public void TakeDamage(float damage)
        {
            if (!IsActive) return;
            Health -= damage;
            if (Health <= 0)
            {
                IsActive = false;
            }
        }
    }

    [Serializable]
    public struct Vector2
    {
        public short X;
        public short Y;

        public Vector2(short x, short y)
        {
            X = x;
            Y = y;
        }
    }

    public static class Pathfinding
    {
        public static Vector2[] FindPath(Map map, Vector2 start, Vector2 end)
        {
            int mapWidth = 0;
            int mapHeight = 0;

            foreach (var tile in map.Tiles)
            {
                if (tile.X > mapWidth) mapWidth = tile.X;
                if (tile.Y > mapHeight) mapHeight = tile.Y;
            }
            mapWidth++;
            mapHeight++;

            bool[,] walkable = new bool[mapWidth, mapHeight];

            // Дороги, портал и база проходимы
            foreach (var tile in map.Tiles)
            {
                if (tile.Id == (byte)TileTypes.Road ||
                    tile.Id == (byte)TileTypes.Portal ||
                    tile.Id == (byte)TileTypes.Base)
                {
                    if (tile.X >= 0 && tile.X < mapWidth && tile.Y >= 0 && tile.Y < mapHeight)
                        walkable[tile.X, tile.Y] = true;
                }
            }

            // Проверка что старт и конец проходимы
            if (start.X < 0 || start.X >= mapWidth || start.Y < 0 || start.Y >= mapHeight ||
                end.X < 0 || end.X >= mapWidth || end.Y < 0 || end.Y >= mapHeight ||
                !walkable[start.X, start.Y] || !walkable[end.X, end.Y])
            {
                System.Diagnostics.Debug.WriteLine($"Start or end not walkable! Start: {start.X},{start.Y} End: {end.X},{end.Y}");
                return new Vector2[0];
            }

            // BFS
            Queue<Vector2> queue = new Queue<Vector2>();
            Dictionary<Vector2, Vector2?> cameFrom = new Dictionary<Vector2, Vector2?>();

            queue.Enqueue(start);
            cameFrom[start] = null;

            Vector2[] directions = new Vector2[]
            {
                new Vector2(0, -1),
                new Vector2(0, 1),
                new Vector2(-1, 0),
                new Vector2(1, 0),
            };

            bool found = false;

            while (queue.Count > 0)
            {
                Vector2 mcurrent = queue.Dequeue();

                if (mcurrent.X == end.X && mcurrent.Y == end.Y)
                {
                    found = true;
                    break;
                }

                foreach (var dir in directions)
                {
                    short nextX = (short)(mcurrent.X + dir.X);
                    short nextY = (short)(mcurrent.Y + dir.Y);
                    Vector2 next = new Vector2(nextX, nextY);

                    if (nextX >= 0 && nextX < mapWidth && nextY >= 0 && nextY < mapHeight &&
                        walkable[nextX, nextY] && !cameFrom.ContainsKey(next))
                    {
                        queue.Enqueue(next);
                        cameFrom[next] = mcurrent;
                    }
                }
            }

            if (!found)
            {
                System.Diagnostics.Debug.WriteLine($"Path not found! Map size: {mapWidth}x{mapHeight}");
                System.Diagnostics.Debug.WriteLine($"Start: {start.X},{start.Y} End: {end.X},{end.Y}");
                return new Vector2[0];
            }

            List<Vector2> path = new List<Vector2>();
            Vector2? current = end;
            while (current.HasValue)
            {
                path.Add(current.Value);
                current = cameFrom.ContainsKey(current.Value) ? cameFrom[current.Value] : null;
            }
            path.Reverse();

            return path.ToArray();
        }

        // Найти все порталы и базу на карте
        public static (List<Vector2> portals, Vector2 basePos) FindSpawnAndBase(Map map)
        {
            List<Vector2> portals = new List<Vector2>();
            Vector2 basePos = new Vector2(0, 0);

            foreach (var tile in map.Tiles)
            {
                if (tile.Id == (byte)TileTypes.Portal)
                {
                    portals.Add(new Vector2(tile.X, tile.Y));
                }
                else if (tile.Id == (byte)TileTypes.Base)
                {
                    basePos = new Vector2(tile.X, tile.Y);
                }
            }

            // Если порталов нет, возвращаем один дефолтный
            if (portals.Count == 0)
            {
                portals.Add(new Vector2(0, 0));
            }

            return (portals, basePos);
        }

        // Найти пути от всех порталов к базе
        public static Vector2[][] FindAllPaths(Map map, List<Vector2> portals, Vector2 basePos)
        {
            Vector2[][] allPaths = new Vector2[portals.Count][];

            for (int i = 0; i < portals.Count; i++)
            {
                allPaths[i] = FindPath(map, portals[i], basePos);

                if (allPaths[i] == null || allPaths[i].Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Path from portal {i} ({portals[i].X},{portals[i].Y}) to base ({basePos.X},{basePos.Y}) not found!");
                }
            }

            return allPaths;
        }
    }
}