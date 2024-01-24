using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirePatrol
{
    public class SimpleTile : MonoBehaviour
    {
        public FireEffect fireEffect;
        public FireParticlesManager fireParticles;

        public List<FireStageData> fireStageData = new List<FireStageData>();

        private float _heat = 0f;
        private float _stageDur = 0f;
        public float BurnLevel => _burnLevel01;
        private float _burnLevel01 = 0f;
        private int _currentStageIndex = -1;
        public bool OnFire => _onFire;
        private bool _onFire = false;
        private static float _heatReductionPerSecond = 1f;

        public void FireTick()
        {
            //If the tile is not on fire (heat = 0), set stage to default and skip fire tick.
            if (_heat <= 0)
            {
                _currentStageIndex = -1;
                _onFire = false;
                return;
            }

            //If there is no stage data, skip fire tick.
            if (fireStageData == null || fireStageData.Count == 0)
                return;

            //Reduce heat by ambient value, stopping at 0.
            _heat = Mathf.Max(0, _heat - _heatReductionPerSecond * Time.fixedDeltaTime);

            //Update the current stage based on heat and burn value.
            bool stageNeedsChecking = true;
            bool goToNextStage = false;
            FireStageData currentStage = StageFromIndex(_currentStageIndex);
            int startingIndex = _currentStageIndex;
            while (stageNeedsChecking)
            {
                //If the stage number is past the end of the stage list, stop the fire
                if (_currentStageIndex >= fireStageData.Count)
                {
                    _heat = 0;
                    _currentStageIndex = -1;
                    break;
                }

                goToNextStage = false;

                //If the tile is hot enough, go to the next stage
                if (currentStage.maxHeat > 0 && _heat > currentStage.maxHeat)
                    goToNextStage = true;

                //If the tile has burnt enough, go to the next stage
                if (_burnLevel01 > currentStage.maxBurnLevel)
                    goToNextStage = true;

                //If the tile is not hot enough, go to the next stage
                if (_heat < currentStage.minHeat)
                    goToNextStage = true;

                if (goToNextStage)
                {
                    //stageNeedsChecking = true;
                    _currentStageIndex += 1;
                    currentStage = StageFromIndex(_currentStageIndex);
                }
                else
                    stageNeedsChecking = false;
            }

            //If fire stage has changed, update the particle effects
            if (startingIndex != _currentStageIndex)
                fireParticles.ShowStage(currentStage.particleType);

            //Set the On Fire bool to true if particle is not none or ashes for simplified game logic checks
            _onFire = currentStage.particleType != FireStage.none && currentStage.particleType != FireStage.ashes;

            //Increase burn value and apply to fireEffect
            if (currentStage.burnTime > 0)
            {
                float burnIncrease = (1f / currentStage.burnTime) * currentStage.maxBurnLevel * Time.fixedDeltaTime;
                _burnLevel01 = Mathf.Clamp(_burnLevel01 + burnIncrease, _burnLevel01, currentStage.maxBurnLevel);
                fireEffect.SetBurntness(_burnLevel01);
            }
        }

        public float HeatOutput => StageFromIndex(_currentStageIndex).heatOutput;

        private FireStageData StageFromIndex(int index)
        {
            if (index < 0 || index >= fireStageData.Count)
                return defaultFireStage;
            else
                return fireStageData[index];
        }

        public void AddHeat(float heat)
        {
            _heat += heat;
        }

        public void StartFire()
        {
            _heat = StageFromIndex(0).minHeat;
            if (_heat <= 0)
                Debug.LogError("The first fire stage on [" + name + "] has a min heat of 0, meaning the fire was instantly put out.");
        }

        public void Wet(float newHeat)
        {
            _heat = newHeat;
        }

        public float GetMaxBurnLevel()
        {
            float max = 0;
            foreach (FireStageData stage in fireStageData)
                max = Mathf.Max(max, stage.maxBurnLevel);

            return max;
        }

        private static FireStageData defaultFireStage = new FireStageData(MaxHeat: 1f);
        [System.Serializable]
        public class FireStageData
        {
            [Tooltip("Name of the stage. If the final stage is ended the tile will no longer be on fire.")]
            public string name = "Not On Fire";

            [Tooltip("Particle effect shown when this stage starts.")]
            public FireStage particleType = FireStage.none;

            [Tooltip("The heat value needed for this stage. If heat is not sufficient at any point during this stage it will be skipped.")]
            [Min(0f)]
            public float minHeat = 0f;

            [Tooltip("If heat is greater than this value, and less than the MinHeat of the next stage, finish this stage early. (Set to 0 to disable)")]
            [Min(0f)]
            public float maxHeat = 1f;

            [Tooltip("The total burn amount before this stage will end.")]
            [Range(0f, 1f)]
            public float maxBurnLevel = 0f;

            [Tooltip("The time it takes to reach the Max Burn Level. Note - stages often start with some burn, so fire may not last this whole duration.")]
            [Min(0)]
            public float burnTime = 0f;

            [Tooltip("Amount of heat per second to spread to neighboring tiles (or further tiles if windy).")]
            [Min(0f)]
            public float heatOutput = 0f;

            public FireStageData(string Name = "Not On Fire", FireStage ParticleType = FireStage.none,
                float MinHeat = 0f, float MaxHeat = 0f, float MaxBurnValue = 0f, float BurnTime = 0f, float HeatOutput = 0f)
            {
                name = Name;
                particleType = ParticleType;
                minHeat = MinHeat;
                maxHeat = MaxHeat;
                maxBurnLevel = MaxBurnValue;
                burnTime = BurnTime;
                heatOutput = HeatOutput;
            }
        }
    }
}