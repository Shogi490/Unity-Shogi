using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Timer : MonoBehaviour
{

    public float timeRemaining = 60;
    public bool timerIsRunning = false;
    public Text timeText;
    [SerializeField]
    private Button _button = null;
    

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

    public void updateTime(float x)
    {
        timeRemaining = x;
    }



    public void OnClick()
    {
         timerIsRunning = true;  
    }
}