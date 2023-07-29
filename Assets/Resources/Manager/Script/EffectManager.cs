using Assets.GameComponent.Card.CardComponents.Script.UI;
using Assets.GameComponent.Card.Logic;
using Assets.GameComponent.Card.Logic.ConditionTrigger.Round;
using Assets.GameComponent.Card.Logic.Effect.Destroy;
using Assets.GameComponent.Card.Logic.Effect.Gain;
using Assets.GameComponent.Card.Logic.RegisterLocalEvent;
using Assets.GameComponent.Card.Logic.TargetObject;
using Assets.GameComponent.Card.Logic.TargetObject.Select;
using Assets.GameComponent.Card.LogicCard;
using Assets.GameComponent.Card.LogicCard.ConditionTrigger.Summon;
using Assets.GameComponent.Card.LogicCard.ListLogic.Effect;
using Assets.GameComponent.Manager;
using Card;
using ExitGames.Client.Photon;
using Microsoft.Win32;
using Photon.Pun;
using Photon.Realtime;
using PlayFab.ClientModels;
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
using UnityEngine.Rendering;
//using static Assets.GameComponent.Card.Logic.TargetObject.Select.SelectTargetPlayer;
//using static Assets.GameComponent.Card.Logic.TargetObject.SelectTargetObject;
using static K_Player;
using static UnityEngine.Rendering.DebugUI;
using Assets.GameComponent.Card.Logic.Effect;
using Assets.GameComponent.Card.Logic.Have;
using UnityEngine.Rendering.VirtualTexturing;
using Assets.GameComponent.Card.Logic.Effect.CreateCard;
using Assets.GameComponent.Card.Logic.ConditionTrigger.CardStatus;
using Assets.GameComponent.Card.Logic.TargetObject.Target.PlayerTarget;
using static EventGameHandler;
//using static Assets.GameComponent.Card.Logic.Have.Have.AbstractHaveTargetData;
using System.Diagnostics.Eventing.Reader;
//using static Assets.GameComponent.Card.Logic.Have.Have.AbstractHaveTargetData.HavePlayerTarget.AbstractHavePlayerTargetData;
//using static Assets.GameComponent.Card.Logic.Have.Have.AbstractHaveTargetData.HavePlayerTarget.AbstractHavePlayerTargetData.HavePlayerTargetValue.HavePlayerTargetValueData;
//using static Assets.GameComponent.Card.Logic.Have.Have.AbstractHaveTargetData.HaveCardTarget.AbstractHaveCardTargetData;
//using static Assets.GameComponent.Card.Logic.Have.Have.AbstractHaveTargetData.HaveCardTarget.AbstractHaveCardTargetData.HaveCardTargetValue.HaveCardTargetValueData;
using Assets.GameComponent.Card.Logic.Effect.Store;
using Assets.GameComponent.Manager.IManager;
using Photon.Pun.Demo.PunBasics;
using Assets.GameComponent.Card.Logic.TargetObject.Target;
using Assets.GameComponent.Card.Logic.Actions.SelectAction.Select_Multi;
using static Assets.GameComponent.Card.Logic.Actions.SelectAction.Select_Multi.SelectMulti.MultiTargetType;
using static UnityEngine.UI.GridLayoutGroup;
using Assets.GameComponent.Card.Logic.Actions.SelectAction.Select_Self;
using Assets.GameComponent.Card.Logic.TargetObject.Target.CardTarget;
using Photon.Pun.Demo.Cockpit;
using static Assets.GameComponent.Card.Logic.Actions.SelectAction.Select_Self.SelectSelf.SelfType;
using Assets.GameComponent.Card.Logic.TargetObject.Target.AnyTarget;
using Assets.GameComponent.Card.Logic.Actions.Specify;
using System.Security.Principal;
using static Assets.GameComponent.Card.Logic.Actions.Specify.SpecifyAction.SpecifyType;
using static Assets.GameComponent.Card.Logic.TargetObject.Target.PlayerTarget.PlayerTarget;
using Assets.GameComponent.Card.Logic.Actions.SelectAction.Select_Special;
using static Assets.GameComponent.Card.Logic.TargetObject.Target.AbstractTarget.AbstractTargetDataType.AbstractTargetDataTypeValue.ValueNumber;
using UnityEngine.Windows;
using Unity.VisualScripting;
using System.Security.Policy;
using MoreMountains.Tools;

public class EffectManager : MonoBehaviourPun
{
    public static EffectManager Instance;
    public Dictionary<EventID, List<(object register, List<AbstractAction> Actions, LifeTime lifetime, WhenDie WhenDie)>> EventEffectDispatcher = new();

    public EffectStatus status = EffectStatus.none;

    public enum EffectStatus
    {
        none,
        running,
        success,
        fail
    }
    private void NetworkingClient_EventReceived(EventData obj)
    {
        Debug.LogFormat("C25F28 obj code = {0}", obj.Code.ToString());
        var args = obj.GetData();
        switch((RaiseEvent)obj.Code)
        {
            case RaiseEvent.EFFECT_EXCUTE:
                {
                    Debug.Log("C25F28-01 RaiseEvent.EFFECT_EXCUTE");
                    // abstractEffect, selectTarget, targetType, targetID
                    var senderPlayerSide = args.senderPlayerSide as string;
                    var effect = args.abstractEffect as AbstractEffect;
                    var selectTarget = args.selectTarget as AbstractTarget;
                    var targetType = args.targetType as string;
                    var targetID = (int)args.targetID;
                    /*
                     * the target player be equal is "You" then:
                     * in the localside of sender: target is "You"
                     * in ther opponent' localside of sender: target must be "Opponent" because "You" inside the reciver mean "Opponent"
                     */
                    if(string.Compare(MatchManager.instance.LocalPlayer.side, senderPlayerSide, true) != 0)
                    {
                        Debug.LogFormat("C25F28-01-01 compare local side with sender side = {0}", string.Compare(MatchManager.instance.LocalPlayer.side, senderPlayerSide, true) != 0);
                        if(selectTarget != null)
                        {
                            Debug.LogFormat("C25F28-01-01-01 selectTarget is not null = {0}", selectTarget != null);
                            if(selectTarget is PlayerTarget selectPlayer)
                            {
                                Debug.LogFormat("C25F28-01-01-01-01 selectTarget is PlayerTarget = {0}", selectTarget is PlayerTarget);
                                Debug.LogFormat("C25F28-01-01-01-01 selectPlayer.side before swap = {0}", selectPlayer.side);
                                selectPlayer.side = selectPlayer.side == CardOwner.You ? CardOwner.Opponent : CardOwner.You;
                                Debug.LogFormat("C25F28-01-01-01-01 selectPlayer.side after swap = {0}", selectPlayer.side);
                            }
                            else if(selectTarget is CardTarget selectCard)
                            {
                                Debug.LogFormat("C25F28-01-01-01-02 selectTarget is CardTarget = {0}", selectTarget is CardTarget);
                                Debug.LogFormat("C25F28-01-01-01-02 selectCard.owner before swap = {0}", selectCard.owner);
                                selectCard.owner = selectCard.owner == CardOwner.You ? CardOwner.Opponent : CardOwner.You;
                                Debug.LogFormat("C25F28-01-01-01-02 selectCard.owner after swap = {0}", selectCard.owner);
                            }
                            else
                            {
                                Debug.LogFormat("C25F28-01-01-01-03 can not found type selectTarget");
                            }
                        }
                        else
                        {
                            Debug.LogFormat("C25F28-01-01-01 selectTarget is not null = {0}", selectTarget != null);
                        }
                    }
                    else
                    {
                        Debug.LogFormat("C25F28-01-02 compare local side with sender side = {0}", string.Compare(MatchManager.instance.LocalPlayer.side, senderPlayerSide, true) != 0);
                    }
                    StartCoroutine(EffectAction(effect, selectTarget, targetID));
                    break;
                }
            case RaiseEvent.EFFECT_UPDATE_STATUS:
                Debug.Log("C25F28-02 RaiseEvent.EFFECT_UPDATE_STATUS");
                Debug.LogFormat("C25F28-02 befor effect status change = {0}", this.status);
                this.status = args.status;
                Debug.LogFormat("C25F28-02 after effect status change = {0}", this.status);
                break;
        }
    }

