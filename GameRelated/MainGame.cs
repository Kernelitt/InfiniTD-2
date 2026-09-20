using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        [NonSerialized] private readonly FontInstance gameFont = FontManager.GetFont("Bahnschrift", 72, 80, 108);
        [NonSerialized] private readonly FontInstance buttonFont = FontManager.GetFont("Arial", 32, 32, 48);
        [NonSerialized] private readonly List<Enemy> enemies = new List<Enemy>();
        [NonSerialized] private float spawnTimer = 0f;
        [NonSerialized] private Vector2[] currentPath;
        [NonSerialized] private readonly List<Tower> towers = new List<Tower>();
        [NonSerialized] private readonly List<Projectile> projectiles = new List<Projectile>();
        [NonSerialized] private Tower selectedTower = null;
        [NonSerialized] private byte selectedTowerType = 1;
        [NonSerialized] private readonly int[] towerCosts = new int[] { 0, 50, 120};

        [NonSerialized] private static bool IsGamePaused = false;

        [NonSerialized]
        private readonly UIButton PauseButton = new UIButton(10, 830, 60, 60, ";")
        {
            OnClick = () => { IsGamePaused = true; }
        };
        private readonly UIButton PauseResumeButton = new UIButton(1300, 630, 220, 60, "Resume")
        {
            OnClick = () => { IsGamePaused = false; }
        };
        private readonly UIButton PauseSaveAndExitButton = new UIButton(1300, 720, 220, 60, "Save and quit");

        private readonly UIButton PauseExitButton = new UIButton(1300, 810, 220, 60, "Quit without saving")
        {
            OnClick = () => { MainMenu.CurrentScene = MenuScenes.Main;  SaveSystem.SaveGame(new MainGame()); IsGamePaused = false; }
        };

        [NonSerialized]
        private readonly UISlider speedSlider = new UISlider(140, 850, 300, 20)
        {
            OnChanged = (Value) => { SimulationSpeed = Value; },
            MinValue = 0.1f,
            MaxValue = 5f,
            Value = 1f
        };

        [NonSerialized]
        private readonly UIButton defeatBackButton = new UIButton(730, 530, 150, 40, "Back to menu")
        {
            OnClick = () => { MainMenu.CurrentScene = MenuScenes.Main; }
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
            public byte selectedTowerType;
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
            public int TargetingMode;
            public byte TowerType; // 1=Basic, 2=Rapid, 3=Sniper, 4=Splash
            public int Level;
            public float Damage;
            public float Range;
            public float FireRate;
            public int UpgradeCost;
        }

        [Serializable]
        public struct ProjectileState
        {
            public float X;
            public float Y;
            public Enemy Enemy;
            public float Damage;
            public bool IsSplash;
            public float SplashRadius;
        }

        public MainGame(string mapPath = null)
        {
            camera = new Camera();

            PauseSaveAndExitButton.OnClick = () =>
            {
                MainMenu.CurrentScene = MenuScenes.Main;
                SaveSystem.SaveGame(this);
                IsGamePaused = false;
            };

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
                    UpgradeCost = towers[i].UpgradeCost,
                    TargetingMode = (int)towers[i].TargetingMode,
                    TowerType = GetTowerType(towers[i]),
                    Level = towers[i].Level,
                    Damage = towers[i].Damage,
                    Range = towers[i].Range,
                    FireRate = towers[i].FireRate

                };
            }

            for (int i = 0; i < projectiles.Count; i++)
            {
                var proj = projectiles[i];
                state.Projectiles[i] = new ProjectileState
                {
                    X = proj.X,
                    Y = proj.Y,
                    Enemy = proj.Target,
                    Damage = proj.Damage
                };
            }
            state.usedMap = currentMap;

            return state;
        }

        private byte GetTowerType(Tower tower)
        {
            if (tower is BasicTower) return 1;
            if (tower is SniperTower) return 2;
            return 1;
        }

        private Tower CreateTower(byte type, short x, short y)
        {
            switch (type)
            {
                case 1: return new BasicTower(x, y);
                case 2: return new SniperTower(x, y);
                default: return new BasicTower(x, y);
            }
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
                var tower = CreateTower(towerState.TowerType, towerState.X, towerState.Y);
                tower.TargetingMode = (TargetingMode)towerState.TargetingMode;
                tower.Level = towerState.Level;
                tower.Damage = towerState.Damage;
                tower.Range = towerState.Range;
                tower.FireRate = towerState.FireRate;
                tower.UpgradeCost = towerState.UpgradeCost;
                if (tower.Level >= tower.MaxLevel) tower.CanUpgrade = false;
                towers.Add(tower);
            }

            projectiles.Clear();
            foreach (var projState in state.Projectiles)
            {
                Projectile proj;
                proj = new Projectile(projState.X, projState.Y, projState.Enemy, projState.Damage);
                
                projectiles.Add(proj);
            }

        }

        public void Update()
        {
            camera.Update();
            
            if (BaseHP <= 0)
            {
                SimulationSpeed = 0;
                defeatBackButton.Update();
            }
            else if (IsGamePaused)
            {
                SimulationSpeed = 0;
                PauseResumeButton.Update();
                PauseExitButton.Update();
                PauseSaveAndExitButton.Update();
            }
            else
            {
                PauseButton.Update();
                speedSlider.Update();
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
                        if (BaseHP <= 0)
                            SaveSystem.SaveGame(new MainGame());
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

                // Выбор типа башни
                if (Input.IsKeyPressed('1')) selectedTowerType = 1;
                if (Input.IsKeyPressed('2')) selectedTowerType = 2;

                if (Input.IsKeyDown('Q')) HandleTowerPlacement();
                if (Input.IsKeyDown('E')) HandleTowerMenu();

                // В Update() добавить callback на улучшение:
                if (selectedTower != null && selectedTower.ShowMenu)
                {
                    selectedTower.UpdateButtons();
                    if (selectedTower.upgradeButton != null)
                    {
                        selectedTower.upgradeButton.OnClick = () =>
                        {
                            if (Money >= selectedTower.UpgradeCost)
                            {
                                Money -= selectedTower.UpgradeCost;
                                selectedTower.Upgrade();
                            }
                        };
                    }
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

                SimulationSpeed = speedSlider.Value;
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
                towers.Add(CreateTower(selectedTowerType, (short)tileX, (short)tileY));
            }
        }

        // В HandleTowerMenu() добавить обновление кнопок:
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

            // Обновляем кнопки выбранной башни
            if (selectedTower != null && selectedTower.ShowMenu)
            {
                selectedTower.UpdateButtons();
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
                tower.Draw(camera, buttonFont);
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

            // Отображение доступных башен
            gameFont.DrawText("Towers: 1=Basic($50), 2=Sniper($120)", 10, 150, 0.4f, 0.8f, 0.8f, 0.8f);
            gameFont.DrawText($"Selected: {selectedTowerType}", 10, 180, 0.5f, 1f, 1f, 0.5f);

            gameFont.DrawText("Q: Place Tower, E: Tower Menu", 10, 680, 0.4f, 0.6f, 0.6f, 0.6f);

            if (IsWaveActive)
            {
                gameFont.DrawText($"Enemies: {EnemiesSpawned}/{EnemiesPerWave}", 10, 740, 0.6f, 1f, 1f, 1f);
            }
            else
            {
                gameFont.DrawText($"Next wave: {WaveInterval - WaveTimer:F1}s", 10, 740, 0.6f, 0.8f, 0.8f, 0.8f);
            }
            speedSlider.Draw(gameFont);

            if (BaseHP <= 0)
            {
                Primitives.DrawQuad(0, 0, 1600, 900, 0.3f, 0.3f, 0.3f, 0.3f);
                Primitives.DrawQuad(500, 300, 600, 300, 0.4f, 0.4f, 0.4f, 1f);
                gameFont.DrawText("Game Over", 670, 310, 1, 1 ,0, 0, 1);
                gameFont.DrawText("Waves completed: "+CurrentWave, 500, 400, 0.6f);
                defeatBackButton.Draw(buttonFont);
            }
            else if (IsGamePaused)
            {
                Primitives.DrawQuad(0, 0, 1600, 900, 0.2f, 0.2f, 0.2f, 0.5f);
                gameFont.DrawText("Paused", 10, 10, 2f);
                PauseResumeButton.Draw(buttonFont);
                PauseSaveAndExitButton.Draw(buttonFont);
                PauseExitButton.Draw(buttonFont);
            }
            else
            {
                PauseButton.Draw(iconsFont);
            }
        }
    }
}