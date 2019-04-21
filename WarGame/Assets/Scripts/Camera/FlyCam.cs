using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyCam : MonoBehaviour
{
    public float rotSpeed, strafeSpeed, forwardSpeed, verticalSpeed, boost;
    private Vector2 rotation;

    void FixedUpdate()
    {
        float tempBoost = 1.0f;
        if (Input.GetButton("Boost"))
        {
            tempBoost = boost;
        }

        float xSpeed = Input.GetAxis("Horizontal") * strafeSpeed * Time.deltaTime *tempBoost;
        float ySpeed = Input.GetAxis("Up") * verticalSpeed * Time.deltaTime * tempBoost;
        float zSpeed = Input.GetAxis("Vertical") * forwardSpeed * Time.deltaTime * tempBoost;
        transform.Translate(new Vector3(xSpeed, ySpeed, zSpeed));

        if(Input.GetButton("Fire2"))
        {
            rotation.y += Input.GetAxis("Mouse X");
            rotation.x += -Input.GetAxis("Mouse Y");
            transform.eulerAngles = (Vector2)rotation * rotSpeed;
        }
    }
}
