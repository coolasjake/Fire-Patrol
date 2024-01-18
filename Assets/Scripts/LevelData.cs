using UnityEngine;
using System;
using System.Collections.Generic;

namespace FirePatrol
{
    public enum PropType
    {
        Tree1,
        Tree2,
        Flower,
        Mushroom,
        GrassTuft,
        Rock,
    }

    [Serializable]
    public class PropInstance
    {
        public PropType PropType;
        public GameObject GameObject;
    }

    [Serializable]
    public class TileData
    {
        public int TileType;
        public Vector3 CenterPosition;
        public GameObject Model;
        public List<PropInstance> Props;
        public int Id;
        public int Row;
        public int Col;
    }

    public enum PointTypes
    {
        Water,
        Grass,
    }

    [Serializable]
    public class PointData
    {
        public PointTypes Type;
        public Vector3 Position;
        public int Id;
        public int Row;
        public int Col;
    }

    public class LevelData : MonoBehaviour
    {
        const float TILE_UNSCALED_SIZE = 30.0f;

        public int PointsPerRow;
        public float TileScale = 0.3f;
        public float TreesScale = 1.0f;

        public List<TileData> Tiles;
        public List<PointData> Points;

        public float TileSize
        {
            get { return TILE_UNSCALED_SIZE * TileScale; }
        }

        public int TilesPerRow
        {
            get { return PointsPerRow - 1; }
        }

        public TileData TryGetTileData(int i, int k)
        {
            if (i < 0 || i >= TilesPerRow || k < 0 || k >= TilesPerRow)
            {
                return null;
            }

            return Tiles[i * TilesPerRow + k];
        }

        public TileData GetTileDataById(int id)
        {
            return Tiles[id];
        }

        public TileData GetTileData(int i, int k)
        {
            var result = TryGetTileData(i, k);
            Assert.That(result != null);
            return result;
        }

        public PointData TryGetPointData(int i, int k)
        {
            if (i < 0 || i >= PointsPerRow || k < 0 || k >= PointsPerRow)
            {
                return null;
            }

            return Points[i * PointsPerRow + k];
        }

        public PointData GetPointDataById(int id)
        {
            return Points[id];
        }

        public PointData GetPointData(int i, int k)
        {
            var result = TryGetPointData(i, k);
            Assert.That(result != null);
            return result;
        }

        public List<TileData> GetNeighbourTiles(PointData pointData)
        {
            var tiles = new List<TileData>();

            for (int i = pointData.Row - 1; i <= pointData.Row; i++)
            {
                for (int k = pointData.Col - 1; k <= pointData.Col; k++)
                {
                    var tile = TryGetTileData(i, k);

                    if (tile != null)
                    {
                        tiles.Add(tile);
                    }
                }
            }

            return tiles;
        }
    }
}
