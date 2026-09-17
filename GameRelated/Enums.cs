using System;

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
    public struct Map
    {
        public Tile[] Tiles;
        public float Difficulty;
    }
    [Serializable]
    public struct Tile
    {
        public short X;
        public short Y;
        public byte Id;
    }
}
