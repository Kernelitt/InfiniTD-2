using System;
using System.Collections.Generic;

namespace InfiniTD_2.GameRelated
{
    public enum TileTypes
    {
        None,
        Road,
        Platform,
        Base,
        Portal
    }

    public readonly struct PortalSettings
    {
        readonly float Difficulty;
        readonly EnemyTypes[] EnemyList;
    }

    public enum EnemyTypes
    {
        Basic
    }

    [Serializable]
    public class Map
    {
        public Tile[] Tiles;
        public List<Vector2> Portals = new List<Vector2>(); // Несколько порталов
        public Vector2 Base;
        public float DifficultyMultiplier = 1.0f; // Множитель HP врагов
        public string MapName = "Unknown";
        public int StarsRequired = 0; // Нужно звёзд для открытия
    }

    [Serializable]
    public struct Tile
    {
        public short X;
        public short Y;
        public byte Id;
    }
}
