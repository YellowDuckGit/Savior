

using Assets.GameComponent.Card.CardComponents.Script.UI;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EventGameHandler;

public class SummonZone : MonoBehaviourPun, IPunObservable
{
    [HideInInspector]
    private MonsterCard _monsterCard { get; set; } = default!;
    public MonsterCard monsterCard
    {
        get { return _monsterCard; }
        set
        {
            _monsterCard = value;
            //value.TriggerSummoned();
        }
    }

    [HideInInspector] public CardPlayer player;

    [HideInInspector] public MatchManager matchManager;

    [HideInInspector] public bool isFill;

    [HideInInspector] public bool isSelectable;

    public bool Selectable() => isSelectable;

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
        switch ((RaiseEvent)obj.Code)
        {
            case RaiseEvent.MOVE_SUMMONZONE:


                if (args.summonZoneID.Equals(photonView.ViewID))
                {
                    if (args.playerSide.Equals(K_Player.K_PlayerSide.Blue))
                    {
                        FightZone fightZone = matchManager.bluePlayer.fightZones.Single(a => a.photonView.ViewID.Equals(args.fightZoneID));
                        MonsterCard monsterCard = fightZone.monsterCard;
                        monsterCard.MoveToSummonZone(fightZone, this);
                    }
                    else if (args.playerSide.Equals(K_Player.K_PlayerSide.Red))
                    {
                        FightZone fightZone = matchManager.redPlayer.fightZones.Single(a => a.photonView.ViewID.Equals(args.fightZoneID));
                        MonsterCard monsterCard = fightZone.monsterCard;
                        monsterCard.MoveToSummonZone(fightZone, this);
                    }
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

        print(this.debug(cardselected != null ? $"summon card {cardselected.ToString()}" : "card selected be null", new { cardselected }));
        if (cardselected != null)
        {
            if (cardselected is MonsterCard monsterCard)
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

    public IEnumerator SummonCardInZoneEvent(MonsterCard monsterCardInHand)
    {
        print(this.debug());
        //summon if monsterCard is select not null and monster card enough condition summon: Mana
        if (monsterCardInHand != null && monsterCardInHand.isCanSummon(player.mana.Number))
        {
            print(this.debug($"can summon with enough mana {monsterCardInHand.ToString()}"));
            yield return StartCoroutine(EffectManager.Instance.OnBeforeSummon(monsterCardInHand, () =>
            {
                print(this.debug($"Summon {monsterCardInHand.ToString()}"));
                this.PostEvent(EventID.OnSummonMonster,
                       new SummonArgs
                       {
                           card = monsterCardInHand,
                           summonZone = this
                       }
                       );
            }));
        }
    }

    public void RaiseMoveCardFromAttackFieldToSummonField(FightZone fightZone, MonsterCard monsterCard)
    {
        object[] datas = new object[] { fightZone.photonView.ViewID, photonView.ViewID, monsterCard.photonView.ViewID, player.side };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte)RaiseEvent.MOVE_SUMMONZONE, datas, raiseEventOptions, SendOptions.SendUnreliable);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

    public override string ToString()
    {
        return $"SummonZone {photonView.ViewID} {(isFill ? $"filled by {_monsterCard.ToString()}" : "not any card inside")}";
    }
    // Start is called before the first frame update


}
