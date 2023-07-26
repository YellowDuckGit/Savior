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
    public MMF_Player OpenPack;
    public MMF_Player Click_Payment;

    // IN MATCH
    public MMF_Player DrawCard;
    public MMF_Player Battle;
    public MMF_Player OutBlood; // BREAK TUBE // GAME OVER

    public MMF_Player SummonMonster;
    public MMF_Player CardGetDame;
    public MMF_Player DestroyCard;
    public MMF_Player SkipTurn;

    public MMF_Player FoundMatch;
    public MMF_Player SwitchCam;

    public MMF_Player Victory;
    public MMF_Player Defeat;

    public MMF_Player Background_Match;

    public MMF_Player YourTurn;
    public MMF_Player YourAttack;
    public MMF_Player YourDefense;

    public MMF_Player PourMana;
    public MMF_Player ATKHP;
    public MMF_Player Coin;
    public MMF_Player Painful;
    #endregion

    #region PLAY SOUND UI
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

    public void PlayOpenPack()
    {
        print("PLAY OPEN PACK SOUND");
        OpenPack.PlayFeedbacks();
    }
    public void PlayClick_Payment()
    {
        print("PLAY CLICK PAYMENT SOUND");
        Click_Payment.PlayFeedbacks();
    }
    #endregion

    #region PLAY SOUND IN MATCH
    public void PlayPainful()
    {
        print("PLAY PAINFUL SOUND");
        Painful.PlayFeedbacks();
    }
    public void PlayATKHP()
    {
        print("PLAY ATK HP SOUND");
        ATKHP.PlayFeedbacks();
    }
    public void PlayCoinSound()
    {
        print("PLAY COIN SOUND");
        Coin.PlayFeedbacks();
    }
    public void StopCoinSound()
    {
        print("PLAY COIN SOUND");
        Coin.StopFeedbacks();
    }
    public void PlayPourMana()
    {
        print("PLAY POUR MANA SOUND");
        PourMana.PlayFeedbacks();
    }
    public void PlayYourTurn()
    {
        print("PLAY YOUR TURN SOUND");
        YourTurn.PlayFeedbacks();
    }
    public void PlayYourAttack()
    {
        print("PLAY YOUR ATTACK SOUND");
        YourAttack.PlayFeedbacks();
    }
    public void PlayYourDefense()
    {
        print("PLAY YOUR DEFENSE SOUND");
        YourDefense.PlayFeedbacks();
    }
    public void PlayDrawCard()
    {
        print("PLAY DRAW CARD SOUND");
        DrawCard.PlayFeedbacks();
    }
    public void PlayBattle()
    {
        print("PLAY BATTLE SOUND");
        Battle.PlayFeedbacks();
    }
    public void PlayOutBlood()
    {
        print("PLAY OUT BLOOD SOUND");
        OutBlood.PlayFeedbacks();
    }
    public void PlaySummonMonster()
    {
        print("PLAY SUMMON MONSTER SOUND");
        SummonMonster.PlayFeedbacks();
    }
    public void PlayGetDame()
    {
        print("PLAY GET DAME SOUND");
        CardGetDame.PlayFeedbacks();
    }
    public void PlayDestroyCard()
    {
        print("PLAY DESTROY CARD SOUND");
        DestroyCard.PlayFeedbacks();
    }
    public void PlaySkipTurn()
    {
        print("PLAY SKIP TURN SOUND");
        SkipTurn.PlayFeedbacks();
    }
    public void PlayFoundMatch()
    {
        print("PLAY FOUND MATCH SOUND");
        FoundMatch.PlayFeedbacks();
    }
    public void PlaySwitchCam()
    {
        print("PLAY SWITCH CAM SOUND");
        SwitchCam.PlayFeedbacks();
    }
    public void PlayVictory()
    {
        print("PLAY VICTORY SOUND");
        Victory.PlayFeedbacks();
    }
    public void PlayDefeat()
    {
        print("PLAY DEFEAT SOUND");
        Defeat.PlayFeedbacks();
    }
    public void PlayBackground_Match()
    {
        print("PLAY BACKGROUND MATCH SOUND");
        Background_Match.PlayFeedbacks();
    }
    #endregion
}
