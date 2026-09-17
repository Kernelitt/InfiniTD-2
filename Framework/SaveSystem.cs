using InfiniTD_2.GameRelated;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    [Serializable]
    public class SaveData
    {
        public float MasterVolume;
        public float MusicVolume;
        public float SfxVolume;
        public int ScreenWidth;
        public int ScreenHeight;
        public bool Fullscreen;

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

    private static string savePath = "save.dat";

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

    public static bool SaveExists()
    {
        return File.Exists(savePath);
    }

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