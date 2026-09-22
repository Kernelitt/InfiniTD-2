using System;
using System.Collections.Generic;
using System.IO;

namespace InfiniTD_2.GameRelated
{
    [Serializable]
    public class CampaignProgress
    {
        public int TotalStars;
        public List<int> MapStars = new List<int>();
        public List<bool> MapsUnlocked = new List<bool>();
    }

    public static class CampaignManager
    {
        private static CampaignProgress progress = new CampaignProgress();
        private const string SAVE_FILE = "campaign.dat";

        // Встроенные карты
        public static readonly Map[] BuiltInMaps = new Map[]
        {
            CreateMap1(),
            CreateMap2(),
            CreateMap3(),
            CreateMap4(),
            CreateMap5(),
        };

        public static void Initialize()
        {
            SaveSystem.LoadCampaign();

            if (progress.MapStars.Count == 0)
            {
                progress.MapStars = new List<int>(new int[BuiltInMaps.Length]);
                progress.MapsUnlocked = new List<bool>(new bool[BuiltInMaps.Length]);
                progress.MapsUnlocked[0] = true;
                SaveSystem.SaveCampaign(progress);
            }
        }

        public static Map GetMap(int mapId)
        {
            if (mapId >= 0 && mapId < BuiltInMaps.Length)
                return BuiltInMaps[mapId];
            return null;
        }

        public static int GetTotalMaps()
        {
            return BuiltInMaps.Length;
        }

        // Метод для получения ID карты по имени
        public static int GetMapIdByName(string mapName)
        {
            for (int i = 0; i < BuiltInMaps.Length; i++)
            {
                if (BuiltInMaps[i].MapName == mapName)
                    return i;
            }
            return -1;
        }

        private static Map CreateMap1()
        {
            return new Map
            {
                MapName = "First Steps",
                DifficultyMultiplier = 1.0f,
                StarsRequired = 0,
                Tiles = new Tile[]
                {
                    new Tile { X = 0, Y = 5, Id = 4 },  // Portal
                    new Tile { X = 1, Y = 5, Id = 1 },  // Road
                    new Tile { X = 2, Y = 5, Id = 1 },
                    new Tile { X = 3, Y = 5, Id = 1 },
                    new Tile { X = 4, Y = 5, Id = 1 },
                    new Tile { X = 5, Y = 5, Id = 1 },
                    new Tile { X = 5, Y = 4, Id = 1 },
                    new Tile { X = 5, Y = 3, Id = 1 },
                    new Tile { X = 5, Y = 2, Id = 1 },
                    new Tile { X = 5, Y = 1, Id = 1 },
                    new Tile { X = 5, Y = 0, Id = 1 },
                    new Tile { X = 6, Y = 0, Id = 1 },
                    new Tile { X = 7, Y = 0, Id = 1 },
                    new Tile { X = 8, Y = 0, Id = 1 },
                    new Tile { X = 9, Y = 0, Id = 1 },
                    new Tile { X = 10, Y = 0, Id = 1 },
                    new Tile { X = 10, Y = 1, Id = 1 },
                    new Tile { X = 10, Y = 2, Id = 1 },
                    new Tile { X = 10, Y = 3, Id = 1 },
                    new Tile { X = 10, Y = 4, Id = 1 },
                    new Tile { X = 10, Y = 5, Id = 1 },
                    new Tile { X = 11, Y = 5, Id = 1 },
                    new Tile { X = 12, Y = 5, Id = 1 },
                    new Tile { X = 13, Y = 5, Id = 1 },
                    new Tile { X = 14, Y = 5, Id = 1 },
                    new Tile { X = 15, Y = 5, Id = 3 },  // Base
                    // Platforms
                    new Tile { X = 3, Y = 4, Id = 2 },
                    new Tile { X = 3, Y = 6, Id = 2 },
                    new Tile { X = 7, Y = 1, Id = 2 },
                    new Tile { X = 7, Y = -1, Id = 2 },
                    new Tile { X = 12, Y = 4, Id = 2 },
                    new Tile { X = 12, Y = 6, Id = 2 },
                },
                Portals = new List<Vector2> { new Vector2(0, 5) },
                Base = new Vector2(15, 5)
            };
        }

