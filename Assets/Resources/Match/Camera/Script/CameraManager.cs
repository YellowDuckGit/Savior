using Cinemachine;
using MoreMountains.Feedbacks;
using MoreMountains.FeedbacksForThirdParty;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    public enum ChanelCamera
    {
        Normal = 0, Board = 1, Hand = 2, MP_HP = 3, SkipTurn = 4, Card = 5
    }

    public int presentChannel = 0;
    public int lastChannel = 0;

    // Start is called before the first frame update
    public MMFeedbacks CameraFeedBacks;

    public CinemachineVirtualCamera CardCamera;

    //[SerializeField] Button B_Normal;
    //[SerializeField] Button B_Board;
    //[SerializeField] Button B_Hand;
    //[SerializeField] Button B_MP_HP;
    //[SerializeField] Button B_SkipTurn;
    //[SerializeField] Button B_Card;

    private void Start()
    {
        //B_Normal.onClick.AddListener(() => SwitchCamera(ChanelCamera.Normal));
        //B_Board.onClick.AddListener(() => SwitchCamera(ChanelCamera.Board));
        //B_Hand.onClick.AddListener(() => SwitchCamera(ChanelCamera.Hand));
        //B_MP_HP.onClick.AddListener(() => SwitchCamera(ChanelCamera.MP_HP));
        //B_SkipTurn.onClick.AddListener(() => SwitchCamera(ChanelCamera.SkipTurn));
        //B_Card.onClick.AddListener(() => SwitchCamera(ChanelCamera.Card));

    }

    private void Awake()
    {
        //DontDestroyOnLoad(gameObject);

        if (instance != null && instance != this)
        {
            Debug.LogError("CameraManger have 2");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public void SwitchCamera(ChanelCamera chanel, Transform Card = null)
    {
        lastChannel = presentChannel;
        presentChannel = (int)chanel;

        print("lastChannel: " + lastChannel);
        print("presentChannel: " + presentChannel);

        switch (chanel)
        {
            case ChanelCamera.Normal:
                CameraFeedBacks.Feedbacks[(int)ChanelCamera.Normal].Play(Vector3.zero,1f);
                break;
            case ChanelCamera.Board:
                CameraFeedBacks.Feedbacks[(int)ChanelCamera.Board].Play(Vector3.zero, 1f);
                break;
            case ChanelCamera.Hand:
                CameraFeedBacks.Feedbacks[(int)ChanelCamera.Hand].Play(Vector3.zero, 1f);
                break;
            case ChanelCamera.MP_HP:
                CameraFeedBacks.Feedbacks[(int)ChanelCamera.MP_HP].Play(Vector3.zero, 1f);
                break;
            case ChanelCamera.SkipTurn:
                CameraFeedBacks.Feedbacks[(int)ChanelCamera.SkipTurn].Play(Vector3.zero, 1f);
                break;
            case ChanelCamera.Card:
                CardCamera.Follow = Card;
                CameraFeedBacks.Feedbacks[(int)ChanelCamera.Card].Play(Vector3.zero, 1f);
                
                break;
        }


        if (lastChannel == (int)presentChannel) //click again to object 
        {
            print("ClickAgian");
            switch (chanel)
            {
                case ChanelCamera.Normal:

                    break;
                case ChanelCamera.Board:
                case ChanelCamera.Hand:
                case ChanelCamera.MP_HP:
                case ChanelCamera.SkipTurn:
                    SwitchCamera(ChanelCamera.Normal,null);
                    break;
                case ChanelCamera.Card:
                    SwitchCamera(ChanelCamera.Board, null);
                    break;
            }
        }
    }

    public void OnclickSwitchCameraNormal()
    {
        SwitchCamera(ChanelCamera.Normal,null);
    }
    public void OnclickSwitchCameraBoard()
    {
        SwitchCamera(ChanelCamera.Board, null);
    }
    public void OnclickSwitchCameraHand()
    {
        SwitchCamera(ChanelCamera.Hand, null);
    }
    public void OnclickSwitchCameraMP_HP()
    {
        SwitchCamera(ChanelCamera.MP_HP, null);
    }
    public void OnclickSwitchCameraSkipTurn()
    {
        SwitchCamera(ChanelCamera.SkipTurn, null);
    }

    public void SwitchCameraBefore()
    {
        SwitchCamera((ChanelCamera)lastChannel, null);
    }


}
