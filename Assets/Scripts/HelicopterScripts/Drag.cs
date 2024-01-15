using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Drag : MonoBehaviour
{
    private const float airDensity = 0.01f;

    private Rigidbody RB;

    [Min(0)]
    public float drag = 1;
    public bool getDragFromTerminalVelocity = true;
    public float terminalVelocity = 1;
    public float gravityMagnitude = Physics.gravity.magnitude;

    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody>();
        if (RB.isKinematic)
            enabled = false;

        if (getDragFromTerminalVelocity)
            TerminalVelocityToDrag();
        else
            DragToTerminalVelocity();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        VelocityBasedDrag();
    }

    private void TerminalVelocityToDrag()
    {
        drag = (2f * RB.mass * gravityMagnitude) / Mathf.Pow(terminalVelocity, 2);
    }

    private void DragToTerminalVelocity()
    {
        terminalVelocity = Mathf.Sqrt((2f * RB.mass * gravityMagnitude) / drag);
        drag = (2f * RB.mass * gravityMagnitude) / Mathf.Pow(terminalVelocity, 2);
    }

    private void VelocityBasedDrag()
    {
        //Calculate then apply drag
        float currentVel = RB.velocity.magnitude;
        float dragValue = (Mathf.Pow(currentVel, 2) * drag * airDensity) / RB.mass;
        dragValue = Mathf.Min(dragValue, currentVel);
        Vector3 dragForce = -RB.velocity.normalized * dragValue;
        RB.velocity += dragForce;
    }
}
