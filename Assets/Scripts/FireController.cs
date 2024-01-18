using FirePatrol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireController : MonoBehaviour
{
    public LevelData leveldata;
    public ParticleSystem fireParticlePrefab;
    public float fireSpawnHeight = 2f;
    public float dropAnimationSpeed = 30f;

    [EnumNamedArray(typeof(FireStage))]
    public float[] fireStageDurations = new float[System.Enum.GetValues(typeof(FireStage)).Length];

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
                if (point.fireprogress >= fireStageDurations[(int)point.fireStage])
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
                burntLevel = (point.fireprogress / fireStageDurations[(int)FireStage.sparks]) * 0.25f;
                break;
            case FireStage.inferno:
                burntLevel = 0.25f + (point.fireprogress / fireStageDurations[(int)FireStage.sparks]) * 0.5f;
                break;
            case FireStage.dying:
                burntLevel = 0.75f + (point.fireprogress / fireStageDurations[(int)FireStage.sparks]) * 0.25f;
                break;
        }

        List<TileData> tiles = leveldata.GetNeighbourTiles(point);
        foreach (TileData tile in tiles)
        {
            tile.burntEffect.SetBurntness(burntLevel);
            if (tile.burntEffect.CanStartDropAnimation())
            {
                tile.burntEffect.DropCoroutine = StartCoroutine(BurntEffect.BurnablesDropAnimation(tile.burntEffect));
                print("Dropping!");
            }
        }
    }

    private void ProgressFireStage(PointData point)
    {
        //Go to next stage and trigger behaviours
        switch (point.fireStage)
        {
            case FireStage.none:
                point.fireStage = FireStage.sparks;
                point.fireParticle.Play();
                break;
            case FireStage.sparks:
                point.fireStage = FireStage.inferno;
                SpreadFireWithChance(point, 0.7f);
                break;
            case FireStage.inferno:
                point.fireStage = FireStage.dying;
                SpreadFireWithChance(point, 0.3f);
                break;
            case FireStage.dying:
                point.fireStage = FireStage.ashes;
                point.fireParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                point.onFire = false;
                break;
        }
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
        foreach (PointData point in leveldata.Points)
        {
            if (point.Type == PointTypes.Water)
                point.fireParticle = null;
            else
                point.fireParticle = Instantiate(fireParticlePrefab, point.Position + new Vector3(0, fireSpawnHeight, 0), Quaternion.identity, transform);
        }
    }

    private void AddFireToRandomPoint()
    {
        List<int> landPoints = new List<int>();
        for(int i = 0; i < leveldata.Points.Count; ++i)
        {
            if (leveldata.Points[i].Type == PointTypes.Grass)
                landPoints.Add(i);
        }
        int index = landPoints[Random.Range(0, landPoints.Count)];
        SetPointOnFire(index);

    }

    private void SetPointOnFire(int index)
    {
        SetPointOnFire(leveldata.Points[index]);
    }

    private void SetPointOnFire(PointData point)
    {
        print("Set point" + point.Row + ", " + point.Col + " (type = " + point.Type + "), on fire.");
        point.onFire = true;
        //point.fireParticle.Play();
    }
}
