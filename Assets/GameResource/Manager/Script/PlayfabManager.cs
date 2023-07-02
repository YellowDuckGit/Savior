using PlayFab.ClientModels;
using PlayFab;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using PlayFab.ServerModels;
using Photon.Pun;
using Unity.VisualScripting;

public class PlayfabManager : MonoBehaviour
{
    public static PlayfabManager instance;

    private bool isAuthented = false; 
    //public Button loginButton;
    //public Button registerButton;
    //public Button Recovery;


    private void Awake()
    {
        Application.wantsToQuit += waitExitApplication;


        DontDestroyOnLoad(gameObject);

        if (instance != null && instance != this)
        {
            Debug.LogError("PlayfabManager have 2");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    void Start()
    {


    }

    #region orther

    #endregion

    #region Account
    public void TurnOffApllication()
    {
        Application.Quit();
    }

    public void Logout()
    {
        PlayFabClientAPI.ForgetAllCredentials();
        PhotonNetwork.Disconnect();
        ChatManager.instance.chatClient.Disconnect();
    }

    public void Login(string username, string password)
    {
        UIManager.instance.EnableLoadingAPI(true);

        Debug.Log("Click LOGIn");
        LoginWithPlayFabRequest loginRequest = new LoginWithPlayFabRequest();
        loginRequest.Username = username;
        loginRequest.Password = password;
        PlayFabClientAPI.LoginWithPlayFab(loginRequest,
            result =>
            {
                StartCoroutine(CheckIfSessionIsStillValid(DeviceUniqueIdentifier));

            },
            error =>
            {
                if (!UIManager.instance.LoginMessage.transform.parent.gameObject.activeSelf)
                    UIManager.instance.LoginMessage.transform.parent.gameObject.SetActive(true);
                UIManager.instance.LoginMessage.text = "";
                UIManager.instance.LoginMessage.text += "<align=center><b><size=200%>Login Failed</b><align=left><size=100%>\n";

                if (error.ErrorDetails != null)
                {
                    if (error.ErrorDetails.ContainsKey("Username"))
                        error.ErrorDetails["Username"].ForEach(a => UIManager.instance.LoginMessage.text += "- " + a + "\n");
                    if (error.ErrorDetails.ContainsKey("Password"))
                        error.ErrorDetails["Password"].ForEach(a => UIManager.instance.LoginMessage.text += "- " + a + "\n");
                }
                else
                {
                    UIManager.instance.LoginMessage.text += "- " + error.ErrorMessage + "\n";
                }
                UIManager.instance.EnableLoadingAPI(false);

            }, null);

    }



    public void Register(string email, string username, string password, string RePasssword)
    {
        UIManager.instance.EnableLoadingAPI(true);
        RegisterPlayFabUserRequest registerRequest = new RegisterPlayFabUserRequest();

        registerRequest.RequireBothUsernameAndEmail = true;

        registerRequest.Email = email;
        registerRequest.Username = username;
        registerRequest.Password = password;

        if (RePasssword.Equals(password))
        {
            PlayFabClientAPI.RegisterPlayFabUser(registerRequest,
                result =>
                {
                    Login(username, password);
                },
                error =>
                {
                    if (!UIManager.instance.RegisterMessage.transform.parent.gameObject.activeSelf)
                        UIManager.instance.RegisterMessage.transform.parent.gameObject.SetActive(true);
                    UIManager.instance.RegisterMessage.text = "";
                    UIManager.instance.RegisterMessage.text += "<align=center><b><size=200%>SignUp Failed</b><align=left><size=100%> \n";

                    if (error.ErrorDetails != null)
                    {
                        if (error.ErrorDetails.ContainsKey("Username"))
                            error.ErrorDetails["Username"].ForEach(a => UIManager.instance.RegisterMessage.text += "- " + a + "\n");
                        if (error.ErrorDetails.ContainsKey("Password"))
                            error.ErrorDetails["Password"].ForEach(a => UIManager.instance.RegisterMessage.text += "- " + a + "\n");
                        if (error.ErrorDetails.ContainsKey("Email"))
                            error.ErrorDetails["Email"].ForEach(a => UIManager.instance.RegisterMessage.text += "- " + a + "\n");
                    }
                    else
                    {
                        UIManager.instance.RegisterMessage.text += "- " + error.ErrorMessage + "\n";
                    }

                    UIManager.instance.EnableLoadingAPI(false);

                });
        }
        else
        {
            if (!UIManager.instance.RegisterMessage.transform.parent.gameObject.activeSelf)
                UIManager.instance.RegisterMessage.transform.parent.gameObject.SetActive(true);
            UIManager.instance.RegisterMessage.text = "";
            UIManager.instance.RegisterMessage.text += "<align=center><b><size=200%>SignUp Failed</b><align=left><size=100%> \n";
            UIManager.instance.RegisterMessage.text += "- " + "Password and re-password do not match" + "\n";

        }

    }

    public void RecoverUser(string email)
    {
        UIManager.instance.EnableLoadingAPI(true);
        //print(email);
        SendAccountRecoveryEmailRequest request = new SendAccountRecoveryEmailRequest
        {

            //Email = "taintce150462@gmail.com",
            Email = email,
            TitleId = "63D1C"

        };

        PlayFabClientAPI.SendAccountRecoveryEmail(request,
            result =>
            {
                UIManager.instance.RecoverMessage.text = "Send mail success";
                UIManager.instance.EnableLoadingAPI(false);

            },
            error =>
            {
                if (!UIManager.instance.RecoverMessage.transform.parent.gameObject.activeSelf)
                    UIManager.instance.RecoverMessage.transform.parent.gameObject.SetActive(true);
                UIManager.instance.RecoverMessage.text = "";
                UIManager.instance.RecoverMessage.text += "<align=center><b><size=200%>Recovery Failed</b><align=left><size=100%> \n";

                if (error.ErrorDetails != null)
                {
                    if (error.ErrorDetails.ContainsKey("Email") && error.ErrorDetails["Email"].Count > 0)
                        error.ErrorDetails["Email"].ForEach(a => UIManager.instance.RecoverMessage.text += "- " + a + "\n");
                }
                else
                {
                    UIManager.instance.RecoverMessage.text += "- " + error.ErrorMessage + "\n";
                }
                UIManager.instance.EnableLoadingAPI(false);
            });
    }

    IEnumerator CheckIfSessionIsStillValid(string ID)
    {
        bool IsApiExecuting = true;
        PlayFabClientAPI.GetUserData(new PlayFab.ClientModels.GetUserDataRequest()
        {
        }, result =>
        {
            Debug.Log("Got user data:");
            if (result.Data == null || !result.Data.ContainsKey("DeviceUniqueIdentifier"))
            {
                //true
                AuthencatonSuccess();
            }
            else
            {
                string data = result.Data["DeviceUniqueIdentifier"].Value;
                if (data.Equals("Notyet") || data.Equals(ID))
                {
                    AuthencatonSuccess();
                }
                else
                {

                    if (!UIManager.instance.LoginMessage.transform.parent.gameObject.activeSelf)
                        UIManager.instance.LoginMessage.transform.parent.gameObject.SetActive(true);

                    UIManager.instance.LoginMessage.text += "This account is logged in on other computer" + "\n";
                    UIManager.instance.EnableLoadingAPI(false);

                    //false
                }
            }

            IsApiExecuting = false;

        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });
        yield return new WaitUntil(() => !IsApiExecuting);

    }
    #endregion

    #region Set-Get Player Data (Title)
    public IEnumerator SetUserData(string key, string value)
    {
        bool IsApiExecuting = true;
        PlayFabClientAPI.UpdateUserData(new PlayFab.ClientModels.UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>() {
            {key, value},
        },
            Permission = PlayFab.ClientModels.UserDataPermission.Public
        },
        result =>
        {
            Debug.Log("Successfully updated user data");
            IsApiExecuting = false;
        },
        error =>
        {
            Debug.Log("Got error setting user data Ancestor to Arthur");
            Debug.Log(error.GenerateErrorReport());
        });

        yield return new WaitUntil(() => !IsApiExecuting);

    }

    public IEnumerator GetUserData(string key, Action<string> action)
    {
        bool IsApiExecuting = true;
        PlayFabClientAPI.GetUserData(new PlayFab.ClientModels.GetUserDataRequest()
        {
        }, result =>
        {
            Debug.Log("Got user data:");
            if (result.Data == null || !result.Data.ContainsKey(key))
            {
                Debug.Log("No " + key);
            }
            else
            {
                string data = result.Data[key].Value;
                Debug.Log(key + ": " + data);
                action(data);
            }

            IsApiExecuting = false;

        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });

        yield return new WaitUntil(() => !IsApiExecuting);
    }
    #endregion

    #region Set-Get Data In Inventory
    public IEnumerator GetCards()
    {
        bool IsApiExecuting = true;

        PlayFab.ClientModels.GetUserInventoryRequest request = new PlayFab.ClientModels.GetUserInventoryRequest();
        List<string> listCard = new List<string>();
        PlayFabClientAPI.GetUserInventory(request, result =>
        {
            foreach (var item in result.Inventory)
            {
                if (item.CatalogVersion == "Card" && item.ItemClass == "Card")
                {
                    int numberCard = (int)item.RemainingUses;
                    listCard.Add(item.ItemId + ":" + numberCard);
                }
            }
            GameData.instance.listCard = listCard;
            IsApiExecuting = false;

        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });

        yield return new WaitUntil(() => !IsApiExecuting);
    }

    public IEnumerator GetElo()
    {
        bool IsApiExecuting = true;
        PlayFab.ClientModels.GetUserInventoryRequest request = new PlayFab.ClientModels.GetUserInventoryRequest() { };
        PlayFabClientAPI.GetUserInventory(request, result =>
        {
            PlayFab.ClientModels.ItemInstance item = result.Inventory.Single(a => a.CatalogVersion == "Reward" && a.ItemClass == "Elo");
            int elo = (int)item.RemainingUses;
            GameData.instance.Elo = elo;
            IsApiExecuting = false;
        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });

        yield return new WaitUntil(() => !IsApiExecuting);
    }

    public IEnumerator GetVirtualCurrency()
    {
        bool IsApiExecuting = true;
        PlayFab.ClientModels.GetUserInventoryRequest request = new PlayFab.ClientModels.GetUserInventoryRequest();
        PlayFabClientAPI.GetUserInventory(request, result =>
        {
            print("INVENTORY MONEY: " + result.VirtualCurrency["MC"]);
            GameData.instance.Coin = result.VirtualCurrency["MC"];
            IsApiExecuting = false;
        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });

        UIManager.instance.EnableLoadingAPI(false);
        yield return new WaitUntil(() => !IsApiExecuting);
    }

    #endregion

    #region Cloud Script

    public void AddItemsForNewPlayer(string playfabId)
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "Hello",
            FunctionParameter = new
            {
            }
        };
        PlayFabClientAPI.ExecuteCloudScript(request, OnExecuteSuccess, OnError);
    }

    void OnExecuteSuccess(PlayFab.ClientModels.ExecuteCloudScriptResult result)
    {
        string str = result.FunctionResult.ToString();
    }

    void OnError(PlayFabError error)
    {
        Debug.LogError(error.Error);
    }

    #endregion

    public IEnumerator GetStores(string cataLog, string storeId)
    {
        bool IsApiExecuting = true;
        Debug.Log("START GET STORE");
        List<PlayFab.ClientModels.StoreItem> storeItems = new();
        List<Data_Pack> packs = new List<Data_Pack>();

        List<(string id, string name)> cards = new List<(string id, string name)>();

        //GetStoreItemsRequest request = new GetStoreItemsRequest()
        // {
        //    CatalogVersion = "Card",
        //    StoreId = "BS1"
        //};
        //test
        PlayFabClientAPI.GetCatalogItems(new PlayFab.ClientModels.GetCatalogItemsRequest() { CatalogVersion = "Card" }, result =>
        {
            var catalogItem = result.Catalog;
            foreach (var item in catalogItem)
            {

                if (item.ItemClass == "Card")
                {
                    cards.Add((item.ItemId, item.DisplayName));
                }
                else if (item.ItemClass == "Bundle")
                {
                    //debug
                    //if (item.Bundle != null)
                    //{
                    //    print(string.Join("=", new string[30]));
                    //    item.Bundle.BundledItems.ForEach((x) => print($"BundledItems({item.Bundle.BundledItems.IndexOf(x)}): {x}"));
                    //    item.Bundle.BundledResultTables.ForEach((x) => print($"BundledResultTables({item.Bundle.BundledResultTables.IndexOf(x)}): {x}"));
                    //    print(string.Join("=", new string[30]));
                    //}

                    var pack = new Data_Pack(item.ItemId);
                    //bundles.Add(new Data_Pack(item.ItemId));
                    if (item.Bundle != null)
                    {
                        print($"Bundle name: {item.DisplayName}");
                        //sell pack (random card) -> droptables
                        if (item.Bundle.BundledResultTables != null)
                        {
                            foreach (var x in item.Bundle.BundledResultTables)
                            {
                                pack.dropTableId.Add(x);
                                var dropTable = result.Catalog.FirstOrDefault(item => item.ItemId == x);
                                //catalogItem.ForEach((x) => print($"catalogItem({catalogItem.IndexOf(x)}): {x.DisplayName ?? "null"}, id: {x.ItemId ?? "null"}, {x.Container.ItemContents.Count}"));
                                print("bundle result table data: " + x);
                            }
                        }
                        // sell card item
                        foreach (var x in item.Bundle.BundledItems)
                        {
                            pack.cardItemsId.Add(x);
                            print($"bundle({item.DisplayName}) add item data id : " + x);
                        }
                    }
                    packs.Add(pack);
                }
                // Check if the item is a bundle
            }
            print("number List<Data_Pack>:" + packs.Count);
            GameData.instance.listPackData = packs;
            IsApiExecuting = false;
        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });

        //PlayFabClientAPI.GetStoreItems(request, result =>
        //{
        //    result.Store.ForEach(i =>
        //    {
        //        pack.Add(new Data_Pack(i.ItemId));
        //    });
        //    GameData.instance.listPackData = pack;
        //    IsApiExecuting = false;
        //}, (error) =>
        //{
        //    Debug.Log("Got error retrieving user data:");
        //    Debug.Log(error.GenerateErrorReport());
        //});

        yield return new WaitUntil(() => !IsApiExecuting);

    }

    private IEnumerator StartPurchases(string catalog, string storeId, List<ItemPurchaseRequest> itemPurchases, string currency)
    {
        UIManager.instance.EnableLoadingAPI(true);
        print("start purchase");
        bool IsApiExecuting = true;
        string orderID = default;
        string providerName = default;
        var request = new StartPurchaseRequest()
        {
            CatalogVersion = catalog,
            StoreId = storeId,
            Items = itemPurchases
        };
        PlayFabClientAPI.StartPurchase(request, result =>
        {
            orderID = result.OrderId;
            providerName = result.PaymentOptions[0].ProviderName;
            Debug.Log("PURCHASE PACK: " + orderID);
            IsApiExecuting = false;
        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });
        yield return new WaitUntil(() => !IsApiExecuting);
        yield return StartCoroutine(DefinePayment(orderID, currency, providerName));

    }

    private IEnumerator DefinePayment(string orderID, string currency, string providerName)
    {
        print("start define");
        bool IsApiExecuting = true;
        var request2 = new PayForPurchaseRequest()
        {
            OrderId = orderID,
            Currency = currency,
            ProviderName = providerName
        };
        PlayFabClientAPI.PayForPurchase(request2, result =>
        {
            Debug.Log("PURCHASE STATUS: " + result.Status);
            IsApiExecuting = false;
        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });

        yield return new WaitUntil(() => !IsApiExecuting);

        yield return StartCoroutine(FinishPurchase(orderID));

    }

    private IEnumerator FinishPurchase(string orderID)
    {
        print("start finish");
        bool IsApiExecuting = true;
        var request3 = new ConfirmPurchaseRequest() { OrderId = orderID };
        PlayFabClientAPI.ConfirmPurchase(request3, result =>
        {
            result.Items.ForEach(x => Debug.Log("item name:  " + x.DisplayName));
            //get pack item id

            StartCoroutine(UIManager.instance.LoadVirtualMoney());
            IsApiExecuting = false;
        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });

        yield return new WaitUntil(() => !IsApiExecuting);
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(GameData.instance.LoadCardInInventoryUser());
    }
    public IEnumerator BuyPacks(string catalog, string storeId, List<ItemPurchaseRequest> itemPurchases, string currency)
    {
        print("start buy pack");
        yield return StartPurchases(catalog, storeId, itemPurchases, currency);
        yield return null;

    }


    public IEnumerator GetDropTable(List<string> listTableId)
    {
        GameData.instance.dropTableInforList.Clear();
        bool IsApiExecuting = true;
        var request = new GetRandomResultTablesRequest
        {
            TableIDs = listTableId.Distinct().ToList()
        };

        // Call the method asynchronously and handle the result or error
        PlayFabServerAPI.GetRandomResultTables(request, result =>
        {
            var tables = result.Tables;
            foreach (var table in tables)
            {
                Console.WriteLine($"Table name: {table.Key}");
                Console.WriteLine($"Table nodes: {table.Value.Nodes.Count}"); // node = item
                var id = table.Key;
                var items = table.Value.Nodes.ToDictionary(item => item.ResultItem, item => item.Weight);

                GameData.instance.dropTableInforList.Add(new DropTableInfor()
                {
                    id = id,
                    items = items
                });
            }
            IsApiExecuting = false;
            print("GameData.instance.dropTableInforList: " + GameData.instance.dropTableInforList.Count);
            print("GetRandomResultTables Finished");
        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });
        print("End GetDropTable()");
        yield return new WaitUntil(() => !IsApiExecuting);
    }

    public string DeviceUniqueIdentifier
    {
        get
        {
            var deviceId = "";
            deviceId = SystemInfo.deviceUniqueIdentifier;
            return deviceId;
        }

    }

    bool waitExitApplication()
    {
        print("wait");
        if (isAuthented)
        {
            StartCoroutine(SetUserData("DeviceUniqueIdentifier", "Notyet"));
            print("SetUserData");
        }
        return true;
    }

    #region Friend
    public IEnumerator GetFriends()
    {
        bool IsApiExecuting = true;
        PlayFabClientAPI.GetFriendsList(new PlayFab.ClientModels.GetFriendsListRequest
        {
            XboxToken = null
        }, result => {
            GameData.instance.listFriendData = result.Friends;
            IsApiExecuting = false;
            /*DisplayFriends(_friends);*/ // triggers your UI
        }, DisplayPlayFabError);
        yield return new WaitUntil(() => !IsApiExecuting);

    }

    public enum FriendIdType { PlayFabId, Username, DisplayName };

    public void AddFriend(FriendIdType idType, string friendId)
    {
        var request = new PlayFab.ClientModels.AddFriendRequest();
        switch (idType)
        {
            case FriendIdType.PlayFabId:
                request.FriendPlayFabId = friendId;
                break;
            case FriendIdType.Username:
                request.FriendUsername = friendId;
                break;
            case FriendIdType.DisplayName:
                request.FriendTitleDisplayName = friendId;
                break;
        }
        // Execute request and update friends when we are done
        PlayFabClientAPI.AddFriend(request, result => {
            Debug.Log("Friend added successfully!");
            UIManager.instance.AddFriendMessage.gameObject.transform.parent.gameObject.SetActive(false);
            UIManager.instance.AddFriendMessage.gameObject.transform.parent.gameObject.transform.parent.gameObject.SetActive(false);
            StartCoroutine(GameData.instance.LoadFriendItem());

        }, (error) =>
        {
            UIManager.instance.AddFriendMessage.gameObject.transform.parent.gameObject.SetActive(true);
            UIManager.instance.AddFriendMessage.text = error.ErrorMessage;
        });
    }

    public IEnumerator RemoveFriend(string Username)
    {
        bool IsApiExecuting = true;
        PlayFab.ClientModels.FriendInfo friendInfo = GameData.instance.listFriendData.Find(friend => friend.Username.Equals(Username));
        
        if(friendInfo != null)
        {
            Debug.Log("remove" + friendInfo.FriendPlayFabId);
            PlayFabClientAPI.RemoveFriend(new PlayFab.ClientModels.RemoveFriendRequest
            {
                FriendPlayFabId = friendInfo.FriendPlayFabId
            }, result =>
            {
                GameData.instance.listFriendData.Remove(friendInfo);
                IsApiExecuting = false;
                Debug.Log("Friend remove successfully!");
            }, DisplayPlayFabError);
        }
        yield return new WaitUntil(() => !IsApiExecuting);
        yield return StartCoroutine(GameData.instance.LoadFriendItem());
    }

   

    //IEnumerator RemoveFriends()
    //{
    //    yield return StartCoroutine(WaitForFriend());
    //    string playFabId = "783226D757A49054";
    //    FriendInfo selectedFriend = _friends.Find(friend => friend.FriendPlayFabId == playFabId);
    //    Debug.Log("remove void" + selectedFriend.Username);
    //    RemoveFriend(selectedFriend);
    //}

    //hàm delete friends
    //public void DeleteFriend()
    //{
    //    StartCoroutine(RemoveFriends());
    //}

    void DisplayPlayFabError(PlayFabError error) { Debug.Log(error.GenerateErrorReport()); }
    void DisplayError(string error) { Debug.LogError(error); }


    void AuthencatonSuccess()
    {
        PlayerPrefs.SetString("USERNAME",UIManager.instance.LoginUsername.text);

        StartCoroutine(SetUserData("DeviceUniqueIdentifier", DeviceUniqueIdentifier));
        //connect

        //connect to chat
        ChatManager.instance.ConnectoToPhotonChat();
        PhotonManager.instance.ConnectToMaster();
        isAuthented = true;
    }
    #endregion

    //IEnumerator waitToTurnOff()
    //{
    //    print("Cancel Quit");
    //    yield return 
    //    print("Quit");

    //}

}
