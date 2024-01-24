using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirePatrol
{
    public class FireParticlesManager : MonoBehaviour
    {
        [EnumNamedArray(typeof(FireStage))]
        public ParticleSystem[] fireStageParticles = new ParticleSystem[System.Enum.GetValues(typeof(FireStage)).Length];
        [Min(0.1f)]
        public float transitionTime = 1f;

        private int _currentlyPlayingParticle = 0;

        public void ShowStage(FireStage stage)
        {
            int index = (int)stage;

            if (fireStageParticles == null || index < 0 || index > fireStageParticles.Length - 1)
                return;

            if (index == _currentlyPlayingParticle || fireStageParticles[_currentlyPlayingParticle] == fireStageParticles[index])
            {
                ShowStageSimple(stage);
                return;
            }

            if (_transitionCoroutine == null)
            {
                Debug.Log(name + " Showing stage: " + stage.ToString());
                StartCoroutine(TransitionParticles(fireStageParticles[_currentlyPlayingParticle], fireStageParticles[index]));
                _currentlyPlayingParticle = index;
            }
        }

        public void ShowStageSimple(FireStage stage)
        {
            int index = (int)stage;
            if (fireStageParticles == null || index < 0 || index > fireStageParticles.Length - 1)
                return;


            for (int i = 0; i < fireStageParticles.Length; ++i)
            {
                if (fireStageParticles[i] == null)
                    continue;
                if (i != index)
                    fireStageParticles[i].Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
            if (fireStageParticles[index] != null)
                fireStageParticles[index].Play();
        }

        public void ShowWet()
        {
            ShowStage(FireStage.none);
            //Show wet effect.
        }

        private Coroutine _transitionCoroutine = null;
        private IEnumerator TransitionParticles(ParticleSystem oldParticle, ParticleSystem newParticle)
        {
            bool oldExists = false;
            ParticleSystem.EmissionModule oldEmmission;
            float oldOriginalRate = 1f;
            ParticleSystem.MainModule oldMain;
            float oldOriginalSpeed = 1f;
            if (oldParticle != null)
            {
                oldExists = true;

                oldEmmission = oldParticle.emission;
                oldOriginalRate = oldEmmission.rateOverTimeMultiplier;

                oldMain = oldParticle.main;
                oldOriginalSpeed = oldMain.startSpeedMultiplier;

                Debug.Log(oldOriginalSpeed);
            }

            bool newExists = false;
            ParticleSystem.EmissionModule newEmmission;
            float newOriginalRate = 1f;
            ParticleSystem.MainModule newMain;
            float newOriginalSpeed = 1f;
            if (newParticle != null)
            {
                newExists = true;

                newEmmission = newParticle.emission;
                newOriginalRate = newEmmission.rateOverTimeMultiplier;
                newEmmission.rateOverTimeMultiplier = 0f;

                newMain = newParticle.main;
                newOriginalSpeed = newMain.startSpeedMultiplier;
                newMain.startSpeedMultiplier = 0.5f;

                newParticle.Play();
                Debug.Log(name + " Playing " + newParticle.name);
            }

            WaitForEndOfFrame wait = new WaitForEndOfFrame();
            float startTime = Time.time;
            float t = 0;
            while (Time.time < startTime + transitionTime)
            {
                t = (Time.time - startTime) / transitionTime;
                if (oldExists)
                {
                    oldEmmission.rateOverTimeMultiplier = Mathf.Lerp(oldOriginalRate, 0f, t);
                    oldMain.startSpeedMultiplier = Mathf.Lerp(oldOriginalSpeed, 0.5f, t);
                }
                if (newExists)
                {
                    newEmmission.rateOverTimeMultiplier = Mathf.Lerp(0f, newOriginalRate, t);
                    newMain.startSpeedMultiplier = Mathf.Lerp(0f, newOriginalSpeed, t);
                }
                yield return wait;
            }

            if (oldExists)
            {
                oldParticle.Stop();
                oldEmmission.rateOverTimeMultiplier = oldOriginalRate;
                Debug.Log(name + " Stopping " + oldParticle.name);
            }
            if (newExists)
                newEmmission.rateOverTimeMultiplier = newOriginalRate;

            _transitionCoroutine = null;
        }
    }
}