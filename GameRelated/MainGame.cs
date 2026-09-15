using System;
using System.Collections.Generic;
using System.Drawing;

namespace InfiniTD_2.GameRelated
{
    internal class MainGame
    {
        private int BaseHP = 10;
        private int Money = 100;
        private Map currentMap;
        private Camera camera;

        private readonly FontInstance iconsFont = FontManager.GetFont("Webdings", 72, 108, 108);
        private readonly FontInstance iconsFont2= FontManager.GetFont("Wingdings", 72, 108, 108);
        private readonly FontInstance gameFont = FontManager.GetFont("Bahnschrift", 72, 72, 108);
        private List<Enemy> enemies = new List<Enemy>();
        private float spawnTimer = 0f;
        private float spawnInterval = 2f;
        private Vector2[] currentPath;
        private List<Tower> towers = new List<Tower>();
        private List<Projectile> projectiles = new List<Projectile>();
        private Tower selectedTower = null;
        private short selectedTowerType = 1;
        private readonly int[] towerCosts = new int[] { 0, 50, 100, 150 };

        // Волны
        private int CurrentWave = 0;
        private float WaveTimer = 0f;
        private float WaveInterval = 10f; // Секунд между волнами
        private int EnemiesPerWave = 5;
        private float EnemyHealthMultiplier = 1f;
        private float SpawnInterval = 2f;
        private int EnemiesSpawned = 0;
        private bool IsWaveActive = false;
        public MainGame(string mapPath = null)
        {
            camera = new Camera();

            // Загрузка карты
            if (!string.IsNullOrEmpty(mapPath) && System.IO.File.Exists(mapPath))
            {
                currentMap = MapLoader.LoadMap(mapPath);
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

            // Увеличение сложности
            EnemiesPerWave = 5 + CurrentWave * 2;
            EnemyHealthMultiplier = 1f + (float)Math.Pow(1.15, CurrentWave);
            SpawnInterval = Math.Max(0.3f, 2f - CurrentWave * 0.1f);
        }
        public void Update()
        {
            camera.Update();
            UpdateWaves((float)MainApp.DeltaTime);
            if (IsWaveActive)
            {
                spawnTimer += (float)MainApp.DeltaTime;
                if (spawnTimer >= SpawnInterval && EnemiesSpawned < EnemiesPerWave)
                {
                    spawnTimer = 0f;
                    EnemiesSpawned++;
                    if (currentPath != null && currentPath.Length > 0)
                    {
                        enemies.Add(new Enemy(currentPath, EnemyHealthMultiplier));
                    }
                }

                // Конец волны
                if (EnemiesSpawned >= EnemiesPerWave && enemies.Count == 0)
                {
                    IsWaveActive = false;
                    CurrentWave++;
                    WaveTimer = 0f;
                }
            }

            // Обновление врагов
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Update((float)MainApp.DeltaTime);

                if (!enemies[i].IsActive && enemies[i].PathIndex >= enemies[i].path.Length)
                {
                    BaseHP--;
                }
            }

            // Обновление башен
            foreach (var tower in towers)
            {
                tower.Update(enemies, (float)MainApp.DeltaTime, projectiles);
            }
            foreach (var projectile in projectiles)
            {
                projectile.Update((float)MainApp.DeltaTime);
            }
            projectiles.RemoveAll(p => !p.IsActive);
            // Размещение башни (цифра 1-3)
            if (Input.IsKeyPressed((uint)'1')) selectedTowerType = 1;
            if (Input.IsKeyPressed((uint)'2')) selectedTowerType = 2;
            if (Input.IsKeyPressed((uint)'3')) selectedTowerType = 3;

            // Размещение башни на ЛКМ (если не в меню)
            if (Input.IsKeyDown((uint)'Q'))
            {
                HandleTowerPlacement();
            }

            // Открытие меню башни на ПКМ
            if (Input.IsKeyPressed((uint)'E'))
            {
                HandleTowerMenu();
            }

            // Выбор режима наведения (цифры 4-9)
            if (selectedTower != null && selectedTower.ShowMenu)
            {
                if (Input.IsKeyPressed((uint)'4')) selectedTower.SetTargetingMode(0);
                if (Input.IsKeyPressed((uint)'5')) selectedTower.SetTargetingMode(1);
                if (Input.IsKeyPressed((uint)'6')) selectedTower.SetTargetingMode(2);
                if (Input.IsKeyPressed((uint)'7')) selectedTower.SetTargetingMode(3);
                if (Input.IsKeyPressed((uint)'8')) selectedTower.SetTargetingMode(4);
                if (Input.IsKeyPressed((uint)'9')) selectedTower.SetTargetingMode(5);
            }

            // Очистка неактивных врагов
            foreach (var en in enemies)
            {
                if (!en.IsActive)
                    Money += 12;
            }
            enemies.RemoveAll(e => !e.IsActive);

            // Проверка проигрыша
            if (BaseHP <= 0)
            {
                // Game Over
            }
        }

