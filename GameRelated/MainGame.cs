using System;
using System.Collections.Generic;
using InfiniTD_2.Framework;

namespace InfiniTD_2.GameRelated
{
    [Serializable]
    public class MainGame
    {
        [NonSerialized] private int BaseHP = 10;
        [NonSerialized] private int Money = 100;
        [NonSerialized] private Map currentMap;
        [NonSerialized] private readonly Camera camera;

        [NonSerialized] private readonly FontInstance iconsFont = FontManager.GetFont("Webdings", 72, 108, 108);
        [NonSerialized] private readonly FontInstance iconsFont2 = FontManager.GetFont("Wingdings", 72, 108, 108);
        [NonSerialized] private readonly FontInstance gameFont = FontManager.GetFont("Bahnschrift", 72, 72, 108);
        [NonSerialized] private readonly List<Enemy> enemies = new List<Enemy>();
        [NonSerialized] private float spawnTimer = 0f;
        [NonSerialized] private Vector2[] currentPath;
        [NonSerialized] private readonly List<Tower> towers = new List<Tower>();
        [NonSerialized] private readonly List<Projectile> projectiles = new List<Projectile>();
        [NonSerialized] private Tower selectedTower = null;
        [NonSerialized] private short selectedTowerType = 1;
        [NonSerialized] private readonly int[] towerCosts = new int[] { 0, 50, 100, 150 };
        [NonSerialized]
        private UISlider speedSlider = new UISlider(140, 850, 300, 10)
        {
            OnChanged = (Value) => { SimulationSpeed = Value;},
            MinValue = 0.1f,
            MaxValue = 5f,
            Value = 1f
        };
        // Волны
        [NonSerialized] private int CurrentWave = 0;
        [NonSerialized] private float WaveTimer = 0f;
        [NonSerialized] private readonly float WaveInterval = 20f;
        [NonSerialized] private int EnemiesPerWave = 5;
        [NonSerialized] private float EnemyHealthMultiplier = 1f;
        [NonSerialized] private float SpawnInterval = 2f;
        [NonSerialized] private int EnemiesSpawned = 0;
        [NonSerialized] private bool IsWaveActive = false;
        [NonSerialized] private static float saveTimer = 0f;
        [NonSerialized] private const float SAVE_INTERVAL = 15f;
        [NonSerialized] private static float SimulationSpeed = 1f;

        // Сохраняемые данные
        [Serializable]
        public class GameState
        {
            public int BaseHP;
            public int Money;
            public int CurrentWave;
            public float WaveTimer;
            public int EnemiesPerWave;
            public float EnemyHealthMultiplier;
            public float SpawnInterval;
            public int EnemiesSpawned;
            public bool IsWaveActive;
            public float spawnTimer;
            public short selectedTowerType;
            public Map usedMap;
            // Состояние врагов
            public EnemyState[] Enemies;

            // Состояние башен
            public TowerState[] Towers;

            // Состояние снарядов
            public ProjectileState[] Projectiles;
        }

        [Serializable]
        public struct EnemyState
        {
            public float X;
            public float Y;
            public float Health;
            public int PathIndex;
        }

        [Serializable]
        public struct TowerState
        {
            public short X;
            public short Y;
            public int Cost;
            public int TargetingMode;
        }

        [Serializable]
        public struct ProjectileState
        {
            public float X;
            public float Y;
            public Enemy Enemy;
            public float Damage;
        }

        public MainGame(string mapPath = null)
        {
            camera = new Camera();

            if (!string.IsNullOrEmpty(mapPath) && System.IO.File.Exists(mapPath))
            {
                currentMap = MapLoader.LoadMap(mapPath);
            }
            else
            {
                currentMap = new Map
                {
                    Tiles = new Tile[0]
                };
            }
            var (portal, basePos) = Pathfinding.FindSpawnAndBase(currentMap);
            currentPath = Pathfinding.FindPath(currentMap, portal, basePos);
        }

