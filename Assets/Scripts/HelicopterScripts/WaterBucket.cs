using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBucket : MonoBehaviour
{
    public float raisedLength = 3.5f;
    public float loweredLength = 8f;
    public float testWaterLength = 7f;
    public float launchToLowerDelay = 0.5f;
    public float winchSpeed = 1f;

    private bool _hasWater = false;
    private float _lastWaterLaunch = 0f;

    public Rigidbody RB;
    public CordJoint cord;
    public GameObject waterBlob;

    // Start is called before the first frame update
    void Start()
    {
        
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
        _lastWaterLaunch = Time.time;
        _hasWater = false;
    }

    private void LowerBucket()
    {
        cord.cordLength = Mathf.Clamp(cord.cordLength + winchSpeed * Time.deltaTime, raisedLength, loweredLength);
    }

    private void RaiseBucket()
    {
        cord.cordLength = Mathf.Clamp(cord.cordLength - winchSpeed * Time.deltaTime, raisedLength, loweredLength);
        bool isInWater = cord.cordLength >= testWaterLength;
        if (isInWater)
        {
            _hasWater = true;
        }
    }
}