    #region Condition
    /// <summary>
    /// play card without check condition
    /// </summary>
    /// <param name="register"></param>
    /// <returns></returns>
    public IEnumerator OnAfterSummon(object register)
    {
        Debug.LogFormat("C25F30 register = {0}", register);

        /*
         * if the register have been regis for event after summon excute effect
         * else (not regist) do not any thing
         */
        if(isObjectRegised(typeof(AfterSummon).Name, register, out var abstractData))
        {
            Debug.LogFormat("C25F30-01 register is registed for AfterSummon");
            status = EffectStatus.running;
            Debug.LogFormat("C25F30-01 effect status = {0}", status.ToString());
            if(register is CardBase cardBase)
            {
                Debug.LogFormat("C25F30-01-01 register is CardBase");
                /*
                  * card summoned be own by localPlayer
                  * then excute action and effect
                  * else wait until all effect have been run from the owner
                  */
                if(cardBase.CardPlayer == MatchManager.instance.LocalPlayer)
                {
                    Debug.LogFormat("C25F30-01-01-01 card summoned be own by localPlayer");
                    var Actions = abstractData.Actions; //get all action in after summon event
                    Debug.LogFormat("C25F30-01-01-01 Actions count = {0}", Actions.Count);
                    yield return StartCoroutine(ExecuteActions(register, Actions)); //execute all action
                }
                else
                {
                    Debug.LogFormat("C25F30-01-01-02 not card owner waiting effec status update, before effect status update = {0}", this.status);
                    yield return new WaitUntil(() => this.status != EffectStatus.running);
                    Debug.LogFormat("C25F30-01-01-02 after effect status update = {0}", this.status);

                }

                if(cardBase is SpellCard spellCard)
                {
                    Debug.LogFormat("C25F30-01-01-03 card summoned is SpellCard = {0}", cardBase is SpellCard);
                    spellCard.transform.SetParent(null);
                    spellCard.gameObject.SetActive(false); //destroy spell card after use
                }
            }
            else
            {
                Debug.LogFormat("C25F30-01-02 register is not CardBase");
                var Actions = abstractData.Actions; //get all action in after summon event
                Debug.LogFormat("C25F30-01-02 Actions count = {0}", Actions.Count);
                yield return StartCoroutine(ExecuteActions(register, Actions)); //execute all action
            }
        }
        else
        {
            Debug.LogFormat("C25F30-02 register is not registed for AfterSummon");
        }
        yield return null;
    }
    /// <summary>
    /// Check condition before play card
    /// </summary>
    /// <param name="register"></param>
    /// <param name="callBackAction"></param>
    /// <returns></returns>
    public IEnumerator OnBeforeSummon(object register, Action callBackAction)
    {
        /*
         * some thing onBefore summon action just running on local
         */
        Debug.LogFormat("C25F31 register = {0}", register);
        /*
         * if the register have been regis for event before summon (request for summon this monster) excute effect
         * else (not regist) summon monster without effect
         */
        if(isObjectRegised(typeof(BeforeSummon).Name, register, out var abstractData))
        {
            Debug.LogFormat("C25F31-01 register is registed for BeforeSummon");
            status = EffectStatus.running;
            Debug.LogFormat("C25F31-01 effect status = {0}", status.ToString());
            if(register is CardBase cardBase)
            {
                Debug.LogFormat("C25F31-01-01 register is CardBase = {0}", register is CardBase);
                /*
                 * card summoned be own by localPlayer
                 * then excute action and effect
                 * else wait until all effect have been run from the owner
                 */
                if(cardBase.CardPlayer == MatchManager.instance.LocalPlayer)
                {
                    Debug.LogFormat("C25F31-01-01-01 card summoned be own by localPlayer");
                    var Actions = abstractData.Actions; //get all action in after summon event
                    Debug.LogFormat("C25F31-01-01-01 Actions count = {0}", Actions.Count);
                    if(cardBase is MonsterCard monsterCard)
                    {
                        Debug.LogFormat("C25F31-01-01-01-01 card summoned is MonsterCard = {0}", cardBase is MonsterCard);
                        yield return StartCoroutine(ExecuteActions(monsterCard, Actions)); //excute all action
                    }
                    else if(cardBase is SpellCard)
                    {
                        Debug.LogFormat("C25F31-01-01-01-02 card summoned is SpellCard = {0}", cardBase is SpellCard);
                        if(CheckCardBeforPlay(cardBase))
                        {
                            Debug.LogFormat("C25F31-01-01-01-02-01 card can be play");
                            Debug.LogFormat("C25F31-01-01-01-02-01 status = {0}", status.ToString());
                            status = EffectStatus.success;
                            Debug.LogFormat("C25F31-01-01-01-02-01 status = {0}", status.ToString());

                        }
                        else
                        {
                            Debug.LogFormat("C25F31-01-01-01-02-02 card can not be play");
                            Debug.LogFormat("C25F31-01-01-01-02-02 status = {0}", status.ToString());
                            status = EffectStatus.fail;
                            Debug.LogFormat("C25F31-01-01-01-02-02 status = {0}", status.ToString());
                        }
                    }
                    else
                    {
                        Debug.LogFormat("C25F31-01-01-01-03 can not find type of cardbase, card type = {0}", cardBase.GetType().Name);
                    }
                }
                else
                {
                    Debug.LogFormat("C25F31-01-01-02 not card owner");
                }
                /*
                 * get the result effect execute
                 */
                Debug.LogFormat("C25F31-01-01 status = {0}", status.ToString());
                if(status == EffectStatus.success)
                {
                    Debug.LogFormat("C25F31-01-01-03 status = {0}, execute callback action", status.ToString());
                    callBackAction(); //summon the monster
                }
                else
                {
                    Debug.LogFormat("C25F31-01-01-04 status = {0}, can not summon the monster", status.ToString());
                }
            }
            else
            {
                Debug.LogFormat("C25F31-01-02 register is not CardBase");
                callBackAction(); //use the card 
            }
        }
        else
        {
            Debug.LogFormat("C25F31-02 register is not registed for BeforeSummon");
            /*
             * if the register have not been regis for event before summon do as normal
             */
            callBackAction(); //summon the monster
        }
        yield return null;
    }



    public bool CheckCardBeforPlay(CardBase cardbase)
    {
        bool flag = true;
        if(cardbase is MonsterCard monster)
        {
            if(monster.LogicCard != null && monster.LogicCard.Length > 0)
            {
                for(int i = 0; i < monster.LogicCard.Length; i++)
                {

                    var conditon = monster.LogicCard[i];
                    if(conditon != null)
                    {
                        var actions = conditon.Actions;
                        if(actions != null && actions.Count > 0)
                        {
                            for(int j = 0; j < actions.Count; j++)
                            {

                                var action = actions[j];
                                if(action is SelectTarget selectTarget) //get each action and execute it, here is select card
                                {
                                    var path = SelectManager.Instance.GetGraphPlayerSelectTarget(selectTarget);
                                    flag = flag && path.Count > 0; //Check
                                }
                                else
                                {
                                    flag = false;
                                    Debug.LogError(this.debug("Action dose not available in CheckCardBeforPlayCallback()"));
                                }
                            }
                        }
                        else
                        {
                            flag = false;
                            Debug.LogWarning(this.debug("Action is null", new
                            {
                                index = i
                            }));
                        }
                    }
                    else
                    {
                        Debug.LogWarning(this.debug("Condition is null", new
                        {
                            index = i
                        }));
                        flag = false;
                    }
                }
            }
            else
            {
                flag = false;
                Debug.LogWarning(this.debug("Monster dose not contain any logic"));
            }
        }
        else if(cardbase is SpellCard spell)
        {
        }
        else
        {
            Debug.LogError(this.debug("Not card", new
            {
                cardbase.GetType().Name
            }));
            flag = false;

        }
        Debug.Log(this.debug("Return result after check", new
        {
            flag
        }));

        return flag;
    }

    private IEnumerator OnCardDamaged(MonsterCard monsterCard)
    {
        Debug.LogFormat("C25F32 monsterCard is = {0}", monsterCard);
        status = EffectStatus.running;
        Debug.LogFormat("C25F32 effect status = {0}", status.ToString());
        if(EventEffectDispatcher.ContainsKey(EventID.OnCardDamaged))
        {
            Debug.LogFormat("C25F32-01 EventEffectDispatcher contain key EventID.OnCardDamaged = {0}", EventEffectDispatcher.ContainsKey(EventID.OnCardDamaged));
            var datas = EventEffectDispatcher[EventID.OnCardDamaged];
            if(datas != null && datas.Count > 0)
            {
                Debug.LogFormat("C25F32-01-01 datas is not null = {0} datas count = {1}", datas != null, datas.Count);
                var effectData = datas.FirstOrDefault(Item =>
                {
                    if(Item.register is MonsterCard regisMonster)
                    {
                        return regisMonster == monsterCard;
                    }
                    print(this.debug("Not available for damaged"));
                    return false;
                }
                );

                if(effectData.register != null && effectData.Actions != null && effectData.Actions.Count > 0)
                {
                    Debug.LogFormat("C25F32-01-01-01 effectData is not null = {0}", effectData.register != null && effectData.Actions != null && effectData.lifetime != null && effectData.WhenDie != null);
                    yield return StartCoroutine(ExecuteActions(effectData.register, effectData.Actions));
                }
                else
                {
                    Debug.LogFormat("C25F32-01-01-02 effectData is not null = {0}", effectData.register != null && effectData.Actions != null && effectData.lifetime != null && effectData.WhenDie != null);
                    yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                }
            }
            else
            {
                Debug.LogFormat("C25F32-01-01 datas is not null = {0} datas count = {1}", datas != null, datas != null ? datas.Count : "null");
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
            }
        }
        else
        {
            Debug.LogFormat("C25F32-02 EventEffectDispatcher contain key EventID.OnCardDamaged = {0}", EventEffectDispatcher.ContainsKey(EventID.OnCardDamaged));
            yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
        }
        yield return null;
    }

