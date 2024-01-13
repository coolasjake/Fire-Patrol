using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helicopter : MonoBehaviour
{
    public float propellorSpeed = 90f;
    public float accelleration = 5f;
    public float maxSpeed = 10f;

    public Transform propellor;

    private Rigidbody RB;
    private float drag = 10f;

    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody>();
        drag = MaxSpeedToDrag(maxSpeed, accelleration);
    }

    // Update is called once per frame
    void Update()
    {
        SpinPropellor();
        InputForces();
        DragForces();
    }

    private void InputForces()
    {
        Vector2 input = new Vector2();
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");

        RB.velocity += input.ToVector3();
    }

    private void SpinPropellor()
    {
        propellor.Rotate(0, propellorSpeed * Time.deltaTime, 0);
    }

    private static float MaxSpeedToDrag(float maxSpeed, float accelleration)
    {
        return (2f * accelleration) / Mathf.Pow(maxSpeed, 2);
    }

    private const float airDensity = 0.01f;
    private void DragForces()
    {
        //Calculate then apply drag
        float currentVel = RB.velocity.magnitude;
        float dragValue = (Mathf.Pow(currentVel, 2) * drag * airDensity) / RB.mass;
        dragValue = Mathf.Min(dragValue, currentVel);
        Vector3 dragForce = -RB.velocity.normalized * dragValue;
        RB.velocity += dragForce;
    }
}
