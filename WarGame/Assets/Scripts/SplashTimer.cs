using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashTimer : MonoBehaviour
{
    public float timeToSwitch;
	public int levelToSwitchTo;
	private float startTime;
	
	void Start()
	{
		startTime = Time.time;
	}
	
    void Update()
    {
        if(Time.time - startTime > timeToSwitch)
		{
			SceneManager.LoadScene(levelToSwitchTo);
		}
    }
}
