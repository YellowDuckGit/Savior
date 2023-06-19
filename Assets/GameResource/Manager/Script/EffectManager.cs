using Assets.GameComponent.Card.CardComponents.Script.UI;
using Assets.GameComponent.Card.Logic;
using Assets.GameComponent.Card.Logic.Effect.Gain;
using Assets.GameComponent.Card.Logic.TargetObject;
using Assets.GameComponent.Card.Logic.TargetObject.Select;
using Assets.GameComponent.Card.LogicCard;
using Assets.GameComponent.Card.LogicCard.ConditionTrigger.Summon;
using Assets.GameComponent.Card.LogicCard.ListLogic.Effect;
using Assets.GameComponent.Manager;
using Card;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using PlayFab.GroupsModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using static Assets.GameComponent.Card.Logic.TargetObject.Select.SelectTargetPlayer;
//using static Assets.GameComponent.Card.Logic.TargetObject.SelectTargetObject;


public class EffectManager : MonoBehaviourPun
{
    public static EffectManager Instance;
    public enum EffectStatus
    {
        none,
        running,
        success,
        fail
    }
    public EffectStatus status = EffectStatus.none;
    private void Awake()
    {
        //DontDestroyOnLoad(gameObject);
        if (Instance != null && Instance != this)
        {
            UnityEngine.Debug.LogError("EffectManager have 2");
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }

    private void NetworkingClient_EventReceived(EventData obj)
    {
        var args = obj.GetData();
        switch ((RaiseEvent)obj.Code)
        {
            case RaiseEvent.EFFECT_EXCUTE:
                {
                    // abstractEffect, selectTarget, targetType, targetID
                    var senderPlayerSide = args.senderPlayerSide as string;
                    var effect = args.abstractEffect as AbstractEffect;
                    var selectTarget = args.selectTarget as AbstractSelectTargetObject;
                    var targetType = args.targetType as string;
                    var targetID = (int)args.targetID;

                    print(this.debug("Effect async Execute", new
                    {
                        effect,
                        selectTarget,
                        targetType,
                        targetID
                    }));
                    /*
                     * the target player be equal is "You" then:
                     * in the localside of sender: target is "You"
                     * in ther opponent' localside of sender: target must be "Opponent" because "You" inside the reciver mean "Opponent"
                     */
                    if (string.Compare(MatchManager.instance.localPlayer.side, senderPlayerSide, true) != 0)
                    {
                        if (selectTarget != null)
                        {
                            if (selectTarget is SelectTargetPlayer selectPlayer)
                            {
                                selectPlayer.SelectTarget = selectPlayer.SelectTarget == PlayerTarget.You ? PlayerTarget.Opponent : PlayerTarget.You;
                            }
                            else if (selectTarget is SelectTargetCard selectCard)
                            {
                                selectCard.owner = selectCard.owner == CardOwner.You ? CardOwner.Opponent : CardOwner.You;
                            }
                        }
                    }

                    StartCoroutine(EffectAction(effect, selectTarget, targetType, targetID));
                    break;
                }
            case RaiseEvent.EFFECT_UPDATE_STATUS:
                print(this.debug("update effect status", new
                {
                    status = Enum.GetName(typeof(EffectStatus), args.status)
                }));
                this.status = args.status;
                break;

        };
        //if ((RaiseEvent)obj.Code == RaiseEvent.EFFECT_EXCUTE)
        //{
        //    // abstractEffect, selectTarget, targetType, targetID
        //    var senderPlayerSide = args.senderPlayerSide as string;
        //    var effect = args.abstractEffect as AbstractEffect;
        //    var selectTarget = args.selectTarget as AbstractSelectTargetObject;
        //    var targetType = args.targetType as string;
        //    var targetID = (int)args.targetID;

        //    print(this.debug("Effect async Execute", new
        //    {
        //        effect,
        //        selectTarget,
        //        targetType,
        //        targetID
        //    }));
        //    /*
        //     * the target player be equal is "You" then:
        //     * in the localside of sender: target is "You"
        //     * in ther opponent' localside of sender: target must be "Opponent" because "You" inside the reciver mean "Opponent"
        //     */
        //    if (string.Compare(MatchManager.instance.localPlayer.side, senderPlayerSide, true) != 0)
        //    {
        //        if (selectTarget != null)
        //        {
        //            if (selectTarget is SelectTargetPlayer selectPlayer)
        //            {
        //                selectPlayer.SelectTarget = selectPlayer.SelectTarget == PlayerTarget.You ? PlayerTarget.Opponent : PlayerTarget.You;
        //            }
        //            else if (selectTarget is SelectTargetCard selectCard)
        //            {
        //                selectCard.owner = selectCard.owner == CardOwner.You ? CardOwner.Opponent : CardOwner.You;
        //            }
        //        }
        //    }

        //    StartCoroutine(EffectAction(effect, selectTarget, targetType, targetID));
        //}
        //if ((RaiseEvent)obj.Code == RaiseEvent.EFFECT_UPDATE_STATUS)
        //{
        //    print(this.debug("update effect status", new
        //    {
        //        status = Enum.GetName(typeof(EffectStatus), args.status)
        //    }));
        //    this.status = args.status;
        //}
    }
    private CardBase GetTargetCard(SelectTargetCard selectCard, int targetPhotonID)
    {
        CardBase CardTarget = null;
        CardPlayer CardOwner = null;
        switch (selectCard.owner)
        {
            case global::CardOwner.You:
                CardOwner = MatchManager.instance.localPlayer;
                break;

            case global::CardOwner.Opponent:
                CardOwner = MatchManager.instance.localPlayer == MatchManager.instance.redPlayer ? MatchManager.instance.bluePlayer : MatchManager.instance.redPlayer; ;
                break;
        }

        if (CardOwner != null)
        {
            switch (selectCard.cardPosition)
            {
                case CardPosition.InDeck:
                    //CardTarget = CardOwner.deck._cards.Where(monsterCard => monsterCard != null && monsterCard.photonView.ViewID == targetPhotonID).FirstOrDefault();
                    CardTarget = CardOwner.deck.PeekWithPhoton(targetPhotonID);
                    break;
                case CardPosition.InGraveyard:
                    //TODO: get card from Graveyard
                    break;
                case CardPosition.InHand:
                    CardTarget = CardOwner.hand._cards.Where(monsterCard => monsterCard != null && monsterCard.photonView.ViewID == targetPhotonID).FirstOrDefault();
                    break;
                case CardPosition.InFightField:
                    CardTarget = CardOwner.fightZones.Where(zone => zone.monsterCard != null && zone.monsterCard.photonView.ViewID == targetPhotonID).Select(zone => zone.monsterCard).FirstOrDefault();
                    break;
                case CardPosition.InSummonField:
                    CardTarget = CardOwner.summonZones.Where(zone => zone.monsterCard != null && zone.monsterCard.photonView.ViewID == targetPhotonID).Select(zone => zone.monsterCard).FirstOrDefault();
                    break;
                default: print(this.debug("not found card position")); break;
            }
        }
        return CardTarget;
    }
    private IEnumerator EffectAction(AbstractEffect effect, AbstractSelectTargetObject selectTargetObject, string TargetType, int targetPhotonID)
    {

        if (effect != null)
        {
            /*
             * get the target with the select object data and target ID
             */
            if (effect is BuffStats buffstarts)
            {
                yield return BuffStats(buffstarts, GetTargetCard(selectTargetObject as SelectTargetCard, targetPhotonID) as MonsterCard);
            }
            else if (effect is Dame dame)
            {
                yield return Dame(dame, GetTargetObject(selectTargetObject, targetPhotonID));

            }
            else if (effect is Gain gain)
            {
                yield return Gain(gain, GetTargetObject(selectTargetObject, targetPhotonID));
            }
        }

        yield return null;
    }

    private object GetTargetObject(AbstractSelectTargetObject selectTargetObject, int targetPhotonID)
    {
        object target = null;
        if (selectTargetObject is SelectTargetCard selectTargetCard)
        {
            target = GetTargetCard(selectTargetCard, targetPhotonID);
        }
        else if (selectTargetObject is SelectTargetPlayer selectTargetPlayer)
        {
            target = GetTargetPlayer(selectTargetPlayer);
        }
        return target;
    }

    private CardPlayer GetTargetPlayer(SelectTargetPlayer selectTargetPlayer)
    {
        if (selectTargetPlayer.SelectTarget == PlayerTarget.You)
            return MatchManager.instance.localPlayer;
        else if (selectTargetPlayer.SelectTarget == PlayerTarget.Opponent)
        {
            return MatchManager.instance.localPlayer == MatchManager.instance.redPlayer ? MatchManager.instance.bluePlayer : MatchManager.instance.redPlayer; ;
        }
        else
        {
            print(this.debug("Can not find value of target Player", new
            {
                target = selectTargetPlayer.SelectTarget.ToString()
            }));
        }
        return null;
    }

    public IEnumerator None()
    {
        print(this.debug());
        yield return new WaitForSeconds(2.0f);
    }
    public IEnumerator OnBeforeSummon(object register, Action summonAction)
    {
        /*
         * some thing onBefore summon action just running on local
         */
        print(this.debug("Status at start Before summon", new { status }));

        print(this.debug("Start Before summon", new
        {
            name = typeof(BeforeSummon).Name,
            register = register.ToString()
        }));
        /*
         * if the register have been regis for event before summon (request for summon this monster) excute effect
         * else (not regist) summon monster without effect
         */
        if (isObjectRegised(typeof(BeforeSummon).Name, register, out var abstractData))
        {
            status = EffectStatus.running;
            if (register is MonsterCard monsterCard)
            {
                /*
                 * card summoned be own by localPlayer
                 * then excute action and effect
                 * else wait until all effect have been run from the owner
                 */
                if (monsterCard.CardPlayer == MatchManager.instance.localPlayer)
                {
                    var Actions = abstractData.Action; //get all action in after summon event
                    print(this.debug("Object registed", new
                    {
                        NumberAction = Actions.Count,
                    }));

                    yield return StartCoroutine(ExcuteAction(Actions)); //excute all action
                }
                else
                {
                    print(this.debug("not the owner"));
                }
                //else
                //{
                //    //yield return new WaitUntil(() => this.status != EffectStatus.running); //wait until Finish Event have been 
                //}
                /*
                 * get the result effect execute
                 */
                if (status == EffectStatus.success)
                {
                    print(this.debug("Before summon success summon monster)"));
                    summonAction(); //summon the monster
                }
                else
                {
                    print(this.debug("Before summon fail (Can not summon the monster)", new
                    {
                        status = status.ToString()
                    }));
                }

            }
            else
            {
                print(this.debug("not monster card"));
                summonAction(); //use the card 
            }
        }
        else
        {
            print(this.debug("Object does not regis"));
            /*
             * if the register have not been regis for event before summon do as normal
             */
            summonAction(); //summon the monster
        }
        yield return null;
    }
    public IEnumerator OnAfterSummon(object register)
    {
        print(this.debug("Status at start after summon", new { status }));


        print(this.debug("Start After summon", new
        {
            name = typeof(AfterSummon).Name,
            register = register.ToString()
        }));
        /*
         * if the register have been regis for event after summon excute effect
         * else (not regist) do not any thing
         */
        if (isObjectRegised(typeof(AfterSummon).Name, register, out var abstractData))
        {
            status = EffectStatus.running;
            if (register is MonsterCard monsterCard)
            {
                print(this.debug("register is monster card"));
                /*
                  * card summoned be own by localPlayer
                  * then excute action and effect
                  * else wait until all effect have been run from the owner
                  */
                if (monsterCard.CardPlayer == MatchManager.instance.localPlayer)
                {
                    print(this.debug("player is the owner of card registed for after summon"));

                    var Actions = abstractData.Action; //get all action in after summon event
                    print(this.debug("Object registed", new
                    {
                        NumberAction = Actions.Count,
                    }));

                    yield return StartCoroutine(ExcuteAction(Actions)); //excute all action
                }
                else
                {
                    print(this.debug("player is not the owner of card registed for after summon just watting"));
                    yield return new WaitUntil(() => this.status != EffectStatus.running);
                    print(this.debug("Status at effect after summon done", new { status }));
                }
            }//else not monster card then use without doing any effect
        } //else continue;
        yield return null;
    }

    private IEnumerator ExcuteAction(List<AbstractData> Actions)
    {
        foreach (var action in Actions)
        {
            if (action is SelectTarget selectAction) //get each action and execute it, here is select card
            {

                print(this.debug("is SelectCardAction Action", new { actionName = action.GetType().Name }));
                yield return StartCoroutine(ActionSelectTarget(selectAction));
            }
            else
            {
                print(this.debug("Action dose not available"));
            }
        }
        /*
         * when player throw action have been execute all action then finish the effect process
         */
        print(this.debug("End execute action", new { status = this.status.ToString() }));
        if (this.status == EffectStatus.running)
        {
            yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.success));
        }
    }
    IEnumerator UpdateEffectStatusEvent(EffectStatus status)
    {
        print(this.debug("send Update effect status to", new { status }));
        object[] datas = new object[] { (int)status };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte)RaiseEvent.EFFECT_UPDATE_STATUS, datas, raiseEventOptions, SendOptions.SendUnreliable);

        yield return new WaitUntil(() => this.status == status);
        print(this.debug("effect status update success", new { this.status }));
        yield return null;
    }
    private IEnumerator ActionSelectTarget(SelectTarget SelectTarget)
    {
        print(this.debug());
        if (SelectTarget != null)
        {
            print(this.debug("Select card data", new
            {
                number = SelectTarget.SelectTargetObjects.Count,
            }));
            /*
             * Player select card action
             */
            SelectManager.Instance.SelectTargetQueue.Clear();
            yield return StartCoroutine(SelectManager.Instance.SelectTargets(SelectTarget.SelectTargetObjects));
            foreach (var selectitem in SelectTarget.SelectTargetObjects) //get subsciption of select card action
            {
                if (selectitem.SelectTargetObject != null) //select option with Type target (Player or card)
                {
                    /*
                     * check the target object type
                     */
                    if (selectitem.SelectTargetObject is SelectTargetPlayer selectPlayer)
                    {
                        yield return StartCoroutine(routine: ExecuteEffects(selectitem.Effect, selectitem.SelectTargetObject, GetTargetPlayer(selectPlayer)));//excute effect
                    }
                    else if (selectitem.SelectTargetObject is SelectTargetCard selectCard)
                    {
                        print(this.debug(null, new
                        {
                            owner = selectCard.owner.ToString(),
                            cardPosition = selectCard.cardPosition.ToString(),
                            cardType = selectCard.cardType.ToString()
                        }));

                        if (SelectManager.Instance.SelectTargetQueue.Count >= 1)
                        {
                            yield return StartCoroutine(routine: ExecuteEffects(selectitem.Effect, selectitem.SelectTargetObject, (CardBase)SelectManager.Instance.SelectTargetQueue.Dequeue()));//excute effect
                        }
                        else
                        {
                            print(this.debug("not enought target card"));
                            yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                            break;
                        }

                        print(this.debug("Effect of selected card", new
                        {
                            number = selectitem.Effect.Count,
                        }));
                    }
                    else
                    {
                        print(this.debug("selectitem.SelectFilter is null"));
                        yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                        break;
                    }
                }
            }
        }
        else
        {
            print(this.debug("SelectTarget null"));
            yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));

            //UpdateEffectStatusEvent(EffectStatus.fail);
        }
        yield return null;
    }

    private IEnumerator ExecuteEffects<T>(List<AbstractEffect> effects, AbstractSelectTargetObject selectTargetObject, T target) where T : MonoBehaviourPun
    {
        foreach (var effect in effects)
        {
            if (effect is BuffStats buffStats)
            {
                ExecuteEffectEvent(buffStats, selectTargetObject, target);
            }
            else if (effect is Dame dame)
            {
                ExecuteEffectEvent(dame, selectTargetObject, target);
            }
            else if (effect is Gain gain)
            {
                ExecuteEffectEvent(gain, selectTargetObject, target);
            }
        }
        yield return null;
    }

    public void ExecuteEffectEvent<T>(AbstractEffect AbstractEffect, AbstractSelectTargetObject selectTargetObject, T target) where T : MonoBehaviourPun
    {
        var sender = MatchManager.instance.localPlayer.side;
        var effectType = AbstractEffect.GetType().Name;
        var effectData = JsonUtility.ToJson(AbstractEffect);

        var targetType = target.GetType().Name;
        var targetID = target.photonView.ViewID;

        var selectTargetObjectType = selectTargetObject.GetType().Name;
        var selectTargetObjectJson = JsonUtility.ToJson(selectTargetObject);

        object[] datas = new object[] { sender, effectType, effectData, targetType, targetID, selectTargetObjectType, selectTargetObjectJson };
        print(this.debug("Event photon EFFECT_EXCUTE", new
        {
            sender = datas[0] as string,
            type = datas[1] as string, //type effect
            json = datas[2] as string,// json string

            targetType = datas[3] as string,//target type
            targetID = (int)datas[4], //target photon id

            selectType = datas[5] as string,
            selectJson = datas[6] as string
        }));
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte)RaiseEvent.EFFECT_EXCUTE, datas, raiseEventOptions, SendOptions.SendUnreliable);
    }

    //event action
    private IEnumerator BuffStats(BuffStats buffstarts, MonsterCard target)
    {
        print(this.debug());
        if (buffstarts != null)
        {
            if (target != null)
            {
                buffstarts.GainEffect(target, this);
            }
            else
            {
                print(this.debug("target null"));
            }
        }
        else
        {
            print(this.debug("buffstarts null"));
        }
        yield return null;
    }

    private IEnumerator Dame(Dame dame, object target)
    {
        print(this.debug());
        if (dame != null)
        {
            if (target != null)
            {
                if (target is CardPlayer player)
                {
                    player.hp.Number -= dame.number;
                }
                else if (target is MonsterCard card)
                {
                    card.Hp -= dame.number;
                }
            }
            else
            {
                print(this.debug("target null"));
            }
        }
        else
        {
            print(this.debug("Dame null"));
        }

        yield return null;
    }

    private IEnumerator Heal(Heal heal, object target)
    {
        print(this.debug());
        if (heal != null)
        {
            if (target != null)
            {
                if (target is CardPlayer player)
                {
                    player.hp.Number += heal.number;
                }
                else if (target is MonsterCard card)
                {
                    card.Hp += heal.number;
                }
            }
            else
            {
                print(this.debug("target null"));
            }
        }
        else
        {
            print(this.debug("Dame null"));
        }

        yield return null;
    }

    private IEnumerator Gain(Gain gain, object target)
    {
        if (gain != null && target != null)
        {
            if (target is MonsterCard monster)
            {
                print(this.debug($"Gain effect for {monster}"));
                gain.GainEffect(monster, this);
            }
        }
        else
        {
            print(this.debug("not avaiable"));
        }
        yield return null;
    }
    #region Setup

    /// <summary>
    /// registor an event for an registor Data
    /// </summary>
    Dictionary<string, List<Tuple<AbstractCondition, object>>> ObjectRegisteds = new Dictionary<string, List<Tuple<AbstractCondition, object>>>();

    /// <summary>
    /// regis an event for an registor
    /// </summary>
    /// <param name="event">an event that extend from AbstractCondition</param>
    /// <param name="registor">can be player or card</param>
    public void EffectRegistor(AbstractCondition @event, object registor)
    {
        print(this.debug());
        string key = @event.GetType().Name;
        List<Tuple<AbstractCondition, object>> list;
        if (ObjectRegisteds.TryGetValue(key, out list))
        {
            print(this.debug("Event registed, add registor", new { eventName = key, registor = registor.ToString() }));
            // The key already exists, add to the list
            list.Add(new Tuple<AbstractCondition, object>(@event, registor));
        }
        else
        {
            print(this.debug("Event don't registed yet, create and add first registor", new { eventName = key, registor = registor.ToString() }));
            // The key does not exist, create a new list
            list = new List<Tuple<AbstractCondition, object>>
            {
                new Tuple<AbstractCondition, object>(@event, registor)
            };
            ObjectRegisteds.Add(key, list);
        }
    }
    //tool manager
    /// <summary>
    /// Check if an object is registered for a given event.
    /// </summary>
    /// <param name="event">string of type of event extend from AbstractCondition class </param>
    /// <param name="registor">can be card or player</param>
    /// <param name="condition"> the output when registor have been regis for event</param>
    /// <returns>is registor have been registed for event</returns>
    private bool isObjectRegised(string @event, object registor, out AbstractCondition condition)
    {
        print(this.debug("check object regis for event", new { registor = registor.ToString() }));
        List<Tuple<AbstractCondition, object>> list;
        if (ObjectRegisteds.TryGetValue(@event, out list))
        {
            // The key exists, check the list
            var tuple = list.FirstOrDefault(t => t.Item1.GetType().Name == @event && t.Item2 == registor);
            if (tuple != null)
            {


                // Found a matching tuple, assign the output parameter to its Item1
                condition = tuple.Item1;
                print(this.debug("Found a matching", new
                {
                    condition = condition.GetType().Name,
                    registor = registor.ToString()
                }));
                return true;
            }
            else
            {
                print(this.debug("No matching tuple, assign the output parameter to null"));
                // No matching tuple, assign the output parameter to null
                condition = null;
                return false;
            }
        }
        else
        {
            print(this.debug("The key does not exist, assign the output parameter to null"));
            // The key does not exist, assign the output parameter to null
            condition = null;
            return false;
        }
    }
    private static class EffectTracker
    {

    }

}
#endregion
