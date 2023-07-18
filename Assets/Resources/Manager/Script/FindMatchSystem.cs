using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static K_Player;
using static K_Room;

public enum GameMode
{
    Normal = 1, Rank = 2, Tutorial = 3, PlayWithFriend = 4
}
public class FindMatchSystem : MonoBehaviourPunCallbacks
{
    public static FindMatchSystem instance;

    public string gameVersion;

    //List<PlayerFindMatch> listPlayerFindMatch = new List<PlayerFindMatch>();
    List<RoomInfo> roomRankedInfos = new List<RoomInfo>();
    List<RoomInfo> roomNormalInfos = new List<RoomInfo>();

    public TypedLobby sqlLobby_N = new TypedLobby("Normal", LobbyType.SqlLobby);
    public TypedLobby sqlLobby_R = new TypedLobby("Rank", LobbyType.SqlLobby);

    //state properties
    private bool ConfirmStateRed = false;
    private bool ConfirmStateBlue = false;

    //gamemode
    public GameMode gameMode;

    [Header("Matching Setting")]
    //public int limitTimeout;
    public int eloRangesuitable;


    ExitGames.Client.Photon.Hashtable _myPlayerCustomProperties = new ExitGames.Client.Photon.Hashtable();
    ExitGames.Client.Photon.Hashtable _myRoomCustomProperties = new ExitGames.Client.Photon.Hashtable();

    private Coroutine coroutine;