        private void UpdateWaves(float deltaTime)
        {
            if (!IsWaveActive)
            {
                WaveTimer += deltaTime;
                if (WaveTimer >= WaveInterval)
                {
                    StartWave();
                }
            }
        }

        private void StartWave()
        {
            IsWaveActive = true;
            WaveTimer = 0f;
            EnemiesSpawned = 0;
            spawnTimer = 0f;

            EnemiesPerWave = 5 + (int)Math.Sqrt(CurrentWave) * 2;
            EnemyHealthMultiplier = 1f + (float)Math.Pow(1.15, CurrentWave) / 3;
            SpawnInterval = Math.Max(0.3f, 2f - CurrentWave * 0.01f);
        }

        public GameState GetSaveData()
        {
            var state = new GameState
            {
                BaseHP = BaseHP,
                Money = Money,
                CurrentWave = CurrentWave,
                WaveTimer = WaveTimer,
                EnemiesPerWave = EnemiesPerWave,
                EnemyHealthMultiplier = EnemyHealthMultiplier,
                SpawnInterval = SpawnInterval,
                EnemiesSpawned = EnemiesSpawned,
                IsWaveActive = IsWaveActive,
                spawnTimer = spawnTimer,
                selectedTowerType = selectedTowerType,
                usedMap = currentMap,

                Enemies = new EnemyState[enemies.Count],
                Towers = new TowerState[towers.Count],
                Projectiles = new ProjectileState[projectiles.Count]
            };

            for (int i = 0; i < enemies.Count; i++)
            {
                state.Enemies[i] = new EnemyState
                {
                    X = enemies[i].X,
                    Y = enemies[i].Y,
                    Health = enemies[i].Health,
                    PathIndex = enemies[i].PathIndex
                };
            }

            for (int i = 0; i < towers.Count; i++)
            {
                state.Towers[i] = new TowerState
                {
                    X = towers[i].X,
                    Y = towers[i].Y,
                    Cost = towers[i].Cost,
                    TargetingMode = (int)towers[i].TargetingMode
                };
            }

            for (int i = 0; i < projectiles.Count; i++)
            {
                state.Projectiles[i] = new ProjectileState
                {
                    X = projectiles[i].X,
                    Y = projectiles[i].Y,
                    Enemy = projectiles[i].Target,
                    Damage = projectiles[i].Damage
                };
            }
            state.usedMap = currentMap;

            return state;
        }

        public void LoadFromData(GameState state)
        {
            BaseHP = state.BaseHP;
            Money = state.Money;
            CurrentWave = state.CurrentWave;
            WaveTimer = state.WaveTimer;
            EnemiesPerWave = state.EnemiesPerWave;
            EnemyHealthMultiplier = state.EnemyHealthMultiplier;
            SpawnInterval = state.SpawnInterval;
            EnemiesSpawned = state.EnemiesSpawned;
            IsWaveActive = state.IsWaveActive;
            spawnTimer = state.spawnTimer;
            selectedTowerType = state.selectedTowerType;
            currentMap = state.usedMap;

            var (portal, basePos) = Pathfinding.FindSpawnAndBase(currentMap);
            currentPath = Pathfinding.FindPath(currentMap, portal, basePos);

            enemies.Clear();
            foreach (var enemyState in state.Enemies)
            {
                var enemy = new Enemy(currentPath, EnemyHealthMultiplier)
                {
                    X = enemyState.X,
                    Y = enemyState.Y,
                    Health = enemyState.Health,
                    PathIndex = enemyState.PathIndex,
                    path = currentPath 
                };
                enemies.Add(enemy);
            }

            towers.Clear();
            foreach (var towerState in state.Towers)
            {
                var tower = new Tower(towerState.X, towerState.Y, towerState.Cost);
                tower.TargetingMode = (TargetingMode)towerState.TargetingMode;
                towers.Add(tower);
            }

            projectiles.Clear();
            foreach (var projState in state.Projectiles)
            {
                var proj = new Projectile(projState.X, projState.Y, projState.Enemy, projState.Damage);
                projectiles.Add(proj);
            }

        }

