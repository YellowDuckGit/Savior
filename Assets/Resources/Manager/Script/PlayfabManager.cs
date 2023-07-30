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

        if(instance != null && instance != this)
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
                if(!UIManager.instance.LoginMessage.transform.parent.gameObject.activeSelf)
                    UIManager.instance.LoginMessage.transform.parent.gameObject.SetActive(true);
                UIManager.instance.LoginMessage.text = "";
                UIManager.instance.LoginMessage.text += "<align=center><b><size=200%>Login Failed</b><align=left><size=100%>\n";

                if(error.ErrorDetails != null)
                {
                    if(error.ErrorDetails.ContainsKey("Username"))
                        error.ErrorDetails["Username"].ForEach(a => UIManager.instance.LoginMessage.text += "- " + a + "\n");
                    if(error.ErrorDetails.ContainsKey("Password"))
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

        if(RePasssword.Equals(password))
        {
            PlayFabClientAPI.RegisterPlayFabUser(registerRequest,
                result =>
                {
                    TutorialManager.instance.isNewbie = true;
                    Login(username, password);
                },
                error =>
                {
                    if(!UIManager.instance.RegisterMessage.transform.parent.gameObject.activeSelf)
                        UIManager.instance.RegisterMessage.transform.parent.gameObject.SetActive(true);
                    UIManager.instance.RegisterMessage.text = "";
                    UIManager.instance.RegisterMessage.text += "<align=center><b><size=200%>SignUp Failed</b><align=left><size=100%> \n";

                    if(error.ErrorDetails != null)
                    {
                        if(error.ErrorDetails.ContainsKey("Username"))
                            error.ErrorDetails["Username"].ForEach(a => UIManager.instance.RegisterMessage.text += "- " + a + "\n");
                        if(error.ErrorDetails.ContainsKey("Password"))
                            error.ErrorDetails["Password"].ForEach(a => UIManager.instance.RegisterMessage.text += "- " + a + "\n");
                        if(error.ErrorDetails.ContainsKey("Email"))
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
            if(!UIManager.instance.RegisterMessage.transform.parent.gameObject.activeSelf)
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
                if(!UIManager.instance.RecoverMessage.transform.parent.gameObject.activeSelf)
                    UIManager.instance.RecoverMessage.transform.parent.gameObject.SetActive(true);
                UIManager.instance.RecoverMessage.text = "";
                UIManager.instance.RecoverMessage.text += "<align=center><b><size=200%>Recovery Failed</b><align=left><size=100%> \n";

                if(error.ErrorDetails != null)
                {
                    if(error.ErrorDetails.ContainsKey("Email") && error.ErrorDetails["Email"].Count > 0)
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
        Debug.LogFormat("C30F9 ID = {0}, IsApiExecuting = {1}", ID, IsApiExecuting);
        PlayFabClientAPI.GetUserData(new PlayFab.ClientModels.GetUserDataRequest()
        {
        }, result =>
        {
            Debug.LogFormat("C30F9-01 get the user data successed");
            if(result.Data == null || !result.Data.ContainsKey("DeviceUniqueIdentifier"))
            {
                //true
                Debug.LogFormat("C30F9-01-01 result data is null = {0}, result data not contain key DeviceUniqueIdentifier = {1}", result.Data == null, !result.Data.ContainsKey("DeviceUniqueIdentifier"));
                Debug.LogFormat("C30F9-01-01 AuthencatonSuccess");
                AuthencatonSuccess();
            }
            else
            {
                Debug.LogFormat("C30F9-01-02");
                string data = result.Data["DeviceUniqueIdentifier"].Value;
                Debug.LogFormat("C30F9-01-02 data = {0}", data);

                if(data.Equals("Notyet") || data.Equals(ID))
                {
                    Debug.LogFormat("C30F9-01-02-01 user have been logout from another device or user loged in the same device");
                    AuthencatonSuccess();
                }
                else
                {
                    Debug.LogFormat("C30F9-01-02-02 user have been login from another device");

                    if(!UIManager.instance.LoginMessage.transform.parent.gameObject.activeSelf)
                        UIManager.instance.LoginMessage.transform.parent.gameObject.SetActive(true);

                    UIManager.instance.LoginMessage.text += "This account is logged in on other computer" + "\n";
                    UIManager.instance.EnableLoadingAPI(false);

                    //false
                }
            }

            IsApiExecuting = false;
            Debug.LogFormat("C30F9-01 IsApiExecuting = {0}", IsApiExecuting);
        }, (error) =>
        {
            Debug.Log(error.GenerateErrorReport());
            Debug.LogFormat("C30F9-02 get the user data failed, message = {0}", error.GenerateErrorReport());
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
            if(result.Data == null || !result.Data.ContainsKey(key))
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
        Debug.LogFormat("C30F15 IsApiExecuting = {0}", IsApiExecuting);
        PlayFab.ClientModels.GetUserInventoryRequest request = new PlayFab.ClientModels.GetUserInventoryRequest();

        List<string> listCard = new List<string>();
        PlayFabClientAPI.GetUserInventory(request, result =>
        {
            Debug.LogFormat("C30F15-01 get the user inventory successed, inventory item count = {0}", result.Inventory.Count);
            foreach(var item in result.Inventory)
            {
                Debug.LogFormat("C30F15-01 result.Inventory[{0}]", result.Inventory.IndexOf(item));
                if(item.CatalogVersion == "Card" && item.ItemClass == "Card")
                {
                    Debug.LogFormat("C30F15-01-01 item CatalogVersion = {0}, item ItemClass = {1}", item.CatalogVersion, item.ItemClass);
                    int numberCard = (int)item.RemainingUses;
                    Debug.LogFormat("C30F15-01-01 numberCard = {0}", numberCard);
                    if(numberCard < 4)
                    {
                        Debug.LogFormat("C30F15-01-01-01 numberCard < 4");
                        Debug.LogFormat("C30F15-01-01-01 listCard count before add = {0}", listCard.Count);
                        listCard.Add(item.ItemId + ":" + numberCard);
                        Debug.LogFormat("C30F15-01-01-01 listCard count after add = {0}", listCard.Count);
                    }
                    else
                    {
                        Debug.LogFormat("C30F15-01-01-02 numberCard >= 4");
                        PlayFab.AdminModels.RevokeInventoryItemRequest request2 = new PlayFab.AdminModels.RevokeInventoryItemRequest()
                        {
                            ItemInstanceId = item.ItemInstanceId,
                            PlayFabId = playFabId,
                        };
                        PlayFabAdminAPI.RevokeInventoryItem(request2, result =>
                        {
                            Debug.LogFormat("C30F15-01-01-02-01 revokeInventoryItem successed");
                            GrantItem("Card", item.ItemId, 3);
                            int numberOutlimit = numberCard - 3;
                            Debug.LogFormat("C30F15-01-01-02-01 numberOutlimit = {0}", numberOutlimit);

                            CardItem a = GameData.instance.listCardItem.SingleOrDefault(a => a.cardData.Id.Equals(item.ItemId));
                            string itemID = "";
                            if(a != null)
                            {
                                Debug.LogFormat("C30F15-01-01-02-01-01 value of a is not null = {0}", a != null);
                                switch(a.cardData.RarityCard)
                                {
                                    case Rarity.Normal:
                                        Debug.LogFormat("C30F15-01-01-02-01-01-01 rarity = {0}", a.cardData.RarityCard);
                                        itemID = "NormalRefund";
                                        break;
                                    case Rarity.Elite:
                                        Debug.LogFormat("C30F15-01-01-02-01-01-02 rarity = {0}", a.cardData.RarityCard);
                                        itemID = "EliteRefund";
                                        break;
                                    case Rarity.Epic:
                                        Debug.LogFormat("C30F15-01-01-02-01-01-03 rarity = {0}", a.cardData.RarityCard);
                                        itemID = "EpicRefund";
                                        break;
                                    case Rarity.Legendary:
                                        Debug.LogFormat("C30F15-01-01-02-01-01-04 rarity = {0}", a.cardData.RarityCard);
                                        itemID = "LegenRefund";
                                        break;
                                }
                                Debug.LogFormat("C30F15-01-01-02-01-01 itemID = {0}", itemID);
                                GrantItem("Refund", itemID, numberOutlimit);
                            }
                            else
                            {
                                Debug.LogFormat("C30F15-01-01-02-01-02 value of a is not null = {0}", a != null);
                                Debug.LogError("refund error");
                            }
                        }, (error) =>
                        {
                            Debug.LogFormat("C30F15-01-01-02-02 revokeInventoryItem failed, message = {0}", error.GenerateErrorReport());
                        });


                    }
                    print("CARD INFO: " + item.ItemId + ": " + numberCard);
                }
                else
                {
                    Debug.LogFormat("C30F15-01-02 not catalog card and not card class");
                }
            }
            Debug.LogFormat("C30F15-01 listCard of GameData count before update= {0}", GameData.instance.listCard.Count);
            GameData.instance.listCard = listCard;
            Debug.LogFormat("C30F15-01 listCard of GameData count after update= {0}", GameData.instance.listCard.Count);
            IsApiExecuting = false;
            Debug.LogFormat("C30F15-01 IsApiExecuting = {0}", IsApiExecuting);
        }, (error) =>
        {
            Debug.LogFormat("C30F15-02 get the user inventory failed, message = {0}", error.GenerateErrorReport());
        });

        yield return new WaitUntil(() => !IsApiExecuting);
        Debug.LogFormat("C30F15 End of GetCards");
    }

    public void GrantItem(string catalog, string itemID, int amount)
    {
        List<PlayFab.AdminModels.ItemGrant> itemGrants = new List<PlayFab.AdminModels.ItemGrant>();
        var item1 = new PlayFab.AdminModels.ItemGrant();
        item1.ItemId = itemID;
        item1.PlayFabId = playFabId;

        for(int i = 0; i < amount; i++)
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
        }, result =>
        {
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
                foreach(PlayFab.ClientModels.StatisticValue statisticValue in listStatisticValue)
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

        Debug.LogFormat("C30F7 win = {0}, eloPlayer = {1}, eloOpponent ={2}", win, eloPlayer, eloOpponent);


        if(win)
        {
            Debug.LogFormat("C30F7-01 win = {0}", win);
            W = 1;
            Debug.LogFormat("C30F7-01 W = {0}", W);

        }
        else
        {
            Debug.LogFormat("C30F7-02 win = {0}", win);
            W = 0;
            Debug.LogFormat("C30F7-02 W = {0}", W);
        }

        We = (float)(1 / (1 + Math.Pow(10, (R0 - Rd) / C)));
        int Rn = (int)(R0 + K * (W - We) + B * (N - 1));

        Debug.LogFormat("C30F7 W = {0}, We = {1}, Rn = {2}, R0 = {3}, Rd = {4}, C = {5}, K = {6}, B = {7}, N = {8}", W, We, Rn, R0, Rd, C, K, B, N);



        //Incase Player have below 0 elo
        if(Rn < 0)
        {
            Debug.LogFormat("C30F7-03 before change Rn = {0}", Rn);
            Rn = 0;
            Debug.LogFormat("C30F7-03 after change Rn = {0}", Rn);
        }
        Debug.LogFormat("C30F7 EloResult = {0}", MatchManager.instance.EloResult);
        MatchManager.instance.EloResult = Rn - eloPlayer;
        Debug.LogFormat("C30F7 EloResult = {0}", MatchManager.instance.EloResult);

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

    public IEnumerator GetStores(string cataLog, string storeId)
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
            foreach(var item in catalogItem)
            {

                if(item.ItemClass == "Card")
                {
                    cards.Add(item.ItemId, item.VirtualCurrencyPrices["MC"].ToString());
                }
                else if(item.ItemClass == "Bundle")
                {

                    var currency = item.VirtualCurrencyPrices["MC"];
                    var pack = new Data_Pack(item.ItemId, item.DisplayName, currency + "");
                    //bundles.Add(new Data_Pack(item.ItemId));
                    if(item.Bundle != null)
                    {
                        print($"Bundle name: {item.DisplayName} with price {item.VirtualCurrencyPrices}");
                        //sell pack (random card) -> droptables
                        if(item.Bundle.BundledResultTables != null)
                        {
                            foreach(var x in item.Bundle.BundledResultTables)
                            {
                                pack.dropTableId.Add(x);
                                var dropTable = result.Catalog.FirstOrDefault(item => item.ItemId == x);
                                //catalogItem.ForEach((x) => print($"catalogItem({catalogItem.IndexOf(x)}): {x.DisplayName ?? "null"}, id: {x.ItemId ?? "null"}, {x.Container.ItemContents.Count}"));
                                print("bundle result table data: " + x); //droptable in bundle
                            }
                        }
                        // sell card item
                        foreach(var x in item.Bundle.BundledItems)
                        {
                            pack.cardItemsId.Add(x);
                            print($"bundle({item.DisplayName}) add item data id : " + x); //card in droptable
                        }
                    }
                    packs.Add(pack);
                }
                else if(item.ItemClass == "Deck")
                {
                    var currency = item.VirtualCurrencyPrices["MC"];
                    var deck = new Data_Deck(item.ItemId, item.DisplayName, currency.ToString());
                    if(item.Bundle != null)
                    {
                        print($"Bundle DECK name: {item.DisplayName}");
                        foreach(var x in item.Bundle.BundledItems)
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
        bool IsApiExecuting = true;
        Debug.LogFormat("C30F10 is IsApiExecuting = {0}", IsApiExecuting);
        var request2 = new PayForPurchaseRequest()
        {
            OrderId = orderID,
            Currency = currency,
            ProviderName = providerName
        };
        PlayFabClientAPI.PayForPurchase(request2, result =>
        {
            Debug.LogFormat("C30F10-01 status = {0}", result.Status);
            IsApiExecuting = false;
            Debug.LogFormat("C30F10-01 IsApiExecuting = {0}", IsApiExecuting);

        }, (error) =>
        {
            Debug.LogFormat("C30F10-02 PayForPurchase error, message = {0}", error.GenerateErrorReport());
        });

        yield return new WaitUntil(() => !IsApiExecuting);
        Debug.LogFormat("C30F10 is IsApiExecuting = {0}", IsApiExecuting);
        yield return StartCoroutine(FinishPurchase(orderID, itemPurchases));
    }

    private IEnumerator FinishPurchase(string orderID, List<ItemPurchaseRequest> itemPurchases)
    {
        bool IsApiExecuting = true;
        Debug.LogFormat("C30F14 orderID = {0}, itemPurchases count = {1}, IsApiExecuteing = {2}", orderID, itemPurchases.Count, IsApiExecuting);
        var request3 = new ConfirmPurchaseRequest() { OrderId = orderID };
        string deckName = "";
        Dictionary<string, int> dic = new Dictionary<string, int>();
        PlayFabClientAPI.ConfirmPurchase(request3, result =>
        {
            Debug.LogFormat("C30F14-01 Confirm Purchase success, number item purchased = {0}", result.Items.Count);

            //get pack item id
            if(UIManager.instance.isStorePacks || UIManager.instance.isOpenPack)
            {
                Debug.LogFormat("C30F14-01-01 player in store screen or in open pack screen");
                //set list item open
                UIManager.instance.FeedBackOpenPack.PlayFeedbacks();
            }
            else
            {
                Debug.LogFormat("C30F14-01-02 player in another screen do not play feedback");
            }

            if(UIManager.instance.isStoreDecks)
            {
                Debug.LogFormat("C30F14-01-03 player in store deck screen");
                var deckData = GameData.instance.listDeckDataInStore.Find(i => i.id == itemPurchases[0].ItemId);
                Debug.LogFormat("C30F14-01-03 deckItems count = {0}", deckData.deckItemsId.Count);
                foreach(var key in deckData.deckItemsId)
                {
                    Debug.LogFormat("C30F14-01-03 deckData.deckItemsId[{0}]", deckData.deckItemsId.IndexOf(key));
                    if(dic.ContainsKey(key))
                    {
                        dic[key]++;
                        Debug.LogFormat("C30F14-01-03-01 dic[{0}] = {1}", key, dic[key]);
                    }
                    else
                    {
                        dic.Add(key, 1);
                        Debug.LogFormat("C30F14-01-03-02 add new key dic[{0}] = {1}", key, dic[key]);
                    }
                }
            }
            else
            {
                Debug.LogFormat("C30F14-01-04 not in store deck screen");
            }

            IsApiExecuting = false;
            Debug.LogFormat("C30F14-01 IsApiExecuting = {0}", IsApiExecuting);
        }, (error) =>
        {
            Debug.LogFormat("C30F14-02 Finish Purchase error, message = {0}", error.GenerateErrorReport());
        });

        yield return new WaitUntil(() => !IsApiExecuting);
        Debug.LogFormat("C30F14 wait done");
        yield return new WaitForSeconds(2f);
        Debug.LogFormat("C30F14 wait done");

        if(UIManager.instance.isStoreDecks)
        {
            Debug.LogFormat("C30F14-03 player in store deck");
            if(itemPurchases.Count == 1)
            {
                deckName = UIManager.instance.DeckName;
                Debug.LogFormat("C30F14-03-01 just one item need to purchases, deck name = {0}", deckName);
            }
            yield return StartCoroutine(CollectionManager.instance.CreateDeckFromStore(dic, deckName));
        }
        else
        {
            Debug.LogFormat("C30F14-03-02 player in another screen");
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
        Debug.LogFormat("C30F6 numberContentReturn = {0}", numberContentReturn);
        foreach(ItemPurchaseRequest itemPurchase in itemPurchases)
        {
            Debug.LogFormat("C30F6 itemPurchases[{0}]", itemPurchases.IndexOf(itemPurchase));
            var request = new PurchaseItemRequest() { StoreId = "BS1", ItemId = itemPurchase.ItemId, VirtualCurrency = "MC", Price = 3000 };
            PlayFabClientAPI.PurchaseItem(request, result =>
            {
                List<string> array = result.Items[0].BundleContents;
                Debug.LogFormat("C30F6-01 purchase have been finished");
                array.ForEach(a => print(a));
                GameData.instance.listCardOpenedInPack.Add(array);
                numberContentReturn--;
                print("GameData.instance.listCardOpenedInPack: " + GameData.instance.listCardOpenedInPack.Count);
                StartCoroutine(UIManager.instance.LoadVirtualMoney());

            }, (error) =>
            {
                Debug.Log(error.GenerateErrorReport());
                Debug.LogFormat("C30F6-02 purchase process error message = {0}", error.GenerateErrorReport());
            });
        }

        yield return new WaitUntil(() => (numberContentReturn == 0));

        UIManager.instance.FeedBackOpenPack.PlayFeedbacks();
    }

    #endregion

    public IEnumerator GetDropTable(List<string> listTableId)
    {

        foreach(string item in listTableId.Distinct().ToList())
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
            foreach(var table in tables)
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
        Debug.LogFormat("C30F13 id = {0}, IsApiExecuting = {1}", id, IsApiExecuting);
        var request = new EvaluateRandomResultTableRequest
        {
            TableId = id
        };

        PlayFabServerAPI.EvaluateRandomResultTable(request, result =>
        {
            Debug.LogFormat("C30F13-01 result.ResultItemId = {0}", result.ResultItemId);
            print(result.ResultItemId);
            Debug.LogFormat("C30F13-01 IsApiExecuting = {0}", IsApiExecuting);
            IsApiExecuting = false;
            Debug.LogFormat("C30F13-01 IsApiExecuting = {0}", IsApiExecuting);
        }, (error) =>
        {
            Debug.LogFormat("C30F13-02 error while retrieving user data, message = {0}", error.GenerateErrorReport());
        });
        yield return new WaitUntil(() => !IsApiExecuting);
        Debug.LogFormat("C30F13 End EvaluateRandomResultTable");
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
        if(isAuthented)
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
        Debug.Log("C30F1");
        var request = new PlayFab.ClientModels.AddFriendRequest();
        Debug.LogFormat("C30F1 idType = {0}", idType);
        switch(idType)
        {
            case FriendIdType.PlayFabId:
                Debug.LogFormat("C30F1-01 friendId = {0}", friendId);
                request.FriendPlayFabId = friendId;
                break;
            case FriendIdType.Username:
                Debug.LogFormat("C30F1-02 friendId = {0}", friendId);
                request.FriendUsername = friendId;
                break;
            case FriendIdType.DisplayName:
                Debug.LogFormat("C30F1-03 friendId = {0}", friendId);
                request.FriendTitleDisplayName = friendId;
                break;
            default:
                Debug.LogFormat("C30F1-04 Do not find type of idType");
                break;
        }
        Debug.Log("C30F1");
        // Execute request and update friends when we are done
        PlayFabClientAPI.AddFriend(request, result =>
        {
            Debug.Log("C30F1-05 Friend added successfully");
            UIManager.instance.AddFriendMessage.gameObject.transform.parent.gameObject.SetActive(false);
            UIManager.instance.AddFriendMessage.gameObject.transform.parent.gameObject.transform.parent.gameObject.SetActive(false);
            StartCoroutine(GameData.instance.LoadFriendItem());
            ChatManager.instance.SendDirectMessage(friendId, nameof(MessageType.AddFriend) + "|");
        }, (error) =>
        {
            Debug.LogFormat("C30F1-06 add Friend fail error message = {0}", error.ErrorMessage);
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

    //h�m delete friends
    //public void DeleteFriend()
    //{
    //    StartCoroutine(RemoveFriends());
    //}

    void DisplayPlayFabError(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());
    }
    void DisplayError(string error)
    {
        Debug.LogError(error);
    }


    void AuthencatonSuccess()
    {
        Debug.Log("C30F3");
        string userName = "";

        if(UIManager.instance.isSignIn)
        {
            Debug.LogFormat("C30F3-01 isSignIn = {0}", UIManager.instance.isSignIn);
            userName = UIManager.instance.LoginUsername.text;
            Debug.LogFormat("C30F3-01 userName = {0}", userName);
        }
        else if(UIManager.instance.isSignUp)
        {
            Debug.LogFormat("C30F3-02 isSignUp = {0}", UIManager.instance.isSignUp);
            userName = UIManager.instance.RegisterUsername.text;
            Debug.LogFormat("C30F3-02 userName = {0}", userName);
        }
        else
        {
            Debug.LogFormat("C30F3-03 Continue");
        }

        PlayerPrefs.SetString("USERNAME", userName);

        StartCoroutine(SetUserData("DeviceUniqueIdentifier", DeviceUniqueIdentifier));
        //connect

        //connect to chat
        ChatManager.instance.ConnectoToPhotonChat();
        PhotonManager.instance.ConnectToMaster();
        Debug.LogFormat("C30F3 isAuthented = {0}", isAuthented);
        isAuthented = true;
        Debug.LogFormat("C30F3 isAuthented = {0}", isAuthented);
    }

    #endregion

    //IEnumerator waitToTurnOff()
    //{
    //    print("Cancel Quit");
    //    yield return 
    //    print("Quit");

    //}

}
