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
            Debug.LogFormat("C31F1");
            Debug.LogFormat("C31F1 instance: {0}", Instance);

            if (Instance != null && Instance != this)
            {
                Debug.LogFormat("C31F1-01 destroy gameobject: {0}", gameObject);
                //UnityEngine.Debug.LogError("SelectManager have 2");
                Destroy(gameObject);
            }
            else
            {
                Debug.LogFormat("C31F1-02 instance is null or this, instance: ", Instance);
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
            Debug.LogFormat("C31F10");
            if(cardBase != null)
            {
                Debug.LogFormat("C31F10-01 Cardbase != null");
            }
            else
            {
                Debug.LogFormat("C31F10-02 Cardbase == null");
            }

            Debug.LogFormat("C31F10 Card base is selectable is {0}", cardBase.IsSelectAble);
            /*
             * kiểm tra có thể chọn được hay không(selectAble)
             * nếu chọn được thì đã chọn đủ chưa
             * nếu chưa đủ thì thêm vào queue
             * nếu đủ rồi thì từ chối lựa chọn
             */

            if(cardBase.IsSelectAble)
            {
                Debug.LogFormat("C31F10-03");
                Debug.LogFormat("C31F10-03 Is SelectAble");

                if (cardBase.Position == CardPosition.InHand)
                {
                    Debug.LogFormat("C31F10-03-01 cardbase position is inhand");
                    if(!cardBase.IsSelected)
                    {
                        Debug.LogFormat("C31F10-03-01-01 cardbase isn't selected");
                        var selectedCards = cardBase.CardPlayer.hand.GetAllCardSelected();
                        if(selectedCards.Count > 0)
                        {
                            Debug.LogFormat("C31F10-03-01-01-01 select cards count : {0}", selectedCards.Count);
                            selectedCards.ForEach(card => card.IsSelected = false);
                            print($"C31F10-03-01-01-01 Unselect {selectedCards.Count} cards");
                        }
                        cardBase.IsSelected = true;
                    }
                    else
                    {
                        Debug.LogFormat("C31F10-03-01-02 Card {0} have been selected", cardBase);
                    }
                }
                else if(cardBase is MonsterCard && (cardBase.Position == CardPosition.InSummonField || cardBase.Position == CardPosition.InFightField))
                {
                    Debug.LogFormat("C31F10-03-02 cardbase position is {0}", cardBase.Position);

                    //check if in pharse atk or defense --> user can select card in summon field to move this in action file
                    if(!cardBase.IsSelected)
                    {
                        Debug.LogFormat("C31F10-03-02-01 card base isn't selected");
                        cardBase.IsSelected = true; //co the select nhieu monstercard trong 2 field nay 
                    }
                    else
                    {
                        Debug.LogFormat("C31F10-03-02-02 card {0} is selected", cardBase);
                        cardBase.IsSelected = false;
                    }
                }
            }
            else
            {
                Debug.LogFormat("C31F10-04 Isn't SelectAble");
            }
        }

        private void ObjectSelected(object selectedTarget)
        {
            Debug.LogFormat("C31F5");
            if(selectedTarget is CardBase cardBase)
            {
                Debug.LogFormat("C31F5-01 select target is card base is {0}", cardBase);
                if (IsWatingPlayerSelect)
                {
                    Debug.LogFormat("C31F5-01-01 IsWatingPlayerSelect is {0}", IsWatingPlayerSelect);
                    if (targetPattern.Contains(cardBase))//match pattern
                    {
                        targetSelected = cardBase;
                        print(this.debug("C31F5-01-01-01 Select card", new
                        {
                            card = cardBase.ToString()
                        }));
                    }
                    else
                    {
                        print(this.debug("C31F5-01-01-02 Target dose not valid"));
                    }

                }
                else
                {
                    Debug.LogFormat("C31F5-01-02 IsWatingPlayerSelect is {0}", IsWatingPlayerSelect);
                }
            }
            else if(selectedTarget is CardPlayer cardPlayer)
            {
                Debug.LogFormat("C31F5-02 select target is card player is {0}", cardPlayer);

                if (IsWatingPlayerSelect)
                {
                    Debug.LogFormat("C31F5-02-01 IsWatingPlayerSelect is {0}", IsWatingPlayerSelect);
                    if (targetPattern.Contains(cardPlayer))//match pattern
                    {
                        targetSelected = cardPlayer;
                        print(this.debug("C31F5-02-01-01 Select Player", new
                        {
                            card = cardPlayer.ToString()
                        }));
                    }
                    else
                    {
                        print(this.debug("C31F5-02-01-02 Target dose not valid"));
                    }
                }
                else
                {
                    Debug.LogFormat("C31F5-02-02 IsWatingPlayerSelect is {0}", IsWatingPlayerSelect);
                }
            }
            else
            {
                Debug.LogFormat("C31F5-03 select target is undefined is {0}", cardPlayer);
            }
        }


        public void CheckSelectAble(MatchManager match)
        {
            Debug.LogFormat("C31F2");
            //get player
            var red = match.redPlayer;
            var blue = match.bluePlayer;

            print(red != null ? "C31F2 red ok" : "C31F2 red error");
            print(blue != null ? "C31F2 blue ok" : "C31F2 blue error");

            //get the current player in turn
            var currentplayer = match.getCurrenPlayer();
            print(this.debug(currentplayer != null ? "C31F2 currentplayer ok" : "C31F2 currentplayer error", new
            {
                currentplayer.side
            }));

            (var playerInturn, var playerPassTurn) = currentplayer == red ? (red, blue) : (blue, red);

            revokeSelectCard(playerPassTurn);

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
            Debug.LogFormat("C31F6");
            void revokeSelectCardOnHand()
            {
                Debug.LogFormat("C31F6-01 revokeSelectCardOnHand");
                player.hand.GetAllCardInHand().ForEach(card =>
                {
                    card.IsSelected = false; //if card selected, remote selected
                    card.IsSelectAble = false;    //revoke selectable in hand
                });
            }

            void revokeSelectCardOnFightZone()
            {
                Debug.LogFormat("C31F6-02 revokeSelectCardOnFightZone");
                player.fightZones.ForEach(zonefield =>
                {
                    if(zonefield.monsterCard != null)
                    {
                        Debug.LogFormat("C31F6-02-01");
                        var card = zonefield.monsterCard;
                        Debug.LogFormat("C31F6-02-01 card is {0}", card);
                        card.IsSelected = false;  //if card selected, remote selected
                        card.IsSelectAble = false;  //revoke selectable in fight zone
                    }
                });
            }

            void revokeSelectCardOnSummonZone()
            {
                Debug.LogFormat("C31F6-03 revokeSelectCardOnSummonZone");
                player.summonZones.ForEach(zonefield =>
                {
                    if(zonefield.GetMonsterCard() != null)
                    {
                        Debug.LogFormat("C31F6-03-01");
                        var card = zonefield.GetMonsterCard();
                        Debug.LogFormat("C31F6-03-01 card is {0}", card);
                        card.IsSelected = false;  //if card selected, remote selected
                        card.IsSelectAble = false;  //revoke selectable in fight zone
                    }
                });
            }

            switch(position)
            {
                case CardPosition.Any:
                    Debug.LogFormat("C31F6-04 card position is ANY");
                    revokeSelectCardOnHand();
                    revokeSelectCardOnFightZone();
                    revokeSelectCardOnSummonZone();
                    break;

                case CardPosition.InFightField:
                    Debug.LogFormat("C31F6-05 card position is InFightField");
                    revokeSelectCardOnFightZone();
                    break;

                case CardPosition.InSummonField:
                    Debug.LogFormat("C31F6-06 card position is InSummonField");
                    revokeSelectCardOnSummonZone();
                    break;
                case CardPosition.InHand:
                    Debug.LogFormat("C31F6-07 card position is InHand");
                    revokeSelectCardOnHand();
                    break;
            }
            print(this.debug("C31F6 Revoke select for player", new
            {
                player.side,
                numberCardInHand = player.hand.Count,
            }));
        }
