using System;
using System.Windows.Forms;

namespace InfiniTD_2.GameRelated
{
    public static class LevelEditor
    {
        private static bool isActive = false;
        private static Map currentMap;
        private static byte selectedTileId = 1;
        private static Camera camera;
        private static FontInstance editorFont;
        private static FontInstance iconsFont;
        private static bool firstTimeInEditor = true;

        // Палитра тайлов
        private static readonly string[] tileNames = new string[]
        {
            "None", "Road", "Platform", "Base", "Portal"
        };

        public static bool IsActive => isActive;

        public static void Init()
        {
            camera = new Camera();
            editorFont = FontManager.GetFont("Consolas", 60, 60, 84);
            iconsFont = FontManager.GetFont("Webdings", 48, 72, 72);
        }

        public static void OpenEditor()
        {
            isActive = true;
            currentMap = new Map
            {
                Difficulty = 1.0f,
                Tiles = new Tile[0]
            };
            camera.SetPosition(0, 0);
            camera.Zoom = 1f;
        }

        public static void OpenEditor(Map map)
        {
            isActive = true;
            currentMap = map;
            camera.SetPosition(0, 0);
            camera.Zoom = 1f;
        }

        public static void CloseEditor()
        {
            isActive = false;
            currentMap = new Map
            {
                Difficulty = 1.0f,
                Tiles = new Tile[0]
            };
        }

        public static void Update()
        {
            if (!isActive) return;

            camera.Update();

            // Выбор тайла клавишами 1-4 (используем IsKeyPressed для однократного нажатия)
            if (Input.IsKeyPressed((uint)'1')) selectedTileId = 1;
            if (Input.IsKeyPressed((uint)'2')) selectedTileId = 2;
            if (Input.IsKeyPressed((uint)'3')) selectedTileId = 3;
            if (Input.IsKeyPressed((uint)'4')) selectedTileId = 4;

            // Сохранение на S
            if (Input.IsKeyPressed((uint)'J'))
            {
                SaveMap();
            }

            // Загрузка на L
            if (Input.IsKeyPressed((uint)'L'))
            {
                LoadMap();
            }

            // Размещение/удаление тайлов
            HandleTilePlacement();
        }

        private static void HandleTilePlacement()
        {
            if (Input.IsMouseButtonDown(0) && firstTimeInEditor)
            {
                firstTimeInEditor = false;
                return;
            }

            // Получаем позицию мыши в мире с учётом камеры
            float worldMouseX = (Input.VirtualMouseX / camera.Zoom) + camera.X;
            float worldMouseY = (Input.VirtualMouseY / camera.Zoom) + camera.Y;

            // Вычисляем координаты тайла
            int tileX = (int)((worldMouseX) / 50);
            int tileY = (int)((worldMouseY) / 50);

            // ЛКМ - разместить тайл (удержание)
            if (Input.IsMouseButtonDown(0))
            {
                PlaceTile(tileX, tileY, selectedTileId);
            }

            // ПКМ - удалить тайл (удержание)
            if (Input.IsMouseButtonDown(1))
            {
                RemoveTile(tileX, tileY);
            }
        }

        private static void PlaceTile(int x, int y, byte id)
        {
            if (currentMap.Tiles == null)
            {
                currentMap.Tiles = new Tile[0];
            }

            // Проверяем есть ли уже тайл в этой позиции
            for (int i = 0; i < currentMap.Tiles.Length; i++)
            {
                if (currentMap.Tiles[i].X == x && currentMap.Tiles[i].Y == y)
                {
                    // Обновляем существующий
                    var tile = currentMap.Tiles[i];
                    tile.Id = id;
                    currentMap.Tiles[i] = tile;
                    return;
                }
            }

            // Добавляем новый тайл
            Tile[] newTiles = new Tile[currentMap.Tiles.Length + 1];
            Array.Copy(currentMap.Tiles, newTiles, currentMap.Tiles.Length);
            newTiles[currentMap.Tiles.Length] = new Tile { X = (short)x, Y = (short)y, Id = id };
            currentMap.Tiles = newTiles;
        }

        private static void RemoveTile(int x, int y)
        {
            if (currentMap.Tiles == null || currentMap.Tiles.Length == 0) return;

            // Находим тайл для удаления
            for (int i = 0; i < currentMap.Tiles.Length; i++)
            {
                if (currentMap.Tiles[i].X == x && currentMap.Tiles[i].Y == y)
                {
                    // Удаляем тайл
                    Tile[] newTiles = new Tile[currentMap.Tiles.Length - 1];
                    Array.Copy(currentMap.Tiles, 0, newTiles, 0, i);
                    if (i < currentMap.Tiles.Length - 1)
                    {
                        Array.Copy(currentMap.Tiles, i + 1, newTiles, i, currentMap.Tiles.Length - i - 1);
                    }
                    currentMap.Tiles = newTiles;
                    return;
                }
            }
        }

