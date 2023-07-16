using Assets.GameComponent.Card.CardComponents.Script;
using Assets.GameComponent.Card.Logic.TargetObject.Target;
using Assets.GameComponent.Card.Logic.TargetObject.Target.CardTarget;
using Assets.GameComponent.Card.LogicCard;
using SerializeReferenceEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.GameComponent.Card.Logic.Actions.Specify.SpecifyAction.SpecifyType;
using static Assets.GameComponent.Card.Logic.TargetObject.Target.CardTarget.CardTarget;
using static Assets.GameComponent.Card.Logic.TargetObject.Target.PlayerTarget.PlayerTarget;

namespace Assets.GameComponent.Card.Logic.Actions.Specify
{
    [SRName("Logic/Specify")]
    public class SpecifyAction : AbstractAction
    {
        [Header("TYPE")]
        [SerializeReference]
        [SRLogicCard(typeof(SpecifyType))]
        public SpecifyType target = null;

        [SerializeReference]
        [SRLogicCard(typeof(AbstractEffect))]
        public List<AbstractEffect> Effects = null;

        public abstract class SpecifyType : AbstractTarget
        {
            [SRName("Specify/Type/Card")]
            public class SpecifyCard : SpecifyType
            {
                [Header("COMMON")]
                public CardOwner owner = CardOwner.You;
                public CardPosition cardPosition = CardPosition.Any;
                public Rarity Rarity = Rarity.Any;
                public RegionCard region = RegionCard.Any;

                public ScriptableObject target;

                public List<CardBase> Execute(MatchManager instance)
                {
                    var cardTarget = new CardTarget();
                    cardTarget.owner = owner;
                    cardTarget.cardPosition = cardPosition;
                    cardTarget.Rarity = Rarity;
                    cardTarget.region = region;
                    var cards = cardTarget.Execute(instance);
                    if (cards != null && cards.Count > 0)
                    {
                        if (target is MonsterData mdata)
                        {
                            cards.RemoveAll(card => card.Id != mdata.Id);
                        }
                        else
                        if (target is SpellData sdata)
                        {
                            cards.RemoveAll(card => card.Id != sdata.Id);
                        }
                        else
                        {
                            Debug.Log(this.debug("Not found type of target card specify"));
                        }
                    }
                    else
                    {
                        Debug.Log(this.debug("Can not find type of target in card specify"));
                        return new List<CardBase>();
                    }
                    return cards;
                }
            }

            [SRName("Specify/Type/Player")]
            public class SpecifyCardPlayer : SpecifyType
            {
                [Header("COMMON")]
                public CardOwner side;
                public CardPlayer Execute(MatchManager instance)
                {
                    CardPlayer target = null;
                    switch (side)
                    {
                        case CardOwner.You:
                            target = instance.LocalPlayer;
                            break;
                        case CardOwner.Opponent:
                            target = instance.OpponentPlayer;
                            break;
                        default:
                            break;
                    }
                    return target;
                }
            }
        }
    }
}
