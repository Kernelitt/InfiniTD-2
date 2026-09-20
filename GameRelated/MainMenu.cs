using System;
using InfiniTD_2.Framework.Audio;
using InfiniTD_2.Framework;

namespace InfiniTD_2.GameRelated
{
    public enum MenuScenes
    {
        Main,
        LevelSelect,
        Game,
        Settings,
        LevelEditor
    }

    public static class MainMenu
    {
        public static MenuScenes CurrentScene = MenuScenes.Main;
        public static readonly FontInstance defaultFont = FontManager.GetFont("Consolas", 72, 72, 108);
        private static readonly FontInstance buttonFont = FontManager.GetFont("Cambria", 48, 48, 72);
        private static readonly FontInstance iconsFont = FontManager.GetFont("Webdings", 100, 108, 108);

        // Main menu
        private static readonly UIButton playButton = new UIButton(1200, 600, 380, 60, "Play")
        {
            OnClick = () => { CurrentScene = MenuScenes.LevelSelect; LoadMapList(); }
        };
        private static readonly UIButton editorButton = new UIButton(1200, 680, 380, 60, "Level Editor")
        {
            OnClick = () => { LevelEditor.OpenEditor(); CurrentScene = MenuScenes.LevelEditor; }
        };
        private static readonly UIButton settingsButton = new UIButton(1200, 760, 380, 60, "Settings")
        {
            OnClick = () => { CurrentScene = MenuScenes.Settings; }
        };
        private static readonly UIButton continueButton = new UIButton(1100, 600, 90, 60, "8")
        {
            OnClick = () => {
                CurrentScene = MenuScenes.Game;
                gameInstance = new MainGame();
                SaveSystem.LoadGame(gameInstance);
            }
        };

        // Settings menu
        private static readonly UISlider musicSlider = new UISlider(10, 10, 380, 30)
        {
            OnChanged = (Value) => { MusicPlayer.MusicVolume = Value; MusicPlayer.PlayMainMusic(); SaveSystem.SaveSetting(SaveSystem.SettingType.MusicVolume, Value); },
            MinValue = 0f,
            MaxValue = 1f,
            Value = 0.8f
        };
        private static readonly UISlider sfxSlider = new UISlider(10, 60, 380, 30)
        {
            OnChanged = (Value) => { MusicPlayer.SfxVolume = Value; SaveSystem.SaveSetting(SaveSystem.SettingType.SfxVolume, Value); },
            MinValue = 0f,
            MaxValue = 1f,
            Value = 0.6f
        };

        // Universal back button
        private static readonly UIButton backButton = new UIButton(10, 830, 60, 60, "3")
        {
            OnClick = () => { CurrentScene = MenuScenes.Main; }
        };

        private static MainGame gameInstance;
        private static UIButton[] levelButtons = new UIButton[0];
        private static string[] mapFiles = new string[0];

        public static void Init()
        {
            MusicPlayer.MusicVolume = SaveSystem.LoadSetting<float>(SaveSystem.SettingType.MusicVolume);
            musicSlider.Value = MusicPlayer.MusicVolume;

            MusicPlayer.SfxVolume = SaveSystem.LoadSetting<float>(SaveSystem.SettingType.SfxVolume);
            sfxSlider.Value = MusicPlayer.SfxVolume;

            MusicPlayer.PlayMainMusic();
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

                levelButtons[i] = new UIButton(1100, 600 - i * 90, 400, 80, mapName)
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
                    continueButton.Update();
                    editorButton.Update();
                    settingsButton.Update();
                    break;

                case MenuScenes.LevelSelect:
                    backButton.Update();
                    foreach (var button in levelButtons)
                        button.Update();
                    break;

                case MenuScenes.Settings:
                    musicSlider.Update();
                    sfxSlider.Update();
                    backButton.Update();
                    break;

                case MenuScenes.Game:
                    gameInstance.Update();
                    break;

                case MenuScenes.LevelEditor:
                    LevelEditor.Update();
                    backButton.Update();
                    break;

            }
        }

        public static void Draw()
        {
            switch (CurrentScene)
            {
                case MenuScenes.Main:
                    defaultFont.DrawText("InfiniTD 2", 1200, 500, 1);

                    defaultFont.DrawText($"InfiniTD 2 {Program.AppVersion}", 0, 0, 0.5f);
#if !NO_MOD_SUPPORT
                    
                    for (int i = 0; i < ModManager.loadedMods.Count; i++)
                        defaultFont.DrawText($"{ModManager.loadedMods[i].Id} - {ModManager.loadedMods[i].Name} - {ModManager.loadedMods[i].Author}", 0, 20 + 20 * i, 0.5f);
#endif

                    playButton.Draw(buttonFont);
                    continueButton.Draw(iconsFont);
                    editorButton.Draw(buttonFont);
                    settingsButton.Draw(buttonFont);
                    break;

                case MenuScenes.LevelSelect:
                    backButton.Draw(iconsFont);
                    defaultFont.DrawText("Select Level", 10, 600, 0.7f);
                    foreach (var button in levelButtons)
                        button.Draw(buttonFont);
                    break;

                case MenuScenes.Settings:
                    musicSlider.Draw(buttonFont);
                    sfxSlider.Draw(buttonFont);
                    backButton.Draw(iconsFont);
                    defaultFont.DrawText("Music Volume", 10, 10, 0.7f);
                    defaultFont.DrawText("Sfx Volume", 10, 60, 0.7f);
                    break;

                case MenuScenes.Game:
                    gameInstance.Draw();
                    break;

                case MenuScenes.LevelEditor:
                    LevelEditor.Draw();
                    backButton.Draw(iconsFont);
                    break;
            }
            defaultFont.DrawText("FPS:" + ((int)(1f / MainApp.DeltaTime)).ToString(),10,880, 0.5f);
        }
    }
}