using Assets.GameComponent.Card.Logic.Actions.SelectAction.Select_Self;
using Assets.GameComponent.Card.Logic.ConditionTrigger.CardStatus;
using Assets.GameComponent.Card.Logic.TargetObject;
using Assets.GameComponent.Card.Logic.TargetObject.Target;
using Assets.GameComponent.Card.LogicCard;
using Assets.GameComponent.Card.LogicCard.ConditionTrigger.Summon;
using SerializeReferenceEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.GameComponent.Card.Logic.Actions.SelectAction.Select_Self.SelectSelf;
using static EventGameHandler;

namespace Assets.GameComponent.Card.Logic.RegisterLocalEvent
{
    public enum LifeTime
    {
        LifeTime, OneTime
    }
    [SRName("Logic/RegisterLocalEvent")]

    public class RegisterLocalEvent : AbstractAction
    {
        public EventID EventID;
        public LifeTime LifeTime;
        [SerializeReference]
        [SRLogicCard(typeof(SelectSelf), typeof(Have.Have))]

        public List<AbstractAction> Actions = null;


        public void Execute(object register, EffectManager instance)
        {
            Debug.Log(this.debug($"Regist Event for {register}", new
            {
                Event = EventID.ToString()
            }));
            if (!instance.EventEffectDispatcher.ContainsKey(EventID)) 
            {
                Debug.Log(this.debug($"Regist new key {EventID}", new
                {
                    register,
                    Actions.Count
                }));
                instance.EventEffectDispatcher.Add(EventID, new List<(object register, List<AbstractAction>, LifeTime)> {
                (register, Actions, LifeTime)
                });
            }
            else
            {
                Debug.Log(this.debug($"Regist add {register} to key {EventID}"));
                instance.EventEffectDispatcher[EventID].Add((register, Actions, LifeTime));
            }
        }


    }
}
