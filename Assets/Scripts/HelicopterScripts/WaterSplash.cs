using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirePatrol
{
    [RequireComponent(typeof(Rigidbody))]
    public class WaterSplash : MonoBehaviour
    {
        public Rigidbody RB;
        public ParticleSystem waterTrail;
        public ParticleSystem waterSplash;
        public float splashRadius = 5f;

        private void OnCollisionEnter(Collision collision)
        {
            RB.isKinematic = true;
            waterTrail.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            waterSplash.Play();
            StartCoroutine(DestroyAfterTime(3f));
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