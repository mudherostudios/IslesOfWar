using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class OrbitalFocusCam : MonoBehaviour
{
    public Transform focalTarget, observePoint;
    public Camera cam;
    public float rotationSpeed, camDistance, lerpSpeed, rotLerpSpeed, deltaSensitivity;
    public bool exploring;

    private bool observing, hasMoved, tracking, centered;
    private Vector3 lastMouse, lastPos;
    private Quaternion lastRot;
    private float startTime, moveDistance, rotateAngle;
    private DepthOfField dof;
    private PostProcessVolume ppVolume;

    void Start()
    {
        ppVolume = cam.GetComponent<PostProcessVolume>();
        ppVolume.profile.TryGetSettings(out dof);
        dof.focusDistance.value = Vector3.Distance(focalTarget.position, cam.transform.position);
        exploring = false;
        hasMoved = false;
        tracking = false;
        observing = true;
        centered = true;
        lastMouse = Vector3.zero;
        startTime = Time.time;
        moveDistance = 0;
    }

    void Update()
    {
        if (exploring)
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

                    if (didHit)
                    {
                        if (hit.transform != null && hit.transform != focalTarget && hit.transform.tag == "WorldButton")
                        {
                            if (hit.transform.GetComponent<WorldButton>().buttonType == "IslandTile")
                            {
                                focalTarget = hit.transform;
                                lastPos = transform.position;
                                centered = false;
                                startTime = Time.time;
                                moveDistance = Vector3.Distance(lastPos, focalTarget.position);
                            }
                        }
                    }
                }

                hasMoved = false;
                tracking = false;
            }
        }
        else
        {
            if (!observing)
            {
                float angleCovered = (Time.time - startTime) * rotLerpSpeed;
                float fracRotation = angleCovered / rotateAngle;

                transform.rotation = Quaternion.Lerp(lastRot, observePoint.rotation, fracRotation);

                if (fracRotation >= 1.0f)
                {
                    observing = true;
                }
            }
        }

        if (!centered)
        {
            dof.focusDistance.value = Vector3.Distance(focalTarget.position, cam.transform.position);
            float distCovered = (Time.time - startTime) * lerpSpeed;
            float fracJourney = distCovered / moveDistance;

            if(!exploring)
                transform.position = Vector3.Lerp(lastPos, observePoint.position, fracJourney);
            else
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

    public void ExploreMode(Transform explorationPoint, bool explore)
    {
        focalTarget = explorationPoint;
        lastPos = transform.position;
        lastRot = transform.rotation;
        startTime = Time.time;
        centered = false;
        hasMoved = false;
        tracking = false;

        if (!explore)
        {
            moveDistance = Vector3.Distance(lastPos, observePoint.position);
            rotateAngle = Quaternion.Angle(lastRot, observePoint.rotation);
            exploring = false;
            observing = false;
        }
        else
        {
            moveDistance = Vector3.Distance(lastPos, focalTarget.position);
            exploring = true;
            observing = false;
        }
    }
}
