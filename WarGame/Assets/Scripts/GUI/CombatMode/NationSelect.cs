using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using IslesOfWar;

public class NationSelect : MonoBehaviour
{
    public CommandIslandInteraction commandScript;
    public Image flag;
    public float maxSize;
    public Dropdown countryList;
    public string resourcePathToFlags;
    public string fullPathToFlags;
    List<string> keys;

    public void Start()
    {
        SetList();
        ChangeFlag();
    }

    public void Update()
    {

    }

    public void ChangeFlag()
    {
        int index = countryList.value;
        string countryCode = keys[index];
        string filePath = string.Format("{0}/{1}.png", fullPathToFlags, countryCode.ToLower());
        string resourcePath = string.Format("{0}/{1}", resourcePathToFlags, countryCode.ToLower());

        if (File.Exists(filePath))
        {
            Texture flagTexture = (Texture)Resources.Load(resourcePath);
            flag.material.SetTexture("_MainTex", flagTexture);

            int height = flagTexture.height;
            int width = flagTexture.width;

            float modY = (float)height / width;
            float modX = (float)width / height;

            if (modX >= 1.8f)
            {
                modX *= 0.9f;
                modY *= 0.9f; 
            }

            flag.rectTransform.localScale = new Vector2(modX, modY * modX);

            flag.gameObject.SetActive(false);
            flag.gameObject.SetActive(true);
        }
        else
            Debug.Log(string.Format("Flag File {0} Does Not Exist!", filePath));
    }

    public void SetList()
    {
        keys = new List<string>(Constants.countryCodes.Keys);
        List<string> values = new List<string>(Constants.countryCodes.Values);
        List<string> options = new List<string>();

        for (int k = 0; k < keys.Count; k++)
        {
            options.Add(string.Format("({0}) - {1}", keys[k], values[k]));
        }

        countryList.AddOptions(options);
    }

    public void SubmitFlag()
    {
        int index = countryList.value;
        string countryCode = keys[index];
        commandScript.ChangeNation(countryCode, !commandScript.isPlaying);
        gameObject.SetActive(false);
    }
}
