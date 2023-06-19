using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static EnumDefine;
using static MatchManager;

public class CardPlayer : MonoBehaviourPun, IPunObservable
{
    public Hand hand;
    public PlayerCamera camera;
    public Deck deck;
    public GameObject graveyard;
    public HP hp;
    public Mana mana;
    public string side;
    public GameTokken tokken;
    public List<SummonZone> summonZones = new List<SummonZone>(6);
    public List<FightZone> fightZones = new List<FightZone>(6);
    public TriggerSpell spellZone;
    public PlayerAction playerAction;
    public bool isSkipTurn { get; set; } = default!;
    public bool isAttackAvaliable { get; set; } = default!;

    public MatchManager matchManager;


    private void Awake()
    {
        StartCoroutine(SetupPlayer());
    }
    private void Start()
    {

    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                hand.clickLeftMouse();
            }
        }
    }
    public IEnumerator SetupPlayer()
    {
        print(this.debug());
        if (photonView.IsMine)
        {
            print(this.debug("Gain player side IsMine", new
            {
                MatchManager.instance.localPlayerSide
            }));
            if (MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Blue))
            {

                //set name
                name = "BluePlayer";
                side = K_Player.K_PlayerSide.Blue;

                //set parent local player
                this.gameObject.transform.parent = GameObject.Find(MatchManager.instance.blueSideGameObjectName).transform;
                matchManager = gameObject.GetComponentInParent<MatchManager>();
                matchManager.bluePlayer = this;
                summonZones = this.gameObject.transform.parent.GetComponentsInChildren<SummonZone>().ToList();
                fightZones = this.gameObject.transform.parent.GetComponentsInChildren<FightZone>().ToList();
                spellZone = this.gameObject.transform.parent.GetComponentInChildren<TriggerSpell>();
            }
            else if (MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Red))
            {
                //set name
                name = "RedPlayer";
                side = K_Player.K_PlayerSide.Red;

                //set parent local player
                this.gameObject.transform.parent = GameObject.Find(MatchManager.instance.redSideGameObjectName).transform;
                matchManager = gameObject.GetComponentInParent<MatchManager>();
                matchManager.redPlayer = this;
                summonZones = this.gameObject.transform.parent.GetComponentsInChildren<SummonZone>().ToList();
                fightZones = this.gameObject.transform.parent.GetComponentsInChildren<FightZone>().ToList();
                spellZone = this.gameObject.transform.parent.GetComponentInChildren<TriggerSpell>();

            }


        }
        else //set parent orther player
        {
            print(this.debug("Gain player side not IsMine", new
            {
                MatchManager.instance.localPlayerSide
            }));
            if (MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Blue))
            {
                name = "RedPlayer";
                side = K_Player.K_PlayerSide.Red;

                this.gameObject.transform.parent = GameObject.Find(MatchManager.instance.redSideGameObjectName).transform;
                matchManager = gameObject.GetComponentInParent<MatchManager>();
                matchManager.redPlayer = this;
                summonZones = this.gameObject.transform.parent.GetComponentsInChildren<SummonZone>().ToList();
                fightZones = this.gameObject.transform.parent.GetComponentsInChildren<FightZone>().ToList();
                spellZone = this.gameObject.transform.parent.GetComponentInChildren<TriggerSpell>();

            }
            else if (MatchManager.instance.localPlayerSide.Equals(K_Player.K_PlayerSide.Red))
            {
                name = "BluePlayer";
                side = K_Player.K_PlayerSide.Blue;

                this.gameObject.transform.parent = GameObject.Find(MatchManager.instance.blueSideGameObjectName).transform;
                matchManager = gameObject.GetComponentInParent<MatchManager>();
                matchManager.bluePlayer = this;
                summonZones = this.gameObject.transform.parent.GetComponentsInChildren<SummonZone>().ToList();
                fightZones = this.gameObject.transform.parent.GetComponentsInChildren<FightZone>().ToList();
                spellZone = this.gameObject.transform.parent.GetComponentInChildren<TriggerSpell>();

            }
        }
        print(this.debug(null, new
        {
            this.side,
            this.name,
        }));
        yield return null;
        //gen deck

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
    //public List<MonsterCard> GetAllCard()
    //{
    //    var cardHand = hand._cards;
    //    var cardInSummonZone = summonZones.Where(zone => zone.monsterCard != null).Select(x => x.monsterCard).ToList();
    //    var cardInFightZone = fightZones.Where(zone => zone.monsterCard != null).Select(x => x.monsterCard).ToList();

    //    var allCard = cardHand.Union(cardInSummonZone).Union(cardInFightZone).ToList();
    //    return allCard;
    //}
    //public object getCard(int cardID)
    //{
    //    var allCard = GetAllCard();
    //    if (allCard.Count == 0)
    //    {
    //        return null;
    //    }
    //    print(this.debug("all card current player: ", new
    //    {
    //        count = allCard.Count
    //    }));
    //    print(this.debug($"get card ID from {side}: ", new
    //    {
    //        CardID = cardID
    //    }));
    //    allCard.ForEach(card =>
    //    {
    //        if (card != null)
    //        {
    //            print(this.debug($"card ID: ", new
    //            {
    //                card.photonView.ViewID
    //            }));
    //        }
    //        else
    //        {
    //            print(
    //                this.debug($"card null", new { cardName = card.ToString() })
    //                );
    //        }
    //    });
    //    //print(this.debug(string.Join("\n", allCard.Select(card => card.photonView.ViewID).ToList())));
    //    var cardSelected = allCard.First(card => card.photonView.ViewID == cardID);
    //    print(this.debug("Card Selected: ", new
    //    {
    //        cardSelected = cardSelected != null ? "not null" : "card null"
    //    }));
    //    if (cardSelected != null)
    //        return cardSelected;
    //    else return null;
    //}
}

