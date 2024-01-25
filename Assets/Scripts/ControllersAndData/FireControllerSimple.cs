using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirePatrol
{
    public class FireControllerSimple : FireController
    {
        public string tilePrefabDirectory = "";
        private List<SimpleTile> tilePrefabs = new List<SimpleTile>();
        public List<SimpleTile> ignitionTiles = new List<SimpleTile>();
        public float tileSize = 1f;
        public float wetTileHeat = -20f;
        private SimpleTile[,] _tiles;
        private int _gridWidth = 0;
        private int _gridHeight = 0;
        public override float PercentOfLandOnFire => 0f;//_percentLandOnFire;

        public Direction windDirection = Direction.None;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            
        }

        public override void StartGame()
        {
            throw new System.NotImplementedException();
        }

        public void GenerateTileGrid()
        {

        }


        private void UpdateFireOnTiles()
        {
            //For each tile in the grid
            for (int y = 0; y < _gridHeight; ++y)
            {
                for (int x = 0; x < _gridWidth; ++x)
                {
                    if (_tiles[x, y] == null)
                        continue;

                    //Do its fire tick (change stage and burn value, then update effects)
                    _tiles[x, y].FireTick();

                    //Increase the heat of the tile and its neighbors
                    float heatOutput = _tiles[x, y].HeatOutput * Time.fixedDeltaTime;
                    Vector2Int offset;
                    foreach (Direction direction in System.Enum.GetValues(typeof(Direction)))
                    {
                        offset = DirectionAsV2(direction);
                        if (_tiles[x + offset.x, y + offset.y] != null)
                            _tiles[x + offset.x, y + offset.y].AddHeat(heatOutput);
                    }

                    //Increase the heat of a tile two steps away if there is wind
                    if (windDirection != Direction.None)
                    {
                        offset = DirectionAsV2(windDirection) * 2;
                        if (_tiles[x + offset.x, y + offset.y] != null)
                            _tiles[x + offset.x, y + offset.y].AddHeat(heatOutput);
                    }
                }
            }
        }

        public override void StartRandomFire()
        {
            if (ignitionTiles == null || ignitionTiles.Count == 0)
                Debug.LogError("No ignition tiles set, fire was not started.");
            else
                ignitionTiles[Random.Range(0, ignitionTiles.Count)].StartFire();
        }

        public override void SplashPointsInRadius(Vector3 position, float radius)
        {
            List<SimpleTile> splashedTiles = new List<SimpleTile>();
            foreach (SimpleTile tile in _tiles)
            {
                if (tile != null)
                {
                    if (Utility.WithinRange(tile.transform.position.FixedY(position.y), position, radius + tileSize * 0.5f))
                        splashedTiles.Add(tile);
                }
            }

            foreach (SimpleTile tile in splashedTiles)
                tile.Wet(Mathf.Min(wetTileHeat, 0));
        }

        public override void SplashClosestTwoPoints(Vector3 position, float radius)
        {
            throw new System.NotImplementedException();
        }

        public override void PutOutAllFires()
        {
            throw new System.NotImplementedException();
        }

        public override bool NoFireInLevel()
        {
            foreach (SimpleTile tile in _tiles)
            {
                if (tile == null)
                    continue;
                if (tile.OnFire)
                    return false;
            }
            return true;
        }

        public override float LevelBurntPercentage()
        {
            float max = 0;
            float total = 0;
            foreach (SimpleTile tile in _tiles)
            {
                if (tile == null)
                    continue;
                max += tile.GetMaxBurnLevel();
                total += tile.BurnLevel;
            }

            return total / max;
        }

        public enum Direction
        {
            North,
            East,
            South,
            West,
            None
        }

        public static Vector2Int DirectionAsV2(Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return Vector2Int.up;
                case Direction.East:
                    return Vector2Int.right;
                case Direction.South:
                    return Vector2Int.down;
                case Direction.West:
                    return Vector2Int.left;
            }
            return Vector2Int.zero;
        }
    }
}