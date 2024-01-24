using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirePatrol
{
    public class FireControllerSimple : FireController
    {
        

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public override void StartRandomFire()
        {
            throw new System.NotImplementedException();
        }

        public override void SplashPointsInRadius(Vector3 position, float radius)
        {
            throw new System.NotImplementedException();
        }

        public override void SplashClosestTwoPoints(Vector3 position, float radius)
        {
            throw new System.NotImplementedException();
        }
        public override float LevelBurntPercentage()
        {
            throw new System.NotImplementedException();
        }
    }
}