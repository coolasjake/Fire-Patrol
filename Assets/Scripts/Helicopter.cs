using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helicopter : MonoBehaviour
{
    #region Settings
    [Header("Movement Settings")]
    [Tooltip("Max speed of the helicopter for calculating drag forces.")]
    [Min(0.001f)]
    public float maxSpeed = 10f;

    [Tooltip("How quickly the helicopter accelerates when it is fully tilted.")]
    [Min(0.001f)]
    public float accelleration = 5f;

    [Tooltip("How steep the helicoptor tilt it (doesn't effect movement).")]
    [Min(0.001f)]
    public float tiltAmount = 1f;

    [Tooltip("How quickly the helicoptor changes the angle it's tilted in - effects responsiveness.")]
    [Min(0.001f)]
    public float tiltSpeed = 1f;

    [Header("Turning Settings")]
    [Tooltip("How quickly the helicoptor turns to face the input direction - effects responsiveness based on alignment bonus.")]
    [Min(10f)]
    public float turnSpeed = 1f;

    [Tooltip("How accurate the helicoptor turning is - low values will make it over-turn and then compensate.")]
    [Min(0.001f)]
    public float turnHandling = 1f;

    [Tooltip("How much more/less the helicoptor tilts when moving forwards/backwards.")]
    [Range(0f, 1f)]
    public float alignmentBonus = 1f;

    [Header("Other Settings")]
    public float propellorSpeed = 90f;

    public Transform tiltObject;
    public Transform propellor;
    #endregion

    #region Variables
    private Rigidbody RB;
    private Vector3 _tiltDir = Vector3.zero;
    private Vector2 _forwardsDir = Vector2.up;
    private float _angularVel = 0f;
    private float _drag = 10f;
    #endregion

    #region Unity Events
    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody>();
        _drag = MaxSpeedToDrag(maxSpeed, accelleration);
    }

    // Update is called once per frame
    void Update()
    {
        //Animations:
        SpinPropellor();

        //Input:
        _drag = MaxSpeedToDrag(maxSpeed, accelleration); //Only in update so that max speed can be changed during runtime in inspector
        Vector2 input = GetInput();
        RotateHelicopter(input);
        ApplyPropellorForce();
        ApplyDragForces();
    }
    #endregion

    #region Movement (Input and Physics)
    private Vector2 GetInput()
    {
        Vector3 input = new Vector3();
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");
        return input;
    }

    private void ApplyPropellorForce()
    {
        RB.velocity += _tiltDir * accelleration * Time.deltaTime;
    }

    private void RotateHelicopter(Vector2 input)
    {
        input = Vector2.ClampMagnitude(input, 1f);

        //Turn towards the input direction.
        float angle = Vector2.SignedAngle(_forwardsDir, input);
        _angularVel = Mathf.MoveTowards(_angularVel, Mathf.Clamp(angle, -turnSpeed, turnSpeed), turnHandling * turnSpeed * Time.deltaTime);
        _forwardsDir = _forwardsDir.Rotate(_angularVel * Time.deltaTime);

        //Tilt towards the input direction - more if the direction is aligned with the front of the helicopter.
        input += alignmentBonus * Vector2.Dot(input, _forwardsDir) * input.normalized;
        Vector3 inputDir = input.ToVector3();
        inputDir.y = 1f / tiltAmount;
        _tiltDir = Vector3.MoveTowards(_tiltDir, inputDir, tiltSpeed * Time.deltaTime);
        tiltObject.rotation = Quaternion.LookRotation(_tiltDir, _forwardsDir.ToVector3());
    }


    private static float MaxSpeedToDrag(float maxSpeed, float accelleration)
    {
        return (2f * accelleration) / Mathf.Pow(maxSpeed, 2);
    }

    private const float airDensity = 0.01f;
    private void ApplyDragForces()
    {
        //Calculate then apply drag
        float currentVel = RB.velocity.magnitude;
        float dragValue = (Mathf.Pow(currentVel, 2) * _drag * airDensity) / RB.mass;
        dragValue = Mathf.Min(dragValue, currentVel);
        Vector3 dragForce = -RB.velocity.normalized * dragValue;
        RB.velocity += dragForce;
    }
    #endregion

    #region Animation
    private void SpinPropellor()
    {
        propellor.Rotate(0, propellorSpeed * Time.deltaTime, 0, Space.Self);
    }
    #endregion
}
