using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform followTarget;
    [Tooltip("How much the camera will lag behind the target.")]
    [Min(0.001f)]
    public float smoothness = 1f;
    [Tooltip("The maximum distance from the ideal point the camera can be.")]
    public float maxDist = 10f;
    public float maxAccelleration = 1f;
    /// <summary> If the camera moves in update it not follow a physics object smoothly, because the object moves a lot on fixed-updates
    /// and is still on regular updates, meaning it will jump between ahead of or behind the camera.
    /// However if the camera moves in fixed updates, the camera will not move every frame. </summary>
    [Tooltip("Fixed makes physics objects appear smoother when moving, but the actual camera movement (e.g. backgrounds) feel less smooth.")]
    public bool moveInFixedUpdate = true;

    private Vector3 cameraOffset;
    private Vector3 velocity = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        if (followTarget != null)
            cameraOffset = transform.position - followTarget.position;
        else
            cameraOffset = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (moveInFixedUpdate == false)
            FollowTargetUpdate();
    }

    private void FixedUpdate()
    {
        if (moveInFixedUpdate == true)
            FollowTargetFixed();
    }

    private void FollowTargetFixed()
    {
        FollowTarget(Time.fixedDeltaTime);
    }

    private void FollowTargetUpdate()
    {
        FollowTarget(Time.deltaTime);
    }

    private void FollowTarget(float timeIncrement)
    {
        //Get displacement between the camera and the ideal position.
        Vector3 displacement = (followTarget.position + cameraOffset) - transform.position;
        float dist = displacement.magnitude;

        //Calculate the speed as a function of distance and smoothing
        float camSpeed = Mathf.Min(dist, (dist * dist * timeIncrement) / smoothness);
        //Increase the speed if the target is further than the max distance.
        camSpeed += Mathf.Max(dist - maxDist, 0f);
        //Limit the change in speed by the max accelleration value.
        float currentSpeed = velocity.magnitude;
        camSpeed = Mathf.Clamp(camSpeed, currentSpeed - maxAccelleration * timeIncrement, currentSpeed + maxAccelleration * timeIncrement);
        //Calculate velocity from speed.
        velocity = displacement.normalized * camSpeed;
        //Apply velocity.
        transform.position += velocity;
    }
}
