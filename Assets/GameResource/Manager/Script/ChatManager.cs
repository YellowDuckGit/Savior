using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Chat.Demo;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.PostProcessing.HistogramMonitor;


public enum MessageType
{
    RequestPlay, AcceptRequest, DeclineRequest, RoomPVFCreated, JoinedRoom, AddFriend, DeleteFriend
}

public enum PlayerStatus
{
    offline, invisible, online, away, dnd, lfs, playing
}

public class ChatManager  : MonoBehaviour, IChatClientListener
    {
    public static ChatManager instance;

    [SerializeField] public string nickName = "";
    [SerializeField] public string nickNameFriendinvite = "";

    public ChatClient chatClient;

    string FriendRoomName;

    #region Unity Methods

    void Start()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("ChatManager have 2");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void Awake()
    {
     
    }
    private void OnDestroy()
    {
        //UIFriend.OnInviteFriend -= HandleFriendInvite;
    }


    private void Update()
    {
        if (chatClient != null)
        chatClient.Service();
    }

    #endregion

    #region  Private Methods

    public void ConnectoToPhotonChat()
    {
        nickName = PlayerPrefs.GetString("USERNAME");

        chatClient = new ChatClient(this);
        Debug.Log("Connecting to Photon Chat");
        chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, PhotonNetwork.AppVersion,
        new Photon.Chat.AuthenticationValues(nickName));
    }

        #endregion

        #region  Public Methods

        public void HandleFriendInvite(string recipient)
        {
            if (!PhotonNetwork.InRoom) return;
            chatClient.SendPrivateMessage(recipient, PhotonNetwork.CurrentRoom.Name);
        }

        public void SendDirectMessage(string recipient, string message)
        {
            print("recipient: " + recipient + "  message: " + message);
            chatClient.SendPrivateMessage(recipient,message);
        }

    #endregion

    #region Photon Chat Callbacks

    public void DebugReturn(DebugLevel level, string message)
    {
        Debug.Log($"Photon Chat DebugReturn: {message}");
    }

    public void OnDisconnected()
        {
            Debug.Log("You have disconnected from the Photon Chat");
            chatClient.SetOnlineStatus(ChatUserStatus.Offline);
        }

        public void OnConnected()
        {
            Debug.Log("You have connected to the Photon Chat");
            //OnChatConnected?.Invoke(chatClient);
            chatClient.SetOnlineStatus(ChatUserStatus.Online);
            //SendDirectMessage("vanphu02", "Hi Bri");
        }

    public void OnChatStateChange(ChatState state)
    {
        Debug.Log($"Photon Chat OnChatStateChange: {state.ToString()}");
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        Debug.Log($"Photon Chat OnGetMessages {channelName}");
        for (int i = 0; i < senders.Length; i++)
        {
            Debug.Log($"{senders[i]} messaged: {messages[i]}");
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        if (!string.IsNullOrEmpty(message.ToString()))
        {
            // Channel Name format [Sender : Recipient]
            string[] splitNames = channelName.Split(new char[] { ':' });
            string senderName = splitNames[0];

            if (!sender.Equals(senderName, StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"{sender}: {message}");
                //OnRoomInvite?.Invoke(sender, message.ToString());
                if (message.ToString().Contains("|"))
                {
                    string[] decodeMessage = message.ToString().Split('|');
                    if(decodeMessage.Length > 0)
                    {
                        string typeMessage = decodeMessage[0].Trim();
                        string content = decodeMessage[1].Trim(); //nane rooom
                        switch (typeMessage)
                        {
                            case nameof(MessageType.RequestPlay):
                                print("RequestPlay");
                                if (nickNameFriendinvite.Equals(""))
                                {
                                    nickNameFriendinvite = sender;
                                    //popup
                                    UIManager.instance.RequestPanelContainer.SetActive(true);
                                }else
                                {
                                    Debug.LogError("Friend On Request Invite: "+nickNameFriendinvite);
                                    
                                }
                                break;

                            case nameof(MessageType.AcceptRequest):
                                print("AcceptRequest");
                                nickNameFriendinvite = sender;
                                //PhotonNetwork.JoinLobby(FindMatchSystem.instance.sqlLobby_N);
                                FriendRoomName = FindMatchSystem.instance.CreatePlayWithFriendRoom();
                                break;
                            case nameof(MessageType.DeclineRequest):
                                nickNameFriendinvite = null;
                                print("DeclineRequest");
                                break;
                            case nameof(MessageType.RoomPVFCreated):
                                print(content);
                                PhotonNetwork.JoinRoom(content);
                                print("RoomPVFCreated");
                                break;
                            case nameof(MessageType.JoinedRoom):
                                UIManager.instance.TurnOnChooseDeckPVFScene();
                                print("RoomPVFCreated");
                                break;
                        ////////////////////////////////////////////////////////
                            case nameof(MessageType.AddFriend):
                                PlayfabManager.instance.AddFriend(PlayfabManager.FriendIdType.Username, content);
                                print("AddFriend");
                                break;
                            case nameof(MessageType.DeleteFriend):
                                StartCoroutine(PlayfabManager.instance.RemoveFriend(content));
                                print("Delete");
                                break;
                        }
                    }
                }
            }
        }
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        Debug.Log($"Photon Chat OnSubscribed");
        for (int i = 0; i < channels.Length; i++)
        {
            Debug.Log($"{channels[i]}");
        }
    }

    public void OnUnsubscribed(string[] channels)
    {
        Debug.Log($"Photon Chat OnUnsubscribed");
        for (int i = 0; i < channels.Length; i++)
        {
            Debug.Log($"{channels[i]}");
        }
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        Debug.Log($"Photon Chat OnStatusUpdate: {user} changed to {status}: {message}");
        //PhotonStatus newStatus = new PhotonStatus(user, status, (string)message);
        Debug.Log($"Status Update for {user} and its now {status}.");

        foreach(FriendItem a in GameData.instance.listFriendItem)
        {
            print(a.userName.text);
        }
        FriendItem friendItem =  GameData.instance.listFriendItem.SingleOrDefault(f => f.userName.text.Equals(user));
        if (friendItem != null)
        {
            Debug.Log("Find");

            switch (status)
            {
                case 0: //offline 
                    friendItem.Status = 0;
                    break;
                case 1: //invisible : Be invisible to everyone
                    friendItem.Status = 1;
                    break;
                case 2: //online 
                    friendItem.Status = 2;
                    break;
                case 3: //away: Online but not available
                    friendItem.Status = 3;
                    break;
                case 4: //DND: Do not disturb.
                    friendItem.Status = 4;
                    break;
                case 5: //LFS:  Looking For Game/Group. Could be used when you want to be invited or do matchmaking. More...
                    friendItem.Status = 5;
                    break;
                case 6: //Playing:
                    friendItem.Status = 6;
                    break;
            }
        }
        else
        {
            Debug.Log("Can't Find");
        }
    
        //OnStatusUpdated?.Invoke(newStatus);
    }

    public void OnUserSubscribed(string channel, string user)
    {
        Debug.Log($"Photon Chat OnUserSubscribed: {channel} {user}");
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        Debug.Log($"Photon Chat OnUserUnsubscribed: {channel} {user}");
    }
    #endregion
}