        public void Update()
        {
            camera.Update();
            speedSlider.Update();
            if (BaseHP <= 0)
            {
                SimulationSpeed = 0;
            }
            float DeltaTime = (float)MainApp.DeltaTime * SimulationSpeed;

            UpdateWaves(DeltaTime);
            if (IsWaveActive)
            {
                spawnTimer += DeltaTime;
                if (spawnTimer >= SpawnInterval && EnemiesSpawned < EnemiesPerWave)
                {
                    spawnTimer = 0f;
                    EnemiesSpawned++;
                    if (currentPath != null && currentPath.Length > 0)
                    {
                        enemies.Add(new Enemy(currentPath, EnemyHealthMultiplier));
                    }
                }

                if (EnemiesSpawned >= EnemiesPerWave)
                {
                    IsWaveActive = false;
                    CurrentWave++;
                    WaveTimer = 0f;
                }
            }

            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Update(DeltaTime);

                if (!enemies[i].IsActive && enemies[i].PathIndex >= enemies[i].path.Length)
                {
                    BaseHP--;
                }
            }

            foreach (var tower in towers)
            {
                tower.Update(enemies, DeltaTime, projectiles);
            }
            foreach (var projectile in projectiles)
            {
                projectile.Update(DeltaTime);
            }
            projectiles.RemoveAll(p => !p.IsActive);

            if (Input.IsKeyPressed('1')) selectedTowerType = 1;
            if (Input.IsKeyPressed('2')) selectedTowerType = 2;
            if (Input.IsKeyPressed('3')) selectedTowerType = 3;

            if (Input.IsKeyDown('Q')) HandleTowerPlacement();
            if (Input.IsKeyPressed('E')) HandleTowerMenu();

            if (selectedTower != null && selectedTower.ShowMenu)
            {
                if (Input.IsKeyPressed('4')) selectedTower.SetTargetingMode(0);
                if (Input.IsKeyPressed('5')) selectedTower.SetTargetingMode(1);
                if (Input.IsKeyPressed('6')) selectedTower.SetTargetingMode(2);
                if (Input.IsKeyPressed('7')) selectedTower.SetTargetingMode(3);
                if (Input.IsKeyPressed('8')) selectedTower.SetTargetingMode(4);
                if (Input.IsKeyPressed('9')) selectedTower.SetTargetingMode(5);
            }

            foreach (var en in enemies)
            {
                if (!en.IsActive) Money += 12;
            }
            enemies.RemoveAll(e => !e.IsActive);

            saveTimer += DeltaTime;
            if (saveTimer >= SAVE_INTERVAL)
            {
                saveTimer = 0f;
                SaveSystem.SaveGame(this);
            }
        }

        private void HandleTowerPlacement()
        {
            float worldMouseX = (Input.VirtualMouseX / camera.Zoom) + camera.X;
            float worldMouseY = (Input.VirtualMouseY / camera.Zoom) + camera.Y;

            int tileX = (int)(worldMouseX / 50);
            int tileY = (int)(worldMouseY / 50);

            bool hasPlatform = false;
            foreach (var tile in currentMap.Tiles)
            {
                if (tile.X == tileX && tile.Y == tileY && tile.Id == (byte)TileTypes.Platform)
                {
                    hasPlatform = true;
                    break;
                }
            }

            if (!hasPlatform) return;

            foreach (var tower in towers)
            {
                if (tower.X == tileX && tower.Y == tileY)
                    return;
            }

            int cost = towerCosts[selectedTowerType];
            if (Money >= cost)
            {
                Money -= cost;
                towers.Add(new Tower((short)tileX, (short)tileY, cost));
            }
        }

