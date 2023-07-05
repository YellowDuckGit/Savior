using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager instance;
    public string gameVersion;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (instance != null && instance != this)
        {
            Debug.LogError("PhotonManager have 2");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ConnectToMaster()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToServer");
        //SceneManager.LoadScene("Home");

        if (UIManager.instance.isSignIn || UIManager.instance.isSignUp)
        {
            StartCoroutine(GameData.instance.LoadingGameProcess());
        }
        else if (UIManager.instance.isWatingMatch)
        {
            UIManager.instance.TurnOnChooseDeckScene();
        }
    }


    public void HandlerFriendUpdate(List<PlayFab.ClientModels.FriendInfo> friendInfos)
    {
        if (friendInfos.Count > 0)
        {
            string[] friendDisplayName = friendInfos.Select(f=>f.Username).ToArray();
            //PhotonNetwork.FindFriends(friendDisplayName);
            ChatManager.instance.chatClient.AddFriends(friendDisplayName);
        }
    }

    public override void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        print("OnFriendListUpdate");
        base.OnFriendListUpdate(friendList);
        foreach(FriendInfo a in friendList)
        {
            print(a.UserId +"Status: "+a.IsOnline);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        print(cause.ToString());
        UIManager.instance.TurnOnSignInScene();
    }
}
