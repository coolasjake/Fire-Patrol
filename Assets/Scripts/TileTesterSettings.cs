using System;
using UnityEngine;

namespace FirePatrol
{
    [Serializable]
    public class TileInfo
    {
        public GameObject Prefab;
        public float Rotation;
    }

    [Serializable]
    public class TileTesterSettings
    {
        public TileInfo[] GrassTiles;
        public TileInfo[] SandTiles;
    }
}
