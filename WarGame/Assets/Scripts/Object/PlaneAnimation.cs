using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneAnimation : MonoBehaviour
{
    public Vector3 planeMovementMin, planeMovementMax;
    public Vector3 propellerRotation;
    public float moveTime, resetDistance;
    public Transform[] propellers;
    private Vector3 moveToPos, startPos, parentPos;
    private float moveStart;


    public void Start()
    {
        startPos = transform.localPosition;
        parentPos = transform.parent.position;
        moveToPos = GetMovePosition();
        moveStart = Time.time;
    }

    public void Update()
    {
        RotatePropellers();

        if (Vector3.Distance(parentPos, transform.parent.position) > 0.001)
        {
            parentPos = transform.parent.position;
            moveToPos = GetMovePosition();
            moveStart = Time.time;
        }

        if (Vector3.Distance(transform.position, moveToPos) < resetDistance)
        {
            moveToPos = GetMovePosition();
            moveStart = Time.time;
        }
        else
        {
            MoveToPosition();
        }
    }

    void MoveToPosition()
    {
        float currentTime = Time.time - moveStart;
        transform.position = Vector3.Lerp(transform.position, moveToPos, currentTime / moveTime);
    }

    Vector3 GetMovePosition()
    {
        Vector3 nextMove = Vector3.zero;
        Vector3 origin = transform.parent.TransformPoint(startPos);
        nextMove.x = Random.Range(planeMovementMin.x, planeMovementMax.x) + origin.x;
        nextMove.y = Random.Range(planeMovementMin.y, planeMovementMax.y) + origin.y;
        nextMove.z = Random.Range(planeMovementMin.z, planeMovementMax.z) + origin.z;
        
        return nextMove;
    }

    void RotatePropellers()
    {
        for (int p = 0; p < propellers.Length; p++)
        {
            propellers[p].Rotate(propellerRotation);
        }
    }
}
