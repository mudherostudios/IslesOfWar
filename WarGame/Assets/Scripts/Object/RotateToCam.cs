using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToCam : MonoBehaviour
{
    public float speed;
    public bool lockY = true;
    private Transform mainCam;


    void Start()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").transform;        
    }

    void Update()
    {
        Vector3 targetDir = mainCam.position - transform.position;

        if(lockY)
            targetDir.y = 0;

        float step = speed * Time.deltaTime;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
        Debug.DrawRay(transform.position, newDir, Color.red);
        transform.rotation = Quaternion.LookRotation(newDir);
    }
}
