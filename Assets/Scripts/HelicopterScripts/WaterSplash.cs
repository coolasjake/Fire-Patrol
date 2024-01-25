using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirePatrol
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(AudioSource))]
    public class WaterSplash : MonoBehaviour
    {
        public AudioSource audioSource;
        public AudioClip waterReleased;
        public AudioClip waterSplashing;
        public Rigidbody RB;
        public ParticleSystem waterTrail;
        public ParticleSystem waterSplash;
        public float splashRadius = 5f;

        void Reset()
        {
            RB = GetComponent<Rigidbody>();
            audioSource = GetComponent<AudioSource>();
        }

        void Start()
        {
            audioSource.clip = waterReleased;
            audioSource.Play();
            audioSource.loop = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            //Stop the splash obj bouncing or triggering again
            RB.isKinematic = true;

            //Stop the trail particle and start the splash one
            waterTrail.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            waterSplash.Play();

            //Play the splash SFX
            audioSource.clip = waterSplashing;
            audioSource.Play();

            //Start the destroy coroutine
            StartCoroutine(DestroyAfterTime(3f));

            //Tell the fire controller where the splash happened, with the splash radius
            FireController.singleton.SplashPointsInRadius(transform.position, splashRadius);
        }

        private IEnumerator DestroyAfterTime(float time)
        {
            yield return new WaitForSeconds(time);
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, splashRadius);
        }
    }
}