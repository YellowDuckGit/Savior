using Assets.GameComponent.Card.CardComponents.Script.UI;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EventGameHandler;

public class TriggerSpell : MonoBehaviourPun
{
    [HideInInspector]
    private SpellCard _spellCard { get; set; } = default!;
    public SpellCard SpellCard
    {
        get { return _spellCard; }
        set
        {
            _spellCard = value;
        }
    }

    [HideInInspector] public CardPlayer player;

    [HideInInspector] public MatchManager matchManager;

    [HideInInspector] public bool isFill;

    [HideInInspector] public bool isSelectable;

    public bool Selectable() => isSelectable;
    private void OnMouseDown()
    {
        print(this.debug(player != null ? "player ok" : "player not right"));
        print(this.debug(player.side != null ? "player side ok" : "player side not right"));
        print(this.debug(player.hand != null ? "player hand ok" : "player hand not right"));

        var cardselected = player.hand.GetAllCardInHand().SingleOrDefault(a => a.IsSelected == true);

        print(this.debug(cardselected != null ? $"summon card {cardselected.ToString()}" : "card selected be null", new { cardselected }));
        if (cardselected != null)
        {
            if (cardselected != null && cardselected is SpellCard spellCard)
            {
                print(this.debug("card selected is spell card, execute spell card", new
                {
                    spellCard
                }));

                //photon event throw here
                StartCoroutine(MoveCardInTriggerSpellEvent(spellCard));
            }
            else
            {
                print(this.debug("Only card spell can be used in spell zone"));
            }
        }
    }

    private IEnumerator MoveCardInTriggerSpellEvent(SpellCard spellCard)
    {
        if (spellCard != null && spellCard.Cost <= matchManager.localPlayer.mana.Number)
        {
            print(this.debug($"can use spell {SpellCard}"));
            this.PostEvent(EventID.OnMoveCardInTriggerSpell, this);
        }
        yield return null;
    }

}