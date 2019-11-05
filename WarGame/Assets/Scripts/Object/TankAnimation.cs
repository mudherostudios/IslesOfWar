using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankAnimation : MonoBehaviour
{
    public Transform turret;
    public Vector3 axis;
    public float minRotSpeed, maxRotSpeed, minRotTime, maxRotTime, minWaitTime, maxWaitTime;

    private float rotSpeed, rotTime, waitTime;
    private bool rotPositive;
    private float rotStartTime, waitStartTime, currentWaitTimer;
    private bool isWaiting;

    private void Start()
    {
        rotTime = Random.Range(minRotTime, maxRotTime);
        rotSpeed = Random.Range(minRotSpeed, maxRotSpeed);
        waitTime = Random.Range(minWaitTime, maxWaitTime);
        rotPositive = true;
        isWaiting = false;
    }

    private void Update()
    {
        if (!isWaiting)
        {
            float directionModifier = -1.0f;
            if (rotPositive)
                directionModifier = 1.0f;

            turret.Rotate(axis, directionModifier * rotSpeed);

            if (Time.time - rotStartTime > rotTime)
            {
                waitTime = Random.Range(minWaitTime, maxWaitTime);
                isWaiting = true;
                waitStartTime = Time.time;
            }
        }

        if(isWaiting)
        {
            currentWaitTimer = Time.time - waitStartTime;

            if (currentWaitTimer > waitTime)
            {
                isWaiting = false;
                currentWaitTimer = 0.0f;

                rotTime = Random.Range(minRotTime, maxRotTime);
                rotSpeed = Random.Range(minRotSpeed, maxRotSpeed);
                rotStartTime = Time.time;

                int dir = Mathf.RoundToInt(Random.Range(0, 2));
                rotPositive = true;
                if (dir == 0)
                    rotPositive = false;
            }
        }
    }
}
