using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class OrbitalFocusCam : MonoBehaviour
{
    public Transform focalTarget, observePoint;
    public Vector3 exploreOffset;
    public float exploreAngle;
    public Camera cam;
    public float rotationSpeed, camDistance, lerpSpeed, observeLerpSpeed, deltaSensitivity;
    public bool exploring;

    private bool hasMoved, tracking, centered, offset;
    private Vector3 lastMouse, lastPos, lastCamPos, camTargetPos;
    private Quaternion lastRot, lastCamRot;
    private float startTime;
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
        offset = false;
        lastMouse = Vector3.zero;
        startTime = Time.time;
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
                        Debug.Log(hit.transform.name);
                        if (hit.transform != null && hit.transform != focalTarget && hit.transform.tag == "WorldButton")
                        {
                            if (hit.transform.GetComponent<WorldButton>().buttonType == "IslandTile")
                            {
                                focalTarget = hit.transform;
                                lastPos = transform.position;
                                centered = false;
                                startTime = Time.time;
                            }
                        }
                    }
                }

                hasMoved = false;
                tracking = false;
            }
        }


        if (!centered)
        {
            dof.focusDistance.value = Vector3.Distance(focalTarget.position, cam.transform.position);
            float timeTraversed = (Time.time - startTime);
            float fracJourney = 0;
            
            if (!exploring)
            {
                fracJourney = timeTraversed / observeLerpSpeed;
                transform.position = Vector3.Lerp(lastPos, observePoint.position, fracJourney);
                transform.rotation = Quaternion.Lerp(lastRot, Quaternion.Euler(0, observePoint.rotation.eulerAngles.y, 0), fracJourney);

                if (!offset)
                {
                    cam.transform.localPosition = Vector3.Lerp(lastCamPos, Vector3.zero, fracJourney);
                    cam.transform.localRotation = Quaternion.Lerp(lastCamRot, Quaternion.Euler(observePoint.eulerAngles.x,0,0), fracJourney);
                }
            }
            else
            {
                fracJourney = timeTraversed / lerpSpeed;
                transform.position = Vector3.Lerp(lastPos, focalTarget.position, fracJourney);

                if (!offset)
                {
                    cam.transform.localPosition = Vector3.Lerp(lastCamPos, exploreOffset, fracJourney);
                    cam.transform.localRotation = Quaternion.Lerp(lastCamRot, Quaternion.Euler(25, 0, 0), fracJourney);
                }
            }

            if (fracJourney >= 1.0f)
            {
                centered = true;
                offset = true;
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
        ResetState();
        exploring = explore;

        if (!explore)
            camTargetPos = Vector3.zero;
        else
            camTargetPos = exploreOffset;
    }

    public void SetNewObservePoint(Transform newObservePoint, Transform newFocalTarget)
    {
        if(newObservePoint != null)
            observePoint = newObservePoint;
        if(newFocalTarget != null)
            focalTarget = newFocalTarget;

        ResetState();
    }

    void ResetState()
    {
        lastPos = transform.position;
        lastRot = transform.rotation;
        lastCamPos = cam.transform.localPosition;
        lastCamRot = cam.transform.localRotation;
        startTime = Time.time;
        centered = false;
        hasMoved = false;
        tracking = false;
        offset = false;
    }

    public bool isAtTarget
    {
        get
        {
            return centered;
        }
    }
}