        private void HandleTowerMenu()
        {
            float worldMouseX = (Input.VirtualMouseX / camera.Zoom) + camera.X;
            float worldMouseY = (Input.VirtualMouseY / camera.Zoom) + camera.Y;

            foreach (var tower in towers)
            {
                tower.ShowMenu = false;
            }

            foreach (var tower in towers)
            {
                if (tower.IsMouseOver(worldMouseX, worldMouseY))
                {
                    tower.ToggleMenu();
                    selectedTower = tower;
                    break;
                }
            }
        }

        public void Draw()
        {
            foreach (var tile in currentMap.Tiles)
            {
                float tileX = (tile.X * 50 - camera.X) * camera.Zoom;
                float tileY = (tile.Y * 50 - camera.Y) * camera.Zoom;
                float tileSize = 50 * camera.Zoom;

                if (tileX + tileSize < 0 || tileX > 1600 ||
                    tileY + tileSize < 0 || tileY > 900)
                    continue;

                switch (tile.Id)
                {
                    case (byte)TileTypes.Road:
                        Primitives.DrawQuad(tileX, tileY, tileSize, tileSize, 0.8f, 0.8f, 0.75f, 1);
                        break;
                    case (byte)TileTypes.Platform:
                        Primitives.DrawQuad(tileX, tileY, tileSize, tileSize, 0.6f, 0.6f, 0.62f, 1);
                        break;
                    case (byte)TileTypes.Base:
                        Primitives.DrawQuad(tileX, tileY, tileSize, tileSize, 0.2f, 1f, 0.2f, 1);
                        break;
                    case (byte)TileTypes.Portal:
                        Primitives.DrawQuad(tileX, tileY, tileSize, tileSize, 1f, 0f, 1f, 1);
                        break;
                }
            }

            if (currentPath != null)
            {
                for (int i = 0; i < currentPath.Length - 1; i++)
                {
                    float x1 = (currentPath[i].X * 50 + 25 - camera.X) * camera.Zoom;
                    float y1 = (currentPath[i].Y * 50 + 25 - camera.Y) * camera.Zoom;
                    float x2 = (currentPath[i + 1].X * 50 + 25 - camera.X) * camera.Zoom;
                    float y2 = (currentPath[i + 1].Y * 50 + 25 - camera.Y) * camera.Zoom;

                    Primitives.DrawLine(x1, y1, x2, y2, 2 * camera.Zoom, 1f, 0f, 0f, 0.5f);
                }
            }
            foreach (var tower in towers)
            {
                tower.Draw(camera, gameFont);
            }
            foreach (var enemy in enemies)
            {
                enemy.Draw(camera);
            }
            foreach (var projectile in projectiles)
            {
                projectile.Draw(camera);
            }

            iconsFont.DrawText("Y", 10, 15, 1, 1, 0, 0);
            gameFont.DrawText(BaseHP.ToString(), 50, 20, 1);
            iconsFont.DrawText("n", 10, 65, 1, 1, 1, 0);
            gameFont.DrawText(Money.ToString(), 50, 70, 1);
            iconsFont2.DrawText("h", 10, 115, 1, 0.2f, 0.2f, 1);
            gameFont.DrawText(CurrentWave.ToString(), 50, 120, 1);
            gameFont.DrawText($"Enemies: {enemies.Count}", 10, 800, 0.5f, 1f, 1f, 1f);
            gameFont.DrawText($"Tower: {selectedTowerType} (Cost: {towerCosts[selectedTowerType]})", 10, 150, 0.5f, 0.8f, 0.8f, 0.8f);
            gameFont.DrawText("Q: Place Tower, E: Tower Menu (in menu press 4,5,6,7,8,9 to change priority)", 10, 680, 0.4f, 0.6f, 0.6f, 0.6f);
            if (IsWaveActive)
            {
                gameFont.DrawText($"Enemies: {EnemiesSpawned}/{EnemiesPerWave}", 10, 740, 0.6f, 1f, 1f, 1f);
            }
            else
            {
                gameFont.DrawText($"Next wave: {WaveInterval - WaveTimer:F1}s", 10, 740, 0.6f, 0.8f, 0.8f, 0.8f);
            }
            speedSlider.Draw(gameFont);
        }
    }
}