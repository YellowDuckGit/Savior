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
            print("tutorial manager have 2");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public enum tutorial
    {
        None,
        SkipTutorial,
        WelcomeTutorial,
        ProfileTutorial,
        CoinTutorial,
        FriendTutorial,
        MainNavInHomeTutorial,
        StoreButtonInHome,
        BagButtonInHome,
        PlayButtonInHome,
        DeckButtonInHome,
        SettingButtonInHome,
        ClickingStore,
        NavStoreTutorial,
        PackStoreTutorial,
        PopupPackStoreTutorial,
        NavDeckStoreTutorial,
        DeckStoreTutorial,
        PopupDeckStoreTutorial,
        CollectionDeckTutorial,
        ClickingDeckInBag,
        CardInDeck,
        CardNameInDeck,
        LimitCardInDeck,
        SearchCardInDeck,
        CardCollectionInDeck,
        PopupCardInfo,
        RemoveCardInDeckTutorial,
        PutCardInDeckTutorial,
        SaveDeckInDeck,
        DeckBackToHome,
        ClickingPlayInHome,
        SelectGameModeTutorial,
        ChooseDeckTutorial,
        FindMatchTutorial
    }

    private List<tutorial> tutorialChain = new List<tutorial>()
    {
        tutorial.None ,
        tutorial.SkipTutorial,
        tutorial.WelcomeTutorial,
        tutorial.ProfileTutorial,
        tutorial.CoinTutorial,
        tutorial.FriendTutorial,
        tutorial.MainNavInHomeTutorial,
        tutorial.StoreButtonInHome,
        tutorial.BagButtonInHome,
        tutorial.PlayButtonInHome,
        tutorial.DeckButtonInHome,
        tutorial.SettingButtonInHome,
        tutorial.ClickingStore,
        tutorial.NavStoreTutorial,
        tutorial.PackStoreTutorial,
        tutorial.PopupPackStoreTutorial,
        tutorial.NavDeckStoreTutorial,
        tutorial.DeckStoreTutorial,
        tutorial.PopupDeckStoreTutorial,
        tutorial.CollectionDeckTutorial,
        tutorial.ClickingDeckInBag,
        tutorial.CardInDeck,
        tutorial.CardNameInDeck,
        tutorial.LimitCardInDeck,
        tutorial.SearchCardInDeck,
        tutorial.CardCollectionInDeck,
        tutorial.PopupCardInfo,
        tutorial.RemoveCardInDeckTutorial,
        tutorial.PutCardInDeckTutorial,
        tutorial.SaveDeckInDeck,
        tutorial.DeckBackToHome,
        tutorial.ClickingPlayInHome,
        tutorial.SelectGameModeTutorial,
        tutorial.ChooseDeckTutorial,
        tutorial.FindMatchTutorial
    };

    public tutorial tutorialCurrent;

    #region Define MMF
    /* WELCOME */
    public MMF_Player WelcomeTutorial;
    public MMF_Player SkipTutorial;
    public MMF_Player ProfileTutorial;
    public MMF_Player CoinTutorial;
    public MMF_Player FriendTutorial;
    public MMF_Player MainNavInHomeTutorial;
    public MMF_Player StoreButtonInHome;
    public MMF_Player BagButtonInHome;
    public MMF_Player DeckButtonInHome;
    public MMF_Player SettingButtonInHome;
    public MMF_Player PlayButtonInHome;

    /* STORE */
    public MMF_Player ClickingStore;
    public MMF_Player NavStoreTutorial;
    public MMF_Player PackStoreTutorial;
    public MMF_Player PopupPackStoreTutorial;
    public MMF_Player NavDeckStoreTutorial;
    public MMF_Player DeckStoreTutorial;
    public MMF_Player PopupDeckStoreTutorial;

    /* COLLECTION DECK */
    public MMF_Player CollectionDeckTutorial;
    public MMF_Player ClickingDeckInBag;
    public MMF_Player CardInDeck;
    public MMF_Player CardNameInDeck;
    public MMF_Player LimitCardInDeck;
    public MMF_Player CardCollectionInDeck;
    public MMF_Player PopupCardInfo;
    public MMF_Player SearchCardInDeck;
    public MMF_Player RemoveCardInDeckTutorial;
    public MMF_Player PutCardInDeckTutorial;
    public MMF_Player SaveDeckInDeck;
    public MMF_Player DeckBackToHome;
    /* GAME MODE AND PLAY*/
    public MMF_Player ClickingPlayInHome;
    public MMF_Player SelectGameModeTutorial;
    public MMF_Player ChooseDeckTutorial;
    public MMF_Player FindMatchTutorial;

    public MMF_Player PlayWithFriend;
    public MMF_Player PVFInHome;

    #endregion

    public bool isPlayTutorial;
    public bool isPlayTutorialChain;
    public bool isSkip;
    public bool isNewbie;
    public bool isPVF;

    private void Start()
    {
    }

    public void Skip()
    {
        print("SKIP TUTORIAL");
        PlayTutorial(tutorialChain[0]);
        UIManager.instance.PanelSkipTutorial.gameObject.SetActive(false);
    }

    public void PlayTutorialChain()
    {
        print($"AMOUNT CHAIN IS {tutorialChain.Count}");
        print($"PLAY TUTORIAL CHAIN");
        if (isPlayTutorialChain && !isPlayTutorial && !isSkip && isNewbie)
        {
            if (tutorialCurrent == tutorial.None)
            {
                print("None");
                PlayTutorial(tutorialChain[1]);
            }
            else
            {
                int index = tutorialChain.IndexOf(tutorialCurrent) + 1;
                print($"index is {index}");
                if (index <= tutorialChain.Count - 1)
                {
                    PlayTutorial(tutorialChain[index]);
                    print("Play");
                }
            }
        }
    }
    public void TutorialFinished()
    {
        if (isPlayTutorial)
        {
            print("TUTORIAL FINISHED");
            isPlayTutorial = false;
        }
    }
    public void PlayTutorial(tutorial tutorialEnum)
    {
        print("PLAY TUTORIAL");
        print("PLAY" + tutorialEnum);

        isPlayTutorial = true;
        switch (tutorialEnum)
        {
            /* HOME TUTORIAL */

            case tutorial.WelcomeTutorial:
                print("WELCOME TUTORIAL");
                tutorialCurrent = tutorial.WelcomeTutorial;
                PlayWelcomeTutorial();
                break;
            case tutorial.SkipTutorial:
                print("IS SKIP TUTORIAL");
                tutorialCurrent = tutorial.SkipTutorial;
                PlaySkipTutorial();
                break;
            case tutorial.ProfileTutorial:
                print("PROFILE TUTORIAL");
                tutorialCurrent = tutorial.ProfileTutorial;
                PlayProfileTutorial();
                break;
            case tutorial.CoinTutorial:
                print("COIN TUTORIAL");
                tutorialCurrent = tutorial.CoinTutorial;
                PlayCoinTutorial();
                break;
            case tutorial.FriendTutorial:
                print("FRIEND TUTORIAL");
                tutorialCurrent = tutorial.FriendTutorial;
                PlayFriendTutorial();
                break;
            case tutorial.MainNavInHomeTutorial:
                print("MAIN NAV IN HOME TUTORIAL");
                tutorialCurrent = tutorial.MainNavInHomeTutorial;
                PlayMainNavInHomeTutorial();
                break;
            case tutorial.StoreButtonInHome:
                print("STORE BUTTON IN HOME TUTORIAL");
                tutorialCurrent = tutorial.StoreButtonInHome;
                PlayStoreButtonTutorial();
                break;
            case tutorial.BagButtonInHome:
                print("BAG BUTTON IN HOME TUTORIAL");
                tutorialCurrent = tutorial.BagButtonInHome;
                PlayBagButtonTutorial();
                break;
            case tutorial.PlayButtonInHome:
                print("PLAY BUTTON IN HOME TUTORIAL");
                tutorialCurrent = tutorial.PlayButtonInHome;
                PlayPlayButtonTutorial();
                break;
            case tutorial.DeckButtonInHome:
                print("DECK BUTTON IN HOME TUTORIAL");
                tutorialCurrent = tutorial.DeckButtonInHome;
                PlayDeckButtonTutorial();
                break;
            case tutorial.SettingButtonInHome:
                print("SETTING BUTTON IN HOME TUTORIAL");
                tutorialCurrent = tutorial.SettingButtonInHome;
                PlaySettingButtonTutorial();
                break;

            /* STORE TUTORIAL */
            /* PACK */

            case tutorial.ClickingStore:
                print("CLICKING IN HOME TUTORIAL");
                tutorialCurrent = tutorial.ClickingStore;
                PlayClickingStoreTutorial();
                break;
            case tutorial.PackStoreTutorial:
                print("PACK STORE TUTORIAL");
                tutorialCurrent = tutorial.PackStoreTutorial;
                PlayPackStoreTutorial();
                break;
            case tutorial.NavStoreTutorial:
                print("NAV STORE TUTORIAL");
                tutorialCurrent = tutorial.NavStoreTutorial;
                PlayNavStoreTutorial();
                break;
            case tutorial.PopupPackStoreTutorial:
                print("POPUP PACK STORE TUTORIAL");
                tutorialCurrent = tutorial.PopupPackStoreTutorial;
                PLayPopupPackStoreTutorial();
                break;

            /* DECK */

            case tutorial.NavDeckStoreTutorial:
                print("NAV DECK STORE TUTORIAL");
                tutorialCurrent = tutorial.NavDeckStoreTutorial;
                PlayNavDeckStoreTutorial();
                break;
            case tutorial.DeckStoreTutorial:
                print("DECK STORE TUTORIAL");
                tutorialCurrent = tutorial.DeckStoreTutorial;
                PlayDeckStoreTutorial();
                break;
            case tutorial.PopupDeckStoreTutorial:
                print("POPUP DECK STORE TUTORIAL");
                tutorialCurrent = tutorial.PopupDeckStoreTutorial;
                PlayPopupDeckStoreTutorial();
                break;

            /* COLLECTION DECK*/
            case tutorial.CollectionDeckTutorial:
                print("COLLECTION DECK TUTORIAL");
                tutorialCurrent = tutorial.CollectionDeckTutorial;
                PlayColectionDeckTutorial();
                break;
            case tutorial.ClickingDeckInBag:
                print("CLICKING DECK IN BAG TUTORIAL");
                tutorialCurrent = tutorial.ClickingDeckInBag;
                PlayClickingDeckInBagTutorial();
                break;
            case tutorial.CardInDeck:
                print("CARD IN DECK DECK TUTORIAL");
                tutorialCurrent = tutorial.CardInDeck;
                PlayCardInDeckTutorial();
                break;
            case tutorial.CardNameInDeck:
                print("CARD NAME IN DECK TUTORIAL");
                tutorialCurrent = tutorial.CardNameInDeck;
                PlayCardNameTutorial();
                break;
            case tutorial.LimitCardInDeck:
                print("LIMIT CARD IN DECK TUTORIAL");
                tutorialCurrent = tutorial.LimitCardInDeck;
                PlayLimitCardInDeck();
                break;
            case tutorial.CardCollectionInDeck:
                print("CARD COLLECTION IN DECK TUTORIAL");
                tutorialCurrent = tutorial.CardCollectionInDeck;
                PlayCardCollectionInDeck();
                break;
            case tutorial.SaveDeckInDeck:
                print("SAVE DECK IN DECK TUTORIAL");
                tutorialCurrent = tutorial.SaveDeckInDeck;
                PlaySaveDeckInDeck();
                break;
            case tutorial.PopupCardInfo:
                print("POPUP CARD INFO TUTORIAL");
                tutorialCurrent = tutorial.PopupCardInfo;
                PlayPopupCardInfo();
                break;
            case tutorial.SearchCardInDeck:
                print("SEARCH CARD IN DECK TUTORIAL");
                tutorialCurrent = tutorial.SearchCardInDeck;
                PlaySearchCardInDeck();
                break;
            case tutorial.RemoveCardInDeckTutorial:
                print("REMOVE CARD IN DECK TUTORIAL");
                tutorialCurrent = tutorial.RemoveCardInDeckTutorial;
                PlayRemoveCardInDeck();
                break;
            case tutorial.PutCardInDeckTutorial:
                print("PUT CARD IN DECK TUTORIAL");
                tutorialCurrent = tutorial.PutCardInDeckTutorial;
                PlayPutCardInDeck();
                break;
            case tutorial.DeckBackToHome:
                print("DECK BACK TO HOME TUTORIAL");
                tutorialCurrent = tutorial.DeckBackToHome;
                PlayDeckBackToHome();
                break;

            /* GAME MODE n PLAY*/
            case tutorial.ClickingPlayInHome:
                print("CLICKING PLAY IN HOME TUTORIAL");
                tutorialCurrent = tutorial.ClickingPlayInHome;
                PlayClickingPlayTutorial();
                break;
            case tutorial.SelectGameModeTutorial:
                print("SELECT GAME MODE TUTORIAL");
                tutorialCurrent = tutorial.SelectGameModeTutorial;
                PlayGameModeTutorial();
                break;
            case tutorial.ChooseDeckTutorial:
                print("CHOOSE DECK TUTORIAL");
                tutorialCurrent = tutorial.ChooseDeckTutorial;
                PlayChooseDeckTutorial();
                break;
            case tutorial.FindMatchTutorial:
                print("FIND MATCH TUTORIAL");
                tutorialCurrent = tutorial.FindMatchTutorial;
                PlayFindMatchTutorial();
                break;
            default:
                print("not found tutorial");
                break;


        }
    }


    #region PLay Feedbacks
    public void PlayPVFInHome()
    {
        PVFInHome.PlayFeedbacks();
        print("PLAY PVF IN HOME TUTORIAL");
    }
    public void PlayWithFriendTutorial()
    {
        PlayWithFriend.PlayFeedbacks();
        print("PLAY WITH FRIEND TUTORIAL");
        isPVF = true;
    }
    public void PlaySkipTutorial()
    {
        SkipTutorial.PlayFeedbacks();
        print("PLAY SKIP TUTORIAL");
    }
    public void PlayFindMatchTutorial()
    {
        FindMatchTutorial.PlayFeedbacks();
        print("PLAY FIND MATCH TUTORIAL");
    }

    public void PlayChooseDeckTutorial()
    {
        ChooseDeckTutorial.PlayFeedbacks();
        print("PLAY CHOOSE DECK TUTORIAL");
    }

    public void PlayClickingPlayTutorial()
    {
        ClickingPlayInHome.PlayFeedbacks();
        print("PLAY CLICKING PLAY IN HOME TUTORIAL");
    }

    public void PlayGameModeTutorial()
    {
        SelectGameModeTutorial.PlayFeedbacks();
        print("PLAY GAME MODE TUTORIAL");
    }

    public void PlayDeckBackToHome()
    {
        DeckBackToHome.PlayFeedbacks();
        print("PLAY DECK BACK TO HOME TUTORIAL");
    }

    public void PlayPutCardInDeck()
    {
        PutCardInDeckTutorial.PlayFeedbacks();
        print("PLAY PUT CARD IN DECK TUTORIAL");
    }
    public void PlayRemoveCardInDeck()
    {
        RemoveCardInDeckTutorial.PlayFeedbacks();
        print("PLAY REMOVE CARD IN DECK TUTORIAL");
    }

    public void PlaySearchCardInDeck()
    {
        SearchCardInDeck.PlayFeedbacks();
        print("PLAY SEARCH CARD IN DECK TUTORIAL");
    }

    public void PlayCardCollectionInDeck()
    {
        CardCollectionInDeck.PlayFeedbacks();
        print("PLAY CARD COLLECTION IN DECK");
    }

    public void PlayPopupCardInfo()
    {
        PopupCardInfo.PlayFeedbacks();
        print("PLAY POPUP CARD INFO");
    }

    public void PlaySaveDeckInDeck()
    {
        SaveDeckInDeck.PlayFeedbacks();
        print("PLAY SAVE DECK IN DECK");
    }

    public void PlayLimitCardInDeck()
    {
        LimitCardInDeck.PlayFeedbacks();
        print("PLAY LIMIT CARD IN DECK");
    }

    public void PlayCardNameTutorial()
    {
        CardNameInDeck.PlayFeedbacks();
        print("PLAY CARD NAME IN DECK");
    }

    public void PlayCardInDeckTutorial()
    {
        CardInDeck.PlayFeedbacks();
        print("PLAY CARD IN DECK TUTORIAL");
    }

    public void PlayClickingDeckInBagTutorial()
    {
        ClickingDeckInBag.PlayFeedbacks();
        print("PLAY CLIKING DECK IN BAG");
    }

    public void PlayPopupDeckStoreTutorial()
    {
        PopupDeckStoreTutorial.PlayFeedbacks();
        print("PLAY POPUP DECK STORE TUTORIAL");
    }

    public void PlayColectionDeckTutorial()
    {
        CollectionDeckTutorial.PlayFeedbacks();
        print("PLAY COLLECTION DECK TUTORIAL");
    }

    public void PlayDeckStoreTutorial()
    {
        DeckStoreTutorial.PlayFeedbacks();
        print("PLAY DECK STORE TUTORIAL");
    }
    public void PLayPopupPackStoreTutorial()
    {
        PopupPackStoreTutorial.PlayFeedbacks();
        print("PLAY POPUP PACK STORE TUTORIAL");
    }

    public void PlayNavDeckStoreTutorial()
    {
        NavDeckStoreTutorial.PlayFeedbacks();
        print("PLAY NAV DECK STORE TUTORIAL");
    }
    public void PlayNavStoreTutorial()
    {
        NavStoreTutorial.PlayFeedbacks();
        print("PLAY NAV STORE TUTORIAL");
    }

    public void PlayPackStoreTutorial()
    {
        PackStoreTutorial.PlayFeedbacks();
        print("PLAY PACK STORE TUTORIAL");
    }

    public void PlayClickingStoreTutorial()
    {
        ClickingStore.PlayFeedbacks();
        print("PLAY CLICKING BUTTON IN HOME TUTORIAL");
    }

    public void PlaySettingButtonTutorial()
    {
        SettingButtonInHome.PlayFeedbacks();
        print("PLAY SETTING BUTTON IN HOME TUTORIAL");
    }

    public void PlayPlayButtonTutorial()
    {
        PlayButtonInHome.PlayFeedbacks();
        print("PLAY 'PLAY' BUTTON IN HOME TUTORIAL");
    }

    public void PlayDeckButtonTutorial()
    {
        DeckButtonInHome.PlayFeedbacks();
        print("PLAY DECK BUTTON IN HOME TUTORIAL");
    }

    public void PlayBagButtonTutorial()
    {
        BagButtonInHome.PlayFeedbacks();
        print("PLAY BAG BUTTON IN HOME TUTORIAL");
    }

    public void PlayStoreButtonTutorial()
    {
        StoreButtonInHome.PlayFeedbacks();
        print("PLAY STORE BUTTON IN HOME TUTORIAL");
    }

    public void PlayMainNavInHomeTutorial()
    {
        MainNavInHomeTutorial.PlayFeedbacks();
        print("PLAY MAIN NAV IN HOME TUTORIAL");
    }

    public void PlayFriendTutorial()
    {
        FriendTutorial.PlayFeedbacks();
        print("PLAY FRIEND TUTORIAL");
    }

    public void PlayWelcomeTutorial()
    {
        WelcomeTutorial.PlayFeedbacks(); ;
        print("PLAY WELCOME FEEDBACK");
    }


    public void PlayProfileTutorial()
    {
        ProfileTutorial.PlayFeedbacks();
        print("PLAY PROFILE TUTORIAL");
    }
    public void PlayCoinTutorial()
    {
        CoinTutorial.PlayFeedbacks();
        print("PLAY COIN FEEDBACK");
    }
    #endregion
}
