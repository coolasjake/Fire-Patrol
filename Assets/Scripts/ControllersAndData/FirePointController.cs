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
        public bool startWithoutGameController = false;
        [Range(0f, 1f)]
        public float burnablesScoreWeight = 1f;
        [Range(0f, 1f)]
        public float charablesScoreWeight = 0.1f;

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
        [Tooltip("The max burn value at the end of this stage. Fully burnt is 1. Make sure big numbers are after small numbers.")]
        [EnumNamedArray(typeof(FireStage))]
        public float[] burnValuePerStage = new float[System.Enum.GetValues(typeof(FireStage)).Length];

        private float _maxTotalBurnValue = 0;
        private int _numLandPoints = 0;
        public override float PercentOfLandOnFire => _percentLandOnFire;
        private float _percentLandOnFire = 0;

        public override Vector3 LastFirePos => _lastFirePosition;
        private Vector3 _lastFirePosition;

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

        void Start()
        {
            if (startWithoutGameController)
            {
                StartGame();
                StartRandomFire();
            }
        }

        public override void StartGame()
        {
            BurntEffect.dropSpeed = dropAnimationSpeed;
            SetupFireParticles();
            CalculateMaxBurnValue();
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
            int currentStage = Mathf.Clamp(((int)point.fireStage), 0, burnValuePerStage.Length - 1);
            int prevStage = Mathf.Clamp(currentStage, 0, burnValuePerStage.Length - 1);
            float prevMax = burnValuePerStage[prevStage];
            float max = burnValuePerStage[currentStage];

            float burntLevel = prevMax + (point.fireprogress / StageDurationForPoint(point)) * (max - prevMax);

            List<TileData> tiles = leveldata.GetNeighbourTiles(point);
            foreach (TileData tile in tiles)
            {
                tile.burntEffect.SetBurntness(Mathf.Min(tile.burntEffect.burntLevel + Time.deltaTime, burntLevel));
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
            if (point == null)
                return;
            point.wet = 5;
            point.onFire = false;
            if (point.fireParticles != null)
                point.fireParticles.ShowWet();
        }

        public override void PutOutAllFires()
        {
            foreach (PointData point in leveldata.Points)
            {
                WetPoint(point);
            }
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
                if (neighbor.Type == PointTypes.Water || neighbor.onFire || neighbor.fireStage == FireStage.ashes)
                    continue;

                if (neighbor.wet > 0)
                    neighbor.wet -= 1;
                else if (Random.value <= chance)
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
            _lastFirePosition = point.Position;
            //point.fireParticle.Play();
        }

        public override bool NoFireInLevel()
        {
            bool noFire = true;
            int numFires = 0;
            foreach (PointData point in leveldata.Points)
            {
                if (point.Type != PointTypes.Water && point.onFire)
                {
                    noFire = false;
                    numFires += 1;
                }
            }
            _percentLandOnFire = (float)numFires / _numLandPoints;
            return noFire;
        }

        public void CalculateMaxBurnValue()
        {
            float maxBurntness = 0;
            int landPoints = 0;
            List<TileData> checkedTiles = new List<TileData>();
            foreach (PointData point in leveldata.Points)
            {
                if (point.Type != PointTypes.Water)
                {
                    if (point.Type != PointTypes.Rocky)
                    {
                        List<TileData> tiles = leveldata.GetNeighbourTiles(point);
                        foreach (TileData tile in tiles)
                        {
                            if (checkedTiles.Contains(tile) == false)
                            {
                                checkedTiles.Add(tile);
                                maxBurntness += tile.burntEffect.burnables.Count * burnablesScoreWeight;
                                maxBurntness += tile.burntEffect.charables.Count * charablesScoreWeight;
                            }
                        }
                    }

                    landPoints += 1;
                }

            }

            Debug.Log("Burnables total = " + maxBurntness);
            _maxTotalBurnValue = maxBurntness;
            _numLandPoints = landPoints;
        }

        public override float LevelBurntPercentage()
        {
            float total = 0;
            float burntCount = 0;
            int onFirePoints = 0;
            List<TileData> checkedTiles = new List<TileData>();
            foreach (PointData point in leveldata.Points)
            {
                if (point.Type != PointTypes.Water)
                {
                    if (point.Type != PointTypes.Rocky)
                    {
                        List<TileData> tiles = leveldata.GetNeighbourTiles(point);
                        foreach (TileData tile in tiles)
                        {
                            if (checkedTiles.Contains(tile) == false)
                            {
                                checkedTiles.Add(tile);
                                float burnable = tile.burntEffect.burnables.Count * burnablesScoreWeight;
                                float charable = tile.burntEffect.charables.Count * charablesScoreWeight;
                                total += burnable;
                                total += charable;
                                burntCount += burnable * tile.burntEffect.burntLevel;
                                burntCount += charable * tile.burntEffect.burntLevel;
                            }
                        }
                    }

                    if (point.onFire)
                        onFirePoints += 1;
                }
            }

            Debug.Log("Burnables total = " + total + ", burnt count = " + burntCount);

            _percentLandOnFire = (float)onFirePoints / _numLandPoints;

            return burntCount / total;
        }
    }
}
