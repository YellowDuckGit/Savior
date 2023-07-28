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
        var args = obj.GetData();
        switch((RaiseEvent)obj.Code)
        {
            case RaiseEvent.EFFECT_EXCUTE:

                {
                    // abstractEffect, selectTarget, targetType, targetID
                    var senderPlayerSide = args.senderPlayerSide as string;
                    var effect = args.abstractEffect as AbstractEffect;
                    var selectTarget = args.selectTarget as AbstractTarget;
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
                    if(string.Compare(MatchManager.instance.LocalPlayer.side, senderPlayerSide, true) != 0)
                    {
                        if(selectTarget != null)
                        {
                            if(selectTarget is PlayerTarget selectPlayer)
                            {
                                selectPlayer.side = selectPlayer.side == CardOwner.You ? CardOwner.Opponent : CardOwner.You;
                            }
                            else if(selectTarget is CardTarget selectCard)
                            {
                                selectCard.owner = selectCard.owner == CardOwner.You ? CardOwner.Opponent : CardOwner.You;
                            }
                        }
                    }

                    StartCoroutine(EffectAction(effect, selectTarget, targetID));
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
    }

    #region Condition
    /// <summary>
    /// play card without check condition
    /// </summary>
    /// <param name="register"></param>
    /// <returns></returns>
    public IEnumerator OnAfterSummon(object register)
    {
        print(this.debug("Status at start after summon", new
        {
            status
        }));


        print(this.debug("Start After summon", new
        {
            name = typeof(AfterSummon).Name,
            register = register.ToString()
        }));

        /*
         * if the register have been regis for event after summon excute effect
         * else (not regist) do not any thing
         */
        if(isObjectRegised(typeof(AfterSummon).Name, register, out var abstractData))
        {
            status = EffectStatus.running;
            if(register is CardBase cardBase)
            {
                print(this.debug("register is monster card"));
                /*
                  * card summoned be own by localPlayer
                  * then excute action and effect
                  * else wait until all effect have been run from the owner
                  */
                if(cardBase.CardPlayer == MatchManager.instance.LocalPlayer)
                {
                    print(this.debug("player is the owner of card register for after summon"));

                    var Actions = abstractData.Actions; //get all action in after summon event
                    print(this.debug("Object register", new
                    {
                        NumberAction = Actions.Count,
                    }));

                    yield return StartCoroutine(ExecuteActions(register, Actions)); //execute all action
                }
                else
                {
                    print(this.debug("player is not the owner of card registed for after summon just watting"));
                    yield return new WaitUntil(() => this.status != EffectStatus.running);
                    print(this.debug("Status at effect after summon done", new
                    {
                        status
                    }));
                }

                if(cardBase is SpellCard spellCard)
                {
                    spellCard.transform.SetParent(null);
                    spellCard.gameObject.SetActive(false); //destroy spell card after use
                }
            }//else not monster card then use without doing any effect
        } //else continue;
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
        print(this.debug("Status at start Before summon", new
        {
            status
        }));

        print(this.debug("Start Before summon", new
        {
            name = typeof(BeforeSummon).Name,
            register = register.ToString()
        }));
        /*
         * if the register have been regis for event before summon (request for summon this monster) excute effect
         * else (not regist) summon monster without effect
         */
        if(isObjectRegised(typeof(BeforeSummon).Name, register, out var abstractData))
        {
            status = EffectStatus.running;
            if(register is CardBase cardBase)
            {
                /*
                 * card summoned be own by localPlayer
                 * then excute action and effect
                 * else wait until all effect have been run from the owner
                 */
                if(cardBase.CardPlayer == MatchManager.instance.LocalPlayer)
                {
                    var Actions = abstractData.Actions; //get all action in after summon event
                    print(this.debug("Object registed", new
                    {
                        NumberAction = Actions.Count,
                    }));
                    if(cardBase is MonsterCard monsterCard)
                    {
                        yield return StartCoroutine(ExecuteActions(monsterCard, Actions)); //excute all action
                    }
                    else if(cardBase is SpellCard)
                    {
                        if(CheckCardBeforPlay(cardBase))
                            status = EffectStatus.success;
                        else
                            status = EffectStatus.fail;
                    }
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
                if(status == EffectStatus.success)
                {
                    print(this.debug("Before summon success summon monster)"));
                    callBackAction(); //summon the monster
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
                print(this.debug("target selected not be card"));
                callBackAction(); //use the card 
            }
        }
        else
        {
            print(this.debug("Object does not regis"));
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
                                    print(this.debug("is SelectCardAction Action", new
                                    {
                                        actionName = action.GetType().Name
                                    }));
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
        print(this.debug());
        status = EffectStatus.running;
        if(EventEffectDispatcher.ContainsKey(EventID.OnCardDamaged))
        {
            var datas = EventEffectDispatcher[EventID.OnCardDamaged];
            if(datas != null && datas.Count > 0)
            {
                var effectData = datas.FirstOrDefault(Item =>
                {
                    if(Item.register is MonsterCard regisMonster)
                    {
                        print(this.debug("Monster being damaged", new
                        {
                            result = regisMonster == monsterCard,
                            regis = regisMonster,
                            cardinput = monsterCard
                        }));
                        return regisMonster == monsterCard;
                    }
                    print(this.debug("Not available for damaged"));
                    return false;
                }
                );


                if(effectData.register != null && effectData.Actions != null && effectData.Actions.Count > 0)
                {
                    yield return StartCoroutine(ExecuteActions(effectData.register, effectData.Actions));
                }
                else
                {
                    print(this.debug("Data null OnCardDamaged ", new
                    {
                        register = effectData.register,
                    }));

                    print(this.debug("Data null OnCardDamaged ", new
                    {
                        ActionsCount = effectData.Actions
                    }));
                    yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                }
            }
            else
            {
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                print(this.debug("No register for OnCardDamaged ", new
                {
                    datas
                }));
            }
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
                    Debug.LogFormat("C25F1-01-01-01 before comepare players.Count = {0}, isHave = {1}", players.Count, isHave);
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
                    print(this.debug("Have Specify card", new
                    {
                        cards.Count
                    }));
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
                    print(this.debug("have action condition", new
                    {
                        cards.Count,
                        isHave,
                        result = isHave ^ have._not
                    }));
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
                print(this.debug("Target in have null"));
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
            }
        }
        else
        {
            Debug.LogFormat("C25F1-02");
            print(this.debug("SelectTarget be null", new
            {
                register
            }));
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
                                        spellCard.MoveCardIntoNewParent(zone.transform);

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
        if(gain != null && target != null)
        {
            if(target is MonsterCard monster)
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

    private IEnumerator Heal(Heal heal, object target)
    {
        print(this.debug());
        if(heal != null)
        {
            if(target != null)
            {
                if(target is CardPlayer player)
                {
                    player.hp.Number += heal.number;
                }
                else if(target is MonsterCard card)
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
        foreach(var action in Actions)
        {
            if(action is SelectTarget selectTarget) //get each action and execute it, here is select card
            {
                //TODO: fix
                print(this.debug("is SelectCardAction Action", new
                {
                    actionName = action.GetType().Name
                }));
                yield return StartCoroutine(ActionSelectTarget(register, selectTarget));
            }
            else if(action is SelectMulti multiSelect)
            {
                print(this.debug("is SelectCardAction Action", new
                {
                    actionName = action.GetType().Name
                }));
                yield return StartCoroutine(ActionSelectTarget(register, multiSelect));
            }
            else if(action is SelectSelf self)
            {
                print(this.debug("Self target Action", new
                {
                    actionName = action.GetType().Name,
                    register
                }));
                yield return StartCoroutine(ActionSelectTarget(register, self));
            }
            else if(action is SelectStrongest strongest)
            {
                print(this.debug("is SelectStrongest Action", new
                {
                    actionName = action.GetType().Name
                }));
                yield return StartCoroutine(ActionSelectTarget(register, strongest));
            }
            else if(action is SelectWeakness weakness)
            {
                print(this.debug("is SelectWeakness Action", new
                {
                    actionName = action.GetType().Name
                }));
                yield return StartCoroutine(ActionSelectTarget(register, weakness));
            }
            else if(action is RegisterLocalEvent registerLocalEvent)
            {
                print(this.debug("is RegisterLocalEvent Action", new
                {
                    actionName = action.GetType().Name
                }));
                yield return StartCoroutine(ActionRegisterLocalEvent(register, registerLocalEvent));
            }
            else if(action is Have have)
            {
                print(this.debug("is RegisterLocalEvent Action", new
                {
                    actionName = action.GetType().Name
                }));
                yield return StartCoroutine(ActionHaveCondition(register, have));
            }
            else if(action is SpecifyAction specify)
            {
                print(this.debug("is RegisterLocalEvent Action", new
                {
                    actionName = action.GetType().Name
                }));
                yield return StartCoroutine(ActionSpecify(register, specify));
            }
            else
            {
                print(this.debug("Action dose not available"));
            }
        }
        /*
         * when player throw action have been execute all action then finish the effect process
         */
        print(this.debug("End execute action", new
        {
            status = this.status.ToString()
        }));
        if(this.status == EffectStatus.running)
        {
            yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.success));
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
        print(this.debug("Excute an effect", new
        {
            AbstractEffect.GetType().Name
        }));
        var sender = MatchManager.instance.LocalPlayer.side;
        var effectType = AbstractEffect.GetType().Name;
        var effectData = JsonUtility.ToJson(AbstractEffect);

        var targetType = target.GetType().Name;
        var targetID = target.photonView.ViewID;

        var selectTargetObjectType = targetAbs.GetType().Name;
        var selectTargetObjectJson = JsonUtility.ToJson(targetAbs);
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
        yield return new WaitUntil(() =>
        PhotonNetwork.RaiseEvent((byte)RaiseEvent.EFFECT_EXCUTE, datas, raiseEventOptions, SendOptions.SendUnreliable)
        );

    }

    public IEnumerator ExecuteEffects<T>(object register, List<AbstractEffect> effects, AbstractTarget TargetType, T targetForEffect) where T : MonoBehaviourPun
    {
        foreach(var effect in effects)
        {
            int pitchCount = 0;
            if(effect.pitch != null)
            {
                List<MonsterCard> cardList = new List<MonsterCard>();

                for(int i = 0; i < effect.pitch.Length; i++)
                {
                    pitchCount += effect.pitch[i].pitchType.GetPitch(register, MatchManager.instance);
                }
            }
            print(this.debug("Pitch: ", new
            {
                pitchCount
            }));
            do
            {
                if(effect is BuffStats buffStats)
                {
                    yield return StartCoroutine(ExecuteEffectEvent(buffStats, TargetType, targetForEffect));
                }
                else if(effect is Dame dame)
                {
                    yield return StartCoroutine(ExecuteEffectEvent(dame, TargetType, targetForEffect));
                }
                else if(effect is Gain gain)
                {
                    yield return StartCoroutine(ExecuteEffectEvent(gain, TargetType, targetForEffect));
                }
                else if(effect is Heal heal)
                {
                    yield return StartCoroutine(ExecuteEffectEvent(heal, TargetType, targetForEffect));
                }
                else if(effect is DestroyObject destroy)
                {
                    yield return StartCoroutine(ExecuteEffectEvent(destroy, TargetType, targetForEffect));
                }
                else if(effect is CreateCard createCard)
                {
                    yield return StartCoroutine(ExecuteEffectEvent(createCard, TargetType, targetForEffect));
                    print(this.debug("CREATE CARD SUCCESS"));
                    //ExecuteEffectEvent(createCard, selectTargetObject, target);
                    //StartCoroutine(EffectAction(effect, selectTarget, targetType, targetID));
                }
                else if(effect is TempStore tempStore)
                {
                    yield return StartCoroutine(TempStoreAction(tempStore, register, targetForEffect, this));
                }
                else
                {
                    print(this.debug("Not found type of effect"));
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
        print(this.debug());

        if(EventEffectDispatcher.ContainsKey(EventID.OnEndRound))
        {
            var datas = EventEffectDispatcher[EventID.OnEndRound];
            if(datas != null && datas.Count > 0)
            {
                List<(object register, List<AbstractAction>, LifeTime, WhenDie)> removelist = new List<(object register, List<AbstractAction>, LifeTime, WhenDie)>();
                for(int i = datas.Count - 1; i >= 0; i--)
                {
                    status = EffectStatus.running;
                    var data = datas[i];
                    if(data.register != null && data.Actions != null && data.Actions.Count > 0)
                    {
                        if(data.register is CardBase cardBase)
                        {
                            if(cardBase.CardPlayer == MatchManager.instance.LocalPlayer)
                            {
                                if(cardBase.Position != CardPosition.InGraveyard)
                                {
                                    yield return StartCoroutine(ExecuteActions(data.register, data.Actions));

                                    yield return StartCoroutine(RemoveIfOneTime(datas, data));
                                }
                                else
                                {
                                    yield return new WaitUntil(() => datas.Remove(data));
                                }
                            }
                            else
                            {
                                print(this.debug("player is not the owner of card registed for after summon just watting"));
                                yield return new WaitUntil(() => this.status != EffectStatus.running);
                                print(this.debug("Status at effect after summon done", new
                                {
                                    status
                                }));
                            }
                        }
                    }
                    else
                    {

                        Debug.LogError(this.debug("Data null OnEndRound ", new
                        {
                            register = data.register,
                        }));

                        Debug.LogError(this.debug("Data null OnEndRound ", new
                        {
                            ActionsCount = data.Actions
                        }));
                        yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                    }
                }
            }
            else
            {
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                Debug.Log(this.debug("No register for OnEndRound ", new
                {
                    datas
                }));
                EventEffectDispatcher.Remove(EventID.OnEndRound);
            }
        }

        yield return null;
    }

    private IEnumerator RemoveIfOneTime(List<(object register, List<AbstractAction> Actions, LifeTime lifetime, WhenDie WhenDie)> datas, (object register, List<AbstractAction> Actions, LifeTime lifetime, WhenDie WhenDie) data)
    {
        if(datas.Contains(data))
        {
            if(data.lifetime == LifeTime.OneTime)
            {
                print(this.debug("remove on end round", new
                {
                    data.register,
                    data.Actions.Count
                }));
                yield return new WaitUntil(() => datas.Remove(data));
            }
        }
        else
        {
            Debug.LogError(this.debug("data not contains datas", new
            {
                data.register
            }));
        }
        yield return null;
    }

    private IEnumerator ExecuteOnStartRound(MatchManager matchManager)
    {
        print(this.debug());
        status = EffectStatus.running;
        if(EventEffectDispatcher.ContainsKey(EventID.OnStartRound))
        {
            var datas = EventEffectDispatcher[EventID.OnStartRound];
            if(datas != null && datas.Count > 0)
            {
                for(int i = 0; i < datas.Count; i++)
                {
                    var data = datas[i];
                    if(data.WhenDie == WhenDie.RemoveEffect && data.register is MonsterCard card && card.Position == CardPosition.InGraveyard)
                    {
                        print(this.debug("vong if xac dinh la pola vao hom"));
                    }
                    else
                    {
                        if(data.register != null && data.Actions != null && data.Actions.Count > 0)
                        {
                            yield return StartCoroutine(ExecuteActions(data.register, data.Actions));
                            if(data.lifetime == LifeTime.OneTime)
                            {
                                datas.Remove(data);
                            }
                        }
                        else
                        {

                            Debug.LogError(this.debug("Data null OnEndRound ", new
                            {
                                register = data.register,
                            }));

                            Debug.LogError(this.debug("Data null OnEndRound ", new
                            {
                                ActionsCount = data.Actions
                            }));
                            yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                        }
                    }
                }
            }
            else
            {
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                Debug.LogError(this.debug("No register for OnStartRound ", new
                {
                    datas
                }));
                EventEffectDispatcher.Remove(EventID.OnStartRound);

            }
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
        object target = null;
        if(selectTargetObject is CardTarget selectTargetCard)
        {
            target = GetTargetCard(selectTargetCard, targetPhotonID);
        }
        else if(selectTargetObject is PlayerTarget selectTargetPlayer)
        {
            target = GetTargetPlayer(selectTargetPlayer);
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
        if(register is MonoBehaviourPun pun)
        {
            print(this.debug("Need an pun object Id to store "));
            TempStore.AddToStore(pun.photonView.ViewID, target);
            if(tempStore.Actions != null)
            {
                yield return StartCoroutine(ExecuteActions(register, tempStore.Actions));
            }
            else
            {
                print(this.debug("Can not find actions"));
            }
        }
        else
        {
            print(this.debug("Need an pun object Id to store "));
        }
        yield return null;
    }

    IEnumerator UpdateEffectStatusEvent(EffectStatus status)
    {
        print(this.debug("send Update effect status to", new
        {
            status
        }));
        object[] datas = new object[] { (int)status };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte)RaiseEvent.EFFECT_UPDATE_STATUS, datas, raiseEventOptions, SendOptions.SendUnreliable);

        yield return new WaitUntil(() => this.status == status);
        print(this.debug("effect status update success", new
        {
            this.status
        }));
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
        if(ObjectRegisteds.TryGetValue(key, out list))
        {
            print(this.debug("Event registed, add registor", new
            {
                eventName = key,
                registor = registor.ToString()
            }));
            // The key already exists, add to the list
            list.Add(new Tuple<AbstractCondition, object>(@event, registor));
            print(this.debug("add into keys,", new
            {
                key,
                list.Count
            }));
        }
        else
        {
            print(this.debug("Event don't registed yet, create and add first registor", new
            {
                eventName = key,
                registor = registor.ToString()
            }));
            // The key does not exist, create a new list
            list = new List<Tuple<AbstractCondition, object>>
            {
                new Tuple<AbstractCondition, object>(@event, registor)
            };
            ObjectRegisteds.Add(key, list);
            print(this.debug("create keys,", new
            {
                key,
                list.Count
            }));
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
        print(this.debug("check object regis for event", new
        {
            registor = registor.ToString()
        }));
        List<Tuple<AbstractCondition, object>> list;
        if(ObjectRegisteds.TryGetValue(@event, out list))
        {
            // The key exists, check the list
            var tuple = list.FirstOrDefault(t => t.Item1.GetType().Name == @event && t.Item2 == registor);
            if(tuple != null)
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

    public IEnumerator OnExecuteSpell(SpellCard spellCard)
    {
        print(this.debug("Status at start OnExecuteSpell", new
        {
            status
        }));


        print(this.debug("Start Execute spell", new
        {
            name = typeof(AfterSummon).Name,
            register = spellCard.ToString()
        }));

        status = EffectStatus.running;

        if(spellCard.CardPlayer == MatchManager.instance.LocalPlayer)
        {
            print(this.debug("player is the owner of card register for Execute spell"));

            foreach(var logic in spellCard.LogicCard)
            {
                var Actions = logic.Actions; //get all action in after summon event
                                             //var Actions = spellCard.LogicCard.Actions; //get all action in after summon event
                print(this.debug("Object register", new
                {
                    NumberAction = Actions.Count,
                }));

                yield return StartCoroutine(ExecuteActions(spellCard, Actions)); //execute all action
            }
        }
        else
        {
            print(this.debug("player is not the owner of card registed for Execute spell just watting"));
            yield return new WaitUntil(() => this.status != EffectStatus.running);
            print(this.debug("Status at effect Execute spell done", new
            {
                status
            }));
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
        spellCard.MoveCardIntoNewParent(spellCard.CardPlayer.hand.transform);
        spellCard.CardPlayer.hand.Add(spellCard);
    }
}
#endregion
