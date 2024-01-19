using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBucket : MonoBehaviour
{
    public LayerMask waterLayer;
    public float raisedLength = 3.5f;
    public float loweredLength = 8f;
    public float launchToLowerDelay = 0.5f;
    public float winchSpeed = 1f;
    public bool startWithWater = true;
    public float splashRadius = 20f;

    private bool _hasWater = false;
    private bool _inWater = false;
    private float _lastWaterLaunch = 0f;

    public Rigidbody RB;
    public CordJoint cord;
    public WaterSplash waterBlob;

    // Start is called before the first frame update
    void Start()
    {
        raisedLength = cord.cordLength;
        _hasWater = startWithWater;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire"))
        {
            if (_hasWater)
                LaunchWater();
            else if (Time.time > _lastWaterLaunch + launchToLowerDelay)
                LowerBucket();
        }
        else
            RaiseBucket();
    }

    private void LaunchWater()
    {
        _hasWater = false;
        _lastWaterLaunch = Time.time;
        WaterSplash water = Instantiate(waterBlob, transform.position, Quaternion.identity);
        water.RB.velocity = RB.velocity;
        water.splashRadius = splashRadius;
    }

    private void LowerBucket()
    {
        cord.cordLength = Mathf.Clamp(cord.cordLength + winchSpeed * Time.deltaTime, raisedLength, loweredLength);
    }

    private void RaiseBucket()
    {
        cord.cordLength = Mathf.Clamp(cord.cordLength - winchSpeed * Time.deltaTime, raisedLength, loweredLength);
        if (_inWater)
        {
            _hasWater = true;
            _inWater = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (((1 << other.gameObject.layer) & waterLayer) != 0)
        {
            _inWater = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, splashRadius);
    }
}
