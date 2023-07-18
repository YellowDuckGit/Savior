using Assets.GameComponent.Card.CardComponents.Script;
using Assets.GameComponent.Card.Logic.Effect.CreateCard;
using Assets.GameComponent.Card.Logic.TargetObject.Target.PlayerTarget;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InitCardPlace : MonoBehaviour
{
    private Queue<CardBase> queue = new Queue<CardBase>();
    public void Enqueue(CardBase card)
    {
        card.Parents = this;
        card.transform.position = transform.position;
        queue.Enqueue(card);
    }

    public CardBase Dequeue()
    {
        if(queue.Count > 0)
        {
            return queue.Dequeue();
        }
        Debug.LogWarning("None card waiting create");
        return null;
    }

    public bool onReady()
    {
        return queue.Count > 0;
    }
}
