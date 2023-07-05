using Assets.GameComponent.Card.Logic.TargetObject;
using Assets.GameComponent.Card.Logic.TargetObject.Select;
using Photon.Pun;
using Photon.Realtime;
using PlayFab.MultiplayerModels;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Burst.Intrinsics;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
//using static Assets.GameComponent.Card.Logic.TargetObject.SelectTargetObject;

using static MatchManager;

namespace Assets.GameComponent.Manager
{
    public class SelectManager : MonoBehaviourPunCallbacks
    {
        public static SelectManager Instance;
        private void Awake()
        {
            //DontDestroyOnLoad(gameObject);
            if (Instance != null && Instance != this)
            {
                UnityEngine.Debug.LogError("SelectManager have 2");
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }
        private bool _isWatingPlayerSelect = false;
        public bool IsWatingPlayerSelect
        {
            get
            {
                return _isWatingPlayerSelect;
            }
            set
            {
                _isWatingPlayerSelect = value;
            }
        }
        private SelectTargetCard targetPattern;
        public Queue SelectTargetQueue = new Queue();
        private void Start()
        {
            this.RegisterListener(EventID.OnStartTurn, param => Instance.CheckSelectAble(param as MatchManager));
            this.RegisterListener(EventID.EndAttackAndDefensePhase, param => Instance.CheckSelectAble(param as MatchManager));
            this.RegisterListener(EventID.OnCardSelected, param => Instance.SelectCard(param as CardBase));
            print(this.debug("Regis for event EventID.OnStartTurn"));
        }

        private void SelectCard(CardBase cardBase)
        {
            print(this.debug());
            if (IsWatingPlayerSelect)
            {
                if (!SelectTargetQueue.Contains(cardBase))
                {
                    if(isMatchTargetPattern(cardBase))
                    SelectTargetQueue.Enqueue(cardBase);
                    print(this.debug("Select card", new { card = cardBase.ToString() }));
                }

                print(this.debug("SelectTargetQueue count", new
                {
                    SelectTargetQueue.Count
                }));
            }
        }

        private bool isMatchTargetPattern(CardBase cardBase)
        {
            print(this.debug());
            if (targetPattern == null)
            {
                return true;
            }
            else
            {
                return targetPattern.isMatch(cardBase);
            }
        }

        public void CheckSelectAble(MatchManager match)
        {
            ///cap quyen lua chon bai cho nguoi choi hien tai va revoke nguoi choi truoc
            ///step 0: lay du lieu nguoi choi hien tai va nguoi choi truoc
            ///step 1: revoke quyen lua chon cua nguoi choi truoc
            ///step 2: cap quyen cho nguoi choi hien tai

            print(this.debug());

            //get player
            var red = match.redPlayer;
            var blue = match.bluePlayer;

            print(red != null ? "red ok" : "red error");
            print(blue != null ? "blue ok" : "blue error");


            //get the current player in turn
            var currentplayer = match.getCurrenPlayer();
            print(this.debug(currentplayer != null ? "currentplayer ok" : "currentplayer error", new { currentplayer.side }));

            /*
             * playerInturn: nguoi choi hien tai
             * playerPassTurn: nguoi choi truoc
             */
            (var playerInturn, var playerPassTurn) = currentplayer == red ? (red, blue) : (blue, red);

            revokeSelectCard(playerPassTurn);

            /*
             *provide select for current player
             *Card in hand
             **if the current player in defense phase, then revoke all card in hand
             *Card in fight zone
             *Card in summon zone
             */
            provideSelectCard<CardBase>(playerInturn, position: CardPosition.InHand, card => card.Cost <= currentplayer.mana.Number && !(match.gamePhase == GamePhase.Attack) && card.CardPlayer == match.localPlayer);
            provideSelectCard<CardBase>(playerInturn, position: CardPosition.InSummonField, card => card.CardPlayer == match.localPlayer);
        }
        /// <summary>
        /// revoke select for player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="position"></param>
        private void revokeSelectCard(CardPlayer player, CardPosition position = CardPosition.All)
        {
            /*
             *revoke select for pass player
             *Card in hand
             *Card in fight zone
             *Card in summon zone
             */

            void revokeSelectCardOnHand()
            {
                player.hand._cards.ForEach(card =>
                {
                    card.IsSelected = false; //if card selected, remote selected
                    card.IsSelectAble = false;    //revoke selectable in hand
                });
            }

            void revokeSelectCardOnFightZone()
            {
                player.fightZones.ForEach(zonefield =>
                {
                    if (zonefield.monsterCard != null)
                    {
                        var card = zonefield.monsterCard;
                        card.IsSelected = false;  //if card selected, remote selected
                        card.IsSelectAble = false;  //revoke selectable in fight zone
                    }
                });
            }

            void revokeSelectCardOnSummonZone()
            {
                player.summonZones.ForEach(zonefield =>
                {
                    if (zonefield.monsterCard != null)
                    {
                        var card = zonefield.monsterCard;
                        card.IsSelected = false;  //if card selected, remote selected
                        card.IsSelectAble = false;  //revoke selectable in fight zone
                    }
                });
            }

            switch (position)
            {
                case CardPosition.All:
                    revokeSelectCardOnHand();
                    revokeSelectCardOnFightZone();
                    revokeSelectCardOnSummonZone();
                    break;

                case CardPosition.InFightField:
                    revokeSelectCardOnFightZone();
                    break;

                case CardPosition.InSummonField:
                    revokeSelectCardOnSummonZone();
                    break;
                case CardPosition.InHand:
                    revokeSelectCardOnHand();
                    break;
            }
            print(this.debug("Revoke select for player", new { player.side, numberCardInHand = player.hand._cards.Count, }));
        }
#nullable enable

        private int provideSelectCard<T>(CardPlayer player, CardPosition position, Func<T, bool>? filter = null) where T : CardBase
        {
            List<CardBase> listcard = new();
            switch (position)
            {
                //case CardPosition.InDeck:
                //    listcard = player.deck.cards.Where(monsterCard => monsterCard != null).Select(monsterCard => monsterCard as CardBase).ToList();
                //    break;
                case CardPosition.InGraveyard: break;
                case CardPosition.InHand:
                    listcard = player.hand._cards.Where(monsterCard => monsterCard != null).Select(monsterCard => monsterCard as CardBase).ToList();
                    break;
                case CardPosition.InFightField:
                    listcard = player.fightZones.Where(zone => zone.monsterCard != null).Select(zone => zone.monsterCard as CardBase).ToList();
                    break;
                case CardPosition.InSummonField:
                    listcard = player.summonZones.Where(zone => zone.monsterCard != null).Select(zone => zone.monsterCard as CardBase).ToList();
                    break;
                default: print(this.debug("not found card position")); break;
            }

            if (listcard != null && listcard.Count > 0)
            {
                if (filter != null)
                {
                    // Get the first monster card in the list
                    // Invoke the cardTarget method on the first card and get the return value
                    // Do something with the result
                    foreach (var card in listcard)
                    {
                        card.IsSelected = false;
                        card.IsSelectAble = filter.Invoke(arg: (T)card);
                        print(this.debug(string.Format("{0} | {1} [ID:{2}]", card.ToString(), (card.IsSelectAble ? "can  select" : "can not select"), card.photonView.ViewID), new { card.Cost, player.mana.Number, card.IsSelectAble }));
                    }
                }
                else
                {
                    foreach (var card in listcard)
                    {
                        card.IsSelected = false;
                        card.IsSelectAble = true;
                        print(this.debug(string.Format("{0} | {1} [ID:{2}]", card.ToString(), (card.IsSelectAble ? "can  select" : "can not select"), card.photonView.ViewID), new { card.Cost, player.mana.Number, card.IsSelectAble }));
                    }
                }
            }
            return listcard!.Count;
        }


        /// <summary>
        /// Hàm sử lý lựa chọn
        /// </summary>
        /// <param name="number">số lượng</param>
        /// <param name="_what">loại</param>
        /// <param name="_where">vị trí của đối tượng</param>
        /// <returns></returns>
        public IEnumerator Select(SelectTargetCard cardTarget)
        {
            print(this.debug());
            CardPlayer CardOwner = null;
            switch (cardTarget.owner)
            {
                case global::CardOwner.You:
                    CardOwner = MatchManager.instance.localPlayer;
                    break;

                case global::CardOwner.Opponent:
                    CardOwner = MatchManager.instance.localPlayer == MatchManager.instance.redPlayer ? MatchManager.instance.bluePlayer : MatchManager.instance.redPlayer; ;
                    break;
            }
            print(this.debug("Select from Card Owner", new
            {
                side = CardOwner.side
            }));

            if (CardOwner != null)
            {
                print(this.debug("Select from Card Position", new
                {
                    cardTarget.cardPosition
                }));


                var CardCanBeSelect = provideSelectCard<CardBase>(CardOwner, cardTarget.cardPosition, card => card.IsSelected == false);
                print(this.debug("Number Card can be select", new
                {
                    CardCanBeSelect,
                }));

                if (CardCanBeSelect >= 1)
                {
                    yield return StartCoroutine(PlayerSelectCard(cardTarget));
                }
                else
                {
                    print(this.debug("do not enough card in position selected"));
                }
            }
            else
            {
                print(this.debug("Card Owner null"));
            }
            yield return null;
        }

        private IEnumerator PlayerSelectCard(SelectTargetCard cardTarget)
        {
            print(this.debug(cardTarget.ToString()));
            IsWatingPlayerSelect = true;
            int offset = SelectTargetQueue.Count;
            targetPattern = cardTarget;
            yield return new WaitUntil(() => offset < SelectTargetQueue.Count);
            targetPattern = null;
            IsWatingPlayerSelect = false;
            yield return null;
        }

        public IEnumerator SelectTargets(List<AbstractTargetObject.SelectCardItem> selectTargetObjects)
        {
            /*
             * Check enough base condition for select
             * *enough card respond to the condition 
             * * *select how many card in your option
             */
            print(this.debug("Select target condition", new
            {
                count = selectTargetObjects.Count
            }));

            CheckConditionTreeNode root = new CheckConditionTreeNode("Root"); // Tạo nút gốc
            foreach (var selectitem in selectTargetObjects)
            {
                if (selectitem.SelectTargetObject != null) //select option with Type target (Player or card)
                {
                    print(this.debug($"Item target Number {selectTargetObjects.IndexOf(selectitem)}"));
                    if (selectitem.SelectTargetObject is SelectTargetPlayer selectPlayer)
                    {
                        continue;
                    }
                    else if (selectitem.SelectTargetObject is SelectTargetCard selectCard)
                    {
                        print(this.debug($"Item target Card Number {selectTargetObjects.IndexOf(selectitem)}"));

                        print(this.debug(null, new
                        {
                            owner = selectCard.owner.ToString(),
                            cardPosition = selectCard.cardPosition.ToString(),
                            cardType = selectCard.cardType.ToString()
                        }));
                        root.insert(root, string.Format("{0},{1},{2}", selectCard.owner.ToString(), selectCard.cardPosition.ToString(), selectCard.cardType.ToString()));
                        print(this.debug("After insert to Node"));
                    }
                    else
                    {
                        print(this.debug("Not found type SelectTargetObject"));
                    }
                }
            }
            //check condition
            var flag = root.ProvideCardAndCheck(root, "Root", true);
            /*
             * execute condition
             */
            if (flag)
            {
                foreach (var selectitem in selectTargetObjects)
                {
                    if (selectitem.SelectTargetObject != null) //select option with Type target (Player or card)
                    {
                        if (selectitem.SelectTargetObject is SelectTargetPlayer)
                        {
                            continue;
                        }
                        else if (selectitem.SelectTargetObject is SelectTargetCard selectCard)
                        {
                            print(this.debug(null, new
                            {
                                owner = selectCard.owner.ToString(),
                                cardPosition = selectCard.cardPosition.ToString(),
                                cardType = selectCard.cardType.ToString()
                            }));
                            yield return StartCoroutine(SelectManager.Instance.Select(selectCard));
                        }
                    }
                }

            }
            else
            {
                print(this.debug("Can not respone to the condition"));
            }
            yield return null;
        }


        public class CheckConditionTreeNode
        {
            public string ID { get; set; } // Thuộc tính ID để lưu trữ giá trị của nút
            public CheckConditionTreeNode Parent { get; set; } // Thuộc tính Parent để lưu trữ nút cha của nút hiện tại
            public List<CheckConditionTreeNode> Children { get; set; } // Danh sách các nút con của nút hiện tại


            // Hàm khởi tạo với ID cho trước
            public CheckConditionTreeNode(string id)
            {
                ID = id;
                Parent = null;
                Children = new List<CheckConditionTreeNode>();
            }
            // Phương thức Add để thêm một nút con vào nút hiện tại và cập nhật thuộc tính Parent của nút con
            public void Add(CheckConditionTreeNode child)
            {
                print(this.debug());
                child.Parent = this;
                Children.Add(child);
            }

            // Phương thức GetChild để trả về nút con có ID cho trước hoặc null nếu không tồn tại
            public CheckConditionTreeNode GetChild(string id)
            {
                print(this.debug());
                return Children.Find(c => c.ID == id);
            }

            public void insert(CheckConditionTreeNode root, string input)
            {
                print(this.debug("Inser into root", new
                {
                    input
                }));
                string[] parts = input.Split(','); // Tách đầu vào thành các phần bằng dấu phẩy
                CheckConditionTreeNode current = root; // Gán nút hiện tại là nút gốc
                bool havenewBranch = false;

                foreach (string part in parts) // Duyệt qua các phần của đầu vào
                {
                    CheckConditionTreeNode child = current.GetChild(part); // Tìm nút con có ID bằng phần hiện tại
                    if (child == null) // Nếu không tồn tại nút con
                    {
                        child = new CheckConditionTreeNode(part); // Tạo một nút mới với ID bằng phần hiện tại
                        current.Add(child); // Thêm nút mới vào nút hiện tại
                        havenewBranch = true;
                    }
                    current = child; // Cập nhật nút hiện tại là nút con
                }

                if (havenewBranch)
                {
                    print(this.debug("New branch", new
                    {
                        input
                    }));
                    CheckConditionTreeNode child = new CheckConditionTreeNode("1");
                    current.Add(child); // Thêm nút mới vào nút hiện tại
                    havenewBranch = false;
                }
                else
                {
                    print(this.debug("current note at end", new
                    {
                        current.ID
                    }));
                    CheckConditionTreeNode child = current.Children[0]; // Tìm nút con đầu tiên cùng
                    print(this.debug("Child need to increase value", new { child.ID }));

                    if (int.TryParse(child.ID, out int newValue))
                    {
                        print(this.debug("Increase value", new { newValue }));
                        child.ID = string.Format("{0}", newValue + 1);
                    }
                    else
                    {
                        print(this.debug("TryParse to int fail", new { child.ID }));

                    }
                }
            }

            public bool ProvideCardAndCheck(CheckConditionTreeNode node, string path, bool flag)
            {
                print(this.debug("Check recursive", new { path }));
                if (node.Children.Count == 0) // Nếu nút không có nút con
                {
                    print(this.debug("In leaf", new { path, node.ID }));

                    if (int.TryParse(node.ID, out int numberRequire))
                    {
                        print(this.debug("TryParse", new { numberRequire }));

                        var cardTarget = new SelectTargetCard(path);
                        print(this.debug($"cardTarget is {(cardTarget != null ? "not null" : "Null")}", new
                        {
                            cardTarget.owner,
                            cardTarget.cardPosition,
                            cardTarget.cardType
                        }));
                        if (cardTarget != null)
                        {

                            CardPlayer CardOwner = null;
                            switch (cardTarget.owner)
                            {
                                case global::CardOwner.You:
                                    CardOwner = MatchManager.instance.localPlayer;
                                    break;

                                case global::CardOwner.Opponent:
                                    CardOwner = MatchManager.instance.localPlayer == MatchManager.instance.redPlayer ? MatchManager.instance.bluePlayer : MatchManager.instance.redPlayer;
                                    break;
                            }

                            print(this.debug("Select from Card Owner", new
                            {
                                side = CardOwner.side
                            }));

                            int providedCard = SelectManager.Instance.provideSelectCard<CardBase>(CardOwner, cardTarget.cardPosition, card => card.IsSelected == false);
                            bool result = providedCard >= numberRequire;
                            if (!result)
                            {
                                SelectManager.Instance.revokeSelectCard(CardOwner);
                                flag = false;
                                return false;
                            }
                            return flag;
                        }
                        else
                        {
                            print(this.debug("card target null"));
                            flag = false;
                            return false;
                        }
                    }
                    else
                    {
                        print(this.debug("TryParse to int fail", new { node.ID }));
                        flag = false;
                        return false;
                    }
                }
                foreach (CheckConditionTreeNode child in node.Children)
                {
                    string parentPath = path + "-" + child.ID;
                    flag = ProvideCardAndCheck(child, parentPath, flag) && flag;
                    if (!flag) return false;
                }
                return flag;
            }

            // In ra kết quả cây theo format yêu cầu
            public void PrintTree(CheckConditionTreeNode node, string path) // Hàm đệ quy để in ra kết quả cây từ một nút cho trước
            {
                if (node.Children.Count == 0) // Nếu nút không có nút con
                {
                    print(path); // In ra đường dẫn và số lần xuất hiện của đường dẫn đó
                    return; // Kết thúc hàm đệ quy
                }
                foreach (CheckConditionTreeNode child in node.Children) // Nếu nút có nút con, duyệt qua các nút con
                {
                    PrintTree(child, path + "-" + child.ID); // Gọi hàm đệ quy với nút con, đường dẫn được thêm ID của nút con và số lần xuất hiện được tăng lên 1
                }
            }
        }
    }
}