#nullable enable

        private int provideSelectCard<T>(CardPlayer player, CardPosition position, Func<T, bool>? filter = null) where T : CardBase
        {
            Debug.LogFormat("C31F12");
            List<CardBase> listcard = new();
            switch(position)
            {
                case CardPosition.InGraveyard:
                    Debug.LogFormat("C31F12-01 card position in graveyard");
                    break;
                case CardPosition.InHand:
                    Debug.LogFormat("C31F12-02 card position in hand");
                    listcard = player.hand.GetAllCardInHand().Where(monsterCard => monsterCard != null).Select(monsterCard => monsterCard as CardBase).ToList();
                    break;
                case CardPosition.InFightField:
                    Debug.LogFormat("C31F12-03 card position in InFightField");
                    listcard = player.fightZones.Where(zone => zone.monsterCard != null).Select(zone => zone.monsterCard as CardBase).ToList();
                    break;
                case CardPosition.InSummonField:
                    Debug.LogFormat("C31F12-04 card position in InSummonField");
                    listcard = player.summonZones.Where(zone => zone.GetMonsterCard() != null).Select(zone => zone.GetMonsterCard() as CardBase).ToList();
                    break;
                default:
                    Debug.LogFormat("C31F12-05 not found card position");
                    break;
            }

            if(listcard != null && listcard.Count > 0)
            {
                Debug.LogFormat("C31F12-06");
                Debug.LogFormat("C31F12-06 list card count is : {0}", listcard.Count);
                if (filter != null)
                {
                    Debug.LogFormat("C31F12-06-01 filter != null");
                    foreach (var card in listcard)
                    {
                        Debug.LogFormat("C31F12-06-01 card in list card is {0}", card);
                        card.IsSelected = false;
                        card.IsSelectAble = filter.Invoke(arg: (T)card);
                        print(this.debug(string.Format("C31F12-06-01: {0} | {1} [ID:{2}]", card.ToString(), (card.IsSelectAble ? "can  select" : "can not select"), card.photonView.ViewID), new
                        {
                            card.Cost,
                            player.mana.Number,
                            card.IsSelectAble
                        }));
                    }
                }
                else
                {
                    Debug.LogFormat("C31F12-06-02 filter = null");
                    foreach (var card in listcard)
                    {
                        Debug.LogFormat("C31F12-06-02 card in list card is {0}", card);
                        card.IsSelected = false;
                        card.IsSelectAble = true;
                        print(this.debug(string.Format("C31F12-06-02: {0} | {1} [ID:{2}]", card.ToString(), (card.IsSelectAble ? "can  select" : "can not select"), card.photonView.ViewID), new
                        {
                            card.Cost,
                            player.mana.Number,
                            card.IsSelectAble
                        }));
                    }
                }
            }
            Debug.LogFormat("C31F12 list card!.count is {0}", listcard!.Count);

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
            Debug.LogFormat("C31F11");
            int offset = SelectTargetQueue.Count;
            Debug.LogFormat("C31F11 offset: {0}", offset);
            IsWatingPlayerSelect = true;
            yield return new WaitUntil(() => offset < SelectTargetQueue.Count);
            IsWatingPlayerSelect = false;
            yield return null;
        }

        public IEnumerator SelectTargets(SelectTarget selectTargets, List<(List<AbstractEffect>, AbstractTarget, object)> result)
        {
            Debug.LogFormat("C31F9");
            var ListTargetSelected = new List<ISelectManagerTarget>();
            var graph = GetGraphPlayerSelectTarget(selectTargets);
            if(graph != null && graph.Count > 0)
            {
                Debug.LogFormat("C31F9-01");
                for (int i = 0; i < selectTargets.selectTargets.Count; i++)
                {
                    Debug.LogFormat("C31F9-01 for {0} times", ++i);
                    List<ISelectManagerTarget> targeTips = new();
                    print(this.debug($"C31F9-01 Path guess", new
                    {
                        tips = string.Join("\n", graph.Select(list => string.Join("->", list)))
                    }));
                    foreach(var path in graph)
                    {
                        Debug.LogFormat("C31F9-01-01 foreach");
                        var targetTip = path[i];
                        Debug.LogFormat("C31F9-01-01 targetTip is {0}", targeTip);
                        if (!targeTips.Contains(targetTip))
                        {
                            Debug.LogFormat("C31F9-01-01-01");
                            targeTips.Add(targetTip);
                        }
                        else
                        {
                            Debug.LogFormat("C31F9-01-01-02");
                        }
                    }
                    print(this.debug($"C31F9-01 You can select", new
                    {
                        tips = string.Join(" or ", targeTips)
                    }));

                    yield return StartCoroutine(SelectWithPatterns(targeTips, ListTargetSelected));

                    if(ListTargetSelected.Count == i + 1)
                    {
                        Debug.LogFormat("C31F9-01-02 ListTargetSelected. Count = {0}", ListTargetSelected);
                        var targetSelected = ListTargetSelected[i];
                        print(this.debug("C31F9-01-02 Remove path not valid", new
                        {
                            targetSelected
                        }));

                        var targetElement = selectTargets.selectTargets[i];
                        if(targetElement.target is AnyTarget any)
                        {
                            Debug.LogFormat("C31F9-01-02-01");
                            Debug.LogFormat("C31F9-01-02-01 target element is any, is {0}", any);

                            if (targetSelected is CardBase card)
                            {
                                Debug.LogFormat("C31F9-01-02-01-01 target selected is card base, is {0}", card);
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
                            {
                                Debug.LogFormat("C31F9-01-02-01-02 target selected is not card base, is {0}", card);
                            }
                            if (targetSelected is CardPlayer player)
                            {
                                Debug.LogFormat("C31F9-01-02-01-03 target selected is card player, is {0}", player);
                                var target = new PlayerTarget
                                {
                                    side = CardOwner.You
                                };
                                result.Add((targetElement.Effects, target, targetSelected));
                            }
                            else
                            {
                                Debug.LogFormat("C31F9-01-02-01-04 target selected is not card player, is {0}", player);
                            }
                        }
                        else
                        {
                            Debug.LogFormat("C31F9-01-02-02 target element is not ANY");
                            result.Add((targetElement.Effects, targetElement.target, targetSelected));
                        }
                        //action(targetElement.Effects, targetElement.target, targetSelected);
                        graph.RemoveAll(list => list[i] != targetSelected);
                    }
                    else
                    {
                        print(this.debug("C31F9-01-03 Target Selected Not store!!"));
                    }
                }
            }
            else
            {
                print(this.debug("C31F9-02  does not enough condition to select all targets"));
            }
            yield return null;
        }

        public List<List<ISelectManagerTarget>> GetGraphPlayerSelectTarget(SelectTarget selectTargets)
        {
            Debug.LogFormat("C31F3");
            print(this.debug("C31F3 Select target condition", new
            {
                count = selectTargets.selectTargets.Count
            }));

            List<List<ISelectManagerTarget>> trackedNode = new List<List<ISelectManagerTarget>>(); // Tạo mảng lưu trữ các nút con

            for(int i = 0; i < selectTargets.selectTargets.Count; i++)
            {
                var selectElement = selectTargets.selectTargets[i];
                print(this.debug($"C31F3 Target {i}", new
                {
                    selectElement.GetType().Name
                }));
                if(selectElement.target is PlayerTarget selectPlayer)
                {
                    Debug.LogFormat("C31F3-01");
                    Debug.LogFormat("C31F3-01 select Player is {0}", selectPlayer);
                    var targets = selectPlayer.Execute(MatchManager.instance);
                    print(this.debug($"C31F3-01 Target {i} is target Player", new
                    {
                        targets.Count
                    }));
                    if(targets != null && targets.Count > 0)
                    {
                        Debug.LogFormat("C31F3-01-01 targets not null, count is {0}", targets.Count);
                        trackedNode.Add(targets.Cast<ISelectManagerTarget>().ToList());
                    }
                    else
                    {
                        Debug.LogFormat("C31F3-01-02 targets is {0} or count {1}", targets, targets.Count);
                        trackedNode.Add(new List<ISelectManagerTarget>());
                    }
                }
                else if(selectElement.target is CardTarget selectCard)
                {
                    Debug.LogFormat("C31F3-02");
                    Debug.LogFormat("C31F3-02 select card is {0}", selectCard);
                    var targets = selectCard.Execute(MatchManager.instance);
                    print(this.debug($"C31F3-02 Target {i} is target Card", new
                    {
                        targets.Count
                    }));
                    if(targets != null && targets.Count > 0)
                    {
                        Debug.LogFormat("C31F3-02-01 targets not null, count is {0}", targets.Count);
                        trackedNode.Add(targets.Cast<ISelectManagerTarget>().ToList());
                    }
                    else
                    {
                        Debug.LogFormat("C31F3-02-02 targets is {0} or count {1}", targets, targets.Count);
                        trackedNode.Add(new List<ISelectManagerTarget>());
                    }
                }
                else if(selectElement.target is AnyTarget any)
                {
                    Debug.LogFormat("C31F3-03");
                    var targets = any.Execute(MatchManager.instance);
                    print(this.debug($"C31F3-03 Target {i} is target Any", new
                    {
                        targets.Count
                    }));
                    if(targets != null && targets.Count > 0)
                    {
                        Debug.LogFormat("C31F3-03-01 targets not null, count is {0}", targets.Count);
                        trackedNode.Add(targets.Cast<ISelectManagerTarget>().ToList());
                    }
                    else
                    {
                        Debug.LogFormat("C31F3-03-02 targets is {0} or count {1}", targets, targets.Count);
                        trackedNode.Add(new List<ISelectManagerTarget>());
                    }
                }
                else
                {
                    Debug.LogFormat("C31F3-04 target not undefined");
                }
            }
            var result = GuessPlayerSelectTip(trackedNode);
            return result;
        }

        //xem lại
        private List<List<ISelectManagerTarget>> GuessPlayerSelectTip(List<List<ISelectManagerTarget>> lists)
        {
            Debug.LogFormat("C31F4");
            List<List<ISelectManagerTarget>> result = new List<List<ISelectManagerTarget>>();
            if(lists == null || lists.Count == 0 || lists.Any(sublist => sublist == null || sublist.Count == 0))
            {
                Debug.LogFormat("C31F4-01 No target input");
                return new List<List<ISelectManagerTarget>>();
            }
            else
            {
                Debug.LogFormat("C31F4-02 have target input");
            }
            if (lists.Count == 1)
            {
                Debug.LogFormat("C31F4-03 just 1 target");

                foreach(var element in lists.First())
                {
                    Debug.LogFormat("C31F4-03 {0}: element is {1}", lists.Count, element);
                    result.Add(new List<ISelectManagerTarget> { element });
                }
                return result;
            }
            else
            {

                Debug.LogFormat("C31F4-04 diff 1 target, count of list: {0}", lists.Count);
            }

            Queue<List<ISelectManagerTarget>> queue = new Queue<List<ISelectManagerTarget>>();

            foreach(var target in lists.First())
            {
                Debug.LogFormat("C31F4 target is {0}", target);
                queue.Enqueue(new List<ISelectManagerTarget>() { target });
            }

            for(int i = 1; i < lists.Count; i++)
            {
                Debug.LogFormat("C31F4");
                int size = queue.Count;
                Debug.LogFormat("C31F4 for i, i= {0}", i);
                Debug.LogFormat("C31F4 for i, size = {0}", size);
                for (int j = 0; j < size; j++)
                {
                    Debug.LogFormat("C31F4-01 for j in for i, i= {0}, j= {1}", i, j);
                    var path = queue.Dequeue();
                    foreach(var element in lists[i])
                    {
                        Debug.LogFormat("C31F4-01-01 for each, element is {0}", element);
                        if (!path.Contains(element))
                        {
                            Debug.LogFormat("C31F4-01-01-01 path not exist element");
                            List<ISelectManagerTarget> newPath = new List<ISelectManagerTarget>(path)
                            {
                                element
                            };
                            Debug.LogFormat("C31F4-01-01-01 i= {0}, list.Count -1 = {1}", i, lists.Count-1);
                            if (i == lists.Count - 1)
                            {
                                Debug.LogFormat("C31F4-01-01-01-01 i == lists.Count - 1");
                                result.Add(newPath);
                                print(this.debug("C31F4-01-01-01-01 Found path", new
                                {
                                    path = string.Join("->", newPath)
                                }));
                            }
                            else // Ngược lại, thêm bản sao vào hàng đợi để duyệt tiếp
                            {
                                print(this.debug("C31F4-01-01-01-02 Next, add a copy to the queue for further browsing", new
                                {
                                    path = string.Join("->", newPath) + " -> ?"
                                }));
                                queue.Enqueue(newPath);
                            }
                        }
                        else
                        {
                            Debug.LogFormat("C31F4-01-01-01 path exist element");
                        }
                    }
                }
            }
            return result;
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
            Debug.LogFormat("C31F14");
            targetPattern.Clear();
            foreach(var target in targets)
            {
                Debug.LogFormat("C31F14");
                target.IsSelectAble = true;
                targetPattern.Add(target);
                print(this.debug($"C31F14 Selectable item : {target}"));
            }

            IsWatingPlayerSelect = true;
            do
            {
                print(this.debug("C31F14 Please select target"));
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

