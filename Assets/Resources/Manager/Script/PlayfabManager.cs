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
using Photon.Pun.Demo.PunBasics;
using Assets.GameComponent.UI.CreateDeck.UI.Script;

public class PlayfabManager : MonoBehaviour
{
    public static PlayfabManager instance;

    public bool isAuthented = false;
    private string playFabId;
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
        StartCoroutine(SetUserData("DeviceUniqueIdentifier", "Notyet"));

        UIManager.instance.EnableLoadingAPI(true);
        PlayFabClientAPI.ForgetAllCredentials();
        PhotonNetwork.Disconnect();
        ChatManager.instance.chatClient.Disconnect();
        StartCoroutine(WaitingLogout());
    }

    public IEnumerator WaitingLogout()
    {
        yield return new WaitUntil(() => !PlayFabClientAPI.IsClientLoggedIn() && !PhotonManager.instance.isAuthented && !ChatManager.instance.isAuthented);
        UIManager.instance.EnableLoadingAPI(false);
        UIManager.instance.TurnOnSignInScene();
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
                playFabId = result.PlayFabId;
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
                    TutorialManager.instance.isNewbie = true;
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
            UIManager.instance.EnableLoadingAPI(false);

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
        print("GetCards");
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
                    if (numberCard < 4)
                    {
                        listCard.Add(item.ItemId + ":" + numberCard);
                    }
                    else
                    {
                        PlayFab.AdminModels.RevokeInventoryItemRequest request2 = new PlayFab.AdminModels.RevokeInventoryItemRequest()
                        {
                            ItemInstanceId = item.ItemInstanceId,
                            PlayFabId = playFabId,
                        };
                        PlayFabAdminAPI.RevokeInventoryItem(request2, result =>
                        {
                            GrantItem("Card", item.ItemId, 3);
                            int numberOutlimit = numberCard - 3;
                            print("Number Out Limit: "+numberOutlimit);

                            CardItem a = GameData.instance.listCardItem.SingleOrDefault(a => a.cardData.Id.Equals(item.ItemId));
                            string itemID = "";
                            if (a != null)
                            {
                                switch (a.cardData.RarityCard)
                                {
                                    case Rarity.Normal:
                                        itemID = "NormalRefund";
                                        break;
                                    case Rarity.Elite:
                                        itemID = "EliteRefund";
                                        break;
                                    case Rarity.Epic:
                                        itemID = "EpicRefund";
                                        break;
                                    case Rarity.Legendary:
                                        itemID = "LegenRefund";
                                        break;
                                }

                                GrantItem("Refund", itemID, numberOutlimit);
                            }
                            else
                            {
                                Console.Error.WriteLine("ERROR");
                            }


                            //



                        }, (error) =>
                        {
                            Debug.Log("Got error retrieving user data:");
                            Debug.Log(error.GenerateErrorReport());
                        });


                    }
                    print("CARD INFO: " + item.ItemId + ": " + numberCard);
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

    public void GrantItem(string catalog, string itemID, int amount)
    {
        List<PlayFab.AdminModels.ItemGrant> itemGrants = new List<PlayFab.AdminModels.ItemGrant>();
        var item1 = new PlayFab.AdminModels.ItemGrant();
        item1.ItemId = itemID;
        item1.PlayFabId = playFabId;

        for (int i = 0; i < amount; i++)
        {
            print("Add");
            itemGrants.Add(item1);
        }

        PlayFab.AdminModels.GrantItemsToUserRequest request3 = new PlayFab.AdminModels.GrantItemsToUserRequest()
        {
            CatalogVersion = catalog,
            ItemGrants = itemGrants
        };

        PlayFabAdminAPI.GrantItemsToUsers(request3, result =>
        {
            print("GRANT ITEM TO USER: " + result.ItemGrantResults.Count);
            StartCoroutine(UIManager.instance.LoadVirtualMoney());
        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });
    }

    #region Elo

    public IEnumerator SubmitScore(int playerScore)
    {
        print("SubmitScore");
        bool IsApiExecuting = true;
        PlayFabClientAPI.UpdatePlayerStatistics(new PlayFab.ClientModels.UpdatePlayerStatisticsRequest
        {
            Statistics = new List<PlayFab.ClientModels.StatisticUpdate> {
            new PlayFab.ClientModels.StatisticUpdate {
                StatisticName = "Rank",
                Value = playerScore
            }
    }
        }, result => {
            GameData.instance.Elo = playerScore;
            IsApiExecuting = false;
        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });
        yield return new WaitUntil(() => !IsApiExecuting);
        yield return StartCoroutine(UIManager.instance.LoadElo());
    }



    public IEnumerator GetScore()
    {
        bool IsApiExecuting = true;

        //PlayFabClientAPI.GetPlayerStatistics(new PlayFab.ClientModels.GetPlayerStatisticsRequest
        //{
        //    Statistics = new List<PlayFab.ClientModels.StatisticUpdate> {
        //    new PlayFab.ClientModels.StatisticUpdate {
        //        StatisticName = "Rank",
        //        Value = playerScore
        //    }
        //}
        //}, result => OnStatisticsUpdated(result), FailureCallback);
        print("Elo");
        PlayFab.ClientModels.GetPlayerStatisticsRequest request = new PlayFab.ClientModels.GetPlayerStatisticsRequest() { };
        request.StatisticNames = new List<string>() { "Rank" };
        PlayFabClientAPI.GetPlayerStatistics(request, result =>
        {
            if(result.Statistics.Count > 0)
            {
                List<PlayFab.ClientModels.StatisticValue> listStatisticValue;
                listStatisticValue = result.Statistics;
                foreach (PlayFab.ClientModels.StatisticValue statisticValue in listStatisticValue)
                {
                    print(statisticValue.StatisticName + " " + statisticValue.Value + " ");
                    GameData.instance.Elo = statisticValue.Value;
                }
            }
            else
            {
                StartCoroutine(SubmitScore(0));
            }

            IsApiExecuting = false;
        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });
        yield return new WaitUntil(() => !IsApiExecuting);
    }

    string[] rank = { "Topaz", "Spinel", "Aquamarine", "Emerald", "Ruby", "Sapphire", "Diamond" };
    void CalRank(int elo, int partRank, int eloToUpLevelRank)
    {
        int rankIndex = elo / (eloToUpLevelRank * partRank);
        int levelIndex = (elo % eloToUpLevelRank) / (eloToUpLevelRank / partRank) + 1;

        Debug.Log(rank[rankIndex]);
        Debug.Log("Level " + levelIndex);
    }

    public IEnumerator CalElo(bool win, int eloPlayer, int eloOpponent)
    {
        int W;
        float We;
        float R0 = eloPlayer;
        float Rd = eloOpponent;
        float C = 400; //hang so
        float K = 50; //hang so
        float B = 1.5f; ///hang so 
        float N = 0; //Chuoi thang
        if (win) W = 1;
        else W = 0;

        We = (float)(1 / (1 + Math.Pow(10,(R0 - Rd) / C)));
        int Rn = (int)(R0 + K * (W - We) + B*(N - 1));

        print("We: "+We);
        print("Rn :"+Rn);   

        //Incase Player have below 0 elo
        if(Rn < 0)
        {
            Rn = 0;
        }

        MatchManager.instance.EloResult = Rn - eloPlayer;

        yield return StartCoroutine(SubmitScore(Rn));
    }
    #endregion

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

    public IEnumerator GetStores()
    {
        bool IsApiExecuting = true;
        Debug.Log("START GET STORE");
        List<PlayFab.ClientModels.StoreItem> storeItems = new();
        List<Data_Pack> packs = new List<Data_Pack>();
        List<Data_Deck> decks = new List<Data_Deck>();

        Dictionary<string, string> cards = new Dictionary<string, string>();

        PlayFabClientAPI.GetCatalogItems(new PlayFab.ClientModels.GetCatalogItemsRequest() { CatalogVersion = "Card" }, result =>
        {
            var catalogItem = result.Catalog;
            foreach (var item in catalogItem)
            {

                if (item.ItemClass == "Card")
                {
                    cards.Add(item.ItemId, item.VirtualCurrencyPrices["MC"].ToString());
                }
                else if (item.ItemClass == "Bundle")
                {

                    var currency = item.VirtualCurrencyPrices["MC"];
                    var pack = new Data_Pack(item.ItemId, item.DisplayName, currency + "");
                    //bundles.Add(new Data_Pack(item.ItemId));
                    if (item.Bundle != null)
                    {
                        print($"Bundle name: {item.DisplayName} with price {item.VirtualCurrencyPrices}");
                        //sell pack (random card) -> droptables
                        if (item.Bundle.BundledResultTables != null)
                        {
                            foreach (var x in item.Bundle.BundledResultTables)
                            {
                                pack.dropTableId.Add(x);
                                var dropTable = result.Catalog.FirstOrDefault(item => item.ItemId == x);
                                //catalogItem.ForEach((x) => print($"catalogItem({catalogItem.IndexOf(x)}): {x.DisplayName ?? "null"}, id: {x.ItemId ?? "null"}, {x.Container.ItemContents.Count}"));
                                print("bundle result table data: " + x); //droptable in bundle
                            }
                        }
                    }
                    packs.Add(pack);
                }
                else if (item.ItemClass == "Deck")
                {
                    var currency = item.VirtualCurrencyPrices["MC"];
                    var deck = new Data_Deck(item.ItemId, item.DisplayName, currency.ToString());
                    if (item.Bundle != null)
                    {
                        print($"Bundle DECK name: {item.DisplayName}");
                        foreach (var x in item.Bundle.BundledItems)
                        {
                            print($"({item.DisplayName}) deck add item data id : " + x); //card in droptable
                            deck.deckItemsId.Add(x);
                        }
                    }
                    decks.Add(deck);

                }
                // Check if the item is a bundle
            }
            print("number List<Data_Pack>:" + packs.Count + "\nnumber List<Data_Deck>: " + decks.Count);
            GameData.instance.listPackData = packs;
            GameData.instance.listDeckDataInStore = decks;
            GameData.instance.listCardPrice = cards;
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
            Debug.Log("PURCHASE ID: " + orderID);
            IsApiExecuting = false;
        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });
        yield return new WaitUntil(() => !IsApiExecuting);
        yield return StartCoroutine(DefinePayment(orderID, currency, providerName, itemPurchases));

    }

    private IEnumerator DefinePayment(string orderID, string currency, string providerName, List<ItemPurchaseRequest> itemPurchases)
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

        yield return StartCoroutine(FinishPurchase(orderID, itemPurchases));

    }

    private IEnumerator FinishPurchase(string orderID, List<ItemPurchaseRequest> itemPurchases)
    {
        print("start finish");
        bool IsApiExecuting = true;
        var request3 = new ConfirmPurchaseRequest() { OrderId = orderID };
        string deckName = "";
        Dictionary<string, int> dic = new Dictionary<string, int>();
        PlayFabClientAPI.ConfirmPurchase(request3, result =>
        {
            result.Items.ForEach(x => Debug.Log("item name:  " + x.ItemId + ": " + x.RemainingUses));
            print(result.Items.Count);

            //get pack item id
            if (UIManager.instance.isStoreDecks)
            {
                var deckData = GameData.instance.listDeckDataInStore.Find(i => i.id == itemPurchases[0].ItemId);
                foreach (var key in deckData.deckItemsId)
                {
                    if (dic.ContainsKey(key))
                    {
                        dic[key]++;
                    }
                    else
                    {
                        dic.Add(key, 1);
                    }
                }
            }

           
            IsApiExecuting = false;
        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });

        yield return new WaitUntil(() => !IsApiExecuting);
        yield return new WaitForSeconds(2f);
        if (UIManager.instance.isStoreDecks)
        {
            if (itemPurchases.Count == 1)
            {
                deckName = UIManager.instance.DeckName;
                print($"672 {deckName}");
            }
            yield return StartCoroutine(CollectionManager.instance.CreateDeckFromStore(dic, deckName));
        }

        yield return StartCoroutine(GameData.instance.LoadCardInInventoryUser());
        yield return StartCoroutine(UIManager.instance.LoadVirtualMoney());
    }
   
    public IEnumerator BuyItems(string catalog, string storeId, List<ItemPurchaseRequest> itemPurchases, string currency)
    {
        print("start buy item");
        yield return StartPurchases(catalog, storeId, itemPurchases, currency);
        yield return null;
    }

    #region BuyPack
    public IEnumerator BuyPack(List<ItemPurchaseRequest> itemPurchases)
    {
        int numberContentReturn = itemPurchases.Count;
        foreach (ItemPurchaseRequest itemPurchase in itemPurchases)
        {
            var request = new PurchaseItemRequest() { StoreId = "BS1", ItemId = itemPurchase.ItemId, VirtualCurrency = "MC", Price = 3000 };
            PlayFabClientAPI.PurchaseItem(request, result =>
            {
                List<string> array = result.Items[0].BundleContents;
                array.ForEach(a => print(a));
                GameData.instance.listCardOpenedInPack.Add(array);
                numberContentReturn--;
                print("GameData.instance.listCardOpenedInPack: " + GameData.instance.listCardOpenedInPack.Count);
                StartCoroutine(UIManager.instance.LoadVirtualMoney());

            }, (error) =>
            {
                Debug.Log("Got error retrieving user data:");
                Debug.Log(error.GenerateErrorReport());
            });
        }

        yield return new WaitUntil(() => (numberContentReturn == 0));

        UIManager.instance.FeedBackOpenPack.PlayFeedbacks();
    }

    #endregion

    public IEnumerator GetDropTable(List<string> listTableId)
    {

        foreach (string item in listTableId.Distinct().ToList())
        {
            print(item);
        }
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
                print($"Table name: {table.Key}");
                print($"Table nodes: {table.Value.Nodes.Count}"); // node = item
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


    public IEnumerator EvaluateRandomResultTable(string id)
    {
        bool IsApiExecuting = true;
        var request = new EvaluateRandomResultTableRequest
        {
            TableId = id
        };

        PlayFabServerAPI.EvaluateRandomResultTable(request, result =>
        {
            print(result.ResultItemId);

            IsApiExecuting = false;

        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });
        print("End EvaluateRandomResultTable()");
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
        }, result =>
        {
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
        PlayFabClientAPI.AddFriend(request, result =>
        {
            Debug.Log("Friend added successfully!");
            UIManager.instance.AddFriendMessage.gameObject.transform.parent.gameObject.SetActive(false);
            UIManager.instance.AddFriendMessage.gameObject.transform.parent.gameObject.transform.parent.gameObject.SetActive(false);
            StartCoroutine(GameData.instance.LoadFriendItem());

            ChatManager.instance.SendDirectMessage(friendId, nameof(MessageType.AddFriend) + "|");


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

        if (friendInfo != null)
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

    //h�m delete friends
    //public void DeleteFriend()
    //{
    //    StartCoroutine(RemoveFriends());
    //}

    void DisplayPlayFabError(PlayFabError error) { Debug.Log(error.GenerateErrorReport()); }
    void DisplayError(string error) { Debug.LogError(error); }


    void AuthencatonSuccess()
    {
        string userName = "";

        if (UIManager.instance.isSignIn)
            userName = UIManager.instance.LoginUsername.text;
        else if (UIManager.instance.isSignUp)
        {
            userName = UIManager.instance.RegisterUsername.text;
        }

        PlayerPrefs.SetString("USERNAME", userName);

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
