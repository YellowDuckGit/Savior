using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    private void Awake()
    {
        //DontDestroyOnLoad(gameObject);

        if (instance != null && instance != this)
        {
            Debug.LogError("tutorial manager have 2");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public enum tutorial
    {
        HomeTutorial,
        StoreTutorial
    }

    public List<tutorial> tutorialChain = new List<tutorial>()
    {
        tutorial.HomeTutorial,
        tutorial.StoreTutorial
    };

    public tutorial tutorialCurrent;

    public MMF_Player StoreTutorial;
    public MMF_Player HomeTutorial;

    public bool isPlayTutorial;
    public bool isPlayTutorialChain;

    private void Start()
    {
        isPlayTutorialChain = true;
    }
    public void PlayTutorialChain()
    {
        if (isPlayTutorialChain)
        {
            if (tutorialCurrent == tutorialChain[0])
            {
                PlayTutorial(tutorialChain[0]);
            }
            else
            {
                int index = tutorialChain.IndexOf(tutorialCurrent);
                PlayTutorial(tutorialChain[index + 1]);
            }
        }
    }
    public void TutorialFinished()
    {
        if (isPlayTutorial)
        {
            Debug.LogError("TUTORIAL FINISHED");
            isPlayTutorial = false;
        }
    }
    public void PlayTutorial(tutorial tutorialEnum)
    {
        Debug.LogError("PLAY TUTORIAL");
        isPlayTutorial = true;
        switch (tutorialEnum)
        {
            case tutorial.StoreTutorial:
                Debug.LogError("STORE TUTORIAL");
                tutorialCurrent = tutorial.StoreTutorial;
                PlayStoreTutorial();
                break;

            case tutorial.HomeTutorial:
                Debug.LogError("HOME TUTORIAL");
                tutorialCurrent = tutorial.HomeTutorial;
                PlayHomeTutorial();
                break;
            default:
                Debug.LogError("not found tutorial");
                break;
        }
    }


    public void PlayStoreTutorial()
    {
        StoreTutorial.PlayFeedbacks(this.transform.position, 1);
        print("PLAY STORE FEEDBACK");
    }

    public void PlayHomeTutorial()
    {
        HomeTutorial.PlayFeedbacks(this.transform.position, 1); ;
        print("PLAY HOME FEEDBACK");

    }
}