    private void Start()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("ChatManager have 2");
        }
        else
        {
            instance = this;
        }


        if (PhotonNetwork.IsConnected)
        {
            print("ISCONNECT");
        }

        if (PhotonNetwork.InLobby)
        {
            print("InLobby");
        }

        //PhotonNetwork.JoinLobby();

        UIManager.instance.Button_NormalMode.onClick.AddListener(() => OnClickNormalMode());
        UIManager.instance.Button_RankedMode.onClick.AddListener(() => OnClickRankedMode());
        UIManager.instance.Button_FindMatch.onClick.AddListener(() => OnClickFindMatch());
        UIManager.instance.Button_AcceptMatch.onClick.AddListener(() => OnClickAcceptMatch());
        //UIManager.instance.Button_DelineMatch.onClick.AddListener(() => OnClickDeclineMatch());
        UIManager.instance.Button_StopFind.onClick.AddListener(() => OnClickStopFindMatch());

        //select button automatic

        //EventSystem.current.SetSelectedGameObject(null);
        //EventSystem.current.SetSelectedGameObject(UIManager.instance.Button_NormalMode.gameObject);
        //OnClickNormalMode();
    }

    public void ConnectToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;

        PhotonNetwork.ConnectUsingSettings();
    }

    //run after PhotonNetwork.ConnectUsingSettings();
    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToServer");
        if (UIManager.instance.isWatingMatch)
        {
            UIManager.instance.TurnOnBackScene();
        }
    }


    #region Button Function
    //GameData.instance.selectDeck.Data.deckCode
    //button find call this function
    //Ranked match start when 2 player have elo in suiable range
    void OnClickRankedMode()
    {
        gameMode = GameMode.Rank;
    }

    //button find call this function
    void OnClickNormalMode()
    {
        gameMode = GameMode.Normal;
    }

    void OnClickTutorialMode()
    {
        gameMode = GameMode.Tutorial;
    }

    //button find call this function
    void OnClickPlayWithFriendMode()
    {
        gameMode = GameMode.PlayWithFriend;
    }


    void OnClickFindMatch()
    {

        if (GameData.instance.selectDeck != null)
        {
            print($"OnConnectedToMaster>> {(gameMode == GameMode.Normal ? "Normal" : "Rank")}");
            PhotonNetwork.JoinLobby(gameMode == GameMode.Normal ? sqlLobby_N : sqlLobby_R);
            StartCoroutine(FindRoom());
        }
        else
        {
            ///trigger UI
            //StartCoroutine(UIManager.instance.TurnOutline(UIManager.instance.SeletedDeckOutline, 0.5F, 2));
            //UIManager.instance.UI_FindMatch(true);
        }


    }
    public void OnClickAcceptMatch()
    {
        Debug.Log("\n==========================================");
        Debug.Log("OnClickAcceptMatch()");

        if (PhotonNetwork.IsMasterClient)
        {
            if(_myRoomCustomProperties[K_Player.K_PlayerSide.Blue].Equals(K_Player.K_ConfirmState.Waiting))
            {
                //sysn to current room,because _myroomCustomProperties is local variable
                _myRoomCustomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
                _myRoomCustomProperties[K_Player.K_PlayerSide.Blue] = K_Player.K_ConfirmState.AcceptMatch;
                _myRoomCustomProperties[K_Player.DeckBlue] = GameData.instance.selectDeck.Data.deckCode;

                PhotonNetwork.CurrentRoom.SetCustomProperties(_myRoomCustomProperties);
                PhotonNetwork.LocalPlayer.CustomProperties[K_Player.K_PlayerSide.key] = K_Player.K_PlayerSide.Blue;

                StopCoroutine(coroutine);
                print("Stop Coroutine");
            }
        }
        else
        {
            if(_myRoomCustomProperties[K_Player.K_PlayerSide.Red].Equals(K_Player.K_ConfirmState.Waiting))
            {
                //sysn to current room,because _myroomCustomProperties is local variable
                _myRoomCustomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
                _myRoomCustomProperties[K_Player.K_PlayerSide.Red] = K_Player.K_ConfirmState.AcceptMatch;
                _myRoomCustomProperties[K_Player.DeckRed] = GameData.instance.selectDeck.Data.deckCode;

                PhotonNetwork.CurrentRoom.SetCustomProperties(_myRoomCustomProperties);
                PhotonNetwork.LocalPlayer.CustomProperties[K_Player.K_PlayerSide.key] = K_Player.K_PlayerSide.Red;
            }
        }
    }

    public void OnClickDeclineMatch()
    {
        Debug.Log("\n==========================================");
        Debug.Log("OnClickDeclineMatch()");
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (_myRoomCustomProperties[K_Player.K_PlayerSide.Blue].Equals(K_Player.K_ConfirmState.Waiting))
                {
                    //sysn to current room,because _myroomCustomProperties is local variable
                    _myRoomCustomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
                    _myRoomCustomProperties[K_Player.K_PlayerSide.Blue] = K_Player.K_ConfirmState.DeclineMatch;
                    PhotonNetwork.CurrentRoom.SetCustomProperties(_myRoomCustomProperties);
                    PhotonNetwork.LocalPlayer.CustomProperties[K_Player.K_PlayerSide.key] = K_Player.K_PlayerSide.Blue;
                }
            }
            else
            {
                if (_myRoomCustomProperties[K_Player.K_PlayerSide.Red].Equals(K_Player.K_ConfirmState.Waiting))
                {
                    //sysn to current room,because _myroomCustomProperties is local variable
                    _myRoomCustomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
                    _myRoomCustomProperties[K_Player.K_PlayerSide.Red] = K_Player.K_ConfirmState.DeclineMatch;
                    PhotonNetwork.CurrentRoom.SetCustomProperties(_myRoomCustomProperties);
                    PhotonNetwork.LocalPlayer.CustomProperties[K_Player.K_PlayerSide.key] = K_Player.K_PlayerSide.Red;
                }
            }
        }
    }


    public void OnClickStopFindMatch()
    {
        Debug.Log("\n==========================================");
        //get elo form PlayFab and run this action with parameter Elo
        if (PhotonNetwork.InRoom)
        {
            resetPlayerProperties();
            _myRoomCustomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
            _myRoomCustomProperties[K_Room.K_RoomState.key] = K_Room.K_RoomState.CloseRoom;
            PhotonNetwork.CurrentRoom.SetCustomProperties(_myRoomCustomProperties);
        }

    }
    #endregion

    private IEnumerator JoinedLobby()
    {
        yield return StartCoroutine(UIManager.instance.LoadElo());
        SetDataPlayerFindMatch();
        //UIManager.instance.UI_WaitingOppenent(false);
    }

    private void SetDataPlayerFindMatch()
    {
        Debug.Log("GetDataPlayerFindMatch()");
        _myPlayerCustomProperties[K_Player.Elo] = GameData.instance.Elo;
        _myPlayerCustomProperties[K_Player.K_PlayerSide.key] = K_Player.K_PlayerSide.Unknow;
        PhotonNetwork.LocalPlayer.SetCustomProperties(_myPlayerCustomProperties);
    }

    void CreateRoomMatch(string elo)
    {
        Debug.Log("CreateRoomMatch(function) " + gameMode.ToString());
        int eloInt = Int32.Parse(elo);
        int minElo = eloInt - eloRangesuitable / 2;
        int maxElo = eloInt + eloRangesuitable / 2;

        _myRoomCustomProperties[K_Room.EloRange] = minElo + "|" + maxElo;
        _myRoomCustomProperties[K_RoomState.key] = K_Room.K_RoomState.Waiting;
        _myRoomCustomProperties[K_PlayerSide.Red] = K_ConfirmState.Waiting;
        _myRoomCustomProperties[K_PlayerSide.Blue] = K_ConfirmState.Waiting;
        _myRoomCustomProperties[K_Player.DeckBlue] = "";
        _myRoomCustomProperties[K_Player.DeckRed] = "";
        _myRoomCustomProperties[K_Player.EloRed] = "";
        _myRoomCustomProperties[K_Player.EloBlue] = "";
        _myRoomCustomProperties["GameMode"] = ((int)gameMode).ToString();

        //list properties of room
        string[] listProperties = {
            K_Room.EloRange,
            K_RoomState.key,
            K_PlayerSide.Red,
            K_PlayerSide.Blue,
            K_Player.DeckBlue,
            K_Player.DeckRed,
            K_Player.EloRed,
            K_Player.EloBlue,
            "GameMode"
        };
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = true,
            BroadcastPropsChangeToAll = true,
            CustomRoomPropertiesForLobby = listProperties,
            CustomRoomProperties = _myRoomCustomProperties,
            PlayerTtl = 30, //time player disconnect, can connect again
            EmptyRoomTtl = 1, //time room empty life
        };
        
    }
    #region PVF
    public string CreatePlayWithFriendRoom()
    {

        //string RoomName = CommonFunction.getNewId();
        string RoomName = "Room";

        _myRoomCustomProperties[K_RoomState.key] = K_Room.K_RoomState.Waiting;
        _myRoomCustomProperties[K_PlayerSide.Red] = K_ConfirmState.Waiting;
        _myRoomCustomProperties[K_PlayerSide.Blue] = K_ConfirmState.Waiting;
        _myRoomCustomProperties[K_Player.DeckBlue] = "";
        _myRoomCustomProperties[K_Player.DeckRed] = "";
        _myRoomCustomProperties["GameMode"] = ((int)GameMode.PlayWithFriend).ToString();

        string[] listProperties = {
            K_RoomState.key,
            K_PlayerSide.Red,
            K_PlayerSide.Blue,
            K_Player.DeckBlue,
            K_Player.DeckRed,
            "GameMode"
        };

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = true,
            BroadcastPropsChangeToAll = true,
            CustomRoomPropertiesForLobby = listProperties,
            CustomRoomProperties = _myRoomCustomProperties,
            PlayerTtl = 30, //time player disconnect, can connect again
            EmptyRoomTtl = 1, //time room empty life
        };
        string[] expectedUser = new string[] {ChatManager.instance.nickNameFriendinvite, ChatManager.instance.nickName};

        print("CreateRoom");
        PhotonNetwork.CreateRoom(RoomName, roomOptions, sqlLobby_N, null);

        return RoomName;
    }

    public void Confirm()
    {
        if (PhotonNetwork.IsMasterClient) //blue
        {
            PhotonNetwork.CurrentRoom.CustomProperties[K_Player.DeckBlue] = GameData.instance.selectDeck.Data.deckCode;
            PhotonNetwork.CurrentRoom.CustomProperties[K_PlayerSide.Blue] = K_ConfirmState.AcceptMatch;
        }
        else //red
        {
            PhotonNetwork.CurrentRoom.CustomProperties[K_Player.DeckRed] = GameData.instance.selectDeck.Data.deckCode;
            PhotonNetwork.CurrentRoom.CustomProperties[K_PlayerSide.Red] = K_ConfirmState.AcceptMatch;
        }
    }
    #endregion
    void resetPropertiesRoom()
    {
        string elo = PhotonNetwork.LocalPlayer.CustomProperties[K_Player.Elo].ToString();
        int eloInt = Int32.Parse(elo);
        int minElo = eloInt - eloRangesuitable / 2;
        int maxElo = eloInt + eloRangesuitable / 2;

        _myRoomCustomProperties[K_Room.EloRange] = minElo + "|" + maxElo;
        _myRoomCustomProperties[K_Room.K_RoomState.key] = K_Room.K_RoomState.Waiting;
        _myRoomCustomProperties[K_Player.K_PlayerSide.Red] = K_Player.K_ConfirmState.Waiting;
        _myRoomCustomProperties[K_Player.K_PlayerSide.Blue] = K_Player.K_ConfirmState.Waiting;
        _myRoomCustomProperties[K_Player.DeckBlue] = "";
        _myRoomCustomProperties[K_Player.DeckRed] = "";
        _myRoomCustomProperties[K_Player.EloRed] = "";
        _myRoomCustomProperties[K_Player.EloBlue] = "";
        _myRoomCustomProperties["GameMode"] = "";
        PhotonNetwork.CurrentRoom.SetCustomProperties(_myRoomCustomProperties);
    }

    void resetPlayerProperties()
    {
        _myPlayerCustomProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        _myPlayerCustomProperties[K_Player.K_PlayerSide.key] = K_Player.K_PlayerSide.Unknow;
        PhotonNetwork.LocalPlayer.SetCustomProperties(_myPlayerCustomProperties);
    }

    IEnumerator FindRoom()
    {
        UIManager.instance.CounterTimeWating.ResumeTimer();

        print("Find room");
        print("Elo befor: " + PhotonNetwork.LocalPlayer.CustomProperties[K_Player.Elo]);
        yield return new WaitUntil(() => PhotonNetwork.LocalPlayer.CustomProperties[K_Player.Elo] != null); //wait to get elo score
        yield return new WaitUntil(() => PhotonNetwork.InLobby);//wait to join room
        print($"Joined Lobby :{PhotonNetwork.CurrentLobby.Name}");
        print("Elo after: " + PhotonNetwork.LocalPlayer.CustomProperties[K_Player.Elo]);

        //reset confirm state
        ConfirmStateRed = false;
        ConfirmStateBlue = false;
        //if (PhotonNetwork.LocalPlayer.CustomProperties[K_Player.Elo] != null)
        //{
        string elo = PhotonNetwork.LocalPlayer.CustomProperties[K_Player.Elo].ToString();
        //RoomInfo room = null;

        int eloInt = Int32.Parse(elo);
        int minElo = eloInt - eloRangesuitable / 2;
        int maxElo = eloInt + eloRangesuitable / 2;
        //string stringSqlLobbyFilter = $"{K_Room.EloLobbyFilter} > {minElo} AND {K_Room.EloLobbyFilter} < {maxElo}";
        string sqlEloRange = $"{K_Room.EloLobbyFilter} BETWEEN '{minElo}' AND '{maxElo}'";
        print($"Filter: {sqlEloRange}");
        #region create pro
        _myRoomCustomProperties[K_Room.EloRange] = minElo + "|" + maxElo;
        _myRoomCustomProperties[K_RoomState.key] = K_Room.K_RoomState.Waiting;
        _myRoomCustomProperties[K_PlayerSide.Red] = K_ConfirmState.Waiting;
        _myRoomCustomProperties[K_PlayerSide.Blue] = K_ConfirmState.Waiting;
        _myRoomCustomProperties[K_Player.DeckBlue] = "";
        _myRoomCustomProperties[K_Player.DeckRed] = "";
        _myRoomCustomProperties[K_Player.EloRed] = "";
        _myRoomCustomProperties[K_Player.EloBlue] = "";
        _myRoomCustomProperties["GameMode"] = ((int)gameMode).ToString();
        _myRoomCustomProperties[K_Room.EloLobbyFilter] = elo;
        //list properties of room
        string[] listProperties = {
            K_Room.EloRange,
            K_RoomState.key,
            K_PlayerSide.Red,
            K_PlayerSide.Blue,
            K_Player.DeckBlue,
            K_Player.DeckRed,
            K_Player.EloRed,
            K_Player.EloBlue,
            "GameMode",
            K_Room.EloLobbyFilter
        };
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = true,
            BroadcastPropsChangeToAll = true,
            CustomRoomPropertiesForLobby = listProperties,
            CustomRoomProperties = _myRoomCustomProperties,
            PlayerTtl = 30, //time player disconnect, can connect again
            EmptyRoomTtl = 1, //time room empty life
        };

        print(string.Join(", ", _myRoomCustomProperties.Select(x => x.Key + "=" + x.Value).ToArray()));

        //PhotonNetwork.CreateRoom(CommonFunction.getNewId(), roomOptions, null);
        #endregion
        switch (gameMode)
        {
            case GameMode.Normal:
                PhotonNetwork.JoinRandomOrCreateRoom(null, 0, MatchmakingMode.FillRoom, sqlLobby_N, null, null, roomOptions: roomOptions);
                break;
            case GameMode.Rank:
                PhotonNetwork.JoinRandomOrCreateRoom(null, 0, MatchmakingMode.FillRoom, sqlLobby_R, sqlLobbyFilter: sqlEloRange, null, roomOptions);
                break;
        }
        yield return null;
    }
    IEnumerator PlayerOrtherJoinRoom()
    {
        while (true)
        {
            if (PhotonNetwork.InRoom)
            {
                string RoomGameMode = PhotonNetwork.CurrentRoom.CustomProperties["GameMode"].ToString();
                print("ROOMGAMEMODE: " + RoomGameMode);
                if (RoomGameMode ==  ((int)GameMode.Rank).ToString() || RoomGameMode == ((int)GameMode.Normal).ToString())
                {
                    //sysn to current room,because _myroomCustomProperties is local variable
                    _myRoomCustomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
                    _myRoomCustomProperties[K_Room.K_RoomState.key] = K_Room.K_RoomState.Ready;
                    PhotonNetwork.CurrentRoom.SetCustomProperties(_myRoomCustomProperties);
                    Debug.Log("Player Join Room -> Ready");
                    break;
                }
                else if (RoomGameMode == ((int)GameMode.PlayWithFriend).ToString())
                {
                    Debug.Log("GameMode PlayWithFriend");
                    UIManager.instance.TurnOnChooseDeckPVFScene();
                    ChatManager.instance.SendDirectMessage(ChatManager.instance.nickNameFriendinvite, nameof(MessageType.JoinedRoom) + "|null");
                    break;
                }
                else
                {
                    Debug.LogError("GameMode Undifine");
                    break;
                }

            }
            yield return null;
        }
    }

    public void UIConfirm()
    {
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties[K_Room.K_RoomState.key].ToString() == K_Room.K_RoomState.Waiting)
            {
                Debug.Log("UI Waiting");
            }
            else if (PhotonNetwork.CurrentRoom.CustomProperties[K_Room.K_RoomState.key].ToString() == K_Room.K_RoomState.Ready)
            {
                Debug.Log("UI READY");

                //UIManager.instance.UI_FindMatch(false);
                //UIManager.instance.UI_ConfirmMatchmaking(true);
                //UIManager.instance.UI_WaitingOppenent(false);

                UIManager.instance.WatingAcceptMatch(true);
                coroutine = StartCoroutine(TimeoutWaitingAccept());

            }
            else if (PhotonNetwork.CurrentRoom.CustomProperties[K_Room.K_RoomState.key].ToString() == K_Room.K_RoomState.StartMatch)
            {
                Debug.Log("UI START");
                //UIManager.instance.TurnOnMatchingScene();
                //UIManager.instance.UI_StartMatch(true);
                //UIManager.instance.UI_ConfirmMatchmaking(false);

            }
            else if (PhotonNetwork.CurrentRoom.CustomProperties[K_Room.K_RoomState.key].ToString() == K_Room.K_RoomState.CancelMatch)
            {
                Debug.Log("UI CANCLE");
                UIManager.instance.WatingAcceptMatch(false);
            }
        }
    }

    void ConfirmState()
    {

        if (PhotonNetwork.CurrentRoom.CustomProperties[K_Room.K_RoomState.key].Equals(K_Room.K_RoomState.Ready))
        {
            Debug.Log("Room Ready");

            if (!PhotonNetwork.CurrentRoom.CustomProperties[K_Player.K_PlayerSide.Blue].Equals(K_Player.K_ConfirmState.Waiting)
               && !PhotonNetwork.CurrentRoom.CustomProperties[K_Player.K_PlayerSide.Red].Equals(K_Player.K_ConfirmState.Waiting))
            {

                if (PhotonNetwork.CurrentRoom.CustomProperties[K_Player.K_PlayerSide.Blue].Equals(K_Player.K_ConfirmState.AcceptMatch))
                {
                    //master client
                    ConfirmStateBlue = true;
                }
                else if (PhotonNetwork.CurrentRoom.CustomProperties[K_Player.K_PlayerSide.Blue].Equals(K_Player.K_ConfirmState.DeclineMatch))
                {
                    ConfirmStateBlue = false;
                }

                if (PhotonNetwork.CurrentRoom.CustomProperties[K_Player.K_PlayerSide.Red].Equals(K_Player.K_ConfirmState.AcceptMatch))
                {
                    ConfirmStateRed = true;
                }
                else if (PhotonNetwork.CurrentRoom.CustomProperties[K_Player.K_PlayerSide.Red].Equals(K_Player.K_ConfirmState.DeclineMatch))
                {
                    ConfirmStateRed = false;
                }

                if (!ConfirmStateBlue && ConfirmStateRed)
                {
                    if (!PhotonNetwork.IsMasterClient)
                    {
                        //sysn to current room,because _myroomCustomProperties is local variable
                        _myRoomCustomProperties = PhotonNetwork.CurrentRoom.CustomProperties;

                        _myRoomCustomProperties[K_Room.K_RoomState.key] = K_Room.K_RoomState.CancelMatch;

                        PhotonNetwork.CurrentRoom.SetCustomProperties(_myRoomCustomProperties);
                    }

                }
                else if (ConfirmStateBlue && !ConfirmStateRed)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        //sysn to current room,because _myroomCustomProperties is local variable
                        _myRoomCustomProperties = PhotonNetwork.CurrentRoom.CustomProperties;

                        _myRoomCustomProperties[K_Room.K_RoomState.key] = K_Room.K_RoomState.CancelMatch;

                        PhotonNetwork.CurrentRoom.SetCustomProperties(_myRoomCustomProperties);
                    }

                }
                else if (!ConfirmStateBlue && !ConfirmStateRed)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        //sysn to current room,because _myroomCustomProperties is local variable
                        _myRoomCustomProperties = PhotonNetwork.CurrentRoom.CustomProperties;

                        _myRoomCustomProperties[K_Room.K_RoomState.key] = K_Room.K_RoomState.CancelMatch;

                        PhotonNetwork.CurrentRoom.SetCustomProperties(_myRoomCustomProperties);
                    }

                }
                else if (ConfirmStateBlue && ConfirmStateRed)
                {
                    //sysn to current room,because _myroomCustomProperties is local variable
                    _myRoomCustomProperties = PhotonNetwork.CurrentRoom.CustomProperties;

                    _myRoomCustomProperties[K_Room.K_RoomState.key] = K_Room.K_RoomState.StartMatch;
                    PhotonNetwork.CurrentRoom.SetCustomProperties(_myRoomCustomProperties);
                }

            }
        }
        else if (PhotonNetwork.CurrentRoom.CustomProperties[K_Room.K_RoomState.key].Equals(K_Room.K_RoomState.CancelMatch))
        {

            Debug.Log("Room Cancel");
            if (ConfirmStateBlue && !ConfirmStateRed)
            {
                Debug.Log("Cancel Match Red");
                if (PhotonNetwork.LocalPlayer.CustomProperties[K_PlayerSide.key].ToString().Equals(K_PlayerSide.Red))
                {
                    resetPlayerProperties();
                    PhotonNetwork.LeaveRoom();
                }
                    else if (PhotonNetwork.LocalPlayer.CustomProperties[K_PlayerSide.key].ToString().Equals(K_PlayerSide.Blue))
                    {
                        resetPlayerProperties();
                        resetPropertiesRoom();
                }
            }
            else if (!ConfirmStateBlue && ConfirmStateRed)
            {
                Debug.Log("Cancel Match Blue");
                if (PhotonNetwork.LocalPlayer.CustomProperties[K_PlayerSide.key].ToString().Equals(K_PlayerSide.Blue))
                {
                    resetPlayerProperties();
                    PhotonNetwork.LeaveRoom();
                }
                else if (PhotonNetwork.LocalPlayer.CustomProperties[K_PlayerSide.key].ToString().Equals(K_PlayerSide.Red))
                {
                    resetPlayerProperties();
                    resetPropertiesRoom();
                }
            }
            else if (!ConfirmStateBlue && !ConfirmStateRed)
            {
                Debug.Log("Cancel Match Red, blue");
                if (PhotonNetwork.LocalPlayer.CustomProperties[K_PlayerSide.key].ToString().Equals(K_PlayerSide.Blue))
                {

                    resetPlayerProperties();
                    _myRoomCustomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
                    _myRoomCustomProperties[K_Room.K_RoomState.key] = K_Room.K_RoomState.CloseRoom;
                    PhotonNetwork.CurrentRoom.SetCustomProperties(_myRoomCustomProperties);
                    //PhotonNetwork.LeaveRoom();

                }
                else if (PhotonNetwork.LocalPlayer.CustomProperties[K_PlayerSide.key].ToString().Equals(K_PlayerSide.Red))
                {
                    resetPlayerProperties();
                }
            }
        }
        else if (PhotonNetwork.CurrentRoom.CustomProperties[K_Room.K_RoomState.key].Equals(K_Room.K_RoomState.CloseRoom))
        {
            print("K_Room.K_RoomState.CloseRoom");
            if (PhotonNetwork.CurrentRoom.IsOpen && PhotonNetwork.CurrentRoom.IsVisible)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    Debug.Log("CLose Room");
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    PhotonNetwork.CurrentRoom.IsVisible = false;
                }
                else
                {
                    switch (gameMode)
                    {
                        case GameMode.Normal:
                            roomNormalInfos.Remove(PhotonNetwork.CurrentRoom);
                            break;
                        case GameMode.Rank:
                            roomRankedInfos.Remove(PhotonNetwork.CurrentRoom);
                            break;
                    }

                }
            }
            else
            {
                Debug.Log("Leave");
                PhotonNetwork.LeaveRoom();
            }
        }
        else if (PhotonNetwork.CurrentRoom.CustomProperties[K_Room.K_RoomState.key].Equals(K_Room.K_RoomState.StartMatch))
        {
            if (PhotonNetwork.IsMasterClient)
            {
                print("TwoPLayerReady");
                PhotonNetwork.LoadLevel("MatchScene");
            }
        }
        //Debug.Log("RoomID: " + PhotonNetwork.CurrentRoom.Name + "\n Openn: " + PhotonNetwork.CurrentRoom.IsOpen + "\n Isvisiable: " + PhotonNetwork.CurrentRoom.IsVisible);

        //leave room when room close

    }

    #region Orther
    private bool isEloInRange(string eloRange, string elo)
    {
        List<string> strings = eloRange.Trim().Split('|').ToList();

        if (strings.Count == 2)
        {
            int minElo = Int32.Parse(strings[0]);
            int maxElo = Int32.Parse(strings[1]);

            int eloInt = Int32.Parse(elo);

            Debug.Log(minElo + "-" + maxElo + "VS " + elo);

            return eloInt >= minElo && eloInt <= maxElo;
        }
        else
        {
            Debug.LogError("Split Elo is ERORR");
            return false;
        }
    }

    private RoomInfo NearestEloRangeInListRoom(string elo, List<RoomInfo> roomInfos)
    {
        int eloInt = Int32.Parse(elo);
        string[] array = roomInfos.Select(a => a.CustomProperties[K_Room.EloRange].ToString()).ToArray();
        int[] elos = new int[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            // Note that this is assuming valid input
            // If you want to check then add a try/catch 
            // and another index for the numbers if to continue adding the others (see below)
            elos[i] = Int16.Parse(array[i].Split('|')[0].Trim());
        }

        int indexRoom = CommonFunction.FindIndexClosestNumber(elos, eloInt);
        return roomInfos[indexRoom];
    }

    public IEnumerator TimeoutWaitingAccept()
    {
        while (true)
        {
            float timeWait = UIManager.instance.CountdownTimer.GetCurrentTime();
            if (timeWait <= 0)
            {
                OnClickDeclineMatch();
                break;
            }
            yield return new WaitForSeconds(1f);
        }
        yield return  null;
    }
    #endregion

    #region CallBack

    //Room
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo updatedRoom in roomList)
        {
            string idRoom = updatedRoom.Name;

            if (roomNormalInfos.Count > 0)
            {
                RoomInfo oddRoom = roomNormalInfos.Find(a => a.Name.Equals(idRoom));
                if (oddRoom != null)
                {
                    int index = roomNormalInfos.IndexOf(oddRoom);
                    roomNormalInfos.RemoveAt(index);
                }
            }

            if (roomRankedInfos.Count > 0)
            {
                RoomInfo oddRoom = roomRankedInfos.Find(a => a.Name.Equals(idRoom));
                if (oddRoom != null)
                {
                    int index = roomRankedInfos.IndexOf(oddRoom);
                    roomRankedInfos.RemoveAt(index);
                }
            }

            if (updatedRoom != null && !updatedRoom.RemovedFromList)
            {
                int mode = Int32.Parse(updatedRoom.CustomProperties["GameMode"].ToString());
                switch (mode)
                {
                    case (int)GameMode.Normal:
                        roomNormalInfos.Add(updatedRoom);
                        break;
                    case (int)GameMode.Rank:
                        roomRankedInfos.Add(updatedRoom);
                        break;
                }
            }
        }

        //sort list room
        roomNormalInfos.OrderBy(a => Int32.Parse(a.CustomProperties[K_Room.EloRange].ToString()));
        roomRankedInfos.OrderBy(a => Int32.Parse(a.CustomProperties[K_Room.EloRange].ToString()));
        print("Room Normal: " + roomNormalInfos.Count);
        print("Room Ranked: " + roomRankedInfos.Count);
    }


    public override void OnCreatedRoom()
    {
        Debug.Log($"Lobby {PhotonNetwork.CurrentLobby.Name}, OnCreatedRoom() {PhotonNetwork.CurrentRoom.Name} Number Player({PhotonNetwork.CurrentRoom.PlayerCount})");
        //UIManager.instance.UI_WaitingOppenent(true);
    }

    public override void OnJoinedRoom()
    {
        //UI_roomID.text = PhotonNetwork.CurrentRoom.Name;
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            StartCoroutine(PlayerOrtherJoinRoom());
        Debug.Log($"Lobby {PhotonNetwork.CurrentLobby.Name}, JoinRoom {PhotonNetwork.CurrentRoom.Name} Number Player({PhotonNetwork.CurrentRoom.PlayerCount})");
    }



    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        print(message);
        Debug.Log("OnJoinRoomFailed");
        string elo = PhotonNetwork.LocalPlayer.CustomProperties[K_Player.Elo].ToString();
        CreateRoomMatch(elo);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("JoinRoomFailed");
        string elo = PhotonNetwork.LocalPlayer.CustomProperties[K_Player.Elo].ToString();
        CreateRoomMatch(elo);
    }
    public override void OnLeftRoom()
    {
        //UI_roomID.text = "";
        Debug.Log("LeftRoom");

    }

    public override void OnJoinedLobby()
    {
        Debug.Log("JoinedLobby");
        StartCoroutine(JoinedLobby());

    }

    public override void OnLeftLobby()
    {
        Debug.Log("LeftLobby");
    }
    ///
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        Debug.Log("OnRoomPropertiesUpdate()");

        UIConfirm();
        ConfirmState();
    }


    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        Debug.Log("OnPlayerPropertiesUpdate()");
    }
        

    #endregion
}
public static class K_Room
{
    public static readonly string Key = "ROOM";
    public static readonly string EloRange = "EloRange";
    public static readonly string EloLobbyFilter = "C0";

    public static class K_RoomState
    {
        public static readonly string key = "ROOMSTATE";
        public static readonly string Waiting = "WAITING";
        public static readonly string Ready = "READY";
        public static readonly string StartMatch = "STARTMATCH";
        public static readonly string CancelMatch = "CANCELMATCH";
        public static readonly string CloseRoom = "CLOSEMATCH";

    }
}


public static class K_Player
{
    public static readonly string EloRed = "ELORED";
    public static readonly string EloBlue = "ELOBULE";
    public static readonly string Elo = "ELO";


    public static readonly string DeckRed = "DECKRED";
    public static readonly string DeckBlue = "DECKBLUE";


    public static class K_PlayerSide
    {
        public static readonly string key = "PLAYERSIDE";
        public static readonly string Unknow = "UNKNOW";

        public static readonly string Red = "RED";
        public static readonly string Blue = "BLUE";
    }

    public static class K_ConfirmState
    {
        public static readonly string Key = "CONFIRMSTATE";
        public static readonly string Waiting = "WAITING";
        public static readonly string AcceptMatch = "ACCEPT";
        public static readonly string DeclineMatch = "DECLINE";
    }
}
