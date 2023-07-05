using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CounterTime : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    private float startTime;
    [SerializeField]
    private bool isPaused = true;
    private float deltaTime;

    private void Awake()
    {
        //startTime = Time.time;
        //PauseTimer();
    }

    private void Start()
    {
      
    }

    private void Update()
    {
        if (isPaused)
            return;

        float currentTime = Time.time - startTime - deltaTime;
        string minutes = ((int)currentTime / 60).ToString("00");
        string seconds = (currentTime % 60).ToString("00");
        timerText.text = minutes + ":" + seconds;
    }

    public void PauseTimer()
    {
        print("PauseTimer");
        isPaused = true;
        deltaTime += Time.time - startTime;
    }

    public void ResumeTimer()
    {
        print("ResumeTimer");
        isPaused = false;
        startTime = Time.time;
    }
}
