using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductShowcaser : MonoBehaviour
{
    public float rotateSpeed, verticalSpeed;
    public float minHeight, maxHeight;
    private bool goingUp = true;
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

        if (goingUp)
        {
            transform.Translate(Vector3.up * verticalSpeed * Time.deltaTime);
        }
        else
        {
            transform.Translate(Vector3.down * verticalSpeed * Time.deltaTime);
        }

        if (transform.position.y > startPos.y + maxHeight)
            goingUp = false;
        else if (transform.position.y < startPos.y + minHeight)
            goingUp = true;
    }
}
