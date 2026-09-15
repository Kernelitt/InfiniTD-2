using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public struct PortalSettings
    {
        float Difficulty;
        EnemyTypes[] EnemyList;
    }

    public enum EnemyTypes
    {
        Basic
    }
    public struct Map
    {
        public Tile[] Tiles;
        public float Difficulty;
    }
    public struct Tile
    {
        public short X;
        public short Y;
        public byte Id;
    }
}
