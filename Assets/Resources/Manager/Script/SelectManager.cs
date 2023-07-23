using Assets.GameComponent.Card.Logic.TargetObject;
using Assets.GameComponent.Card.Logic.TargetObject.Select;
using Assets.GameComponent.Card.Logic.TargetObject.Target;
using Assets.GameComponent.Card.Logic.TargetObject.Target.AnyTarget;
using Assets.GameComponent.Card.Logic.TargetObject.Target.CardTarget;
using Assets.GameComponent.Card.Logic.TargetObject.Target.PlayerTarget;
using Assets.GameComponent.Card.LogicCard;
using Assets.GameComponent.Manager.IManager;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using PlayFab.MultiplayerModels;
using PlayFab.ServerModels;
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
using UnityEngine.UIElements;
//using static Assets.GameComponent.Card.Logic.TargetObject.SelectTargetObject;
using static K_Player;
using static MatchManager;

namespace Assets.GameComponent.Manager
{
    public class SelectManager : MonoBehaviourPunCallbacks
    {
        public static SelectManager Instance;

        private void Awake()
        {
            //DontDestroyOnLoad(gameObject);
            if(Instance != null && Instance != this)
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

        private List<ISelectManagerTarget> targetPattern = new();
        private ISelectManagerTarget targetSelected = null;
        public Queue SelectTargetQueue = new Queue();
        private void Start()
        {
            this.RegisterListener(EventID.OnStartTurn, param => Instance.CheckSelectAble(param as MatchManager));
            this.RegisterListener(EventID.EndAttackAndDefensePhase, param => Instance.CheckSelectAble(param as MatchManager));

            this.RegisterListener(EventID.OnLeftClickCard, param => Instance.OnLeftClickCard(param as CardBase));

            this.RegisterListener(EventID.OnObjectSelected, param => Instance.ObjectSelected(param));
            print(this.debug("Regis for event EventID.OnStartTurn"));
        }

        private void OnLeftClickCard(CardBase cardBase)
        {
            print(nameof(OnLeftClickCard));

            if(cardBase != null) print("CardBase != null");

            print(cardBase.IsSelectAble);
            /*
             * kiểm tra có thể chọn được hay không(selectAble)
             * nếu chọn được thì đã chọn đủ chưa
             * nếu chưa đủ thì thêm vào queue
             * nếu đủ rồi thì từ chối lựa chọn
             */

            if(cardBase.IsSelectAble)
            {
                print("IsSelectAble");

                if (cardBase.Position == CardPosition.InHand)
                {
                    print("Card In Hand");
                    if(!cardBase.IsSelected)
                    {
                        var selectedCards = cardBase.CardPlayer.hand.GetAllCardSelected();
                        if(selectedCards.Count > 0)
                        {
                            selectedCards.ForEach(card => card.IsSelected = false);
                            print($"Unselect {selectedCards.Count} cards");
                        }
                        cardBase.IsSelected = true;
                    }
                    else
                    {
                        print(this.debug($"Card {cardBase} have been selected"));
                    }
                }
                else if(cardBase is MonsterCard && (cardBase.Position == CardPosition.InSummonField || cardBase.Position == CardPosition.InFightField))
                {
                    print(this.debug("Select card on field"));

                    //check if in pharse atk or defense --> user can select card in summon field to move this in action file
                    if(!cardBase.IsSelected)
                    {
                        cardBase.IsSelected = true; //co the select nhieu monstercard trong 2 field nay 
                    }
                    else
                    {
                        cardBase.IsSelected = false;
                    }
                }
            }
       
        }

        private void ObjectSelected(object selectedTarget)
        {
            print(this.debug());
            if(selectedTarget is CardBase cardBase)
            {
                if(IsWatingPlayerSelect)
                {
                    if(targetPattern.Contains(cardBase))//match pattern
                    {
                        //if (!SelectTargetQueue.Contains(cardBase))
                        //{
                        targetSelected = cardBase;
                        //SelectTargetQueue.Enqueue(cardBase);
                        print(this.debug("Select card", new
                        {
                            card = cardBase.ToString()
                        }));
                        //}
                        //else
                        //{
                        //    print(this.debug("You have been select this target for another effect, please choose another one"));
                        //}

                        //print(this.debug("SelectTargetQueue count", new
                        //{
                        //    SelectTargetQueue.Count
                        //}));
                    }
                    else
                    {
                        print(this.debug("Target dose not valid"));
                    }

                }
            }
            else if(selectedTarget is CardPlayer cardPlayer)
            {
                if(IsWatingPlayerSelect)
                {
                    if(targetPattern.Contains(cardPlayer))//match pattern
                    {
                        //if (!SelectTargetQueue.Contains(cardPlayer))
                        //{
                        targetSelected = cardPlayer;
                        //SelectTargetQueue.Enqueue(cardPlayer);
                        print(this.debug("Select Player", new
                        {
                            card = cardPlayer.ToString()
                        }));
                        //}
                        //else
                        //{
                        //    print(this.debug("You have been select this target for another effect, please choose another one"));
                        //}
                    }
                    else
                    {
                        print(this.debug("Target dose not valid"));
                    }
                }
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
            print(this.debug(currentplayer != null ? "currentplayer ok" : "currentplayer error", new
            {
                currentplayer.side
            }));

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
            provideSelectCard<CardBase>(playerInturn, position: CardPosition.InHand, card => card.Cost <= currentplayer.mana.Number && !(match.gamePhase == GamePhase.Attack) && card.CardPlayer == match.LocalPlayer);
            provideSelectCard<CardBase>(playerInturn, position: CardPosition.InSummonField, card => card.CardPlayer == match.LocalPlayer);
        }
        /// <summary>
        /// revoke select for player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="position"></param>
        private void revokeSelectCard(CardPlayer player, CardPosition position = CardPosition.Any)
        {
            /*
             *revoke select for pass player
             *Card in hand
             *Card in fight zone
             *Card in summon zone
             */

            void revokeSelectCardOnHand()
            {
                player.hand.GetAllCardInHand().ForEach(card =>
                {
                    card.IsSelected = false; //if card selected, remote selected
                    card.IsSelectAble = false;    //revoke selectable in hand
                });
            }

            void revokeSelectCardOnFightZone()
            {
                player.fightZones.ForEach(zonefield =>
                {
                    if(zonefield.monsterCard != null)
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
                    if(zonefield.GetMonsterCard() != null)
                    {
                        var card = zonefield.GetMonsterCard();
                        card.IsSelected = false;  //if card selected, remote selected
                        card.IsSelectAble = false;  //revoke selectable in fight zone
                    }
                });
            }

            switch(position)
            {
                case CardPosition.Any:
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
            print(this.debug("Revoke select for player", new
            {
                player.side,
                numberCardInHand = player.hand.Count,
            }));
        }
#nullable enable

        private int provideSelectCard<T>(CardPlayer player, CardPosition position, Func<T, bool>? filter = null) where T : CardBase
        {
            List<CardBase> listcard = new();
            switch(position)
            {
                //case CardPosition.InDeck:
                //    listcard = player.deck.cards.Where(monsterCard => monsterCard != null).Select(monsterCard => monsterCard as CardBase).ToList();
                //    break;
                case CardPosition.InGraveyard:
                    break;
                case CardPosition.InHand:
                    listcard = player.hand.GetAllCardInHand().Where(monsterCard => monsterCard != null).Select(monsterCard => monsterCard as CardBase).ToList();
                    break;
                case CardPosition.InFightField:
                    listcard = player.fightZones.Where(zone => zone.monsterCard != null).Select(zone => zone.monsterCard as CardBase).ToList();
                    break;
                case CardPosition.InSummonField:
                    listcard = player.summonZones.Where(zone => zone.GetMonsterCard() != null).Select(zone => zone.GetMonsterCard() as CardBase).ToList();
                    break;
                default:
                    print(this.debug("not found card position"));
                    break;
            }

            if(listcard != null && listcard.Count > 0)
            {
                if(filter != null)
                {
                    // Get the first monster card in the list
                    // Invoke the cardTarget method on the first card and get the return value
                    // Do something with the result
                    foreach(var card in listcard)
                    {
                        card.IsSelected = false;
                        card.IsSelectAble = filter.Invoke(arg: (T)card);
                        print(this.debug(string.Format("{0} | {1} [ID:{2}]", card.ToString(), (card.IsSelectAble ? "can  select" : "can not select"), card.photonView.ViewID), new
                        {
                            card.Cost,
                            player.mana.Number,
                            card.IsSelectAble
                        }));
                    }
                }
                else
                {
                    foreach(var card in listcard)
                    {
                        card.IsSelected = false;
                        card.IsSelectAble = true;
                        print(this.debug(string.Format("{0} | {1} [ID:{2}]", card.ToString(), (card.IsSelectAble ? "can  select" : "can not select"), card.photonView.ViewID), new
                        {
                            card.Cost,
                            player.mana.Number,
                            card.IsSelectAble
                        }));
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
        //public IEnumerator Select(SelectTargetCard cardTarget)
        //{
        //    print(this.debug());
        //    CardPlayer CardOwner = null;
        //    switch (cardTarget.owner)
        //    {
        //        case global::CardOwner.You:
        //            CardOwner = MatchManager.instance.LocalPlayer;
        //            break;

        //        case global::CardOwner.Opponent:
        //            CardOwner = MatchManager.instance.LocalPlayer == MatchManager.instance.redPlayer ? MatchManager.instance.bluePlayer : MatchManager.instance.redPlayer; ;
        //            break;
        //    }
        //    print(this.debug("Select from Card Owner", new
        //    {
        //        side = CardOwner.side
        //    }));

        //    if (CardOwner != null)
        //    {
        //        print(this.debug("Select from Card Position", new
        //        {
        //            cardTarget.cardPosition
        //        }));

        //        var CardCanBeSelect = provideSelectCard<CardBase>(CardOwner, cardTarget.cardPosition,
        //          card => card.IsSelected == false &&
        //          card.IsSelected == false
        //        );
        //        print(this.debug("Number Card can be select", new
        //        {
        //            CardCanBeSelect,
        //        }));

        //        if (CardCanBeSelect >= 1)
        //        {
        //            yield return StartCoroutine(PlayerSelectCard(cardTarget));
        //        }
        //        else
        //        {
        //            print(this.debug("do not enough card in position selected"));
        //        }
        //    }
        //    else
        //    {
        //        print(this.debug("Card Owner null"));
        //    }
        //    yield return null;
        //}

        //private IEnumerator PlayerSelectCard(SelectTargetCard cardTarget)
        //{
        //    //print(this.debug(cardTarget.ToString()));
        //    //IsWatingPlayerSelect = true;
        //    //int offset = SelectTargetQueue.Count;
        //    //targetPattern = cardTarget;
        //    //yield return new WaitUntil(() => offset < SelectTargetQueue.Count);
        //    //targetPattern = null;
        //    //IsWatingPlayerSelect = false;
        //    yield return null;
        //}

        private IEnumerator WaitingSelectTargetAny()
        {
            int offset = SelectTargetQueue.Count;
            IsWatingPlayerSelect = true;
            yield return new WaitUntil(() => offset < SelectTargetQueue.Count);
            IsWatingPlayerSelect = false;
            yield return null;
        }

        public IEnumerator SelectTargets(SelectTarget selectTargets, List<(List<AbstractEffect>, AbstractTarget, object)> result)
        {
            var ListTargetSelected = new List<ISelectManagerTarget>();
            var graph = GetGraphPlayerSelectTarget(selectTargets);
            if(graph != null && graph.Count > 0)
            {
                for(int i = 0; i < selectTargets.selectTargets.Count; i++)
                {
                    List<ISelectManagerTarget> targeTips = new();
                    print(this.debug($"Path guess", new
                    {
                        tips = string.Join("\n", graph.Select(list => string.Join("->", list)))
                    }));
                    foreach(var path in graph)
                    {
                        var targetTip = path[i];
                        if(!targeTips.Contains(targetTip))
                            targeTips.Add(targetTip);
                    }
                    print(this.debug($"You can select", new
                    {
                        tips = string.Join(" or ", targeTips)
                    }));

                    yield return StartCoroutine(SelectWithPatterns(targeTips, ListTargetSelected));

                    if(ListTargetSelected.Count == i + 1)
                    {
                        var targetSelected = ListTargetSelected[i];
                        print(this.debug("Remove path not valid", new
                        {
                            targetSelected
                        }));

                        var targetElement = selectTargets.selectTargets[i];
                        if(targetElement.target is AnyTarget any)
                        {
                            if(targetSelected is CardBase card)
                            {
                                var target = new CardTarget
                                {
                                    cardPosition = (CardPosition)card.Position,
                                    owner = (CardOwner)card.CardOwner,
                                    Rarity = (Rarity)card.RarityCard,
                                    region = (RegionCard)card.RegionCard
                                };
                                result.Add((targetElement.Effects, target, targetSelected));
                            }
                            else
                            if(targetSelected is CardPlayer player)
                            {
                                var target = new PlayerTarget
                                {
                                    side = CardOwner.You
                                };
                                result.Add((targetElement.Effects, target, targetSelected));
                            }
                        }
                        else
                        {
                            result.Add((targetElement.Effects, targetElement.target, targetSelected));
                        }
                        //action(targetElement.Effects, targetElement.target, targetSelected);
                        graph.RemoveAll(list => list[i] != targetSelected);
                    }
                    else
                    {
                        print(this.debug("Target Selected Not store!!"));
                    }
                }
            }
            else
            {
                print(this.debug("does not enough condition to select all targets"));
            }


            yield return null;
        }

        public List<List<ISelectManagerTarget>> GetGraphPlayerSelectTarget(SelectTarget selectTargets)
        {
            /*
             * Check enough base condition for select
             * *enough card respond to the condition 
             * * *select how many card in your option
             */
            print(this.debug("Select target condition", new
            {
                count = selectTargets.selectTargets.Count
            }));

            //CheckConditionTreeNode root = new CheckConditionTreeNode("Root"); // Tạo nút gốc
            List<List<ISelectManagerTarget>> trackedNode = new List<List<ISelectManagerTarget>>(); // Tạo mảng lưu trữ các nút con

            for(int i = 0; i < selectTargets.selectTargets.Count; i++)
            {
                var selectElement = selectTargets.selectTargets[i];
                print(this.debug($"Target {i}", new
                {
                    selectElement.GetType().Name
                }));
                if(selectElement.target is PlayerTarget selectPlayer)
                {

                    var targets = selectPlayer.Execute(MatchManager.instance);
                    print(this.debug($"Target {i} is target Player", new
                    {
                        targets.Count
                    }));
                    if(targets != null && targets.Count > 0)
                    {
                        trackedNode.Add(targets.Cast<ISelectManagerTarget>().ToList());
                    }
                    else
                    {
                        trackedNode.Add(new List<ISelectManagerTarget>());

                    }
                }
                else if(selectElement.target is CardTarget selectCard)
                {
                    var targets = selectCard.Execute(MatchManager.instance);
                    print(this.debug($"Target {i} is target Card", new
                    {
                        targets.Count
                    }));
                    if(targets != null && targets.Count > 0)
                    {
                        trackedNode.Add(targets.Cast<ISelectManagerTarget>().ToList());
                    }
                    else
                    {
                        trackedNode.Add(new List<ISelectManagerTarget>());
                    }
                }
                else if(selectElement.target is AnyTarget any)
                {
                    var targets = any.Execute(MatchManager.instance);
                    print(this.debug($"Target {i} is target Card", new
                    {
                        targets.Count
                    }));
                    if(targets != null && targets.Count > 0)
                    {
                        trackedNode.Add(targets.Cast<ISelectManagerTarget>().ToList());
                    }
                    else
                    {
                        trackedNode.Add(new List<ISelectManagerTarget>());
                    }
                }
            }
            var result = GuessPlayerSelectTip(trackedNode);
            print(this.debug("Guess select Path", new
            {
                paths = string.Join("\n", result.Select(list => string.Join("->", list)))
            }));
            return result;
        }

        private List<List<ISelectManagerTarget>> GuessPlayerSelectTip(List<List<ISelectManagerTarget>> lists)
        {
            List<List<ISelectManagerTarget>> result = new List<List<ISelectManagerTarget>>();
            if(lists == null || lists.Count == 0 || lists.Any(sublist => sublist == null || sublist.Count == 0))
            {
                print(this.debug("No target input"));
                return new List<List<ISelectManagerTarget>>();
            }
            else
            if(lists.Count == 1)
            {
                print(this.debug("just 1 target"));

                foreach(var element in lists.First())
                {
                    result.Add(new List<ISelectManagerTarget> { element });
                }
                return result;
            }

            print(this.debug("Start"));
            Queue<List<ISelectManagerTarget>> queue = new Queue<List<ISelectManagerTarget>>();

            foreach(var target in lists.First())
            {
                queue.Enqueue(new List<ISelectManagerTarget>() { target });
            }

            for(int i = 1; i < lists.Count; i++)
            {
                int size = queue.Count;
                for(int j = 0; j < size; j++)
                {
                    var path = queue.Dequeue();
                    foreach(var element in lists[i])
                    {
                        if(!path.Contains(element))
                        {
                            print(this.debug("New"));

                            List<ISelectManagerTarget> newPath = new List<ISelectManagerTarget>(path)
                            {
                                element
                            };
                            if(i == lists.Count - 1)
                            {
                                result.Add(newPath);
                                print(this.debug("Found path", new
                                {
                                    path = string.Join("->", newPath)
                                }));
                            }
                            else // Ngược lại, thêm bản sao vào hàng đợi để duyệt tiếp
                            {
                                print(this.debug("Next", new
                                {
                                    path = string.Join("->", newPath) + " -> ?"
                                }));
                                queue.Enqueue(newPath);
                            }
                        }
                        else
                        {
                            print(this.debug("repeated"));
                        }

                    }
                }
            }

            return result;
            ;
        }
        public IEnumerator SelectAllCardInField(AbstractTarget item)
        {
            //print(this.debug());
            //SelectTargetQueue.Clear();
            //List<MonsterCard> cardList = new List<MonsterCard>();
            //if (item.SelectTargetObject is SelectTargetCard selectCard)
            //{
            //    CardPlayer CardOwner = null;
            //    switch (selectCard.owner)
            //    {
            //        case global::CardOwner.You:
            //            CardOwner = MatchManager.instance.localPlayer;
            //            break;

            //        case global::CardOwner.Opponent:
            //            CardOwner = MatchManager.instance.localPlayer == MatchManager.instance.redPlayer ? MatchManager.instance.bluePlayer : MatchManager.instance.redPlayer; ;
            //            break;
            //    }
            //    var cardPosition = selectCard.cardPosition;
            //    switch (cardPosition)
            //    {
            //        case CardPosition.All:
            //            break;

            //        case CardPosition.InFightField:
            //            break;

            //        case CardPosition.InSummonField:
            //            cardList = CardOwner.summonZones.Where(zone => zone.monsterCard != null).Select(zone => zone.monsterCard).ToList();
            //            break;
            //        case CardPosition.InHand:
            //            break;
            //    }
            //    foreach (var card in cardList)
            //    {
            //        SelectTargetQueue.Enqueue(card);
            //    }
            //}
            yield return null;
        }
        public IEnumerator SelectAny()
        {
            var red = MatchManager.instance.redPlayer;
            var blue = MatchManager.instance.bluePlayer;
            var CardCanBeSelect_red_summon = provideSelectCard<CardBase>(red, CardPosition.InSummonField, card => card.IsSelected == false);
            var CardCanBeSelect_blue_summon = provideSelectCard<CardBase>(blue, CardPosition.InSummonField, card => card.IsSelected == false);
            var CardCanBeSelect_red_FightField = provideSelectCard<CardBase>(red, CardPosition.InFightField, card => card.IsSelected == false);
            var CardCanBeSelect_blue_FightField = provideSelectCard<CardBase>(blue, CardPosition.InFightField, card => card.IsSelected == false);
            //còn player

            yield return StartCoroutine(WaitingSelectTargetAny());

            yield return null;
        }

        public IEnumerator SelectWithPatterns<T>(List<T> targets, List<ISelectManagerTarget> result) where T : ISelectManagerTarget
        {
            targetPattern.Clear();
            foreach(var target in targets)
            {
                target.IsSelectAble = true;
                targetPattern.Add(target);
                print(this.debug($"Selectable item : {target}"));
            }

            IsWatingPlayerSelect = true;
            do
            {
                print(this.debug("Please select target"));
                yield return new WaitUntil(() => targetSelected != null);
            } while(result.Contains(targetSelected));
            result.Add(targetSelected);
            targetSelected = null;
            IsWatingPlayerSelect = false;
            yield return null;
        }



        public class CheckConditionTreeNode
        {
            public string ID
            {
                get; set;
            } // Thuộc tính ID để lưu trữ giá trị của nút
            public CheckConditionTreeNode Parent
            {
                get; set;
            } // Thuộc tính Parent để lưu trữ nút cha của nút hiện tại
            public List<CheckConditionTreeNode> Children
            {
                get; set;
            } // Danh sách các nút con của nút hiện tại


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

                foreach(string part in parts) // Duyệt qua các phần của đầu vào
                {
                    CheckConditionTreeNode child = current.GetChild(part); // Tìm nút con có ID bằng phần hiện tại
                    if(child == null) // Nếu không tồn tại nút con
                    {
                        child = new CheckConditionTreeNode(part); // Tạo một nút mới với ID bằng phần hiện tại
                        current.Add(child); // Thêm nút mới vào nút hiện tại
                        havenewBranch = true;
                    }
                    current = child; // Cập nhật nút hiện tại là nút con
                }

                if(havenewBranch)
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
                    print(this.debug("Child need to increase value", new
                    {
                        child.ID
                    }));

                    if(int.TryParse(child.ID, out int newValue))
                    {
                        print(this.debug("Increase value", new
                        {
                            newValue
                        }));
                        child.ID = string.Format("{0}", newValue + 1);
                    }
                    else
                    {
                        print(this.debug("TryParse to int fail", new
                        {
                            child.ID
                        }));

                    }
                }
            }

            public bool ProvideCardAndCheck(CheckConditionTreeNode node, string path, bool flag)
            {
                //print(this.debug("Check recursive", new { path }));
                //if (node.Children.Count == 0) // Nếu nút không có nút con
                //{
                //    print(this.debug("In leaf", new { path, node.ID }));

                //    if (int.TryParse(node.ID, out int numberRequire))
                //    {
                //        print(this.debug("TryParse", new { numberRequire }));

                //        var cardTarget = new SelectTargetCard(path);
                //        print(this.debug($"cardTarget is {(cardTarget != null ? "not null" : "Null")}", new
                //        {
                //            cardTarget.owner,
                //            cardTarget.cardPosition,
                //            cardTarget.cardType
                //        }));
                //        if (cardTarget != null)
                //        {

                //            CardPlayer CardOwner = null;
                //            switch (cardTarget.owner)
                //            {
                //                case global::CardOwner.You:
                //                    CardOwner = MatchManager.instance.LocalPlayer;
                //                    break;

                //                case global::CardOwner.Opponent:
                //                    CardOwner = MatchManager.instance.LocalPlayer == MatchManager.instance.redPlayer ? MatchManager.instance.bluePlayer : MatchManager.instance.redPlayer;
                //                    break;
                //            }

                //            print(this.debug("Select from Card Owner", new
                //            {
                //                side = CardOwner.side
                //            }));

                //            int providedCard = SelectManager.Instance.provideSelectCard<CardBase>(CardOwner, cardTarget.cardPosition, card => card.IsSelected == false);
                //            bool result = providedCard >= numberRequire;
                //            if (!result)
                //            {
                //                SelectManager.Instance.revokeSelectCard(CardOwner);
                //                flag = false;
                //                return false;
                //            }
                //            return flag;
                //        }
                //        else
                //        {
                //            print(this.debug("card target null"));
                //            flag = false;
                //            return false;
                //        }
                //    }
                //    else
                //    {
                //        print(this.debug("TryParse to int fail", new { node.ID }));
                //        flag = false;
                //        return false;
                //    }
                //}
                //foreach (CheckConditionTreeNode child in node.Children)
                //{
                //    string parentPath = path + "-" + child.ID;
                //    flag = ProvideCardAndCheck(child, parentPath, flag) && flag;
                //    if (!flag) return false;
                //}
                return flag;
            }

            // In ra kết quả cây theo format yêu cầu
            public void PrintTree(CheckConditionTreeNode node, string path) // Hàm đệ quy để in ra kết quả cây từ một nút cho trước
            {
                if(node.Children.Count == 0) // Nếu nút không có nút con
                {
                    print(path); // In ra đường dẫn và số lần xuất hiện của đường dẫn đó
                    return; // Kết thúc hàm đệ quy
                }
                foreach(CheckConditionTreeNode child in node.Children) // Nếu nút có nút con, duyệt qua các nút con
                {
                    PrintTree(child, path + "-" + child.ID); // Gọi hàm đệ quy với nút con, đường dẫn được thêm ID của nút con và số lần xuất hiện được tăng lên 1
                }
            }
        }
    }
}

