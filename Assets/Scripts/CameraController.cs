using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform helicopter;
    public float accelleration = 1f;
    public float maxVelocity = 5f;

    private Vector3 offset;
    private Vector3 velocity = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        if (helicopter != null)
            offset = transform.position - helicopter.position;
    }

    // Update is called once per frame
    void Update()
    {
        FollowHelicopter();
    }

    private void FollowHelicopter()
    {
        velocity = Vector3.MoveTowards(velocity, (helicopter.position + offset) - transform.position, accelleration * Time.deltaTime);
        transform.position += velocity;
        velocity = Mathf.Min(velocity.magnitude, maxVelocity) * velocity.normalized;
    }
}
