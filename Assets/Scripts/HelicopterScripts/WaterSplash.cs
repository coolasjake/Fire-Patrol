using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WaterSplash : MonoBehaviour
{
    public Rigidbody RB;
    public ParticleSystem waterTrail;
    public ParticleSystem waterSplash;

    private void OnCollisionEnter(Collision collision)
    {
        RB.isKinematic = true;
        waterTrail.Stop(false, ParticleSystemStopBehavior.StopEmitting);
        waterSplash.Play();
        StartCoroutine(DestroyAfterTime(3f));
    }

    private IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