        private static void SaveMap()
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Map files (*.map)|*.map|All files (*.*)|*.*";
                dialog.Title = "Save Map";
                dialog.FileName = "level.map";

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        MapLoader.SaveMap(dialog.FileName, currentMap);
                    }
                    catch (Exception ex)
                    {
                        // Можно добавить отображение ошибки
                        System.Diagnostics.Debug.WriteLine($"Save error: {ex.Message}");
                    }
                }
            }
        }

        private static void LoadMap()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Map files (*.map)|*.map|All files (*.*)|*.*";
                dialog.Title = "Load Map";

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        currentMap = MapLoader.LoadMap(dialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Load error: {ex.Message}");
                    }
                }
            }
        }

        public static void Draw()
        {

            // Отрисовка сетки
            DrawGrid();

            // Отрисовка тайлов
            foreach (var tile in currentMap.Tiles)
            {
                float tileX = (tile.X * 50 - camera.X) * camera.Zoom;
                float tileY = (tile.Y * 50 - camera.Y) * camera.Zoom;
                float tileSize = 50 * camera.Zoom;

                // Отрисовка только видимых тайлов
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

                // Обводка тайла
                Primitives.DrawLine(tileX, tileY, tileX + tileSize, tileY, 2 * camera.Zoom, 0f, 0f, 0f, 0.5f);
                Primitives.DrawLine(tileX + tileSize, tileY, tileX + tileSize, tileY + tileSize, 2 * camera.Zoom, 0f, 0f, 0f, 0.5f);
                Primitives.DrawLine(tileX + tileSize, tileY + tileSize, tileX, tileY + tileSize, 2 * camera.Zoom, 0f, 0f, 0f, 0.5f);
                Primitives.DrawLine(tileX, tileY + tileSize, tileX, tileY, 2 * camera.Zoom, 0f, 0f, 0f, 0.5f);
            }

            // Курсор-выделение
            DrawCursor();

            // UI редактора
            DrawEditorUI();
        }

        private static void DrawGrid()
        {
            float gridSize = 50 * camera.Zoom;
            int gridWidth = (int)(1600 / gridSize) + 2;
            int gridHeight = (int)(900 / gridSize) + 2;

            float offsetX = -(camera.X * camera.Zoom) % gridSize;
            float offsetY = -(camera.Y * camera.Zoom) % gridSize;

            for (int x = 0; x < gridWidth; x++)
            {
                float gx = offsetX + x * gridSize;
                Primitives.DrawLine(gx, 0, gx, 900, 3 * camera.Zoom, 0.3f, 0.3f, 0.3f, 0.3f);
            }

            for (int y = 0; y < gridHeight; y++)
            {
                float gy = offsetY + y * gridSize;
                Primitives.DrawLine(0, gy, 1600, gy, 3 * camera.Zoom, 0.3f, 0.3f, 0.3f, 0.3f);
            }
        }

        private static void DrawCursor()
        {
            float worldMouseX = (Input.VirtualMouseX / camera.Zoom) + camera.X;
            float worldMouseY = (Input.VirtualMouseY / camera.Zoom) + camera.Y;

            int tileX = (int)(worldMouseX / 50);
            int tileY = (int)(worldMouseY / 50);

            float cursorX = (tileX * 50 - camera.X) * camera.Zoom;
            float cursorY = (tileY * 50 - camera.Y) * camera.Zoom;
            float cursorSize = 50 * camera.Zoom;

            // Цвет курсора в зависимости от выбранного тайла
            float r = 1f, g = 1f, b = 1f;

            Primitives.DrawLine(cursorX, cursorY, cursorX + cursorSize, cursorY, 3 * camera.Zoom, r, g, b, 1f);
            Primitives.DrawLine(cursorX + cursorSize, cursorY, cursorX + cursorSize, cursorY + cursorSize, 3 * camera.Zoom, r, g, b, 1f);
            Primitives.DrawLine(cursorX + cursorSize, cursorY + cursorSize, cursorX, cursorY + cursorSize, 3 * camera.Zoom, r, g, b, 1f);
            Primitives.DrawLine(cursorX, cursorY + cursorSize, cursorX, cursorY, 3 * camera.Zoom, r, g, b, 1f);
        }

        private static void DrawEditorUI()
        {
            // Панель информации
            Primitives.DrawQuad(10, 5, 300, 120, 0f, 0f, 0f, 0.7f);

            editorFont.DrawText("Level Editor", 20, 10, 0.7f, 1f, 1f, 1f);
            editorFont.DrawText($"Tiles: {currentMap.Tiles?.Length ?? 0}", 20, 50, 0.5f, 1f, 1f, 1f);
            editorFont.DrawText($"Selected: {selectedTileId} ({tileNames[selectedTileId]})", 20, 90, 0.5f, 1f, 1f, 1f);

            // Подсказки
            editorFont.DrawText("1-4: Select tile", 10, 300, 0.5f, 0.8f, 0.8f, 0.8f);
            editorFont.DrawText("LMB: Place tile",  10, 340, 0.5f, 0.8f, 0.8f, 0.8f);
            editorFont.DrawText("RMB: Remove tile", 10, 380, 0.5f, 0.8f, 0.8f, 0.8f);
            editorFont.DrawText("J: Save map",      10, 420, 0.5f, 0.8f, 0.8f, 0.8f);
            editorFont.DrawText("L: Load map",      10, 460, 0.5f, 0.8f, 0.8f, 0.8f);
            editorFont.DrawText("WASD: Move camera",10, 500, 0.5f, 0.8f, 0.8f, 0.8f);
            editorFont.DrawText("X,C: Zoom",10, 540, 0.5f, 0.8f, 0.8f, 0.8f);
            editorFont.DrawText("ESC: Exit editor", 10, 580, 0.5f, 0.8f, 0.8f, 0.8f);
        }

        public static Map GetCurrentMap()
        {
            return currentMap;
        }
    }
}