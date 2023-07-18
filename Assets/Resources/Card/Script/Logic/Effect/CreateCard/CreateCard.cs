using Assets.GameComponent.Card.CardComponents.Script;
using Assets.GameComponent.Card.LogicCard;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using SerializeReferenceEditor;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static EventGameHandler;

namespace Assets.GameComponent.Card.Logic.Effect.CreateCard
{
    [SRName("Logic/Effect/CreateCard")]
    public class CreateCard : AbstractEffect
    {
        public ScriptableObject CardTarget;
        public CardOwner owner;
        public CardPosition CardPosition;
        public CardBase cardCreated
        {
            get;  set;
        }
        public override bool GainEffect(object register, EffectManager match)
        {
            string CardID;
            int PhotonViewID;
            CardBase Card = null;

            Debug.Log($"Run CreateCard in {MatchManager.instance.localPlayerSide}");
            if(CardTarget is MonsterData monsterData)
            {
                /*
                * Raise Event for 2 player create same data for card
                */
                Debug.Log(MatchManager.instance.debug("Create Monster Card Object"));
                Card = PhotonNetwork.Instantiate("MonsterCard_Prefab", Vector3.zero, Quaternion.identity).GetComponent<MonsterCard>(); //create a monster without data
                object[] datas = new object[] { monsterData.Id, Card.photonView.ViewID };
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                PhotonNetwork.RaiseEvent((byte)RaiseEvent.SET_DATA_CARD_EVENT, datas, raiseEventOptions, SendOptions.SendUnreliable);
            }
            else if(CardTarget is SpellData spellData)
            {
                /*
                * Raise Event for 2 player create same data for card
                */
                Debug.Log(MatchManager.instance.debug("Create Spell Card Object"));
                Card = PhotonNetwork.Instantiate("SpellCard_Prefab", Vector3.zero, Quaternion.identity).GetComponent<SpellCard>(); //create a monster without data
                object[] datas = new object[] { spellData.Id, Card.photonView.ViewID };

                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                PhotonNetwork.RaiseEvent((byte)RaiseEvent.SET_DATA_CARD_EVENT, datas, raiseEventOptions, SendOptions.SendUnreliable);
            }
            else
            {
                Debug.Log("Not found type of card data in create card");
                Debug.Log("Card Target Type: " + CardTarget.GetType().Name);
            }

            cardCreated = Card;


            //// init
            //CardPlayer CardOwner = null;
            //Debug.Log($"CARD OWNER is {owner}");
            //switch(owner)
            //{
            //    case global::CardOwner.You:
            //    CardOwner = MatchManager.instance.LocalPlayer;
            //    break;

            //    case global::CardOwner.Opponent:
            //    CardOwner = MatchManager.instance.LocalPlayer == MatchManager.instance.redPlayer ? MatchManager.instance.bluePlayer : MatchManager.instance.redPlayer;
            //    ;
            //    break;
            //}
            //Debug.Log(MatchManager.instance.debug("Card Owner", new
            //{
            //    CardOwner.side
            //}));
            //Card.Position = CardPosition;
            //switch(CardPosition)
            //{
            //    case CardPosition.Any:
            //    break;
            //    case CardPosition.InDeck:

            //    //becom children of deck
            //    Card.transform.parent = CardOwner.deck.transform;

            //    //add card to list card in deck
            //    CardOwner.deck.Add(Card);
            //    //set position
            //    Card.transform.position = CardOwner.deck.PositionInitialCardInDeck;
            //    Card.transform.Rotate(new Vector3(180f, 0f, 0f));
            //    break;
            //    case CardPosition.InHand:
            //    CardOwner.hand.Add(Card);
            //    Card.gameObject.transform.parent = CardOwner.hand.gameObject.transform;
            //    CardOwner.hand.ScaleCardInHand();
            //    CardOwner.hand.SortPostionRotationCardInHand();
            //    break;
            //    case CardPosition.InFightField:
            //    break;
            //    case CardPosition.InSummonField:
            //    {
            //        Debug.Log(MatchManager.instance.debug("Create into summon field", new
            //        {
            //            Card
            //        }));
            //        if(Card != null)
            //        {
            //            Debug.Log("CREATE CARD ~ CHECK POSITION");
            //            SummonZone zone = CardOwner.summonZones.FirstOrDefault(zone => !zone.isFilled() && !zone.isSelected);
            //            Debug.Log(MatchManager.instance.debug("Zone to put card into", new
            //            {
            //                zone
            //            }));

            //            if(zone != null)
            //            {
            //                zone.isSelected = true;
            //                Debug.Log("zone target: " + zone.photonView.ViewID);
            //                Debug.Log(MatchManager.instance.debug("Start call to SummonCardEvent", new
            //                {
            //                    card = Card as MonsterCard,
            //                    summonZone = zone,
            //                    cardPosition = CardPosition.InDeck
            //                }));
            //                MatchManager.instance.SummonCardEvent(new SummonArgs
            //                {
            //                    card = Card as MonsterCard,
            //                    summonZone = zone,
            //                    cardPosition = CardPosition.InDeck,
            //                    isSpecialSummon = true
            //                });

            //                Debug.Log(MatchManager.instance.debug("End call to SummonCardEvent"));
            //                //MatchManager.instance.ExecuteSummonCardAction(zone, Card as MonsterCard);
            //            }
            //            else
            //            {
            //                Debug.Log("destroy object ~ not enough zone: " + Card.Name);
            //                GameObject.Destroy(Card.gameObject);
            //            }
            //        }
            //        else
            //        {
            //            Debug.Log("Card is NULL");
            //        }
            //    }
            //    break;
            //    case CardPosition.InGraveyard:
            //    break;
            //    case CardPosition.InTriggerSpellField:
            //    break;
            //    default:
            //    break;
            //}

            //if(CardPosition == CardPosition.InTriggerSpellField)
            //{
            //    Debug.Log(MatchManager.instance.debug("In trigger spell", new
            //    {
            //        Card
            //    }));

            //    if(Card != null && Card is SpellCard spellCard)
            //    {

            //        MatchManager.instance.MoveCardInTriggerSpellEvent(new MoveCardInTriggerSpellArgs
            //        {
            //            sender = this,
            //            card = spellCard,
            //            triggerSpell = spellCard.CardPlayer.spellZone
            //        });
            //    }
            //    else if(Card != null && Card is MonsterCard monsterCard)
            //    {
            //        //TODO
            //    }

            //}
            //    else if (CardPosition == CardPosition.InSummonField)
            //    {
            //        Debug.Log(MatchManager.instance.debug("Create into summon field", new { Card }));
            //        if (Card != null)
            //        {
            //            Debug.Log("CREATE CARD ~ CHECK POSITION");
            //            SummonZone zone = CardOwner.summonZones.FirstOrDefault(zone => !zone.isFilled() && !zone.isSelected);
            //            Debug.Log(MatchManager.instance.debug("Zone to put card into", new { zone }));

            //            if (zone != null)
            //            {
            //                zone.isSelected = true;
            //                Debug.Log("zone target: " + zone.photonView.ViewID);
            //                Debug.Log(MatchManager.instance.debug("Start call to SummonCardEvent", new
            //                {
            //                    card = Card as MonsterCard,
            //                    summonZone = zone,
            //                    cardPosition = CardPosition.InDeck
            //                }));
            //                MatchManager.instance.SummonCardEvent(new SummonArgs
            //                {
            //                    card = Card as MonsterCard,
            //                    summonZone = zone,
            //                    cardPosition = CardPosition.InDeck,
            //                    isSpecialSummon = true
            //                });

            //                Debug.Log(MatchManager.instance.debug("End call to SummonCardEvent"));
            //                //MatchManager.instance.ExecuteSummonCardAction(zone, Card as MonsterCard);
            //            }
            //            else
            //            {
            //                Debug.Log("destroy object ~ not enough zone: " + Card.Name);
            //                GameObject.Destroy(Card.gameObject);
            //            }
            //        }
            //        else
            //        {
            //            Debug.Log("Card is NULL");
            //        }
            //        //yield return StartCoroutine(EffectManager.Instance.OnBeforeSummon(monsterCardInHand, () =>
            //        //    {
            //        //        print(this.debug($"Summon {monsterCardInHand.ToString()}"));
            //        //        this.PostEvent(EventID.OnSummonMonster,
            //        //               new SummonArgs
            //        //               {
            //        //                   card = monsterCardInHand,
            //        //                   summonZone = this
            //        //               }
            //        //               );
            //        //    }));
            //    }
            return true;
        }

        public override void RevokeEffect(object register, MatchManager match)
        {
            throw new NotImplementedException();
        }
    }
}
