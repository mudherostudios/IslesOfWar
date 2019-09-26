using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingOrbiter : MonoBehaviour
{
    public float rotationSpeed;
    public float timeToChange;

    private Quaternion rotation;
    private float lastTime;

    private void Start()
    {
        rotation = Random.rotation;
        transform.rotation = Random.rotation;
        lastTime = Time.time;
    }

    private void Update()
    {
        transform.Rotate(rotation.eulerAngles, rotationSpeed * Time.deltaTime);

        if (Time.time - lastTime >= timeToChange)
        {
            lastTime = Time.time;
            rotation = Random.rotation;
        }
    }
}
