using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirePatrol
{
    [RequireComponent(typeof(AudioSource))]
    public class FireParticlesManager : MonoBehaviour
    {
        public static int[] numSoundsPlaying = new int[System.Enum.GetValues(typeof(FireStage)).Length];
        public static int fireExtinguishedsPlaying = 0;
        public static int maxSimultaneousSounds = 3;

        public AudioSource audioSource;
        [EnumNamedArray(typeof(FireStage))]
        public AudioClip[] fireStageSounds = new AudioClip[System.Enum.GetValues(typeof(FireStage)).Length];
        public AudioClip fireExtinguishedSound;
        [EnumNamedArray(typeof(FireStage))]
        public ParticleSystem[] fireStageParticles = new ParticleSystem[System.Enum.GetValues(typeof(FireStage)).Length];
        [Min(0.1f)]
        public float transitionTime = 1f;
        [Min(0.1f)]
        public float audioFadeTime = 1f;

        private int _currentlyPlayingParticle = 0;

        public void ShowStage(FireStage stage)
        {
            int index = (int)stage;

            if (fireStageParticles == null || index < 0 || index > fireStageParticles.Length - 1)
                return;

            //If the state or the particle effect are the same, just play the sound.
            if (index == _currentlyPlayingParticle || fireStageParticles[_currentlyPlayingParticle] == fireStageParticles[index])
            {
                SwapSound(stage);
                _currentlyPlayingParticle = index;
                return;
            }

            //If a transition is already underway, end it, then start a transition between the two effects
            if (_transitionCoroutine != null)
                EndTransition();
            StartCoroutine(TransitionParticles(fireStageParticles[_currentlyPlayingParticle], fireStageParticles[index]));

            //Play the sound of the particle (sound swapping uses currentlyPlayedIndex to fade out the old sound)
            SwapSound(stage);
            _currentlyPlayingParticle = index;
        }

        public void ShowStageSimple(FireStage stage)
        {
            int index = (int)stage;
            if (fireStageParticles == null || index < 0 || index > fireStageParticles.Length - 1)
                return;

            if (_transitionCoroutine != null)
                EndTransition();

            for (int i = 0; i < fireStageParticles.Length; ++i)
            {
                if (fireStageParticles[i] == null)
                    continue;
                if (i != index)
                    fireStageParticles[i].Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
            if (fireStageParticles[index] != null)
                fireStageParticles[index].Play();

            SwapSound(stage);
            _currentlyPlayingParticle = index;
        }

        public void SwapSound(FireStage stage)
        {
            int index = (int)stage;
            if (fireStageSounds == null || index < 0 || index > fireStageSounds.Length - 1)
                return;

            if (_audioFadeCoroutine == null)
                startVolume = audioSource.volume;
            else
                StopCoroutine(_audioFadeCoroutine);
            _audioFadeCoroutine = StartCoroutine(AudioFade(index));
        }

        private float startVolume = 1f;
        private Coroutine _audioFadeCoroutine = null;
        private IEnumerator AudioFade(int newSoundIndex)
        {
            WaitForEndOfFrame wait = new WaitForEndOfFrame();
            WaitForSeconds soundsFullWait = new WaitForSeconds(3f);
            float startTime = Time.time;
            float t = 0;

            //Stop and fade out old sound
            if (audioSource.clip != null)
            {
                while (Time.time < startTime + (audioFadeTime / 2f))
                {
                    t = (Time.time - startTime) / (audioFadeTime / 2f);
                    audioSource.volume = Mathf.Lerp(startVolume, 0, t);
                    yield return wait;
                }
                numSoundsPlaying[_currentlyPlayingParticle] -= 1;
            }

            //Start and fade in new sound
            audioSource.clip = fireStageSounds[newSoundIndex];
            if (audioSource.clip != null)
            {
                //If too many sounds are playing at once 
                if (numSoundsPlaying[newSoundIndex] >= maxSimultaneousSounds)
                    yield return soundsFullWait;

                numSoundsPlaying[newSoundIndex] += 1;
                audioSource.Play();

                while (Time.time < startTime + audioFadeTime)
                {
                    t = (Time.time - startTime - (audioFadeTime / 2f)) / (audioFadeTime / 2f);
                    audioSource.volume = Mathf.Lerp(0, startVolume, t);
                    yield return wait;
                }
            }

            _audioFadeCoroutine = null;
        }

        public void ShowWet()
        {
            ShowStageSimple(FireStage.none);
            //Show wet effect.
        }

        private Coroutine _transitionCoroutine = null;
        private bool oldExists = false;
        private ParticleSystem oldParticle;
        private ParticleSystem.EmissionModule oldEmmission;
        private ParticleSystem.MainModule oldMain;
        private float oldOriginalRate = 1f;
        private float oldOriginalSpeed = 1f;
        private bool newExists = false;
        private ParticleSystem newParticle;
        private ParticleSystem.EmissionModule newEmmission;
        private ParticleSystem.MainModule newMain;
        private float newOriginalRate = 1f;
        private float newOriginalSpeed = 1f;
        private IEnumerator TransitionParticles(ParticleSystem oldP, ParticleSystem newP)
        {
            //Set up original particle effect
            oldParticle = oldP;
            bool oldExists = false;
            ParticleSystem.EmissionModule oldEmmission;
            oldOriginalRate = 1f;
            ParticleSystem.MainModule oldMain;
            oldOriginalSpeed = 1f;
            if (oldParticle != null)
            {
                oldExists = true;

                oldEmmission = oldParticle.emission;
                oldOriginalRate = oldEmmission.rateOverTimeMultiplier;

                oldMain = oldParticle.main;
                oldOriginalSpeed = oldMain.startSpeedMultiplier;
            }

            //Set up and play new particle effect
            newParticle = newP;
            bool newExists = false;
            ParticleSystem.EmissionModule newEmmission;
            newOriginalRate = 1f;
            ParticleSystem.MainModule newMain;
            newOriginalSpeed = 1f;
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
            }

            if (oldExists == false && newExists == false)
                yield break;

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

            EndTransition();

            _transitionCoroutine = null;
        }

        private void EndTransition()
        {

            if (oldExists)
            {
                oldParticle.Stop();
                oldEmmission.rateOverTimeMultiplier = oldOriginalRate;
                oldMain.startSpeedMultiplier = oldOriginalSpeed;
            }
            if (newExists)
            {
                newEmmission.rateOverTimeMultiplier = newOriginalRate;
                newMain.startSpeedMultiplier = newOriginalSpeed;
            }
        }
    }
}