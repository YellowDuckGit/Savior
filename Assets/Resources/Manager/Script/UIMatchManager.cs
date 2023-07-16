using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMatchManager : MonoBehaviour
{
    public static UIMatchManager instance;

    [SerializeField] MatchManager matchManager;
    /// <summary>
    /// Parent GameObject is GameObject which store list children GameObject
    /// Variable bellow use for case you need load list gameobject into parent GameObject
    /// EX: Load list Card into CollectionCard
    /// </summary>
    [Space(5)]
    [SerializeField] GameObject PanelResult;
    [SerializeField] GameObject PanelLoading;
    [SerializeField] GameObject PanelSaviorLogicOption;



    [Space(10)]

    /// <summary>
    /// Variables bellow will be updated one or more times in game
    /// </summary>
    [Header("Loading Data")]
    [Space(5)]
    //Create Card Scene
    [SerializeField] TextMeshProUGUI T_Turn;
    [SerializeField] TextMeshProUGUI T_RightAttack;
    [SerializeField] TextMeshProUGUI T_ACT_SkipTurn;
    [SerializeField] TextMeshProUGUI T_ResultMatch;



    [Space(10)]

    [Header("Button Event")]
    [Space(5)]
    [SerializeField] Button ACT_SkipTurn;

    private void Awake()
    {
        //DontDestroyOnLoad(gameObject);
        if (instance != null && instance != this)
        {
            Debug.LogError("UIManager have 2");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {

        if (SceneManager.GetActiveScene().name.Equals("Home"))
        {

            #region Register Event Load Data
            #endregion

            #region Add Button Event

            #endregion
        }
    }


   

    #region LoadDataFunction
    #endregion

    #region Get Set
    

    public string Turn
    {
        get { return T_Turn.text; }
        set { this.T_Turn.text = value; }
    }

    public string RightAttack
    {
        get { return T_RightAttack.text; }
        set { this.T_RightAttack.text = value; }
    }

    public string TextButton_ACT_SkipTurn
    {
        get { return T_ACT_SkipTurn.text; }
        set { this.T_ACT_SkipTurn.text = value; }
    }

    string ResultMatch
    {
        get { return T_ResultMatch.text; }
        set { this.T_ResultMatch.text = value; }
    }

    public Button GetACT_SkipTurn
    {
        get { return this.ACT_SkipTurn; }
    }


    #endregion

    #region Function
    public IEnumerator flipUI()
    {
        Vector3 rotationUIParameter = new Vector3(0f, 180f, 0f);
        List<Transform> listFlipUI = new List<Transform>(); 
        if(MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Blue))
        {
            RectTransform rectTransformHp = matchManager.redPlayer.hp.gameObject.GetComponent<RectTransform>();
            RectTransform rectTransformMana = matchManager.redPlayer.mana.gameObject.GetComponent<RectTransform>();
            rectTransformMana.Rotate(rotationUIParameter);
            rectTransformHp.Rotate(rotationUIParameter);

        } else if (MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Red))
        {
            RectTransform rectTransformHp = matchManager.bluePlayer.hp.gameObject.GetComponent<RectTransform>();
            RectTransform rectTransformMana = matchManager.bluePlayer.mana.gameObject.GetComponent<RectTransform>();
            rectTransformMana.Rotate(rotationUIParameter);
            rectTransformHp.Rotate(rotationUIParameter);

            listFlipUI.AddRange(matchManager.bluePlayer.summonZones.Select(a => a.transform.parent.transform));
            listFlipUI.AddRange(matchManager.bluePlayer.fightZones.Select(a => a.transform.parent.transform));
            listFlipUI.AddRange(matchManager.redPlayer.summonZones.Select(a => a.transform.parent.transform));
            listFlipUI.AddRange(matchManager.redPlayer.fightZones.Select(a => a.transform.parent.transform));
        }

        foreach (Transform t in listFlipUI)
        {
            print("TRANSFORM");
            Vector3 rotationToAdd = new Vector3(0,-180f, 0f);
            t.localRotation *= Quaternion.Euler(rotationToAdd);
        }
        yield return null;
    }

    public IEnumerator setResultMatch(bool win, bool isRank, float showTime)
    {
        PanelResult.SetActive(true);
        if (win)
        {
            if(isRank)
                ResultMatch = "Your Win\n+250 Coin\n+10 Elo";
            else
            ResultMatch = "Your Win"; 
        }
        else
        {
            if(isRank)
                ResultMatch = "Your Lose";
            else
            ResultMatch = "Your Lose";
        }

        yield return new WaitForSeconds(showTime);
    }

    public void TurnLoadingScene(bool turn)
    {
        PanelLoading.SetActive(turn);
    }
    #endregion

}
