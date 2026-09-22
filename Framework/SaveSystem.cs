using InfiniTD_2.GameRelated;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    // Перечисление для выбора нужной настройки
    public enum SettingType
    {
        MasterVolume,
        MusicVolume,
        SfxVolume,
        ScreenWidth,
        ScreenHeight,
        Fullscreen
    }

    [Serializable]
    public class SaveData
    {
        public float MasterVolume;
        public float MusicVolume;
        public float SfxVolume;
        public int ScreenWidth;
        public int ScreenHeight;
        public bool Fullscreen;
        public int TotalCrystals; // Накопленные кристаллы
        public int MapsUnlocked;  // Количество открытых карт

        public MainGame.GameState GameState;

        public SaveData()
        {
            MasterVolume = 1.0f;
            MusicVolume = 0.8f;
            SfxVolume = 1.0f;
            ScreenWidth = 1600;
            ScreenHeight = 900;
            Fullscreen = false;
        }
    }

    private const string savePath = "save.dat";


    public static void SaveSetting<T>(SettingType setting, T value)
    {
        SaveData data = Load();

        switch (setting)
        {
            case SettingType.MasterVolume:
                data.MasterVolume = (float)(object)value;
                break;
            case SettingType.MusicVolume:
                data.MusicVolume = (float)(object)value;
                break;
            case SettingType.SfxVolume:
                data.SfxVolume = (float)(object)value;
                break;
            case SettingType.ScreenWidth:
                data.ScreenWidth = (int)(object)value;
                break;
            case SettingType.ScreenHeight:
                data.ScreenHeight = (int)(object)value;
                break;
            case SettingType.Fullscreen:
                data.Fullscreen = (bool)(object)value;
                break;
        }

        Save(data);
    }

    public static T LoadSetting<T>(SettingType setting)
    {
        SaveData data = Load();

        switch (setting)
        {
            case SettingType.MasterVolume:
                return (T)(object)data.MasterVolume;
            case SettingType.MusicVolume:
                return (T)(object)data.MusicVolume;
            case SettingType.SfxVolume:
                return (T)(object)data.SfxVolume;
            case SettingType.ScreenWidth:
                return (T)(object)data.ScreenWidth;
            case SettingType.ScreenHeight:
                return (T)(object)data.ScreenHeight;
            case SettingType.Fullscreen:
                return (T)(object)data.Fullscreen;
            default:
                return default;
        }
    }


    public static void SaveGame(MainGame game)
    {
        SaveData data = Load();
        data.GameState = game.GetSaveData();
        Save(data);
    }

    public static void LoadGame(MainGame game)
    {
        SaveData data = Load();
        if (data.GameState != null)
        {
            game.LoadFromData(data.GameState);
        }
    }

    public static void SaveCrystals(int ammount)
    {
        SaveData data = Load();
        data.TotalCrystals += ammount;
        Save(data);
    }

    public static int LoadCrystals()
    {
        SaveData data = Load();
        return data.TotalCrystals;
    }

    public static void Save(SaveData data)
    {
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(savePath, FileMode.Create))
            {
                formatter.Serialize(fs, data);
            }
            Console.WriteLine("Game saved successfully");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Save failed: {e.Message}");
        }
    }

    public static SaveData Load()
    {
        try
        {
            if (!File.Exists(savePath))
            {
                return new SaveData();
            }

            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(savePath, FileMode.Open))
            {
                return (SaveData)formatter.Deserialize(fs);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Load failed: {e.Message}");
            return new SaveData();
        }
    }

    public static bool SaveExists() => File.Exists(savePath);

    public static void Delete()
    {
        try
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Console.WriteLine("Save deleted");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Delete failed: {e.Message}");
        }
    }
}
