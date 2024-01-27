using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirePatrol
{
    [RequireComponent(typeof(Animator))]
    public class WaterBucket : MonoBehaviour
    {
        public Animator animator;
        public LayerMask waterLayer;
        public float raisedLength = 3.5f;
        public float loweredLength = 8f;
        public float launchToLowerDelay = 0.5f;
        public float winchSpeed = 1f;
        public int splashesPerRefill = 1;
        public bool startWithWater = true;
        public float splashRadius = 20f;

        private int _numSplashes = 0;
        private bool _inWater = false;
        private float _lastWaterLaunch = 0f;

        public Rigidbody RB;
        public CordJoint cord;
        public WaterSplash waterBlob;

        // Start is called before the first frame update
        void Start()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            raisedLength = cord.cordLength;
            if (startWithWater)
            _numSplashes = splashesPerRefill;
        }

        // Update is called once per frame
        void Update()
        {
            if (PauseManager.Paused)
                return;

            if (Input.GetButtonDown("Fire"))
            {
                if (_numSplashes > 0 && _inWater == false)
                    LaunchWater();
            }
            else if (Input.GetButton("Fire") && Time.time > _lastWaterLaunch + launchToLowerDelay)
                LowerBucket();
            else
                RaiseBucket();
        }

        private void LaunchWater()
        {
            _numSplashes -= 1;
            _lastWaterLaunch = Time.time;
            WaterSplash water = Instantiate(waterBlob, transform.position, Quaternion.identity);
            water.RB.velocity = RB.velocity;
            water.splashRadius = splashRadius;
            if (_numSplashes == 0)
                animator.Play("EmptyBucket");
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
                _inWater = false;
            }
        }

        private void RefillBucket()
        {
            _inWater = true;
            if (_numSplashes == 0)
            {
                animator.Play("FillBucket");
            }
            _numSplashes = splashesPerRefill;
        }

        private void OnTriggerStay(Collider other)
        {
            if (((1 << other.gameObject.layer) & waterLayer) != 0)
            {
                RefillBucket();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            _inWater = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, splashRadius);
        }
    }
}