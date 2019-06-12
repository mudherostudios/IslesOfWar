using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockToRelativePoint : MonoBehaviour
{
    public Transform objectLock;

    private void Update()
    {
        transform.position = objectLock.position;
    }
}