        private void HandleTowerPlacement()
        {
            float worldMouseX = (Input.VirtualMouseX / camera.Zoom) + camera.X;
            float worldMouseY = (Input.VirtualMouseY / camera.Zoom) + camera.Y;

            int tileX = (int)(worldMouseX / 50);
            int tileY = (int)(worldMouseY / 50);

            // Проверка что есть платформа
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

            // Проверка что ещё нет башни
            foreach (var tower in towers)
            {
                if (tower.X == tileX && tower.Y == tileY)
                    return;
            }

            // Проверка денег
            int cost = towerCosts[selectedTowerType];
            if (Money >= cost)
            {
                Money -= cost;
                towers.Add(new Tower((short)tileX, (short)tileY, cost));
            }
        }

        private void HandleTowerMenu()
        {    
            float worldMouseX = (Input.MouseX / camera.Zoom) + camera.X;
            float worldMouseY = (Input.MouseY / camera.Zoom) + camera.Y;

            // Закрыть все меню
            foreach (var tower in towers)
            {
                tower.ShowMenu = false;
            }

            // Найти башню под курсором
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


            // Отрисовка карты с учётом камеры
            foreach (var tile in currentMap.Tiles)
            {
                // Позиция тайла с учётом камеры
                float tileX = (tile.X * 50 - camera.X) * camera.Zoom;
                float tileY = (tile.Y * 50 - camera.Y) * camera.Zoom;
                float tileSize = 50 * camera.Zoom;

                // Отрисовка только видимых тайлов (оптимизация)
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

            // Отрисовка пути (для отладки)
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
            // UI (без влияния камеры)
            iconsFont.DrawText("Y", 10, 15, 1, 1, 0, 0);
            gameFont.DrawText(BaseHP.ToString(), 50, 20, 1);
            iconsFont.DrawText("n", 10, 65, 1, 1, 1, 0);
            gameFont.DrawText(Money.ToString(), 50, 70, 1);
            iconsFont2.DrawText("h", 10, 115, 1, 0.2f, 0.2f, 1);
            gameFont.DrawText(CurrentWave.ToString(), 50, 120, 1);
            gameFont.DrawText($"Enemies: {enemies.Count}", 10, 800, 0.5f, 1f, 1f, 1f);
            gameFont.DrawText($"Tower: {selectedTowerType} (Cost: {towerCosts[selectedTowerType]})", 10, 150, 0.5f, 0.8f, 0.8f, 0.8f);
            gameFont.DrawText("LMB: Place, RMB: Menu", 10, 680, 0.4f, 0.6f, 0.6f, 0.6f);
            if (IsWaveActive)
            {
                gameFont.DrawText($"Enemies: {EnemiesSpawned}/{EnemiesPerWave}", 10, 740, 0.6f, 1f, 1f, 1f);
            }
            else
            {
                gameFont.DrawText($"Next wave: {WaveInterval - WaveTimer:F1}s", 10, 740, 0.6f, 0.8f, 0.8f, 0.8f);
            }

        }
    }
}