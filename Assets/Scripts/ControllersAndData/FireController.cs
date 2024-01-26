using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirePatrol
{
    public abstract class FireController : MonoBehaviour
    {
        public static FireController singleton;
        public int levelNumber = 0;
        public abstract float PercentOfLandOnFire { get; }
        public abstract Vector3 LastFirePos { get; }

        void Awake()
        {
            Initialize();
        }

        public virtual void Initialize()
        {
            singleton = this;
        }

        public abstract void StartGame();

        public abstract void StartRandomFire();

        public abstract void PutOutAllFires();

        public abstract void SplashClosestTwoPoints(Vector3 position, float radius);

        public abstract void SplashPointsInRadius(Vector3 position, float radius);

        public abstract float LevelBurntPercentage();

        public abstract bool NoFireInLevel();
    }
}