        private static Map CreateMap2()
        {
            return new Map
            {
                MapName = "Double Trouble",
                DifficultyMultiplier = 1.2f,
                StarsRequired = 1,
                Tiles = new Tile[]
                {
                    // Portal 1
                    new Tile { X = 0, Y = 3, Id = 4 },
                    new Tile { X = 1, Y = 3, Id = 1 },
                    new Tile { X = 2, Y = 3, Id = 1 },
                    new Tile { X = 3, Y = 3, Id = 1 },
                    new Tile { X = 4, Y = 3, Id = 1 },
                    new Tile { X = 5, Y = 3, Id = 1 },
                    new Tile { X = 5, Y = 4, Id = 1 },
                    new Tile { X = 5, Y = 5, Id = 1 },
                    new Tile { X = 5, Y = 6, Id = 1 },
                    new Tile { X = 5, Y = 7, Id = 1 },
                    new Tile { X = 5, Y = 8, Id = 1 },
                    new Tile { X = 6, Y = 8, Id = 1 },
                    new Tile { X = 7, Y = 8, Id = 1 },
                    new Tile { X = 8, Y = 8, Id = 1 },
                    new Tile { X = 9, Y = 8, Id = 1 },
                    new Tile { X = 10, Y = 8, Id = 3 },  // Base
                    
                    // Portal 2
                    new Tile { X = 0, Y = 11, Id = 4 },
                    new Tile { X = 1, Y = 11, Id = 1 },
                    new Tile { X = 2, Y = 11, Id = 1 },
                    new Tile { X = 3, Y = 11, Id = 1 },
                    new Tile { X = 4, Y = 11, Id = 1 },
                    new Tile { X = 5, Y = 11, Id = 1 },
                    new Tile { X = 6, Y = 11, Id = 1 },
                    new Tile { X = 7, Y = 11, Id = 1 },
                    new Tile { X = 8, Y = 11, Id = 1 },
                    new Tile { X = 9, Y = 11, Id = 1 },
                    new Tile { X = 10, Y = 11, Id = 1 },
                    new Tile { X = 10, Y = 10, Id = 1 },
                    new Tile { X = 10, Y = 9, Id = 1 },
                    
                    // Platforms
                    new Tile { X = 3, Y = 2, Id = 2 },
                    new Tile { X = 3, Y = 5, Id = 2 },
                    new Tile { X = 3, Y = 9, Id = 2 },
                    new Tile { X = 3, Y = 12, Id = 2 },
                    new Tile { X = 7, Y = 4, Id = 2 },
                    new Tile { X = 7, Y = 10, Id = 2 },
                },
                Portals = new List<Vector2>
                {
                    new Vector2(0, 3),
                    new Vector2(0, 11)
                },
                Base = new Vector2(10, 8)
            };
        }

