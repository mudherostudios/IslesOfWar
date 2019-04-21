using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class OrbitalFocusCam : MonoBehaviour
{
    public Transform focalTarget;
    public Camera cam;
    public float rotationSpeed, camDistance, lerpSpeed, deltaSensitivity;
    private bool hasMoved, tracking, centered;
    private Vector3 lastMouse, lastPos;
    private float startTime, moveDistance;
    private DepthOfField dof;
    private PostProcessVolume ppVolume;

    void Start()
    {
        ppVolume = cam.GetComponent<PostProcessVolume>();
        ppVolume.profile.TryGetSettings(out dof);
        dof.focusDistance.value = Vector3.Distance(focalTarget.position, cam.transform.position);
        hasMoved = false;
        tracking = false;
        centered = true;
        lastMouse = Vector3.zero;
        startTime = Time.time;
        moveDistance = 0;
    }

    void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            transform.Rotate(Vector3.up, rotationSpeed * Input.GetAxis("Mouse X"));
        }

        if (Input.GetButtonUp("Fire1"))
        {
            if (!hasMoved)
            {
                RaycastHit hit;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                bool didHit = Physics.Raycast(ray, out hit);

                if(didHit)
                {
                    if (hit.transform != null && hit.transform != focalTarget)
                    {
                        focalTarget = hit.transform;
                        lastPos = transform.position;
                        centered = false;
                        startTime = Time.time;
                        moveDistance = Vector3.Distance(lastPos, focalTarget.position);
                    }
                }
            }

            hasMoved = false;
            tracking = false;
        }

        

        if (!centered)
        {
            dof.focusDistance.value = Vector3.Distance(focalTarget.position, cam.transform.position);
            float distCovered = (Time.time - startTime) * lerpSpeed;
            float fracJourney = distCovered / moveDistance;
            transform.position = Vector3.Lerp(lastPos, focalTarget.position, fracJourney);

            if (fracJourney >= 1.0f)
            {
                centered = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (Input.GetButton("Fire1"))
        {
            if(!tracking)
            {
                lastMouse = Input.mousePosition;
                tracking = true;
            }

            if (Vector3.Distance(lastMouse, Input.mousePosition) > deltaSensitivity)
            {
                hasMoved = true;
            }
            else
            {
                lastMouse = Input.mousePosition;
            }
        }
    }
}
