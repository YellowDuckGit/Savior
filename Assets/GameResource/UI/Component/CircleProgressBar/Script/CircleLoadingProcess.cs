using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleLoadingProcess : MonoBehaviour
{
    
    public CountdownTimer timer;

    private void Update()
    {
        if(timer.isTimerRunning)
        setProcess(timer.GetCurrentTime());
    }

    private void Start()
    {
      
    }

    public void setProcess(float number)
    {
       
    }
}
