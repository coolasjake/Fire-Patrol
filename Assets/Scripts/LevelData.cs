using UnityEngine;
using System;
using System.Collections.Generic;

namespace FirePatrol
{
    public enum PropType
    {
        Tree,
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
        public BurntEffect burntEffect = new BurntEffect();
        public int Id;
        public int Row;
        public int Col;
    }

    public enum PointTypes
    {
        Water,
        Grass,
    }

    public enum FireStage
    {
        none,
        sparks,
        inferno,
        dying,
        ashes,
    }

    [Serializable]
    public class PointData
    {
        public PointTypes Type;
        public Vector3 Position;
        public ParticleSystem fireParticle;
        public bool onFire = false;
        public bool wet = false;
        public FireStage fireStage = FireStage.none;
        public float fireprogress = 0;
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

        /// <summary> The game-space size of tiles. </summary>
        public float TileSize
        {
            get { return TILE_UNSCALED_SIZE * TileScale; }
        }

        /// <summary> The number of tiles in a row (one less than points per row). </summary>
        public int TilesPerRow
        {
            get { return PointsPerRow - 1; }
        }

        /// <summary> Get the tile data at the given row/column in the list, returning null if it is an invalid point. </summary>
        public TileData TryGetTileData(int i, int k)
        {
            if (i < 0 || i >= TilesPerRow || k < 0 || k >= TilesPerRow)
            {
                return null;
            }

            return Tiles[i * TilesPerRow + k];
        }

        /// <summary> Get the tile with the given ID (index) in the list. </summary>
        public TileData GetTileDataById(int id)
        {
            return Tiles[id];
        }

        /// <summary> Get the tile data at the given row/column in the list. </summary>
        public TileData GetTileData(int i, int k)
        {
            var result = TryGetTileData(i, k);
            Assert.That(result != null);
            return result;
        }

        /// <summary> Get the point data at the given row/column in the list, returning null if it is an invalid point. </summary>
        public PointData TryGetPointData(int i, int k)
        {
            if (i < 0 || i >= PointsPerRow || k < 0 || k >= PointsPerRow)
            {
                return null;
            }

            return Points[i * PointsPerRow + k];
        }

        /// <summary> Get the point with the given ID (index) in the list. </summary>
        public PointData GetPointDataById(int id)
        {
            return Points[id];
        }

        /// <summary> Get the point data at the given row/column in the list. </summary>
        public PointData GetPointData(int i, int k)
        {
            var result = TryGetPointData(i, k);
            Assert.That(result != null);
            return result;
        }

        /// <summary> Get a list of the 4 tiles that surround this point. </summary>
        public List<PointData> GetDirectNeighbourPoints(PointData pointData)
        {
            var points = new List<PointData>();

            var point = TryGetPointData(pointData.Row + 1, pointData.Col);
            if (point != null)
                points.Add(point);
            point = TryGetPointData(pointData.Row - 1, pointData.Col);
            if (point != null)
                points.Add(point);
            point = TryGetPointData(pointData.Row, pointData.Col + 1);
            if (point != null)
                points.Add(point);
            point = TryGetPointData(pointData.Row, pointData.Col - 1);
            if (point != null)
                points.Add(point);

            return points;
        }

        /// <summary> Get a list of the 4 tiles that surround this point. </summary>
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
