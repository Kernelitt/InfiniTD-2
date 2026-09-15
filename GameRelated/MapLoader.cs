using System;
using System.IO;
using System.Collections.Generic;

namespace InfiniTD_2.GameRelated
{
    public static class MapLoader
    {
        // Загрузка карты из бинарного файла
        public static Map LoadMap(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Map file not found: {filePath}");

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(stream))
            {
                Map map = new Map();

                // Читаем сложность
                map.Difficulty = reader.ReadSingle();

                // Читаем количество тайлов
                int tileCount = reader.ReadInt32();

                // Читаем тайлы
                map.Tiles = new Tile[tileCount];
                for (int i = 0; i < tileCount; i++)
                {
                    map.Tiles[i] = new Tile
                    {
                        X = reader.ReadInt16(),
                        Y = reader.ReadInt16(),
                        Id = reader.ReadByte()
                    };
                }

                return map;
            }
        }

        // Сохранение карты в бинарный файл
        public static void SaveMap(string filePath, Map map)
        {
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(stream))
            {
                // Пишем сложность
                writer.Write(map.Difficulty);

                // Пишем количество тайлов
                writer.Write(map.Tiles.Length);

                // Пишем тайлы
                foreach (var tile in map.Tiles)
                {
                    writer.Write(tile.X);
                    writer.Write(tile.Y);
                    writer.Write(tile.Id);
                }
            }
        }

        // Поиск всех .map файлов в папке
        public static string[] FindMapFiles(string directory)
        {
            if (!Directory.Exists(directory))
                return new string[0];

            return Directory.GetFiles(directory, "*.map", SearchOption.AllDirectories);
        }

        // Получение имени карты из пути
        public static string GetMapName(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }
    }
}