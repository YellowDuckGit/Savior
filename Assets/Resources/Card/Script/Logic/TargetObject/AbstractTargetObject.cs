using Assets.GameComponent.Card.Logic.TargetObject.Select;
using Assets.GameComponent.Card.LogicCard;
using SerializeReferenceEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.GameComponent.Card.Logic.TargetObject
{
    [Serializable]
    public abstract class AbstractTargetObject : AbstractData
    {

        public List<SelectCardItem> SelectTargetObjects;


        [Serializable]
        public class SelectCardItem
        {
            [SerializeReference]
            [SRLogicCard(typeof(AbstractSelectTargetObject))]
            public AbstractSelectTargetObject SelectTargetObject = null;

            [SerializeReference]
            [SRLogicCard(typeof(AbstractEffect))]
            public List<AbstractEffect> Effect = null;
        }
        //public abstract void doAction();
    }


    //[SRName("SelectFilter/Target/Player")]
    //public class SelectTargetPlayer : AbstractSelectTargetObject
    //{
    //    public enum PlayerTarget
    //    {
    //        You,
    //        Opponent
    //    }
    //    public PlayerTarget SelectTarget;
    //}

    //[SRName("SelectFilter/Target/Card")]
    //public class SelectTargetCard : SelectTargetObject
    //{
    //    public CardOwner owner;

    //    public CardPosition cardPosition;

    //    public CardType cardType;
    //    public bool isDamed;
    //}
}
