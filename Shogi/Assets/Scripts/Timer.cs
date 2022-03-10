using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Timer : MonoBehaviour
{
    public float timeRemaining = 60; //one minute 
    public bool timerIsRunning = false;
    public Text timeText;
    private void Start()
    {
        
	//if(//botton is clicked)
	//{        
	   timerIsRunning = true;
	//}
    }
    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
		DisplayTime(timeRemaining);
            }
            else
            {
                Debug.Log("Time has run out!");
                timeRemaining = 0;
                timerIsRunning = false;
		//end game
            }
        }
    }
	void DisplayTime(float timeToDisplay)
    {
	float seconds = Mathf.FloorToInt(timeToDisplay % 60);
	timeText.text = string.Format("{00}", seconds);
    }
}