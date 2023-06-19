using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FightZone : MonoBehaviourPun, IPunObservable
{
    public enum FieldType
    {
        Attack, Defense
    }

    [HideInInspector] public FieldType type;

    public Renderer Icon;
    public Material AttackIconMaterial;
    public Material DefenseIconMaterial;

    [HideInInspector]
    public bool isSelect;

    [HideInInspector] public MonsterCard monsterCard;

    [HideInInspector] public CardPlayer player;

    [HideInInspector] public MatchManager matchManager;

    private void Start()
    {
        setIconField();
    }

    public void setIconField()
    {
        if (type == FieldType.Attack)
        {
            Icon.material = AttackIconMaterial;
        }
        else
        {
            Icon.material = DefenseIconMaterial;
        }
    }

    public void SwitchActionField(FieldType type)
    {
        this.type = type;
        setIconField();
    }

    #region Raise Event
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
        if ((RaiseEvent)obj.Code == RaiseEvent.MOVE_FIGHTZONE)
        {
            object[] datas = (object[])obj.CustomData;
            int fightZoneID = (int)datas[0];
            int summonZoneID = (int)datas[1];
            int cardID = (int)datas[2];
            string playerSide = (string)datas[3];

            if (fightZoneID.Equals(photonView.ViewID))
            {
                print("REventReceived MOVE_FIGHTZONE");

                //CardTarget cardTarget = player.hand.getMonsterCardsInHand().Find(a => a.photonView.ViewID.Equals(cardID)).cardTarget;
                SummonZone summonZone = null;
                if (playerSide.Equals(K_Player.K_PlayerSide.Blue))
                {
                    summonZone = matchManager.bluePlayer.summonZones.Single(a => a.photonView.ViewID.Equals(summonZoneID));
                }
                else if (playerSide.Equals(K_Player.K_PlayerSide.Red))
                {
                    summonZone = matchManager.redPlayer.summonZones.Single(a => a.photonView.ViewID.Equals(summonZoneID));
                }

                monsterCard = summonZone.monsterCard;
                if (monsterCard != null)
                {
                    monsterCard.MoveToFightZone(this);
                    this.PostEvent(EventID.OnMoveCardToFightZone, playerSide);
                }
            }
        }
    }
    #endregion

    private void OnMouseDown()
    {
        Debug.Log("Select Fight Field");
        bool A = matchManager.isRightToAttack(player.side); //player who have attack tokken
        bool B = matchManager.gamePhase == MatchManager.GamePhase.Attack; //In attack phase
        bool C = matchManager.isRightToDefense(player.side); //player who have defense tokken
        if (C)
        {
            var PlayerAttack = matchManager.GetAttackPlayer();
            var AttackFieldFillByMonster = PlayerAttack.fightZones.Where(zone => zone.monsterCard != null).ToList();
            C = C && AttackFieldFillByMonster.Count > 0; //Is defense and have monster attack
        }
        print(this.debug("Player click on fight zone", new
        {
            A,
            B,
            C,
            result = (A && !B && !C) || (C && B)
        }));
        /*
         * Case 1: Player Attack who have an attack tokken, do not attack yet
         * Case 2: Player Defense who have an defense tokken and he in attack phase (blueTriggerAttack|redTriggerAttack is true)
         */
        if ((A && !B && !C) || (C && B))
        {
            print("is Right To Attack");
            MonsterCard monsterCard = null;
            SummonZone summonZoneSelected = null;
            List<SummonZone> summonZones = new List<SummonZone>();

            //get single monster card is select by hand
            if (player.side.Equals(K_Player.K_PlayerSide.Blue))
            {
                if (matchManager.bluePlayer.summonZones.Any(a => a.monsterCard != null))
                {
                    print("1");
                    summonZones = matchManager.bluePlayer.summonZones.FindAll(a => a.monsterCard != null);
                    print("2");

                    if (summonZones.Count > 0)
                    {
                        print("3");
                        foreach (SummonZone monsterZone in summonZones)
                        {
                            print("4");

                            if (monsterZone.monsterCard.IsSelected)
                            {
                                print(this.debug($"Fight zone process for {monsterZone.monsterCard}", new { monsterZone.monsterCard.IsCharming }));
                                if ((player.tokken == EnumDefine.GameTokken.Attack && !monsterZone.monsterCard.IsCharming) || player.tokken == EnumDefine.GameTokken.Defend)
                                {
                                    print("5");
                                    monsterCard = monsterZone.monsterCard;
                                    summonZoneSelected = monsterZone;
                                }
                                else
                                {
                                    print("6");
                                    print(this.debug("charming count", new { monsterZone.monsterCard.charmingCount }));
                                    monsterZone.monsterCard.IsSelected = false;
                                }
                                break;
                            }
                            else
                            {
                                print("No monster selected");
                            }
                        }
                    }
                }
                else
                {
                    print("No exist any monster in fightzone");
                }

            }
            else if (player.side.Equals(K_Player.K_PlayerSide.Red))
            {
                if (matchManager.redPlayer.summonZones.Any(a => a.monsterCard != null))
                {
                    print("1");
                    summonZones = matchManager.redPlayer.summonZones.FindAll(a => a.monsterCard != null);
                    print("2");

                    if (summonZones.Count > 0)
                    {
                        print("3");
                        foreach (SummonZone monsterZone in summonZones)
                        {
                            print("4");

                            if (monsterZone.monsterCard.IsSelected)
                            {
                                print(this.debug($"Fight zone process for n{monsterZone.monsterCard}", new { monsterZone.monsterCard.IsCharming }));
                                if ((player.tokken == EnumDefine.GameTokken.Attack && !monsterZone.monsterCard.IsCharming) || player.tokken == EnumDefine.GameTokken.Defend)
                                {
                                    print("5");
                                    monsterCard = monsterZone.monsterCard;
                                    summonZoneSelected = monsterZone;
                                }
                                else
                                {
                                    print("6");
                                    monsterZone.monsterCard.IsSelected = false;
                                }
                                break;
                            }
                            else
                            {
                                print("No monster selected");
                            }
                        }
                    }
                }
                else
                {
                    print("No exist any monster in fightzone");
                }
            }

            if (monsterCard != null && summonZoneSelected != null)
            {
                print("RaiseEvent.MOVE_FIGHTZONE");
                // ID zone, ID cardTarget
                object[] datas = new object[] { photonView.ViewID, summonZoneSelected.photonView.ViewID, monsterCard.photonView.ViewID, player.side };
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                PhotonNetwork.RaiseEvent((byte)RaiseEvent.MOVE_FIGHTZONE, datas, raiseEventOptions, SendOptions.SendUnreliable);
            }
        }
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
    // Start is called before the first frame update
}
