using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TickerTapeTextureScroll : MonoBehaviour
{
    public Renderer rendererToScroll;
    public Vector3 speed;
    private Vector3 current;

    public void Update()
    {
        current = rendererToScroll.material.mainTextureOffset;
        Vector3 multiplied = speed * Time.deltaTime;
        rendererToScroll.material.mainTextureOffset = new Vector3(current.x + multiplied.x, current.y + multiplied.y, current.z + multiplied.z);
    }
}
