using System;
using System.IO;

namespace InfiniTD_2.GameRelated
{
    public enum MenuScenes
    {
        Main,
        LevelSelect,
        Game,
        LevelEditor
    }

    public static class MainMenu
    {
        public static MenuScenes CurrentScene = MenuScenes.Main;
        private static FontInstance defaultFont = FontManager.GetFont("Consolas", 72, 72, 108);
        private static FontInstance buttonFont = FontManager.GetFont("Cambria", 48, 48, 72);

        private static UIButton playButton = new UIButton(1200, 600, 380, 60, "Play")
        {
            OnClick = () => { CurrentScene = MenuScenes.LevelSelect; LoadMapList(); }
        };
        private static UIButton editorButton = new UIButton(1200, 680, 380, 60, "Level Editor")
        {
            OnClick = () => { LevelEditor.OpenEditor(); CurrentScene = MenuScenes.LevelEditor; }
        };

        private static UIButton levelSelectBackButton = new UIButton(10, 30, 100, 40, "<=")
        {
            OnClick = () => { CurrentScene = MenuScenes.Main; }
        };

        private static UIButton gameBackButton = new UIButton(10, 830, 60, 60, "<=")
        {
            OnClick = () => { CurrentScene = MenuScenes.Main; }
        };

        private static MainGame gameInstance;
        private static UIButton[] levelButtons = new UIButton[0];
        private static string[] mapFiles = new string[0];

        public static void Init()
        {
            LoadMapList();
        }

        private static void LoadMapList()
        {
            // Поиск .map файлов в корневой папке игры
            string gameDirectory = AppDomain.CurrentDomain.BaseDirectory;
            mapFiles = MapLoader.FindMapFiles(gameDirectory);

            // Создание кнопок для каждой карты
            levelButtons = new UIButton[mapFiles.Length];
            for (int i = 0; i < mapFiles.Length; i++)
            {
                string mapName = MapLoader.GetMapName(mapFiles[i]);
                int buttonIndex = i;

                levelButtons[i] = new UIButton(10, 700 - i * 90, 400, 80, mapName)
                {
                    OnClick = () =>
                    {
                        CurrentScene = MenuScenes.Game;
                        gameInstance = new MainGame(mapFiles[buttonIndex]);
                    }
                };
            }
        }

        public static void Update()
        {
            switch (CurrentScene)
            {
                case MenuScenes.Main:
                    playButton.Update();
                    editorButton.Update();
                    break;
                case MenuScenes.LevelSelect:
                    levelSelectBackButton.Update();
                    foreach (var button in levelButtons)
                        button.Update();
                    break;
                case MenuScenes.Game:
                    gameInstance.Update();
                    gameBackButton.Update();
                    break;
                case MenuScenes.LevelEditor:
                    LevelEditor.Update();
                    levelSelectBackButton.Update();
                    break;
            }
        }

        public static void Draw()
        {
            switch (CurrentScene)
            {
                case MenuScenes.Main:
                    defaultFont.DrawText("InfiniTD 2", 1200, 500, 1);
                    playButton.Draw(buttonFont);
                    editorButton.Draw(buttonFont);
                    break;
                case MenuScenes.LevelSelect:
                    levelSelectBackButton.Draw(buttonFont);
                    defaultFont.DrawText("Select Level", 10, 600, 0.7f);
                    foreach (var button in levelButtons)
                        button.Draw(buttonFont);
                    break;
                case MenuScenes.Game:
                    gameInstance.Draw();
                    gameBackButton.Draw(buttonFont);
                    break;
                case MenuScenes.LevelEditor:
                    LevelEditor.Draw();
                    levelSelectBackButton.Draw(buttonFont);
                    break;
            }
        }
    }
}