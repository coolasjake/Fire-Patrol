using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirePatrol
{
    public class FirePointController : FireController
    {
        public LevelData leveldata;
        public FireParticlesManager fireParticlePrefab;
        public float fireSpawnHeight = 2f;
        public float dropAnimationSpeed = 30f;

        [Tooltip("Duration of fire stages for 'Grass' type points. Set to 0 or less to skip the stage.")]
        [EnumNamedArray(typeof(FireStage))]
        public float[] grassStageDurations = new float[System.Enum.GetValues(typeof(FireStage)).Length];
        [Tooltip("Duration of fire stages for 'Forest' type points. Set to 0 or less to skip the stage.")]
        [EnumNamedArray(typeof(FireStage))]
        public float[] forestStageDurations = new float[System.Enum.GetValues(typeof(FireStage)).Length];
        [Tooltip("Duration of fire stages for 'Rocky' type points. Set to 0 or less to skip the stage.")]
        [EnumNamedArray(typeof(FireStage))]
        public float[] rockyStageDurations = new float[System.Enum.GetValues(typeof(FireStage)).Length];

        [Tooltip("Chance at the end of each fire stage to spread fire to neighboring points.")]
        [EnumNamedArray(typeof(FireStage))]
        public float[] stageStartSpreadChance = new float[System.Enum.GetValues(typeof(FireStage)).Length];

        private float _maxTotalBurnValue = 0;

        private float StageDurationForPoint(PointData point)
        {
            switch (point.Type)
            {
                case PointTypes.Grass:
                    return grassStageDurations[(int)point.fireStage];
                case PointTypes.Forest:
                    return forestStageDurations[(int)point.fireStage];
                case PointTypes.Rocky:
                    return rockyStageDurations[(int)point.fireStage];
            }
            return -1;
        }

        // Start is called before the first frame update
        void Start()
        {
            BurntEffect.dropSpeed = dropAnimationSpeed;
            SetupFireParticles();
            AddFireToRandomPoint();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateFires();
        }

        private void UpdateFires()
        {
            foreach (PointData point in leveldata.Points)
            {
                if (point.onFire)
                {
                    point.fireprogress += Time.deltaTime;
                    float stageDur = StageDurationForPoint(point);
                    if (stageDur <= 0)
                        GoToNextFireStage(point);
                    else if (point.fireprogress >= stageDur)
                        ProgressFireStage(point);
                    SetBurntLevel(point);
                }
            }
        }

        private void SetBurntLevel(PointData point)
        {
            float burntLevel = 0;
            switch (point.fireStage)
            {
                case FireStage.none:
                    break;
                case FireStage.sparks:
                    break;
                case FireStage.smallFlames:
                    burntLevel = (point.fireprogress / StageDurationForPoint(point)) * 0.25f;
                    break;
                case FireStage.inferno:
                    burntLevel = 0.25f + (point.fireprogress / StageDurationForPoint(point)) * 0.5f;
                    break;
                case FireStage.dying:
                    burntLevel = 0.75f + (point.fireprogress / StageDurationForPoint(point)) * 0.25f;
                    break;
            }

            List<TileData> tiles = leveldata.GetNeighbourTiles(point);
            foreach (TileData tile in tiles)
            {
                tile.burntEffect.SetBurntness(burntLevel);
                if (tile.burntEffect.CanStartDropAnimation())
                {
                    tile.burntEffect.DropCoroutine = StartCoroutine(BurntEffect.BurnablesDropAnimation(tile.burntEffect));
                }
            }
        }

        public override void SplashClosestTwoPoints(Vector3 position, float radius)
        {
            float closestDistSqr = float.PositiveInfinity;
            PointData closestPoint = null;
            PointData secondClosestPoint = null;
            foreach (PointData point in leveldata.Points)
            {
                float distSqr = (position - point.Position).sqrMagnitude;
                if (distSqr < closestDistSqr)
                {
                    closestDistSqr = distSqr;
                    secondClosestPoint = closestPoint;
                    closestPoint = point;
                }
            }

            if (closestPoint != null && closestPoint.Type != PointTypes.Water)
                WetPoint(closestPoint);
            if (secondClosestPoint != null && secondClosestPoint.Type != PointTypes.Water)
                WetPoint(secondClosestPoint);
        }

        public override void SplashPointsInRadius(Vector3 position, float radius)
        {
            List<PointData> splashedPoints = new List<PointData>();
            foreach (PointData point in leveldata.Points)
            {
                if (point.Type != PointTypes.Water)
                {
                    if (Utility.WithinRange(point.Position.FixedY(position.y), position, radius + leveldata.TileSize * 0.5f))
                        splashedPoints.Add(point);
                }
            }

            foreach (PointData point in splashedPoints)
            {
                WetPoint(point);
            }
        }

        private void WetPoint(PointData point)
        {
            //Debug.Log("Wetting point: " + point.Row + ", " + point.Col + " (type = " + point.Type + ")", point.fireParticle);
            point.wet = true;
            point.onFire = false;
            point.fireParticles.ShowWet();
        }

        private void ProgressFireStage(PointData point)
        {
            GoToNextFireStage(point);

            //Trigger behaviours for the new stage
            switch (point.fireStage)
            {
                case FireStage.ashes:
                    point.onFire = false;
                    break;
                case FireStage.none:
                    point.onFire = false;
                    break;
            }
            SpreadFireWithChance(point, stageStartSpreadChance[(int)point.fireStage]);
        }

        private void GoToNextFireStage(PointData point)
        {
            //Go to next stage and trigger behaviours
            switch (point.fireStage)
            {
                case FireStage.none:
                    point.fireStage = FireStage.sparks;
                    break;
                case FireStage.sparks:
                    point.fireStage = FireStage.smallFlames;
                    break;
                case FireStage.smallFlames:
                    point.fireStage = FireStage.inferno;
                    break;
                case FireStage.inferno:
                    point.fireStage = FireStage.dying;
                    break;
                case FireStage.dying:
                    point.fireStage = FireStage.ashes;
                    break;
                case FireStage.ashes:
                    point.fireStage = FireStage.none;
                    break;
            }
            point.fireprogress = 0;
            point.fireParticles.ShowStage(point.fireStage);
        }

        private void SpreadFireWithChance(PointData point, float chance)
        {
            List<PointData> neighbors = leveldata.GetDirectNeighbourPoints(point);
            foreach (PointData neighbor in neighbors)
            {
                if (neighbor.Type == PointTypes.Water || neighbor.onFire || neighbor.wet || neighbor.fireStage == FireStage.ashes)
                    continue;
                if (Random.value <= chance)
                    SetPointOnFire(neighbor);
            }
        }    

        private void SetupFireParticles()
        {
            int i = 0;
            foreach (PointData point in leveldata.Points)
            {
                if (point.Type == PointTypes.Water)
                    point.fireParticles = null;
                else
                {
                    point.fireParticles = Instantiate(fireParticlePrefab, point.Position + new Vector3(0, fireSpawnHeight, 0), Quaternion.identity, transform);
                    point.fireParticles.name = "FireParticles " + i;
                }
                ++i;
            }
        }

        public override void StartRandomFire()
        {
            //Should change to choose from preset list of points.
            AddFireToRandomPoint();
        }

        private void AddFireToRandomPoint()
        {
            List<int> landPoints = new List<int>();
            for(int i = 0; i < leveldata.Points.Count; ++i)
            {
                if (leveldata.Points[i].Type == PointTypes.Grass || leveldata.Points[i].Type == PointTypes.Grass)
                    landPoints.Add(i);
            }
            int index = landPoints[Random.Range(0, landPoints.Count)];
            Debug.Log("Started fire on " + leveldata.Points[index].Id + " (type = " + leveldata.Points[index].Type + ")");
            SetPointOnFire(index);

        }

        private void SetPointOnFire(int index)
        {
            SetPointOnFire(leveldata.Points[index]);
        }

        private void SetPointOnFire(PointData point)
        {
            point.onFire = true;
            //point.fireParticle.Play();
        }

        public override bool NoFireInLevel()
        {
            foreach (PointData point in leveldata.Points)
            {
                if (point.onFire)
                    return false;
            }

            return true;
        }

        public void CalculateMaxBurnValue()
        {
            float max = 0;
            List<TileData> checkedTiles = new List<TileData>();
            foreach (PointData point in leveldata.Points)
            {
                if (point.Type != PointTypes.Water && point.Type != PointTypes.Rocky)
                {
                    List<TileData> tiles = leveldata.GetNeighbourTiles(point);
                    foreach (TileData tile in tiles)
                    {
                        if (checkedTiles.Contains(tile) == false)
                        {
                            checkedTiles.Add(tile);
                            max += 1f;
                        }
                    }
                }
            }

            _maxTotalBurnValue = max;
        }

        public override float LevelBurntPercentage()
        {
            float total = 0;
            List<TileData> checkedTiles = new List<TileData>();
            foreach (PointData point in leveldata.Points)
            {
                if (point.Type != PointTypes.Water && point.Type != PointTypes.Rocky)
                {
                    List<TileData> tiles = leveldata.GetNeighbourTiles(point);
                    foreach (TileData tile in tiles)
                    {
                        if (checkedTiles.Contains(tile) == false)
                        {
                            checkedTiles.Add(tile);
                            total += tile.burntEffect.burntLevel;
                        }
                    }
                }
            }

            return total / _maxTotalBurnValue;
        }
    }
}
