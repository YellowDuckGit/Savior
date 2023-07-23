using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    private void Awake()
    {
        //DontDestroyOnLoad(gameObject);

        if (instance != null && instance != this)
        {
            print("sound manager have 2");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    #region Define MMF
    public MMF_Player Background_Login;
    public MMF_Player Background_Home;
    public MMF_Player ClickNormal;
    public MMF_Player ClickBack;
    public MMF_Player Background_FindMatch;

    #endregion

    #region PLAY SOUND
    public void PlayBackground_Login()
    {
        print("PLAY BACKGROUND LOGIN SOUND");
        Background_Login.PlayFeedbacks();
    }

    public void PLayBackground_Home()
    {
        print("PLAY BACKGROUND HOME SOUND");
        Background_Home.PlayFeedbacks();
    }

    public void PlayClick_Normal()
    {
        print("PLAY CLICK NORMAL SOUND");
        ClickNormal.PlayFeedbacks();
    }

    public void PlayClick_Back()
    {
        print("PLAY CLICK BACK SOUND");
        ClickBack.PlayFeedbacks();
    }

    public void PLayBackground_FindMatch()
    {
        print("PLAY BACKGROUND FIND MATCH SOUND");
        Background_FindMatch.PlayFeedbacks();
    }
    #endregion
}