    #endregion New Region

    #region Actions

    /// <summary>
    /// Check request to execute action of card
    /// </summary>
    /// <testID>#C25F1</testID>
    /// <param name="register"></param>
    /// <param name="have"></param>
    /// <returns></returns>
    public IEnumerator ActionHaveCondition(object register, Have have)
    {
        Debug.Log("C25F1");
        int number = have.number;
        print(this.debug());
        bool isHave = false;
        Action ExecuteEffectQ = () => { print(this.debug("Execute Effect")); };
        if(have != null)
        {
            Debug.LogFormat("C25F1-01");
            if(have.target != null)
            {
                Debug.LogFormat("C25F1-01-01");

                if(have.target is PlayerTarget targetPlayer)
                {
                    var players = targetPlayer.Execute(MatchManager.instance);
                    isHave = false;
                    Debug.LogFormat("C25F1-01-01-01 have.target is PlayerTarget = {0}, players count = {1}, isHave = {2}", have.target is PlayerTarget, players.Count, isHave);
                    switch(have.comepare)
                    {
                        case compareType.equal:
                            isHave = players.Count == number;
                            break;
                        case compareType.more:
                            isHave = players.Count > number;
                            break;

                        case compareType.moreEqual:
                            isHave = players.Count >= number;
                            break;

                        case compareType.less:
                            isHave = players.Count < number;
                            break;

                        case compareType.lessEqual:
                            isHave = players.Count <= number;
                            break;
                        default:
                            Debug.Log("Not found compare type");
                            break;
                    }
                    Debug.LogFormat("C25F1-01-01-01 after comepare players.Count = {0}, isHave = {1}", players.Count, isHave);

                    if(isHave ^ have._not)
                    {
                        Debug.LogFormat("C25F1-01-01-01-01");
                        yield return StartCoroutine(ExecuteActions(register, have.Actions));
                    }
                    else
                    {
                        Debug.LogFormat("C25F1-01-01-01-02");
                        yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                    }
                }
                else if(have.target is SpecifyCard specifyCard)
                {
                    var cards = specifyCard.Execute(MatchManager.instance);
                    //isHave = cards.Count > 0;
                    isHave = false;
                    Debug.LogFormat("C25F1-01-01-02 before comepare cards.Count = {0}, isHave = {1}", cards.Count, isHave);

                    switch(have.comepare)
                    {
                        case compareType.equal:
                            isHave = cards.Count == number;
                            break;
                        case compareType.more:
                            isHave = cards.Count > number;
                            break;

                        case compareType.moreEqual:
                            isHave = cards.Count >= number;
                            break;

                        case compareType.less:
                            isHave = cards.Count < number;
                            break;

                        case compareType.lessEqual:
                            isHave = cards.Count <= number;
                            break;
                        default:
                            Debug.Log("Not found compare type");
                            break;
                    }
                    Debug.LogFormat("C25F1-01-01-02 after comepare cards.Count = {0}, isHave = {1}", cards.Count, isHave);

                    print(this.debug("have action condition", new
                    {
                        cards.Count,
                        isHave,
                        result = isHave ^ have._not
                    }));
                    if(isHave ^ have._not)
                    {
                        Debug.LogFormat("C25F1-01-01-02-01");
                        yield return StartCoroutine(ExecuteActions(register, have.Actions));
                    }
                    else
                    {
                        Debug.LogFormat("C25F1-01-01-02-02");
                        yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                    }
                }
                else if(have.target is SpecifyCardPlayer specifyCardPlayer)
                {
                    Debug.LogFormat("C25F1-01-01-03");
                }
                else if(have.target is CardTarget targetCard)
                {
                    Debug.LogFormat("C25F1-01-01-04");
                    var cards = targetCard.Execute(MatchManager.instance);
                    isHave = false;
                    switch(have.comepare)
                    {
                        case compareType.equal:
                            isHave = cards.Count == number;
                            break;
                        case compareType.more:
                            isHave = cards.Count > number;
                            break;

                        case compareType.moreEqual:
                            isHave = cards.Count >= number;
                            break;

                        case compareType.less:
                            isHave = cards.Count < number;
                            break;

                        case compareType.lessEqual:
                            isHave = cards.Count <= number;
                            break;
                        default:
                            Debug.Log("Not found compare type");
                            break;
                    }
                    if(isHave ^ have._not)
                    {
                        yield return StartCoroutine(ExecuteActions(register, have.Actions));
                    }
                    else
                    {
                        Debug.LogError(this.debug("condition false"));
                        yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                    }
                }
                else
                {
                    Debug.LogFormat("C25F1-01-01-05");
                    print(this.debug("Can not find type target"));
                    yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                }
            }
            else
            {
                Debug.LogFormat("C25F1-01-02");
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
            }

        }
        else
        {
            Debug.LogFormat("C25F1-02");
        }
        yield return null;
    }

    private IEnumerator ActionSelectTarget(object register, SelectSelf self)
    {
        Debug.Log("C25F3");

        if(self.target is SelfCard selfCard)
        {
            Debug.LogFormat("C25F3-01 is SelfCard = {0}", self.target is SelfCard);

            if(register is CardBase card)
            {
                Debug.LogFormat("C25F3-01-01");

                yield return StartCoroutine(ExecuteEffects(register, self.Effects, new CardTarget
                {
                    owner = (CardOwner)card.CardOwner,
                    cardPosition = (CardPosition)card.Position,
                    Rarity = (Rarity)card.RarityCard,
                    region = (RegionCard)card.RegionCard

                }, card));
            }
            else
            {
                Debug.LogFormat("C25F3-01-02");
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
            }
        }
        else if(self.target is SelfCardPlayer selfCardPlayer)
        {
            Debug.LogFormat("C25F3-02");

            if(register is CardBase card)
            {
                Debug.LogFormat("C25F3-02-01");
                yield return StartCoroutine(ExecuteEffects(register, self.Effects, new PlayerTarget
                {
                    side = CardOwner.You
                }, card.CardPlayer));
            }
            else
            {
                Debug.LogFormat("C25F3-02-02");
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
            }
        }
        else
        {
            Debug.LogFormat("C25F3-03");
            yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
        }
    }

    private IEnumerator ActionSelectTarget(object register, SelectTarget selectTarget)
    {
        Debug.LogFormat("C25F4");

        if(selectTarget != null)
        {
            Debug.Log("C25F4-01");
            Action ExecuteEffectQ = () => { print(this.debug("Execute Effect")); };
            List<(List<AbstractEffect>, AbstractTarget, object)> result = new();
            yield return StartCoroutine(SelectManager.Instance.SelectTargets(selectTarget, result));

            if(result != null && result.Count > 0)
            {
                Debug.LogFormat("C25F4-01-01 result not null = {0},  count = {1}", result != null, result.Count);

                foreach((var effects, var absTarget, var target) in result)
                {
                    var index = result.IndexOf((effects, absTarget, target));
                    Debug.LogFormat("C25F4-01-01 result[{0}])", index);
                    if(absTarget is CardTarget)
                    {
                        Debug.LogFormat("C25F4-01-01-01 absTarget is CardTarget = {0}", absTarget is CardTarget);
                        ExecuteEffectQ += () =>
                        {
                            StartCoroutine(ExecuteEffects(register, effects, absTarget, (CardBase)target));
                        };
                    }
                    else if(absTarget is PlayerTarget)
                    {
                        Debug.LogFormat("C25F4-01-01-02 absTarget is PlayerTarget = {0}", absTarget is PlayerTarget);
                        ExecuteEffectQ += () =>
                        {
                            StartCoroutine(ExecuteEffects(register, effects, absTarget, (CardPlayer)target));
                        };
                    }
                    else if(absTarget is AnyTarget)
                    {
                        Debug.LogFormat("C25F4-01-01-03 absTarget is AnyTarget = {0}", absTarget is AnyTarget);
                        if(target is CardBase)
                        {
                            Debug.LogFormat("C25F4-01-01-03-01 target is CardBase = {0}", target is CardBase);

                            ExecuteEffectQ += () =>
                            {
                                StartCoroutine(ExecuteEffects(register, effects, absTarget, (CardBase)target));
                            };
                        }
                        else if(target is CardPlayer)
                        {
                            Debug.LogFormat("C25F4-01-01-03-02 target is CardPlayer = {0}", target is CardPlayer);
                            ExecuteEffectQ += () =>
                            {
                                StartCoroutine(ExecuteEffects(register, effects, absTarget, (CardPlayer)target));
                            };
                        }

                    }
                    else
                    {
                        Debug.LogFormat("C25F4-01-01-04");
                        yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                    }
                }
            }
            else
            {
                Debug.LogFormat("C25F4-01-02 result not null = {0},  count = {1}", result != null, result.Count);
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
            }

            if(ExecuteEffectQ != null)
            {
                Debug.LogFormat("C25F4-01-03");
                ExecuteEffectQ();
            }
            else
            {
                Debug.LogFormat("C25F4-01-04");
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
            }
        }
        else
        {
            Debug.Log("C25F4-02");
            yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
        }
        yield return null;
    }

