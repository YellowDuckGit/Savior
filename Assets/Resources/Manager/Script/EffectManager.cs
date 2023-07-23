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

    public IEnumerator OnBeforeSummon(object register, Action summonAction)
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

                    yield return StartCoroutine(ExecuteActions(register, Actions)); //excute all action
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


    public IEnumerator ActionHaveCondition(object register, Have have)
    {
        int number = have.number;
        print(this.debug());
        bool isHave = false;
        Action ExecuteEffectQ = () => { print(this.debug("Execute Effect")); };
        if(have != null)
        {
            if(have.target != null)
            {

                if(have.target is PlayerTarget targetPlayer)
                {
                    var players = targetPlayer.Execute(MatchManager.instance);
                    isHave = false;
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
                    if(isHave ^ have._not)
                    {
                        yield return StartCoroutine(ExecuteActions(register, have.Actions));
                    }
                    else
                    {
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
                else if(have.target is SpecifyCardPlayer specifyCardPlayer)
                {

                }
                else if(have.target is CardTarget targetCard)
                {
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
                    print(this.debug("Can not find type target"));
                    yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                }
            }
            else
            {
                print(this.debug("Target in have null"));
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
            }
        }
        else
        {
            print(this.debug("SelectTarget be null", new
            {
                register
            }));
        }

        yield return null;
    }

    private IEnumerator ActionSelectTarget(object register, SelectSelf self)
    {

        if(self.target is SelfCard selfCard)
        {
            if(register is CardBase card)
            {
                print(this.debug("Self Action", new
                {
                    card.CardOwner,
                    card.Position,
                    card.RarityCard,
                    card.RegionCard
                }));

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
                print(this.debug("Can not get cardbase"));
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
            }
        }
        else if(self.target is SelfCardPlayer selfCardPlayer)
        {
            if(register is CardBase card)
            {
                yield return StartCoroutine(ExecuteEffects(register, self.Effects, new PlayerTarget
                {
                    side = CardOwner.You
                }, card.CardPlayer));
            }
            else
            {
                print(this.debug("Can not get cardbase"));
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
            }
        }
        else
        {
            print(this.debug("Not found type target", new
            {
                type = self.target.GetType().Name
            }));
            yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
        }

    }

    private IEnumerator ActionSelectTarget(object register, SelectTarget selectTarget)
    {
        print(this.debug());

        if(selectTarget != null)
        {
            Action ExecuteEffectQ = () => { print(this.debug("Execute Effect")); };
            List<(List<AbstractEffect>, AbstractTarget, object)> result = new();
            yield return StartCoroutine(SelectManager.Instance.SelectTargets(selectTarget, result));

            print(this.debug("Player selectted:", new
            {
                result
            }));
            print(this.debug("Player selectted:", new
            {
                result.Count
            }));
            if(result != null && result.Count > 0)
            {
                foreach((var effects, var absTarget, var target) in result)
                {
                    if(absTarget is CardTarget)
                    {
                        ExecuteEffectQ += () =>
                        {
                            StartCoroutine(ExecuteEffects(register, effects, absTarget, (CardBase)target));
                        };
                    }
                    else if(absTarget is PlayerTarget)
                    {
                        ExecuteEffectQ += () =>
                        {
                            StartCoroutine(ExecuteEffects(register, effects, absTarget, (CardPlayer)target));
                        };
                    }
                    else if(absTarget is AnyTarget)
                    {
                        if(target is CardBase)
                        {
                            ExecuteEffectQ += () =>
                            {
                                StartCoroutine(ExecuteEffects(register, effects, absTarget, (CardBase)target));
                            };
                        }
                        else if(target is CardPlayer)
                        {
                            ExecuteEffectQ += () =>
                            {
                                StartCoroutine(ExecuteEffects(register, effects, absTarget, (CardPlayer)target));
                            };
                        }

                    }
                    else
                    {
                        print(this.debug("Not set target for select Target"));
                        yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                    }
                }
            }
            else
            {
                print(this.debug("Not any target selected"));
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
            }

            if(ExecuteEffectQ != null)
            {
                ExecuteEffectQ();
            }
            else
            {
                print(this.debug("Effect null"));
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
            }
        }
        else
        {
            print(this.debug("SelectTarget be null", new
            {
                register
            }));
            yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
        }
        yield return null;
    }

    private IEnumerator ActionSelectTarget(object register, SelectStrongest strongest)
    {
        print(this.debug("SELECT STRONGEST"));
        var strongestMonster = strongest.Execute(register, MatchManager.instance);

        if(strongestMonster != null)
        {
            print(this.debug("EXECUTE STRONGEST EFF"));
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
            print(this.debug("not found object strongest"));
            yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
        }
    }

    private IEnumerator ActionSelectTarget(object register, SelectMulti multiSelect)
    {
        print(this.debug());
        Action ExecuteEffectQ = () => { print(this.debug("Execute Effect")); };

        if(multiSelect != null)
        {
            if(multiSelect.multiTargetType != null)
            {
                if(multiSelect.multiTargetType is MultiTargetPlayer multiTargetPlayer)
                {
                    var playersTarget = new PlayerTarget
                    {
                        side = CardOwner.Any
                    };
                    var players = playersTarget.Execute(MatchManager.instance);
                    if(players != null && players.Count > 0)
                    {
                        foreach(var player in players)
                        {
                            ExecuteEffectQ += () =>
                            {
                                if(player == MatchManager.instance.LocalPlayer)
                                {
                                    StartCoroutine(ExecuteEffects(register, multiTargetPlayer.Effects, new PlayerTarget
                                    {
                                        side = CardOwner.You
                                    }, player));
                                }
                                else
                                {
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
                        print(this.debug("Not any target selected"));
                        yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                    }
                }
                else if(multiSelect.multiTargetType is MultiTargetCard multiTargetCard)
                {
                    List<CardBase> cards = multiTargetCard.target.Execute(MatchManager.instance);
                    print(this.debug("Select Card Target", new
                    {
                        cards.Count
                    }));
                    if(cards != null && cards.Count > 0)
                    {
                        //Select manager provide select action
                        foreach(var card in cards)
                        {
                            ExecuteEffectQ += () =>
                            {
                                StartCoroutine(ExecuteEffects(register, multiTargetCard.Effects, multiTargetCard.target, card));
                            };
                        }

                    }
                    else
                    {
                        print(this.debug("Not any target selected"));
                        yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));

                    }

                }
                else
                {
                    print(this.debug("do not find type of multi select"));
                    yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));

                }
                if(ExecuteEffectQ != null)
                {
                    ExecuteEffectQ();
                }
                else
                {
                    print(this.debug("Effect null"));
                    yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                }
            }
            else
            {
                print(this.debug(" multi select type be null"));
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
            }
        }
        else
        {
            print(this.debug("SelectTarget be null", new
            {
                register
            }));
            yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));


        }
        yield return null;
    }

    private IEnumerator ActionSpecify(object register, SpecifyAction specify)
    {
        if(specify != null)
        {
            Action ExecuteEffectQ = () => { print(this.debug("Execute Effect")); };
            if(specify.target is SpecifyCard cardSpecify)
            {
                var cards = cardSpecify.Execute(MatchManager.instance);
                if(cards != null && cards.Count > 0)
                {
                    foreach(var card in cards)
                    {
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
                    print(this.debug("Not found any card"));
                    yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                }
            }
            else
            if(specify.target is SpecifyCardPlayer cardPlayerSpecify)
            {
                var player = cardPlayerSpecify.Execute(MatchManager.instance);
                if(player != null)
                {
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
                    print(this.debug("Not Found player"));
                    yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
                }
            }
            else
            {
                print(this.debug("Not set target for Specify Action"));
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
            }

            if(ExecuteEffectQ != null)
            {
                ExecuteEffectQ();
            }
            else
            {
                print(this.debug("Effect null"));
                yield return StartCoroutine(UpdateEffectStatusEvent(EffectStatus.fail));
            }

        }
        yield return null;
    }

    #endregion New Region

    #region Effects

    public IEnumerator CreateCard(CreateCard createCard, object target)
    {

        if(createCard != null && target != null)
        {
            print(this.debug("Create new card action", new
            {
                createCard.CardTarget,
                createCard.owner,
                createCard.CardPosition,
            }));
            if(target is CardPlayer player)
            {
                if(player == MatchManager.instance.LocalPlayer)
                {
                    print(this.debug("IE CreateCard", new
                    {
                        createCard,
                        target
                    }));

                    if(createCard != null && target != null)
                    {
                        print(this.debug("1", new
                        {
                            createCard,
                            target
                        }));

                        yield return new WaitUntil(() => createCard.GainEffect(target, this));
                    }
                    else
                    {
                        print(this.debug("create card not avaiable"));
                    }
                }

                CardBase createdCard = null;

                if(player == MatchManager.instance.LocalPlayer)
                {
                    yield return new WaitUntil(() =>
                    {
                        createdCard = MatchManager.instance.LocalPlayer.initialCardPlace.Dequeue();
                        return createdCard != null;
                    }
                   );
                }
                else
                {
                    yield return new WaitUntil(() =>
                    {
                        createdCard = MatchManager.instance.OpponentPlayer.initialCardPlace.Dequeue();
                        return createdCard != null;
                    }
                   );
                }

                if(createdCard != null)
                {
                    print(this.debug($"{target} Create new card {createdCard}", new
                    {
                        createCard.CardTarget,
                        createCard.owner,
                        createCard.CardPosition,
                    }));

                    CardPlayer CardOwner = player;

                    print(this.debug("Card Owner", new
                    {
                        createdCard,
                        createCard.CardPosition,
                        CardOwner.side,
                        player
                    }));
                    createdCard.Position = createCard.CardPosition;


                    switch(createCard.CardPosition)
                    {
                        case CardPosition.Any:
                        case CardPosition.InDeck:
                            createdCard.Parents = CardOwner.deck;
                            //becom children of deck
                            createdCard.transform.parent = CardOwner.deck.transform;

                            //add card to list card in deck
                            CardOwner.deck.Add(createdCard);
                            //set position
                            createdCard.transform.position = CardOwner.deck.PositionInitialCardInDeck;
                            break;
                        case CardPosition.InHand:
                            print(this.debug("check hand", new
                            {
                                createdCard
                            }));
                            createdCard.Parents = CardOwner.hand;
                            CardOwner.hand.Add(createdCard);
                            //createdCard.gameObject.transform.parent = CardOwner.hand.gameObject.transform;
                            //CardOwner.hand.ScaleCardInHand();
                            //CardOwner.hand.SortPostionRotationCardInHand();
                            break;
                        case CardPosition.InFightField:

                            break;
                        case CardPosition.InSummonField:
                            {
                                yield return new WaitUntil(() => createdCard.IsReady);
                                Debug.Log(MatchManager.instance.debug("Create into summon field", new
                                {
                                    createdCard
                                }));
                                if(createdCard != null && createdCard is MonsterCard monsterCard)
                                {
                                    Debug.Log("CREATE CARD ~ CHECK POSITION");
                                    SummonZone zone = CardOwner.summonZones.FirstOrDefault(zone => !zone.isFilled() && !zone.isSelected);
                                    Debug.Log(MatchManager.instance.debug("Zone to put card into", new
                                    {
                                        zone
                                    }));

                                    if(zone != null)
                                    {
                                        zone.isSelected = true;
                                        print(this.debug());
                                        zone.SetMonsterCard(monsterCard);
                                        yield return StartCoroutine(EffectManager.Instance.OnAfterSummon(monsterCard));
                                        Debug.Log(this.debug("End call to SummonCardEvent"));
                                    }
                                    else
                                    {
                                        Debug.LogWarning("destroy object ~ not enough zone: " + createdCard);
                                        createdCard.gameObject.SetActive(false);
                                    }
                                }
                                else
                                {
                                    Debug.LogError(this.debug("Card not valid", new
                                    {
                                        createdCard
                                    }));
                                }
                            }
                            break;
                        case CardPosition.InGraveyard:
                            break;
                        case CardPosition.InTriggerSpellField:
                            {
                                yield return new WaitUntil(() => createdCard.IsReady);
                                Debug.Log(MatchManager.instance.debug("Create into summon field", new
                                {
                                    createdCard
                                }));
                                if(createdCard != null && createdCard is SpellCard spellCard)
                                {
                                    Debug.Log("CREATE CARD ~ CHECK POSITION");
                                    TriggerSpell zone = CardOwner.spellZone;
                                    Debug.Log(MatchManager.instance.debug("Zone to put card into", new
                                    {
                                        zone
                                    }));

                                    if(zone != null)
                                    {
                                        //zone.isSelected = true;
                                        print(this.debug());
                                        //change card from hand to summon zone 
                                        spellCard.Position = CardPosition.InTriggerSpellField;

                                        spellCard.RemoveCardFormParentPresent();
                                        spellCard.MoveCardIntoNewParent(zone.transform);

                                        zone.SpellCard = spellCard;
                                        yield return StartCoroutine(EffectManager.Instance.OnAfterSummon(spellCard));
                                        if(spellCard != null)
                                        {
                                            spellCard.gameObject.SetActive(false);
                                            //Destroy(); //destroy spell card after use
                                        }
                                        //yield return StartCoroutine(MatchManager.instance.SummonCardAction(zone, monsterCard, true));

                                        //Debug.Log("zone target: " + zone.photonView.ViewID);
                                        //Debug.Log(MatchManager.instance.debug("Start call to SummonCardEvent", new
                                        //{
                                        //    card = createdCard as MonsterCard,
                                        //    summonZone = zone,
                                        //    cardPosition = CardPosition.InDeck
                                        //}));
                                        //MatchManager.instance.SummonCardEvent(new SummonArgs
                                        //{
                                        //    card = createdCard as MonsterCard,
                                        //    summonZone = zone,
                                        //    cardPosition = CardPosition.InDeck,
                                        //    isSpecialSummon = true
                                        //});

                                        Debug.Log(MatchManager.instance.debug("End call to Spell zone"));
                                        //MatchManager.instance.ExecuteSummonCardAction(zone, Card as MonsterCard);
                                    }
                                    else
                                    {
                                        Debug.LogWarning("destroy object ~ not enough zone: " + createdCard.Name);
                                        GameObject.Destroy(createdCard.gameObject);
                                    }
                                }
                                else
                                {
                                    Debug.Log("Card is NULL");
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    SelectManager.Instance.CheckSelectAble(MatchManager.instance);
                }
                else
                {
                    Debug.LogError(this.debug("None card created", new
                    {
                        target,
                        createCard.cardCreated,
                        createCard.CardTarget,
                        createCard.owner,
                        createCard.CardPosition,
                    }
                    ));
                }
            }
        }


        yield return null;
    }

    //event action
    private IEnumerator BuffStats(BuffStats buffstarts, MonsterCard target)
    {
        print(this.debug());
        if(buffstarts != null)
        {
            if(target != null)
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
        if(dame != null)
        {
            if(target != null)
            {
                if(target is CardPlayer player)
                {
                    player.hp.Number -= dame.number;
                }
                else if(target is MonsterCard card)
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

    private IEnumerator DestroyObject(DestroyObject destroy, object target)
    {
        if(destroy != null && target != null)
        {
            if(target is MonsterCard monster)
            {
                print(this.debug($"Gain effect for {monster}"));
                destroy.GainEffect(monster, this);

            }
        }
        else
        {
            print(this.debug("not avaiable"));
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
        print(this.debug("SELECT WEAKNESS"));
        var strongestMonster = weakness.Execute(register, MatchManager.instance);

        if(strongestMonster != null)
        {
            print(this.debug("EXECUTE STRONGEST EFF"));
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
            print(this.debug("not found object strongest"));
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
        registerLocalEvent.Execute(register, this);

        //if (register is CardBase cardBase)
        //{
        //    if (registerLocalEvent.EventID == EventID.OnEndRound)
        //    {
        //        //List<AbstractAction> abstractDatas = new List<AbstractAction>();
        //        //abstractDatas.Add(new SelectSelf { Effects = registerLocalEvent.Effects });
        //        //var abstracCondition = new End { Actions = abstractDatas };
        //        //EffectRegistor(abstracCondition, register);
        //    }
        //    if (registerLocalEvent.EventID == EventID.OnCardDamaged)
        //    {
        //        registerLocalEvent.RegistEvent(register);

        //        //List<AbstractAction> abstractDatas = new List<AbstractAction>();
        //        //abstractDatas.Add(new SelectSelf { Effects = registerLocalEvent.Effects });
        //        //var abstracCondition = new CardDamaged { Actions = abstractDatas };
        //        //EffectRegistor(abstracCondition, register);
        //    }
        //}
        yield return null;
    }
    private void Awake()
    {
        //DontDestroyOnLoad(gameObject);
        if(Instance != null && Instance != this)
        {
            UnityEngine.Debug.LogError("EffectManager have 2");
            Destroy(gameObject);
        }
        else
        {
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

        if(effect != null)
        {
            /*
             * get the target with the select object data and target ID
             */
            if(effect is BuffStats buffstarts)
            {
                yield return BuffStats(buffstarts, GetTargetCard(targetAbs as CardTarget, targetPhotonID) as MonsterCard);
            }
            else if(effect is Dame dame)
            {
                yield return Dame(dame, GetTargetObject(targetAbs, targetPhotonID));

            }
            else if(effect is Gain gain)
            {
                yield return Gain(gain, GetTargetObject(targetAbs, targetPhotonID));
            }
            else if(effect is Heal heal)
            {
                yield return Heal(heal, GetTargetObject(targetAbs, targetPhotonID));
            }
            else if(effect is DestroyObject destroy)
            {
                yield return DestroyObject(destroy, GetTargetObject(targetAbs, targetPhotonID));
            }
            else if(effect is CreateCard createCard)
            {
                yield return CreateCard(createCard, GetTargetObject(targetAbs, targetPhotonID));
            }
            else
            {
                print(this.debug("Not found type of effect", new
                {
                    effect.GetType().Name
                }));
            }
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


}
#endregion