        private static Map CreateMap3()
        {
            return new Map
            {
                MapName = "Spiral",
                DifficultyMultiplier = 1.4f,
                StarsRequired = 2,
                Tiles = new Tile[]
                {
                    new Tile { X = 0, Y = 0, Id = 4 },
                    new Tile { X = 1, Y = 0, Id = 1 },
                    new Tile { X = 2, Y = 0, Id = 1 },
                    new Tile { X = 3, Y = 0, Id = 1 },
                    new Tile { X = 4, Y = 0, Id = 1 },
                    new Tile { X = 4, Y = 1, Id = 1 },
                    new Tile { X = 4, Y = 2, Id = 1 },
                    new Tile { X = 4, Y = 3, Id = 1 },
                    new Tile { X = 4, Y = 4, Id = 1 },
                    new Tile { X = 3, Y = 4, Id = 1 },
                    new Tile { X = 2, Y = 4, Id = 1 },
                    new Tile { X = 1, Y = 4, Id = 1 },
                    new Tile { X = 0, Y = 4, Id = 1 },
                    new Tile { X = 0, Y = 5, Id = 1 },
                    new Tile { X = 0, Y = 6, Id = 1 },
                    new Tile { X = 0, Y = 7, Id = 1 },
                    new Tile { X = 0, Y = 8, Id = 1 },
                    new Tile { X = 1, Y = 8, Id = 1 },
                    new Tile { X = 2, Y = 8, Id = 1 },
                    new Tile { X = 3, Y = 8, Id = 1 },
                    new Tile { X = 4, Y = 8, Id = 1 },
                    new Tile { X = 5, Y = 8, Id = 1 },
                    new Tile { X = 6, Y = 8, Id = 1 },
                    new Tile { X = 7, Y = 8, Id = 1 },
                    new Tile { X = 8, Y = 8, Id = 1 },
                    new Tile { X = 8, Y = 7, Id = 1 },
                    new Tile { X = 8, Y = 6, Id = 1 },
                    new Tile { X = 8, Y = 5, Id = 1 },
                    new Tile { X = 8, Y = 4, Id = 1 },
                    new Tile { X = 8, Y = 3, Id = 1 },
                    new Tile { X = 8, Y = 2, Id = 1 },
                    new Tile { X = 8, Y = 1, Id = 1 },
                    new Tile { X = 8, Y = 0, Id = 1 },
                    new Tile { X = 9, Y = 0, Id = 1 },
                    new Tile { X = 10, Y = 0, Id = 1 },
                    new Tile { X = 11, Y = 0, Id = 1 },
                    new Tile { X = 12, Y = 0, Id = 1 },
                    new Tile { X = 12, Y = 1, Id = 1 },
                    new Tile { X = 12, Y = 2, Id = 1 },
                    new Tile { X = 12, Y = 3, Id = 1 },
                    new Tile { X = 12, Y = 4, Id = 3 },
                    // Platforms
                    new Tile { X = 2, Y = 2, Id = 2 },
                    new Tile { X = 6, Y = 2, Id = 2 },
                    new Tile { X = 2, Y = 6, Id = 2 },
                    new Tile { X = 6, Y = 6, Id = 2 },
                    new Tile { X = 10, Y = 2, Id = 2 },
                    new Tile { X = 10, Y = 6, Id = 2 },
                },
                Portals = new List<Vector2> { new Vector2(0, 0) },
                Base = new Vector2(12, 4)
            };
        }

        private static Map CreateMap4()
        {
            return new Map
            {
                MapName = "Crossfire",
                DifficultyMultiplier = 1.6f,
                StarsRequired = 3,
                Tiles = new Tile[]
                {
                    // Верхний путь
                    new Tile { X = 0, Y = 2, Id = 4 },
                    new Tile { X = 1, Y = 2, Id = 1 },
                    new Tile { X = 2, Y = 2, Id = 1 },
                    new Tile { X = 3, Y = 2, Id = 1 },
                    new Tile { X = 4, Y = 2, Id = 1 },
                    new Tile { X = 5, Y = 2, Id = 1 },
                    new Tile { X = 6, Y = 2, Id = 1 },
                    new Tile { X = 7, Y = 2, Id = 1 },
                    new Tile { X = 8, Y = 2, Id = 1 },
                    new Tile { X = 9, Y = 2, Id = 1 },
                    new Tile { X = 10, Y = 2, Id = 1 },
                    new Tile { X = 10, Y = 3, Id = 1 },
                    new Tile { X = 10, Y = 4, Id = 1 },
                    new Tile { X = 10, Y = 5, Id = 3 },
                    
                    // Нижний путь
                    new Tile { X = 0, Y = 7, Id = 4 },
                    new Tile { X = 1, Y = 7, Id = 1 },
                    new Tile { X = 2, Y = 7, Id = 1 },
                    new Tile { X = 3, Y = 7, Id = 1 },
                    new Tile { X = 4, Y = 7, Id = 1 },
                    new Tile { X = 5, Y = 7, Id = 1 },
                    new Tile { X = 6, Y = 7, Id = 1 },
                    new Tile { X = 7, Y = 7, Id = 1 },
                    new Tile { X = 8, Y = 7, Id = 1 },
                    new Tile { X = 9, Y = 7, Id = 1 },
                    new Tile { X = 10, Y = 7, Id = 1 },
                    new Tile { X = 10, Y = 6, Id = 1 },
                    
                    // Platforms
                    new Tile { X = 2, Y = 1, Id = 2 },
                    new Tile { X = 2, Y = 4, Id = 2 },
                    new Tile { X = 2, Y = 6, Id = 2 },
                    new Tile { X = 2, Y = 9, Id = 2 },
                    new Tile { X = 5, Y = 1, Id = 2 },
                    new Tile { X = 5, Y = 4, Id = 2 },
                    new Tile { X = 5, Y = 6, Id = 2 },
                    new Tile { X = 5, Y = 9, Id = 2 },
                    new Tile { X = 8, Y = 1, Id = 2 },
                    new Tile { X = 8, Y = 4, Id = 2 },
                    new Tile { X = 8, Y = 6, Id = 2 },
                    new Tile { X = 8, Y = 9, Id = 2 },
                },
                Portals = new List<Vector2>
                {
                    new Vector2(0, 2),
                    new Vector2(0, 7)
                },
                Base = new Vector2(10, 5)
            };
        }