    private IEnumerator ActionSelectTarget(object register, SelectStrongest strongest)
    {
        Debug.Log("C25F5");
        var strongestMonster = strongest.Execute(register, MatchManager.instance);

        if(strongestMonster != null)
        {
            Debug.LogFormat("C25F5-01 strongestMonster is not null = {0}", strongestMonster != null);

            yield return StartCoroutine(routine: ExecuteEffects(register, strongest.Effects, new CardTarget
            {
                owner = strongest.owner,
                cardPosition = strongest.cardPosition,
                Rarity = strongest.Rarity,
                region = strongest.region,
            }, (CardBase)strongestMonster));//excute effect
        }
        else
        {
            Debug.LogFormat("C25F5-02 strongestMonster is not null = {0}", strongestMonster != null);
            yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
        }
    }

    private IEnumerator ActionSelectTarget(object register, SelectMulti multiSelect)
    {
        Debug.Log("C25F6");
        Action ExecuteEffectQ = () => { print(this.debug("Execute Effect")); };

        if(multiSelect != null)
        {
            Debug.LogFormat("C25F6-01 multiSelect is not null = {0}", multiSelect != null);

            if(multiSelect.multiTargetType != null)
            {
                Debug.LogFormat("C25F6-01-01 multiSelect.multiTargetType is not null = {0}", multiSelect.multiTargetType != null);
                if(multiSelect.multiTargetType is MultiTargetPlayer multiTargetPlayer)
                {
                    Debug.LogFormat("C25F6-01-01-01 multiTargetType is MultiTargetPlayer = {0}", multiSelect.multiTargetType is MultiTargetPlayer);

                    var playersTarget = new PlayerTarget
                    {
                        side = CardOwner.Any
                    };
                    var players = playersTarget.Execute(MatchManager.instance);
                    if(players != null && players.Count > 0)
                    {
                        Debug.LogFormat("C25F6-01-01-01-01 players not null ={0}, players count = {1}", players != null, players.Count);
                        foreach(var player in players)
                        {
                            var index = players.IndexOf(player);
                            Debug.LogFormat("C25F6-01-01-01-01 players[{0}])", index);

                            ExecuteEffectQ += () =>
                            {
                                if(player == MatchManager.instance.LocalPlayer)
                                {
                                    Debug.LogFormat("C25F6-01-01-01-01-01 players[{0}] is local player = {1}", index, player == MatchManager.instance.LocalPlayer);

                                    StartCoroutine(ExecuteEffects(register, multiTargetPlayer.Effects, new PlayerTarget
                                    {
                                        side = CardOwner.You
                                    }, player));
                                }
                                else
                                {
                                    Debug.LogFormat("C25F6-01-01-01-01-02 players[{0}] is not local player = {1}", index, player == MatchManager.instance.LocalPlayer);
                                    StartCoroutine(ExecuteEffects(register, multiTargetPlayer.Effects, new PlayerTarget
                                    {
                                        side = CardOwner.Opponent
                                    }, player));
                                }
                            };
                        }
                    }
                    else
                    {
                        Debug.LogFormat("C25F6-01-01-01-02 players not null ={0}, players count = {1}", players != null, players.Count);
                        yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                    }
                }
                else if(multiSelect.multiTargetType is MultiTargetCard multiTargetCard)
                {
                    Debug.LogFormat("C25F6-01-01-02 multiTargetType is MultiTargetCard = {0}", multiSelect.multiTargetType is MultiTargetCard);

                    List<CardBase> cards = multiTargetCard.target.Execute(MatchManager.instance);
                    if(cards != null && cards.Count > 0)
                    {
                        Debug.LogFormat("C25F6-01-01-02-01 cards not null ={0}, cards count = {1}", cards != null, cards.Count);
                        //Select manager provide select action
                        foreach(var card in cards)
                        {
                            Debug.LogFormat("C25F6-01-01-02-01 cards[{0}])", cards.IndexOf(card));
                            ExecuteEffectQ += () =>
                            {
                                StartCoroutine(ExecuteEffects(register, multiTargetCard.Effects, multiTargetCard.target, card));
                            };
                        }

                    }
                    else
                    {
                        Debug.LogFormat("C25F6-01-01-02-02 cards not null ={0}, cards count = {1}", cards != null, cards.Count);
                        yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                    }

                }
                else
                {
                    Debug.LogFormat("C25F6-01-01-03 do not find type of multi select");
                    yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));

                }
                if(ExecuteEffectQ != null)
                {
                    Debug.LogFormat("C25F6-01-01-04");
                    ExecuteEffectQ();
                }
                else
                {
                    Debug.LogFormat("C25F6-01-01-05");
                    yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                }
            }
            else
            {
                Debug.LogFormat("C25F6-01-02 multiSelect.multiTargetType is not null = {0}", multiSelect.multiTargetType != null);
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
            }
        }
        else
        {
            Debug.LogFormat("C25F6-02 multiSelect is not null = {0}", multiSelect != null);
            yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
        }
        yield return null;
    }

    private IEnumerator ActionSpecify(object register, SpecifyAction specify)
    {
        Debug.Log("C25F8");
        if(specify != null)
        {
            Debug.LogFormat("C25F8-01 specify is not null = {0}", specify != null);

            Action ExecuteEffectQ = () => { print(this.debug("Execute Effect")); };
            if(specify.target is SpecifyCard cardSpecify)
            {
                Debug.LogFormat("C25F8-01-01 specify.target is SpecifyCard = {0}", specify.target is SpecifyCard);

                var cards = cardSpecify.Execute(MatchManager.instance);
                if(cards != null && cards.Count > 0)
                {
                    Debug.Log("C25F8-01-01-01");

                    foreach(var card in cards)
                    {
                        Debug.LogFormat("C25F8-01-01-01 cards[{0}]", cards.IndexOf(card));

                        ExecuteEffectQ += () =>
                        {
                            StartCoroutine(ExecuteEffects(register, specify.Effects,
                                new CardTarget
                                {
                                    cardPosition = (CardPosition)card.Position,
                                    owner = (CardOwner)card.CardOwner,
                                    Rarity = (Rarity)card.RarityCard,
                                    region = (RegionCard)card.RegionCard
                                },
                            (CardBase)card));
                        };
                    }
                }
                else
                {
                    Debug.Log("C25F8-01-01-02");
                    yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                }
            }
            else if(specify.target is SpecifyCardPlayer cardPlayerSpecify)
            {
                Debug.LogFormat("C25F8-01-02 specify.target is SpecifyCardPlayer = {0}", specify.target is SpecifyCardPlayer);

                var player = cardPlayerSpecify.Execute(MatchManager.instance);
                if(player != null)
                {
                    Debug.LogFormat("C25F8-01-02-01");
                    ExecuteEffectQ += () =>
                    {
                        StartCoroutine(ExecuteEffects(register, specify.Effects,
                            new PlayerTarget
                            {
                                side = cardPlayerSpecify.side
                            },
                        (CardPlayer)player));
                        ;
                    };
                }
                else
                {
                    Debug.LogFormat("C25F8-01-02-02");
                    yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                }
            }
            else
            {
                Debug.LogFormat("C25F8-01-03");
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
            }

            if(ExecuteEffectQ != null)
            {
                Debug.LogFormat("C25F8-01-04");

                ExecuteEffectQ();
            }
            else
            {
                Debug.LogFormat("C25F8-01-05");
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
            }

        }
        else
        {
            Debug.LogFormat("C25F8-02 specify is not null = {0}", specify != null);
        }
        yield return null;
    }

    #endregion New Region

    #region Effects

    public IEnumerator CreateCard(CreateCard createCard, object target)
    {
        Debug.Log("C25F12");
        if(createCard != null && target != null)
        {
            Debug.LogFormat("C25F12-01 createCard is not null = {0},target is not null = {1} ", createCard != null, target != null);

            if(target is CardPlayer player)
            {
                Debug.LogFormat("C25F12-01-01 target is CardPlayer = {0}", target is CardPlayer);
                if(player == MatchManager.instance.LocalPlayer)
                {
                    Debug.LogFormat("C25F12-01-01-01 player is LocalPlayer = {0}", player == MatchManager.instance.LocalPlayer);

                    if(createCard != null && target != null)
                    {
                        Debug.LogFormat("C25F12-01-01-01-01 createCard is not null = {0}, target is not null = {1}", createCard != null, target != null);

                        yield return new WaitUntil(() => createCard.GainEffect(target, this));
                    }
                    else
                    {
                        Debug.LogFormat("C25F12-01-01-01-02 createCard is not null = {0}, target is not null = {1}", createCard != null, target != null);
                    }
                }
                else
                {
                    Debug.LogFormat("C25F12-01-01-02 player is LocalPlayer = {0}", player == MatchManager.instance.LocalPlayer);
                }

                CardBase createdCard = null;

                if(player == MatchManager.instance.LocalPlayer)
                {
                    Debug.LogFormat("C25F12-01-01-03");
                    yield return new WaitUntil(() =>
                    {
                        createdCard = MatchManager.instance.LocalPlayer.initialCardPlace.Dequeue();
                        return createdCard != null;
                    }
                   );
                }
                else
                {
                    Debug.LogFormat("C25F12-01-01-04");

                    yield return new WaitUntil(() =>
                    {
                        createdCard = MatchManager.instance.OpponentPlayer.initialCardPlace.Dequeue();
                        return createdCard != null;
                    }
                   );
                }

                if(createdCard != null)
                {

                    CardPlayer CardOwner = player;

                    createdCard.Position = createCard.CardPosition;

                    Debug.LogFormat("C25F12-01-01-05 createdCard is not null = {0}, create at position = {1}", createdCard != null, createCard.CardPosition.ToString());


                    switch(createCard.CardPosition)
                    {
                        case CardPosition.Any:
                        case CardPosition.InDeck:
                            Debug.LogFormat("C25F12-01-01-05-01 Create card in deck");
                            createdCard.Parents = CardOwner.deck;
                            //becom children of deck
                            createdCard.transform.parent = CardOwner.deck.transform;

                            //add card to list card in deck
                            CardOwner.deck.Add(createdCard);
                            //set position
                            createdCard.transform.position = CardOwner.deck.PositionInitialCardInDeck;
                            break;
                        case CardPosition.InHand:
                            Debug.LogFormat("C25F12-01-01-05-02 Create card in hand");

                            createdCard.Parents = CardOwner.hand;
                            CardOwner.hand.Add(createdCard);

                            break;
                        case CardPosition.InFightField:
                            Debug.LogFormat("C25F12-01-01-05-03 Create card in fight field");
                            break;
                        case CardPosition.InSummonField:
                            {
                                Debug.LogFormat("C25F12-01-01-05-04 Create card in summon field");
                                yield return new WaitUntil(() => createdCard.IsReady);

                                if(createdCard != null && createdCard is MonsterCard monsterCard)
                                {
                                    Debug.LogFormat("C25F12-01-01-05-04-01 createdCard is not null = {0}, createdCard is MonsterCard = {1}", createdCard != null, createdCard is MonsterCard);
                                    SummonZone zone = CardOwner.summonZones.FirstOrDefault(zone => !zone.isFilled() && !zone.isSelected);

                                    if(zone != null)
                                    {
                                        Debug.LogFormat("C25F12-01-01-05-04-01-01 zone is not null = {0}", zone != null);
                                        zone.isSelected = true;
                                        zone.SetMonsterCard(monsterCard);
                                        yield return StartCoroutine(EffectManager.Instance.OnAfterSummon(monsterCard));
                                    }
                                    else
                                    {
                                        Debug.LogFormat("C25F12-01-01-05-04-01-01 zone is not null = {0}, destroy object ~ not enough zone", zone != null);
                                        createdCard.gameObject.SetActive(false);
                                    }
                                }
                                else
                                {
                                    Debug.LogFormat("C25F12-01-01-05-04-02 createdCard is not null = {0}, createdCard is MonsterCard = {1}", createdCard != null, createdCard is MonsterCard);
                                }
                            }
                            break;
                        case CardPosition.InGraveyard:
                            Debug.LogFormat("C25F12-01-01-05-05 Create card in summon field");

                            break;
                        case CardPosition.InTriggerSpellField:
                            {
                                Debug.LogFormat("C25F12-01-01-05-06 Create card in summon field");

                                yield return new WaitUntil(() => createdCard.IsReady);

                                if(createdCard != null && createdCard is SpellCard spellCard)
                                {
                                    Debug.LogFormat("C25F12-01-01-05-06-01 createdCard is not null = {0}, createdCard is SpellCard = {1}", createdCard != null, createdCard is SpellCard);

                                    TriggerSpell zone = CardOwner.spellZone;

                                    if(zone != null)
                                    {
                                        Debug.LogFormat("C25F12-01-01-05-06-01-01 zone is not null = {0}", zone != null);
                                        spellCard.Position = CardPosition.InTriggerSpellField;

                                        spellCard.RemoveCardFormParentPresent();
                                        spellCard.MoveCardIntoNewParent(zone.transform,true);

                                        zone.SpellCard = spellCard;
                                        yield return StartCoroutine(EffectManager.Instance.OnAfterSummon(spellCard));
                                        if(spellCard != null)
                                        {
                                            Debug.LogFormat("C25F12-01-01-05-06-01-01-01 spell card is not null = {0}", spellCard != null);
                                            spellCard.gameObject.SetActive(false);
                                        }
                                        else
                                        {
                                            Debug.LogFormat("C25F12-01-01-05-06-01-01-02 spell card is not null = {0}", spellCard != null);
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogFormat("C25F12-01-01-05-06-01-02 zone is not null = {0}, destroy object ~ not enough zone", zone != null);
                                        GameObject.Destroy(createdCard.gameObject);
                                    }
                                }
                                else
                                {
                                    Debug.LogFormat("C25F12-01-01-05-06-02 createdCard is not null = {0}, createdCard is SpellCard = {1}", createdCard != null, createdCard is SpellCard);
                                }
                            }
                            break;
                        default:
                            Debug.LogFormat("C25F12-01-01-05-07 not found position");
                            break;
                    }
                    SelectManager.Instance.CheckSelectAble(MatchManager.instance);
                }
                else
                {
                    Debug.LogFormat("C25F12-01-01-06 createdCard is not null = {0}", createdCard != null);
                }
            }
            else
            {
                Debug.LogFormat("C25F12-01-02 target is CardPlayer = {0}", target is CardPlayer);
            }
        }
        else
        {
            Debug.LogFormat("C25F12-02 createCard is not null = {0}, target is not null", createCard != null, target != null);
        }
        yield return null;
    }

    //event action
    private IEnumerator BuffStats(BuffStats buffstarts, MonsterCard target)
    {
        Debug.Log("C25F10");
        if(buffstarts != null)
        {
            Debug.LogFormat("C25F10-01 buffstarts is not null = {0}", buffstarts != null);
            if(target != null)
            {
                Debug.LogFormat("C25F10-01-01 target is not null = {0}", target != null);
                buffstarts.GainEffect(target, this);
            }
            else
            {
                Debug.LogFormat("C25F10-01-02 target is not null = {0}", target != null);
                print(this.debug("target null"));
            }
        }
        else
        {
            Debug.LogFormat("C25F10-02 buffstarts is not null = {0}", buffstarts != null);
        }
        yield return null;
    }

    private IEnumerator Dame(Dame dame, object target)
    {
        Debug.Log("C25F13");
        if(dame != null)
        {
            Debug.LogFormat("C25F13-01 dame is not null = {0}", dame != null);

            if(target != null)
            {
                Debug.LogFormat("C25F13-01-01 target is not null = {0}", target != null);

                if(target is CardPlayer player)
                {
                    player.hp.Number -= dame.number;
                    Debug.LogFormat("C25F13-01-01-01 target is CardPlayer = {0}, dame = {1}", target is CardPlayer, dame.number);
                }
                else if(target is MonsterCard card)
                {
                    Debug.LogFormat("C25F13-01-01-02 target is MonsterCard = {0}, dame = {1}", target is MonsterCard, dame.number);
                    card.Hp -= dame.number;
                }
                else
                {
                    Debug.LogFormat("C25F13-01-01-03 not found type of target");
                }
            }
            else
            {
                Debug.LogFormat("C25F13-01-02 target is not null = {0}", target != null);
            }
        }
        else
        {
            Debug.LogFormat("C25F13-02 dame is not null = {0}", dame != null);
        }
        yield return null;
    }

    private IEnumerator DestroyObject(DestroyObject destroy, object target)
    {
        Debug.Log("C25F14");
        if(destroy != null && target != null)
        {
            Debug.LogFormat("C25F14-01 destroy is not null = {0}, target is not null = {1}", destroy != null, target != null);

            if(target is MonsterCard monster)
            {
                Debug.LogFormat("C25F14-01-01 target is MonsterCard");
                destroy.GainEffect(monster, this);
            }
            else
            {
                Debug.LogFormat("C25F14-01-02 target is MonsterCard");
            }
        }
        else
        {
            Debug.LogFormat("C25F14-02 destroy is not null = {0}, target is not null = {1}", destroy != null, target != null);
        }
        yield return null;
    }

    private IEnumerator Gain(Gain gain, object target)
    {
        Debug.Log("C25F22");
        if(gain != null && target != null)
        {
            Debug.LogFormat("C25F22-01 gain is not null = {0}, target is not null = {1}", gain != null, target != null);
            if(target is MonsterCard monster)
            {
                Debug.LogFormat("C25F22-01-01 target is MonsterCard");
                gain.GainEffect(monster, this);
            }
            else
            {
                Debug.LogFormat("C25F22-01-02 target not a MonsterCard");
            }
        }
        else
        {
            Debug.LogFormat("C25F22-02 gain is not null = {0}, target is not null = {1}", gain != null, target != null);
        }
        yield return null;
    }
    private IEnumerator Heal(Heal heal, object target)
    {
        Debug.Log("C25F26");
        if(heal != null)
        {
            Debug.LogFormat("C25F26-01 heal is not null = {0}", heal != null);
            if(target != null)
            {
                Debug.LogFormat("C25F26-01-01 target is not null = {0}", target != null);
                if(target is CardPlayer player)
                {
                    Debug.LogFormat("C25F26-01-01-01 target is CardPlayer = {0}", target is CardPlayer);
                    Debug.LogFormat("C25F26-01-01-01 player hp before heal = {0}", player.hp.Number);
                    player.hp.Number += heal.number;
                    Debug.LogFormat("C25F26-01-01-01 player hp after heal = {0}", player.hp.Number);
                }
                else if(target is MonsterCard card)
                {
                    Debug.LogFormat("C25F26-01-01-02 target is MonsterCard = {0}", target is MonsterCard);
                    Debug.LogFormat("C25F26-01-01-02 card hp before heal = {0}", card.Hp);
                    card.Hp += heal.number;
                    Debug.LogFormat("C25F26-01-01-02 card hp after heal = {0}", card.Hp);
                }
                else
                {
                    Debug.LogFormat("C25F26-01-01-03 can not found type target");
                }
            }
            else
            {
                Debug.LogFormat("C25F26-01-02 target is not null = {0}", target != null);
            }
        }
        else
        {
            Debug.LogFormat("C25F26-02 heal is not null = {0}", heal != null);
        }

        yield return null;
    }

    #endregion New Region

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }
    public IEnumerator ExecuteActions(object register, List<AbstractAction> Actions)
    {
        Debug.Log("C25F17");
        Debug.LogFormat("C25F17 Action count = {0}", Actions.Count);
        foreach(var action in Actions)
        {
            Debug.LogFormat("C25F17 Actions[{0}]", Actions.IndexOf(action));

            if(action is SelectTarget selectTarget) //get each action and execute it, here is select card
            {
                Debug.LogFormat("C25F17-01 is SelectTarget Action = {0}", action is SelectTarget);
                yield return StartCoroutine(ActionSelectTarget(register, selectTarget));
            }
            else if(action is SelectMulti multiSelect)
            {
                Debug.LogFormat("C25F17-02 is SelectMulti Action = {0}", action is SelectMulti);
                yield return StartCoroutine(ActionSelectTarget(register, multiSelect));
            }
            else if(action is SelectSelf self)
            {
                Debug.LogFormat("C25F17-03 is SelectSelf Action = {0}", action is SelectSelf);
                yield return StartCoroutine(ActionSelectTarget(register, self));
            }
            else if(action is SelectStrongest strongest)
            {
                Debug.LogFormat("C25F17-04 is SelectStrongest Action = {0}", action is SelectStrongest);
                yield return StartCoroutine(ActionSelectTarget(register, strongest));
            }
            else if(action is SelectWeakness weakness)
            {
                Debug.LogFormat("C25F17-05 is SelectWeakness Action = {0}", action is SelectWeakness);
                yield return StartCoroutine(ActionSelectTarget(register, weakness));
            }
            else if(action is RegisterLocalEvent registerLocalEvent)
            {
                Debug.LogFormat("C25F17-06 is RegisterLocalEvent Action = {0}", action is RegisterLocalEvent);
                yield return StartCoroutine(ActionRegisterLocalEvent(register, registerLocalEvent));
            }
            else if(action is Have have)
            {
                Debug.LogFormat("C25F17-07 is Have Action = {0}", action is Have);
                yield return StartCoroutine(ActionHaveCondition(register, have));
            }
            else if(action is SpecifyAction specify)
            {
                Debug.LogFormat("C25F17-08 is SpecifyAction Action = {0}", action is SpecifyAction);
                yield return StartCoroutine(ActionSpecify(register, specify));
            }
            else
            {
                Debug.LogFormat("C25F17-09 is not found type of action");
                print(this.debug("Action dose not available"));
            }
        }
        /*
         * when player throw action have been execute all action then finish the effect process
         */
        if(this.status == EffectStatus.running)
        {
            Debug.LogFormat("C25F17-10 before update status this.status = {0}", this.status);
            yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.success));
            Debug.LogFormat("C25F17-10 after update status this.status = {0}", this.status);
        }
        else
        {
            Debug.LogFormat("C25F17-11 this.status = {0}", this.status);
        }
    }

    private IEnumerator ActionSelectTarget(object register, SelectWeakness weakness)
    {
        Debug.Log("C25F7");
        var strongestMonster = weakness.Execute(register, MatchManager.instance);

        if(strongestMonster != null)
        {
            Debug.LogFormat("C25F7-01 strongestMonster is not null = {0}", strongestMonster != null);
            yield return StartCoroutine(routine: ExecuteEffects(register, weakness.Effects, new CardTarget
            {
                owner = weakness.owner,
                cardPosition = weakness.cardPosition,
                Rarity = weakness.Rarity,
                region = weakness.region,
            }, (CardBase)strongestMonster));//excute effect
        }
        else
        {
            Debug.LogFormat("C25F7-02 strongestMonster is not null = {0}", strongestMonster != null);
            yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
        }
    }

    public IEnumerator ExecuteEffectEvent<T>(AbstractEffect AbstractEffect, AbstractTarget targetAbs, T target) where T : MonoBehaviourPun
    {
        Debug.Log("C25F18");
        var sender = MatchManager.instance.LocalPlayer.side;
        var effectType = AbstractEffect.GetType().Name;
        var effectData = JsonUtility.ToJson(AbstractEffect);

        var targetType = target.GetType().Name;
        var targetID = target.photonView.ViewID;

        var selectTargetObjectType = targetAbs.GetType().Name;
        var selectTargetObjectJson = JsonUtility.ToJson(targetAbs);
        object[] datas = new object[] { sender, effectType, effectData, targetType, targetID, selectTargetObjectType, selectTargetObjectJson };
        Debug.LogFormat("C25F18 datas count = {0}\n " +
            "sender= {1},\n effectType= {2},\n effectData= {3},\n targetType= {4},\n targetID= {5},\n selectTargetObjectType= {6},\n selectTargetObjectJson= {7}",
            datas.Length, sender, effectType, effectData, targetType, targetID, selectTargetObjectType, selectTargetObjectJson);

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        yield return new WaitUntil(() =>
        PhotonNetwork.RaiseEvent((byte)RaiseEvent.EFFECT_EXCUTE, datas, raiseEventOptions, SendOptions.SendUnreliable)
        );
    }

    public IEnumerator ExecuteEffects<T>(object register, List<AbstractEffect> effects, AbstractTarget TargetType, T targetForEffect) where T : MonoBehaviourPun
    {
        Debug.LogFormat("C25F19 effects count = {0}", effects.Count);
        foreach(var effect in effects)
        {

            int pitchCount = 0;
            Debug.LogFormat("C25F19 effects[{0}] = {1}, before change pitchCount = {2}", effects.IndexOf(effect), effect.GetType().Name, pitchCount);
            if(effect.pitch != null)
            {
                Debug.LogFormat("C25F19-01 effect pitch is not null = {0}, effect pitch count = {1}", effect.pitch != null, effect.pitch.Length);
                List<MonsterCard> cardList = new List<MonsterCard>();

                for(int i = 0; i < effect.pitch.Length; i++)
                {
                    Debug.LogFormat("C25F19-01 effect.pitch[{0}]", i);
                    pitchCount += effect.pitch[i].pitchType.GetPitch(register, MatchManager.instance);
                }
                Debug.LogFormat("C25F19-01 after change pitchCount = {0}", pitchCount);
            }

            do
            {
                Debug.LogFormat("C25F19 execute effect repeat = {0}", pitchCount);
                if(effect is BuffStats buffStats)
                {
                    Debug.LogFormat("C25F19-02 effect is BuffStarts");
                    yield return StartCoroutine(ExecuteEffectEvent(buffStats, TargetType, targetForEffect));
                }
                else if(effect is Dame dame)
                {
                    Debug.LogFormat("C25F19-03 effect is Dame");
                    yield return StartCoroutine(ExecuteEffectEvent(dame, TargetType, targetForEffect));
                }
                else if(effect is Gain gain)
                {
                    Debug.LogFormat("C25F19-04 effect is Gain");
                    yield return StartCoroutine(ExecuteEffectEvent(gain, TargetType, targetForEffect));
                }
                else if(effect is Heal heal)
                {
                    Debug.LogFormat("C25F19-05 effect is Heal");
                    yield return StartCoroutine(ExecuteEffectEvent(heal, TargetType, targetForEffect));
                }
                else if(effect is DestroyObject destroy)
                {
                    Debug.LogFormat("C25F19-06 effect is DestroyObject");
                    yield return StartCoroutine(ExecuteEffectEvent(destroy, TargetType, targetForEffect));
                }
                else if(effect is CreateCard createCard)
                {
                    Debug.LogFormat("C25F19-07 effect is CreateCard");
                    yield return StartCoroutine(ExecuteEffectEvent(createCard, TargetType, targetForEffect));
                }
                else if(effect is TempStore tempStore)
                {
                    Debug.LogFormat("C25F19-08 effect is TempStore");
                    yield return StartCoroutine(TempStoreAction(tempStore, register, targetForEffect, this));
                }
                else
                {
                    Debug.LogFormat("C25F19-09 Not found type of effect");
                }
            } while(--pitchCount > 0);
        }
        yield return null;
    }

    public IEnumerator None()
    {
        print(this.debug());
        yield return new WaitForSeconds(2.0f);
    }
    private IEnumerator ActionRegisterLocalEvent(object register, RegisterLocalEvent registerLocalEvent)
    {
        Debug.Log("C25F2");
        registerLocalEvent.Execute(register, this);
        yield return null;
    }
    private void Awake()
    {
        Debug.Log("C25F9");

        if(Instance != null && Instance != this)
        {
            Debug.Log("C25F9-01");
            UnityEngine.Debug.LogError("EffectManager have 2");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("C25F9-02");
            Instance = this;
        }
    }
    private bool compareCardEffectAttributes(IEffectAttributes obj1, IEffectAttributes obj2)
    {
        print(this.debug("compare Attribute", new
        {
            obj1_IsCharming = obj1.IsCharming,
            obj2_IsCharming = obj2.IsCharming,
            obj1_IsTreating = obj1.IsTreating,
            obj2_IsTreating = obj2.IsTreating,
            obj1_IsDominating = obj1.IsDominating,
            obj2_IsDominating = obj2.IsDominating,
            obj1_IsBlockAttack = obj1.IsBlockAttack,
            obj2_IsBlockAttack = obj2.IsBlockAttack,
            obj1_IsBlockDefend = obj1.IsBlockDefend,
            obj2_IsBlockDefend = obj2.IsBlockDefend
        }));
        return
        obj1.IsCharming == obj2.IsCharming &&
        obj1.IsTreating == obj2.IsTreating &&
        obj1.IsDominating == obj2.IsDominating &&
        obj1.IsBlockAttack == obj2.IsBlockAttack &&
        obj1.IsBlockDefend == obj2.IsBlockDefend;
    }
    private IEnumerator EffectAction(AbstractEffect effect, AbstractTarget targetAbs, int targetPhotonID)
    {
        Debug.Log("C25F15");
        if(effect != null)
        {
            Debug.LogFormat("C25F15-01 effect is not null = {0}", effect != null);

            /*
             * get the target with the select object data and target ID
             */
            if(effect is BuffStats buffstarts)
            {
                Debug.Log("C25F15-01-01 effect is BuffStats");
                yield return BuffStats(buffstarts, GetTargetCard(targetAbs as CardTarget, targetPhotonID) as MonsterCard);
            }
            else if(effect is Dame dame)
            {
                Debug.Log("C25F15-01-02 effect is Dame ");
                yield return Dame(dame, GetTargetObject(targetAbs, targetPhotonID));

            }
            else if(effect is Gain gain)
            {
                Debug.Log("C25F15-01-03 effect is Gain ");
                yield return Gain(gain, GetTargetObject(targetAbs, targetPhotonID));
            }
            else if(effect is Heal heal)
            {
                Debug.Log("C25F15-01-04 effect is Heal ");
                yield return Heal(heal, GetTargetObject(targetAbs, targetPhotonID));
            }
            else if(effect is DestroyObject destroy)
            {
                Debug.Log("C25F15-01-05 effect is DestroyObject ");
                yield return DestroyObject(destroy, GetTargetObject(targetAbs, targetPhotonID));
            }
            else if(effect is CreateCard createCard)
            {
                Debug.Log("C25F15-01-06 effect is CreateCard ");
                yield return CreateCard(createCard, GetTargetObject(targetAbs, targetPhotonID));
            }
            else
            {
                Debug.Log("C25F15-01-07 do not found type of effect");
            }
        }
        else
        {
            Debug.LogFormat("C25F15-02 effect is not null = {0}", effect != null);
        }

        yield return null;
    }

    public IEnumerator ExecuteOnEndRound(MatchManager matchManager)
    {
        Debug.Log("C25F20");
        if(EventEffectDispatcher.ContainsKey(EventID.OnEndRound))
        {
            Debug.LogFormat("C25F20-01 EventEffectDispatcher contain key EventID.OnEndround = {0}", EventEffectDispatcher.ContainsKey(EventID.OnEndRound));
            var datas = EventEffectDispatcher[EventID.OnEndRound];
            if(datas != null && datas.Count > 0)
            {
                Debug.LogFormat("C25F20-01-01 datas count = {0}", datas.Count);
                for(int i = datas.Count - 1; i >= 0; i--)
                {
                    status = EffectStatus.running;
                    var data = datas[i];
                    Debug.LogFormat("C25F20-01-01 datas[{0}], EffectManger status = {1}", i, status.ToString());
                    if(data.register != null && data.Actions != null && data.Actions.Count > 0)
                    {
                        Debug.LogFormat("C25F20-01-01-01 register not null = {0}, actions not null = {1}, actions count = {2}", data.register != null, data.Actions != null, data.Actions.Count);
                        if(data.register is CardBase cardBase)
                        {
                            Debug.LogFormat("C25F20-01-01-01-01 register is CardBase = {0}", data.register is CardBase);
                            if(cardBase.CardPlayer == MatchManager.instance.LocalPlayer)
                            {
                                Debug.LogFormat("C25F20-01-01-01-01-01 cardBase.CardPlayer == MatchManager.instance.LocalPlayer = {0}", cardBase.CardPlayer == MatchManager.instance.LocalPlayer);
                                if(cardBase.Position != CardPosition.InGraveyard)
                                {
                                    Debug.LogFormat("C25F20-01-01-01-01-01-01 cardBase.Position is not in CardPosition.InGraveyard = {0}", cardBase.Position != CardPosition.InGraveyard);
                                    yield return StartCoroutine(ExecuteActions(data.register, data.Actions));
                                    yield return StartCoroutine(RemoveIfOneTime(datas, data));
                                }
                                else
                                {
                                    Debug.LogFormat("C25F20-01-01-01-01-01-02 cardBase.Position is in CardPosition.InGraveyard = {0}", cardBase.Position == CardPosition.InGraveyard);
                                    yield return new WaitUntil(() => datas.Remove(data));
                                }
                            }
                            else
                            {
                                Debug.LogFormat("C25F20-01-01-01-01-02 cardBase.CardPlayer != MatchManager.instance.LocalPlayer = {0}", cardBase.CardPlayer != MatchManager.instance.LocalPlayer);
                                yield return new WaitUntil(() => this.status != EffectStatus.running);
                            }
                        }
                        else
                        {
                            Debug.LogFormat("C25F20-01-01-01-02 register is CardBase = {0}", data.register is CardBase);
                        }
                    }
                    else
                    {
                        Debug.LogFormat("C25F20-01-01-02 register not null = {0}, actions not null = {1}, actions count = {2}", data.register != null, data.Actions != null, data.Actions != null ? data.Actions.Count : "null");
                        yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                    }
                }
            }
            else
            {
                Debug.LogFormat("C25F20-01-02 datas count = {0}", datas != null ? datas.Count : "null");
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                EventEffectDispatcher.Remove(EventID.OnEndRound);
            }
        }
        else
        {
            Debug.LogFormat("C25F20-02 EventEffectDispatcher contain key EventID.OnEndround = {0}", EventEffectDispatcher.ContainsKey(EventID.OnEndRound));
        }
        yield return null;
    }

    private IEnumerator RemoveIfOneTime(List<(object register, List<AbstractAction> Actions, LifeTime lifetime, WhenDie WhenDie)> datas, (object register, List<AbstractAction> Actions, LifeTime lifetime, WhenDie WhenDie) data)
    {
        Debug.Log("C25F35");
        if(datas.Contains(data))
        {
            Debug.LogFormat("C25F35-01 data contains datas = {0}", datas.Contains(data));
            if(data.lifetime == LifeTime.OneTime)
            {
                Debug.LogFormat("C25F35-01-01 data.lifetime = {0}", data.lifetime.ToString());
                Debug.LogFormat("C25F35-01-01 datas count before remove = {0}", datas.Count);
                yield return new WaitUntil(() => datas.Remove(data));
                Debug.LogFormat("C25F35-01-01 datas count after remove = {0}", datas.Count);
            }
            else
            {
                Debug.LogFormat("C25F35-01-02 data.lifetime = {0}", data.lifetime.ToString());
            }
        }
        else
        {
            Debug.LogFormat("C25F35-02 data contains datas = {0}", datas.Contains(data));
        }
        yield return null;
    }

    private IEnumerator ExecuteOnStartRound(MatchManager matchManager)
    {
        status = EffectStatus.running;
        Debug.LogFormat("C25F21 effect status = {0}", status.ToString());
        if(EventEffectDispatcher.ContainsKey(EventID.OnStartRound))
        {
            Debug.LogFormat("C25F21-01 EventEffectDispatcher contain key EventID.OnStartRound = {0}", EventEffectDispatcher.ContainsKey(EventID.OnStartRound));
            var datas = EventEffectDispatcher[EventID.OnStartRound];
            if(datas != null && datas.Count > 0)
            {
                Debug.LogFormat("C25F21-01-01 datas is not null = {0},datas count = {1}", datas != null, datas.Count);
                for(int i = 0; i < datas.Count; i++)
                {
                    Debug.LogFormat("C25F21-01-01 datas[{0}]", i);
                    var data = datas[i];
                    if(data.WhenDie == WhenDie.RemoveEffect && data.register is MonsterCard card && card.Position == CardPosition.InGraveyard)
                    {
                        Debug.LogFormat("C25F21-01-01-01");
                    }
                    else
                    {
                        Debug.LogFormat("C25F21-01-01-02");
                        if(data.register != null && data.Actions != null && data.Actions.Count > 0)
                        {
                            Debug.LogFormat("C25F21-01-01-02-01 register not null = {0}, actions not null = {1}, actions count = {2}", data.register != null, data.Actions != null, data.Actions.Count);
                            yield return StartCoroutine(ExecuteActions(data.register, data.Actions));
                            if(data.lifetime == LifeTime.OneTime)
                            {
                                Debug.LogFormat("C25F21-01-01-02-01-01 lifetime is LifeTime.OneTime = {0}", data.lifetime == LifeTime.OneTime);
                                datas.Remove(data);
                            }
                            else
                            {
                                Debug.LogFormat("C25F21-01-01-02-01-02 lifetime is LifeTime.OneTime = {0}", data.lifetime == LifeTime.OneTime);
                            }
                        }
                        else
                        {
                            Debug.LogFormat("C25F21-01-01-02-02 register not null = {0}, actions not null = {1}, actions count = {2}", data.register != null, data.Actions != null, data.Actions != null ? data.Actions.Count : "null");
                            yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                        }
                    }
                }
            }
            else
            {
                Debug.LogFormat("C25F21-01-02 datas is not null = {0},datas count = {1}", datas != null, datas != null ? datas.Count : "null");
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                EventEffectDispatcher.Remove(EventID.OnStartRound);
            }
        }
        else
        {
            Debug.LogFormat("C25F21-02 EventEffectDispatcher contain key EventID.OnStartRound = {0}", EventEffectDispatcher.ContainsKey(EventID.OnStartRound));
        }
        yield return null;
    }

    private CardBase GetTargetCard(CardTarget selectCard, int targetPhotonID)
    {
        var cards = selectCard.Execute(MatchManager.instance);
        var target = cards.FirstOrDefault(card => card.photonView.ViewID == targetPhotonID);
        return target;
    }

    private object GetTargetObject(AbstractTarget selectTargetObject, int targetPhotonID)
    {
        Debug.Log("C25F24");
        object target = null;
        if(selectTargetObject is CardTarget selectTargetCard)
        {
            Debug.LogFormat("C25F24-01 selectTargetObject is CardTarget = {0}", selectTargetObject is CardTarget);
            target = GetTargetCard(selectTargetCard, targetPhotonID);
        }
        else if(selectTargetObject is PlayerTarget selectTargetPlayer)
        {
            Debug.LogFormat("C25F24-02 selectTargetObject is PlayerTarget = {0}", selectTargetObject is PlayerTarget);
            target = GetTargetPlayer(selectTargetPlayer);
        }
        else
        {
            Debug.LogFormat("C25F24-03 can not find type of selectTargetObject = {0}", selectTargetObject.GetType().Name);
        }
        return target;
    }

    private CardPlayer GetTargetPlayer(PlayerTarget selectTargetPlayer)
    {
        if(selectTargetPlayer.side == CardOwner.You)
            return MatchManager.instance.LocalPlayer;
        else if(selectTargetPlayer.side == CardOwner.Opponent)
        {
            return MatchManager.instance.OpponentPlayer;
        }
        else
        {
            print(this.debug("Can not find value of target Player", new
            {
                target = selectTargetPlayer.ToString()
            }));
        }
        return null;
    }
    private void Start()
    {
        //this.RegisterListener(EventID.OnEndRound, param => StartCoroutine(ExecuteOnEndRound(param as MatchManager)));
        this.RegisterListener(EventID.OnStartRound, param => StartCoroutine(ExecuteOnStartRound(param as MatchManager)));
        this.RegisterListener(EventID.OnCardDamaged, param => StartCoroutine(OnCardDamaged(param as MonsterCard)));
    }

    private IEnumerator TempStoreAction<T>(TempStore tempStore, object register, T target, EffectManager effectManager)
    {
        Debug.LogFormat("C25F37");
        if(register is MonoBehaviourPun pun)
        {
            Debug.LogFormat("C25F37-01 register is MonoBehaviourPun = {0}", register is MonoBehaviourPun);
            TempStore.AddToStore(pun.photonView.ViewID, target);
            if(tempStore.Actions != null)
            {
                Debug.LogFormat("C25F37-01-01 tempStore.Actions is not null = {0}", tempStore.Actions != null);
                yield return StartCoroutine(ExecuteActions(register, tempStore.Actions));
            }
            else
            {
                Debug.LogFormat("C25F37-01-02 tempStore.Actions is not null = {0}", tempStore.Actions != null);
            }
        }
        else
        {
            Debug.LogFormat("C25F37-02 register is MonoBehaviourPun = {0}", register is MonoBehaviourPun);
        }
        yield return null;
    }

    IEnumerator UpdateEffectStatusEvent(EffectStatus status)
    {
        object[] datas = new object[] { (int)status };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte)RaiseEvent.EFFECT_UPDATE_STATUS, datas, raiseEventOptions, SendOptions.SendUnreliable);

        yield return new WaitUntil(() => this.status == status);
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
        Debug.LogFormat("C25F16 @event = {0}, register = {1}", @event.GetType().Name, registor.GetType().Name);
        string key = @event.GetType().Name;
        List<Tuple<AbstractCondition, object>> list;

        if(ObjectRegisteds.TryGetValue(key, out list))
        {
            Debug.LogFormat("C25F16-01 key ={0}, list count = {1}", key, list.Count);
            // The key already exists, add to the list
            list.Add(new Tuple<AbstractCondition, object>(@event, registor));
            Debug.LogFormat("C25F16-01 list count after add = {0}", list.Count);

        }
        else
        {
            Debug.LogFormat("C25F16-02 key ={0}, list is null = {1}", key, list != null);

            // The key does not exist, create a new list
            list = new List<Tuple<AbstractCondition, object>>
            {
                new Tuple<AbstractCondition, object>(@event, registor)
            };
            Debug.LogFormat("C25F16-02 create new list, list is null = {0}", list != null);

            ObjectRegisteds.Add(key, list);
            Debug.LogFormat("C25F16-02 key = {0}, list count after add = {1}", key, list.Count);
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
        Debug.Log("C25F27");
        List<Tuple<AbstractCondition, object>> list;
        if(ObjectRegisteds.TryGetValue(@event, out list))
        {
            Debug.LogFormat("C25F27-01 key = {0}, list count = {1}", @event, list.Count);
            // The key exists, check the list
            var tuple = list.FirstOrDefault(t => t.Item1.GetType().Name == @event && t.Item2 == registor);
            if(tuple != null)
            {
                Debug.LogFormat("C25F27-01-01 tuple is not null = {0}, tuple.Item1 = {1}, tuple.Item2 = {2}", tuple != null, tuple.Item1.GetType().Name, tuple.Item2.GetType().Name);
                // Found a matching tuple, assign the output parameter to its Item1
                condition = tuple.Item1;
                return true;
            }
            else
            {
                Debug.LogFormat("C25F27-01-02 tuple is not null = {0}", tuple != null);
                // No matching tuple, assign the output parameter to null
                condition = null;
                return false;
            }
        }
        else
        {
            Debug.LogFormat("C25F27-02 key = {0}, list is null = {1}", @event, list != null);
            // The key does not exist, assign the output parameter to null
            condition = null;
            return false;
        }
    }

    public IEnumerator OnExecuteSpell(SpellCard spellCard)
    {
        status = EffectStatus.running;

        if(spellCard.CardPlayer == MatchManager.instance.LocalPlayer)
        {
            print(this.debug("player is the owner of card register for Execute spell"));

            foreach(var logic in spellCard.LogicCard)
            {
                var Actions = logic.Actions;

                yield return StartCoroutine(ExecuteActions(spellCard, Actions)); //execute all action
            }
        }
        else
        {
            print(this.debug("player is not the owner of card registed for Execute spell just watting"));
            yield return new WaitUntil(() => this.status != EffectStatus.running);
        }
        if(this.status == EffectStatus.success)
        {
            spellCard.transform.SetParent(null);
            spellCard.gameObject.SetActive(false); //destroy spell card after use
        }
        else
        {
            ReturnCardOnHand(spellCard);
        }
        yield return null;
    }

    private void ReturnCardOnHand(SpellCard spellCard)
    {
        Debug.Log(this.debug("ReturnCardOnHand"));
        spellCard.RemoveCardFormParentPresent();
        spellCard.MoveCardIntoNewParent(spellCard.CardPlayer.hand.transform,false);
        spellCard.CardPlayer.hand.Add(spellCard);
    }
}
#endregion
