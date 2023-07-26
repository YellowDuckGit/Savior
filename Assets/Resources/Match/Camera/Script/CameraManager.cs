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
    public MMF_Player BlueCameraFeedBacks;
    public MMF_Player RedCameraFeedBacks;

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

    public void SwitchCamera(ChanelCamera chanel, CardBase Card = null)
    {

        //SFX: Swtich cam
        lastChannel = presentChannel;
        presentChannel = (int)chanel;
        print("lastChannel: " + lastChannel);
        print("presentChannel: " + presentChannel);

        if (MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Blue))
        {
            List<MMF_Feedback> mMF_Feedbacks = BlueCameraFeedBacks.FeedbacksList;
            print("mMF_Feedbacks Count: "+ mMF_Feedbacks.Count);


            switch (chanel)
            {
                case ChanelCamera.Normal:
                    mMF_Feedbacks[(int)ChanelCamera.Normal].Play(Vector3.zero, 1f);
                    break;
                case ChanelCamera.Board:
                    mMF_Feedbacks[(int)ChanelCamera.Board].Play(Vector3.zero, 1f);
                    break;
                case ChanelCamera.Hand:
                    MMF_CinemachineTransition mMF_CinemachineTransition1 = (MMF_CinemachineTransition)mMF_Feedbacks[(int)ChanelCamera.Hand];
                    CinemachineVirtualCamera virtualCamera1 = mMF_CinemachineTransition1.TargetVirtualCamera;
                    //virtualCamera1.Follow = Card.transform;
                    if(Card != null)
                    {
                        virtualCamera1.LookAt = Card.transform;
                    }

                    mMF_Feedbacks[(int)ChanelCamera.Hand].Play(Vector3.zero, 1f);
                    break;
                case ChanelCamera.MP_HP:
                    mMF_Feedbacks[(int)ChanelCamera.MP_HP].Play(Vector3.zero, 1f);
                    break;
                case ChanelCamera.SkipTurn:
                    mMF_Feedbacks[(int)ChanelCamera.SkipTurn].Play(Vector3.zero, 1f);
                    break;
                case ChanelCamera.Card:
                    MMF_CinemachineTransition mMF_CinemachineTransition = (MMF_CinemachineTransition)mMF_Feedbacks[(int)ChanelCamera.Card];
                    CinemachineVirtualCamera virtualCamera = mMF_CinemachineTransition.TargetVirtualCamera;
                    virtualCamera.Follow = Card.transform;
                    mMF_Feedbacks[(int)ChanelCamera.Card].Play(Vector3.zero, 1f);
                    break;
            }
        }
        else if (MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Red))
        {
            List<MMF_Feedback> mMF_Feedbacks = RedCameraFeedBacks.FeedbacksList;
            print("mMF_Feedbacks Count: " + mMF_Feedbacks.Count);

            switch (chanel)
            {
                case ChanelCamera.Normal:
                    mMF_Feedbacks[(int)ChanelCamera.Normal].Play(Vector3.zero, 1f);
                    break;
                case ChanelCamera.Board:
                    mMF_Feedbacks[(int)ChanelCamera.Board].Play(Vector3.zero, 1f);
                    break;
                case ChanelCamera.Hand:
                    MMF_CinemachineTransition mMF_CinemachineTransition1 = (MMF_CinemachineTransition)mMF_Feedbacks[(int)ChanelCamera.Hand];
                    CinemachineVirtualCamera virtualCamera1 = mMF_CinemachineTransition1.TargetVirtualCamera;
                    //virtualCamera1.Follow = Card.transform;
                    if (Card != null)
                    {
                        virtualCamera1.LookAt = Card.transform;
                    }

                    mMF_Feedbacks[(int)ChanelCamera.Hand].Play(Vector3.zero, 1f);
                    break;
                case ChanelCamera.MP_HP:
                    mMF_Feedbacks[(int)ChanelCamera.MP_HP].Play(Vector3.zero, 1f);
                    break;
                case ChanelCamera.SkipTurn:
                    mMF_Feedbacks[(int)ChanelCamera.SkipTurn].Play(Vector3.zero, 1f);
                    break;
                case ChanelCamera.Card:
                     MMF_CinemachineTransition mMF_CinemachineTransition = (MMF_CinemachineTransition)mMF_Feedbacks[(int)ChanelCamera.Card];
                    CinemachineVirtualCamera virtualCamera = mMF_CinemachineTransition.TargetVirtualCamera;
                    virtualCamera.Follow = Card.transform;
                    mMF_Feedbacks[(int)ChanelCamera.Card].Play(Vector3.zero, 1f);
                    break;
            }
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
                    SwitchCamera(ChanelCamera.Normal, null);
                    break;
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

    public void LookAtBlueHand(CardBase card)
    {
        print("LookAtBlueHand");
        List<MMF_Feedback> mMF_Feedbacks = BlueCameraFeedBacks.FeedbacksList;
        MMF_CinemachineTransition mMF_CinemachineTransition = (MMF_CinemachineTransition)mMF_Feedbacks[(int)ChanelCamera.Card];
        CinemachineVirtualCamera virtualCamera = mMF_CinemachineTransition.TargetVirtualCamera;
        if (virtualCamera != null) print("virtualCamera");
        virtualCamera.LookAt = card.transform;
        mMF_Feedbacks[(int)ChanelCamera.Hand].Play(Vector3.zero, 1f);
    }

    public void LookAtRedHand(CardBase card)
    {
        print("LookAtRedHand");
        List<MMF_Feedback> mMF_Feedbacks = RedCameraFeedBacks.FeedbacksList;
        MMF_CinemachineTransition mMF_CinemachineTransition = (MMF_CinemachineTransition)mMF_Feedbacks[(int)ChanelCamera.Card];
        CinemachineVirtualCamera virtualCamera = mMF_CinemachineTransition.TargetVirtualCamera;
        if (virtualCamera != null) print("virtualCamera");
        virtualCamera.LookAt = card.transform;
        mMF_Feedbacks[(int)ChanelCamera.Hand].Play(Vector3.zero, 1f);
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

    public void SetupCamera(string Player, CardPlayer cardPlayer)
    {
        int startChanelBlue = 0;
        int startChanelRed = 10;


        if (K_Player.K_PlayerSide.Blue.Equals(Player))
        {
            //MMF_Player camera = cardPlayer.gameObject.transform.Find("Camera_Blue").GetComponent<MMF_Player>();
            //CameraManager.instance.BlueCameraFeedBacks = camera;


            print("Camera_Blue");

            int a = startChanelBlue;
            for(int i =0; i< BlueCameraFeedBacks.transform.childCount; i++)
            {
                BlueCameraFeedBacks.transform.GetChild(i).GetComponent<MMCinemachinePriorityListener>().Channel = a;
                a++;
               print(BlueCameraFeedBacks.transform.GetChild(i).gameObject.name);
            }

            AnimationCardManager.instance.CreateAniamtionCamera(ref BlueCameraFeedBacks, startChanelBlue);
        }
        else if (K_Player.K_PlayerSide.Red.Equals(Player))
        {
            //MMF_Player camera = cardPlayer.gameObject.transform.Find("Camera_Red").GetComponent<MMF_Player>();
            //CameraManager.instance.RedCameraFeedBacks = camera;
            print("Camera_Red");

            int a = startChanelRed;
            for (int i = 0; i < RedCameraFeedBacks.transform.childCount; i++)
            {
                RedCameraFeedBacks.transform.GetChild(i).GetComponent<MMCinemachinePriorityListener>().Channel = a;
                a++;
            }
            AnimationCardManager.instance.CreateAniamtionCamera(ref RedCameraFeedBacks, startChanelRed);

        }
    }

}
