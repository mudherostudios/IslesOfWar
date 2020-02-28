using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScreenshot : MonoBehaviour
{
    public string filePath = "C:\\Users\\Caz\\Desktop\\test.png";

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SaveScreenshotToFile(filePath);
        }
        else if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SaveScreenshotToFile(filePath, false);
        }
    }

    public static Texture2D TakeScreenShot()
    {
        return Screenshot();
    }

    static Texture2D TransparentScreenshot()
    {
        int resWidth = Camera.main.pixelWidth;
        int resHeight = Camera.main.pixelHeight;
        Camera camera = Camera.main;
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 32);
        camera.targetTexture = rt;
        Texture2D screenshot = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);
        camera.Render();
        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        screenshot.Apply();
        camera.targetTexture = null;
        RenderTexture.active = null; 
        Destroy(rt);
        return screenshot;
    }

    static Texture2D Screenshot()
    {
        int resWidth = Camera.main.pixelWidth;
        int resHeight = Camera.main.pixelHeight;
        Camera camera = Camera.main;
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 32);
        camera.targetTexture = rt;
        Texture2D screenshot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        screenshot.Apply();
        camera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        return screenshot;
    }

    public static Texture2D SaveScreenshotToFile(string fileName, bool transparent=true)
    {
        Texture2D screenshot = Screenshot();
        byte[] bytes = screenshot.EncodeToPNG();
        System.IO.File.WriteAllBytes(fileName, bytes);
        return screenshot;
    }
}
