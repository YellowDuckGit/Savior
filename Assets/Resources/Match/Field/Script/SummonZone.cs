

using Assets.GameComponent.Card.CardComponents.Script.UI;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using UnityEngine;
using static EventGameHandler;

public class SummonZone : MonoBehaviourPun, IPunObservable
{
    public MonsterCard MonsterCard
    {
        get
        {
            return GetMonsterCard();
        }
    }

    [HideInInspector] public CardPlayer player;

    [HideInInspector] public MatchManager matchManager;

    [HideInInspector] public bool isSelected = false;

    #region Raise Event
    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }
    /// <summary>
    /// Event excute when receive event from other player
    /// </summary>
    /// <param name="obj"></param>
    private void NetworkingClient_EventReceived(EventData obj)
    {
        var args = obj.GetData();
        switch((RaiseEvent)obj.Code)
        {
            case RaiseEvent.MOVE_SUMMONZONE:
                if(photonView.ViewID.Equals(args.summonZoneID))
                {
                    if(args.playerSide.Equals(K_Player.K_PlayerSide.Blue))
                    {
                        FightZone fightZone = matchManager.bluePlayer.fightZones.FirstOrDefault(a => a.photonView.ViewID.Equals(args.fightZoneID));
                        if(fightZone != null)
                        {
                            MonsterCard monsterCard = fightZone.monsterCard;
                            if(monsterCard != null)
                            {
                                monsterCard.MoveToSummonZone(fightZone, this);
                                print(this.debug("MOVE_SUMMONZONE", new
                                {
                                    fightZone,
                                    monsterCard
                                }));
                            }
                            else
                            {
                                Debug.LogError(this.debug("monsterCard be null", new
                                {
                                    monsterFilled = monsterCard,
                                    summonZoneID = photonView.ViewID,
                                    fightZoneID = fightZone.photonView.ViewID
                                }));
                            }
                        }
                        else
                        {
                            Debug.LogError(this.debug("fightZone be null"));
                        }
                    }
                    else if(args.playerSide.Equals(K_Player.K_PlayerSide.Red))
                    {
                        FightZone fightZone = matchManager.redPlayer.fightZones.FirstOrDefault(a => a.photonView.ViewID.Equals(args.fightZoneID));
                        MonsterCard monsterCard = fightZone.monsterCard;

                        if(monsterCard != null)
                        {
                            monsterCard.MoveToSummonZone(fightZone, this);
                            print(this.debug("MOVE_SUMMONZONE", new
                            {
                                fightZone,
                                monsterCard
                            }));
                        }
                        else
                        {
                            Debug.LogError(this.debug("monsterCard be null", new
                            {
                                monsterFilled = monsterCard,
                                summonZoneID = photonView.ViewID,
                                fightZoneID = fightZone.photonView.ViewID
                            }));
                        }

                    }
                    else
                    {
                        Debug.LogError(this.debug("playerSide not right"));
                    }
                    isSelected = false;
                }
                break;
        }

    }
    #endregion
    public event OnSummon OnSummonEvent;
    private void OnMouseDown()
    {
        print(this.debug(player != null ? "player ok" : "player not right"));
        print(this.debug(player.side != null ? "player side ok" : "player side not right"));
        print(this.debug(player.hand != null ? "player hand ok" : "player hand not right"));

        var cardselected = player.hand.GetAllCardInHand().SingleOrDefault(a => a.IsSelected == true);

        print(this.debug(cardselected != null ? $"summon card {cardselected.ToString()}" : "card selected be null", new
        {
            cardselected
        }));
        if(cardselected != null)
        {
            if(cardselected is MonsterCard monsterCard)
            {
                //photon event throw here
                StartCoroutine(SummonCardInZoneEvent(monsterCard));
            }
            else
            {
                print(this.debug("Only card monster can be summon"));
            }
        }
    }

    public MonsterCard GetMonsterCard(bool remove = false)
    {

        if(isFilled())
        {

            MonsterCard monsterCard = this.gameObject.transform.GetChild(0).GetComponent<MonsterCard>();
            if(remove)
            {
                monsterCard.transform.SetParent(null);
            }
            return monsterCard;
        }
        return null;
    }

    public void SetMonsterCard(MonsterCard monsterCard)
    {
        if(!isFilled())
        {
            Debug.Log(this.debug("SummonZone is not filled", new
            {
                beingFill = monsterCard
            }));
            monsterCard.RemoveCardFormParentPresent();
            monsterCard.MoveCardIntoNewParent(transform);
            monsterCard.Position = CardPosition.InSummonField;
        }
        else
        {
            Debug.LogError(this.debug("SummonZone is filled", new
            {
                monsterFilled = monsterCard,
                monsterID = monsterCard.photonView.ViewID,
                summonZoneID = photonView.ViewID
            }));
        }
        isSelected = false;
    }

    public bool isFilled()
    {
        print(this.debug($"summon zone {name} children count: " + this.gameObject.transform.childCount));
        return this.gameObject.transform.childCount > 0;
    }
    public IEnumerator SummonCardInZoneEvent(MonsterCard monsterCardInHand)
    {
        print(this.debug());
        //summon if monsterCard is select not null and monster card enough condition summon: Mana
        if(monsterCardInHand != null && monsterCardInHand.isCanSummon(player.mana.Number))
        {
            print(this.debug($"can summon with enough mana {monsterCardInHand.ToString()}"));
            yield return StartCoroutine(EffectManager.Instance.OnBeforeSummon(monsterCardInHand, () =>
            {
                print(this.debug($"Summon {monsterCardInHand.ToString()}"));
                this.PostEvent(EventID.OnSummonMonster,
                       new SummonArgs
                       {
                           card = monsterCardInHand,
                           summonZone = this,
                           cardPosition = monsterCardInHand.Position,
                           isSpecialSummon = false
                       }
                       );
            }));
        }
    }

    public void RaiseMoveCardFromAttackFieldToSummonField(FightZone fightZone, MonsterCard monsterCard)
    {
        isSelected = true;
        Debug.Log(this.debug("Move card from attack field to summon field", new
        {
            fightZoneID = fightZone.photonView.ViewID,
            monsterID = monsterCard.photonView.ViewID,
            summonZoneID = photonView.ViewID
        }));
        object[] datas = new object[] { fightZone.photonView.ViewID, photonView.ViewID, monsterCard.photonView.ViewID, player.side };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte)RaiseEvent.MOVE_SUMMONZONE, datas, raiseEventOptions, SendOptions.SendUnreliable);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

    public override string ToString()
    {
        return $"SummonZone {photonView.ViewID} {(isFilled() ? $"filled by {MonsterCard.ToString()}" : "not any card inside")}";
    }
    // Start is called before the first frame update


}
