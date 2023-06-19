using PlayFab.ClientModels;
using PlayFab;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using PlayFab.ServerModels;

public class PlayfabManager : MonoBehaviour
{
    public static PlayfabManager instance;


    //public Button loginButton;
    //public Button registerButton;
    //public Button Recovery;


    private void Awake()
    {
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
    public void Login(string username, string password)
    {


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
                Debug.Log(error.GenerateErrorReport());

                if (error.ErrorDetails.ContainsKey("Username"))
                    error.ErrorDetails["Username"].ForEach(a => print(a));
                if (error.ErrorDetails.ContainsKey("Password"))
                    error.ErrorDetails["Password"].ForEach(a => print(a));


                UIManager.instance.LoginMessage = error.ErrorMessage;
            }, null);

    }



    public void Register(string email, string username, string password)
    {
        RegisterPlayFabUserRequest registerRequest = new RegisterPlayFabUserRequest();

        registerRequest.RequireBothUsernameAndEmail = true;

        registerRequest.Email = email;
        registerRequest.Username = username;
        registerRequest.Password = password;

        Debug.Log(registerRequest.Username);
        Debug.Log(registerRequest.Password);
        Debug.Log(registerRequest.Email);

        PlayFabClientAPI.RegisterPlayFabUser(registerRequest,
            result =>
            {
                UIManager.instance.RegisterMessage = "Register Success";
            },
            error =>
            {
                Debug.Log(error.GenerateErrorReport());
                UIManager.instance.RegisterMessage = error.ErrorMessage;
                Debug.Log(error.GenerateErrorReport());
                if (error.ErrorDetails.ContainsKey("Username"))
                    error.ErrorDetails["Username"].ForEach(a => print(a));
                if (error.ErrorDetails.ContainsKey("Password"))
                    error.ErrorDetails["Password"].ForEach(a => print(a));
                if (error.ErrorDetails.ContainsKey("Email"))
                    error.ErrorDetails["Email"].ForEach(a => print(a));

            });

    }

    public void RecoverUser(string email)
    {

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
                UIManager.instance.RecoverMessage = "Send mail success";
            },
            error =>
            {
                Debug.Log(error.GenerateErrorReport());
                UIManager.instance.RecoverMessage = error.ErrorMessage;
                Debug.Log(error.ErrorDetails);

                if (error.ErrorDetails.ContainsKey("Email") && error.ErrorDetails["Email"].Count > 0)
                    error.ErrorDetails["Email"].ForEach(a => print(a));
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
                StartCoroutine(SetUserData("DeviceUniqueIdentifier", DeviceUniqueIdentifier));

                UIManager.instance.LoginMessage = "Login Success";
                //connect
                PhotonManager.instance.ConnectToMaster();
            }
            else
            {
                string data = result.Data["DeviceUniqueIdentifier"].Value;
                if (data.Equals("Notyet") || data.Equals(ID))
                {
                    //true
                    StartCoroutine(SetUserData("DeviceUniqueIdentifier", DeviceUniqueIdentifier));

                    UIManager.instance.LoginMessage = "Login Success";

                    //connect
                    PhotonManager.instance.ConnectToMaster();
                }
                else
                {
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
            PlayerHomeScene.instance.Elo = elo;
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
            PlayerHomeScene.instance.Coin = result.VirtualCurrency["MC"];
            IsApiExecuting = false;
        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });
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

            //StartCoroutine(UIManager.instance.LoadVirtualMoney());
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


}
