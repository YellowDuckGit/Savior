using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CountdownTimer : MonoBehaviour
{
    public float totalTime = 60f; 
    private float currentTime;
    public TextMeshProUGUI timerText;
    public bool isTimerRunning;
    private bool isEnd;

    public Image circle;

    public Transform parentEffect;
    public Transform childrenEffect;

    private void OnEnable()
    {
        StartTimer();
    }
   

    void Update()
    {
        if (isTimerRunning)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerText();
            SetProcessBar();
            MoveingEffect();
            if (currentTime <= 0f)
            {
                TimerComplete();
            }
        }
    }

    public void StartTimer()
    {
        ResetTimer();
        isTimerRunning = true;
    }

    public void PauseTimer()
    {
        isTimerRunning = false;
    }

    public void ResetTimer()
    {
        circle.fillAmount = 1;
        currentTime = totalTime;
        UpdateTimerText();
        isTimerRunning = false;
        isEnd = false;
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }

    private void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        string timerString = string.Format("{0:00}:{1:00}", minutes, seconds);
        timerText.text = timerString;

    }

    private void SetProcessBar()
    {
        circle.fillAmount = currentTime/totalTime;
    }

    private void MoveingEffect()
    {
        float persent = currentTime/totalTime;
    
        parentEffect.eulerAngles = new Vector3(0,0,-360 * persent);
        childrenEffect.eulerAngles = new Vector3(0, 0, parentEffect.rotation.z);


    }

    private void TimerComplete()
    {
        isTimerRunning = false;
        isEnd = true;
        Debug.Log("Countdown timer completed!");
    }
}