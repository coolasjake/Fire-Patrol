using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangingCord : MonoBehaviour
{
    public bool generatePoints;
    public Transform relativeObject;
    public List<Cord> cords;

    // Start is called before the first frame update
    void Start()
    {
        if (generatePoints)
        {
            foreach (Cord cord in cords)
            {
                if (cord.LR.positionCount > 1)
                {
                    cord.fixedPointIndex = cord.LR.positionCount - 1;
                    cord.GetFixedPosition(relativeObject);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Cord cord in cords)
        {
            UpdateFixedPoint(cord);
        }
    }

    public Vector3 GetFixedPosition(Vector3 localPos, Cord cord)
    {
        Vector3 worldPos = cord.LR.transform.TransformPoint(localPos);
        if (relativeObject == null)
            return worldPos;
        else
            return relativeObject.InverseTransformPoint(worldPos);
    }

    public void UpdateFixedPoint(Cord cord)
    {
        if (relativeObject == null)
        {
            cord.LR.SetPosition(cord.fixedPointIndex, transform.InverseTransformPoint(cord.relativeFixedPosition));
        }
        else
        {
            Vector3 worldPos = relativeObject.TransformPoint(cord.relativeFixedPosition);
            cord.LR.SetPosition(cord.fixedPointIndex, cord.LR.transform.InverseTransformPoint(worldPos));
        }
    }

    [System.Serializable]
    public class Cord
    {
        public LineRenderer LR;
        [Tooltip("The index of the point in the line-renderer that is attached to the target object.")]
        public int fixedPointIndex;
        [Tooltip("The offset of the connection point from the connected bodys origin.")]
        public Vector3 relativeFixedPosition;


        public void GetFixedPosition(Transform relativeObject)
        {
            Vector3 worldPos = LR.transform.TransformPoint(LR.GetPosition(fixedPointIndex));
            if (relativeObject == null)
                relativeFixedPosition = worldPos;
            else
                relativeFixedPosition = relativeObject.InverseTransformPoint(worldPos);
        }
    }
}
