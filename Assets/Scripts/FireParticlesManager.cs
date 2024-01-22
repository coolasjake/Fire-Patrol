using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirePatrol
{
    public class FireParticlesManager : MonoBehaviour
    {
        [EnumNamedArray(typeof(FireStage))]
        public ParticleSystem[] fireStageParticles = new ParticleSystem[System.Enum.GetValues(typeof(FireStage)).Length];

        public void ShowStage(FireStage stage)
        {
            int index = (int)stage;
            if (fireStageParticles == null || index < 0 || index > fireStageParticles.Length - 1)
                return;

            Debug.Log("Showing stage: " + stage.ToString());

            for (int i = 0; i < fireStageParticles.Length; ++i)
            {
                if (fireStageParticles[i] == null)
                    continue;
                if (i == index)
                    fireStageParticles[i].Play();
                else
                    fireStageParticles[i].Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        public void ShowWet()
        {
            ShowStage(FireStage.none);
            //Show wet effect.
        }
    }
}