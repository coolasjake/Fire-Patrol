using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CordJoint : MonoBehaviour
{
    private Rigidbody RB;
    public Rigidbody connectedBody;
    public bool autoConfigureLength = true;
    public float cordLength = 1f;
    public float elasticStrength = 1f;
    public float maxStretch = 1f;

    private void OnEnable()
    {
        RB = GetComponent<Rigidbody>();
    }

    private void Awake()
    {
        RB = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (connectedBody != null)
            cordLength = (RB.position - connectedBody.position).magnitude;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (connectedBody == null)
        {
            Debug.LogError("No connected body on Cord Joint", this);
            return;
        }

        //Only apply forces if the body is further than the minimum length of the cord.
        if (Utility.WithinRange(RB.position, connectedBody.position, cordLength) == false)
        {
            ElasticCord();
        }
    }

    private void ElasticCord()
    {
        Vector3 relativePos = RB.position - connectedBody.position;
        Vector3 relativeDir = relativePos.normalized;
        float dist = relativePos.magnitude;
        float stretching = dist - cordLength;

        Vector3 elasticForce = (elasticStrength * stretching * stretching) / Time.fixedDeltaTime * -relativeDir;
        RB.AddForce(elasticForce);
        connectedBody.AddForce(-elasticForce);

        if (stretching > maxStretch)
        {
            Vector3 tautForce = -Physics.gravity.magnitude * RB.mass * relativeDir;
            RB.AddForce(tautForce);
            connectedBody.AddForce(-tautForce);
        }
    }

    private void RigidCord()
    {
        Vector3 relativePos = RB.position - connectedBody.position;
        Vector3 relativeDir = relativePos.normalized;

        //Snap back to max dist
        RB.position = connectedBody.position + (relativeDir * cordLength);

        //Add a force towards the pivot which is weaker when the existing velocity is not directly away from the pivot.
        float alignment = Vector3.Dot(RB.velocity.normalized, relativeDir);
        if (alignment > 0)
        {
            Vector3 pendulumForce = alignment * -RB.velocity.magnitude * relativeDir / Time.fixedDeltaTime;
            RB.AddForce(pendulumForce * RB.mass);
            connectedBody.AddForce(-pendulumForce * RB.mass);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        RB = GetComponent<Rigidbody>();
        if (connectedBody == null)
        {
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawWireDisc(RB.position, Vector3.right, autoConfigureLength ? 2 : cordLength);
        }
        else
        {
            UnityEditor.Handles.color = Color.blue;
            if (autoConfigureLength && !Application.isPlaying)
                cordLength = (RB.position - connectedBody.position).magnitude;
            UnityEditor.Handles.DrawWireDisc(connectedBody.position, Vector3.right, cordLength);
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawWireDisc(connectedBody.position, Vector3.right, cordLength + maxStretch);
        }
    }
#endif
}