        private static Map CreateMap5()
        {
            return new Map
            {
                MapName = "Final Stand",
                DifficultyMultiplier = 2.0f,
                StarsRequired = 4,
                Tiles = new Tile[]
                {
                    // S-образный путь
                    new Tile { X = 0, Y = 0, Id = 4 },
                    new Tile { X = 1, Y = 0, Id = 1 },
                    new Tile { X = 2, Y = 0, Id = 1 },
                    new Tile { X = 3, Y = 0, Id = 1 },
                    new Tile { X = 4, Y = 0, Id = 1 },
                    new Tile { X = 5, Y = 0, Id = 1 },
                    new Tile { X = 5, Y = 1, Id = 1 },
                    new Tile { X = 5, Y = 2, Id = 1 },
                    new Tile { X = 5, Y = 3, Id = 1 },
                    new Tile { X = 5, Y = 4, Id = 1 },
                    new Tile { X = 4, Y = 4, Id = 1 },
                    new Tile { X = 3, Y = 4, Id = 1 },
                    new Tile { X = 2, Y = 4, Id = 1 },
                    new Tile { X = 1, Y = 4, Id = 1 },
                    new Tile { X = 0, Y = 4, Id = 1 },
                    new Tile { X = 0, Y = 5, Id = 1 },
                    new Tile { X = 0, Y = 6, Id = 1 },
                    new Tile { X = 0, Y = 7, Id = 1 },
                    new Tile { X = 0, Y = 8, Id = 1 },
                    new Tile { X = 1, Y = 8, Id = 1 },
                    new Tile { X = 2, Y = 8, Id = 1 },
                    new Tile { X = 3, Y = 8, Id = 1 },
                    new Tile { X = 4, Y = 8, Id = 1 },
                    new Tile { X = 5, Y = 8, Id = 1 },
                    new Tile { X = 6, Y = 8, Id = 1 },
                    new Tile { X = 7, Y = 8, Id = 1 },
                    new Tile { X = 8, Y = 8, Id = 1 },
                    new Tile { X = 9, Y = 8, Id = 1 },
                    new Tile { X = 10, Y = 8, Id = 1 },
                    new Tile { X = 11, Y = 8, Id = 1 },
                    new Tile { X = 12, Y = 8, Id = 1 },
                    new Tile { X = 13, Y = 8, Id = 1 },
                    new Tile { X = 14, Y = 8, Id = 1 },
                    new Tile { X = 15, Y = 8, Id = 3 },
                    // Platforms
                    new Tile { X = 2, Y = 2, Id = 2 },
                    new Tile { X = 3, Y = 2, Id = 2 },
                    new Tile { X = 2, Y = 6, Id = 2 },
                    new Tile { X = 3, Y = 6, Id = 2 },
                    new Tile { X = 7, Y = 2, Id = 2 },
                    new Tile { X = 8, Y = 2, Id = 2 },
                    new Tile { X = 7, Y = 6, Id = 2 },
                    new Tile { X = 8, Y = 6, Id = 2 },
                    new Tile { X = 11, Y = 2, Id = 2 },
                    new Tile { X = 12, Y = 2, Id = 2 },
                    new Tile { X = 11, Y = 6, Id = 2 },
                    new Tile { X = 12, Y = 6, Id = 2 },
                },
                Portals = new List<Vector2> { new Vector2(0, 0) },
                Base = new Vector2(15, 8)
            };
        }

        // ... остальные методы (LoadProgress, SaveProgress, etc.) ...
    }
}