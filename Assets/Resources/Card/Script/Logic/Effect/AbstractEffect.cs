using Assets.GameComponent.Card.Logic;
using Assets.GameComponent.Card.Logic.TargetObject.Target;
using Assets.GameComponent.Card.Logic.TargetObject.Target.CardTarget;
using Assets.GameComponent.Card.Logic.TargetObject.Target.PlayerTarget;
using SerializeReferenceEditor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using static Assets.GameComponent.Card.Logic.Actions.Specify.SpecifyAction.SpecifyType;

namespace Assets.GameComponent.Card.LogicCard
{
    [Serializable]
    public abstract class AbstractEffect : AbstractData
    {
        [Header("Pitch")]
        [Tooltip("Times for effect be run")]
        [UnityEngine.InspectorName("Pitch (Sum)")]
        public EffectPitch[] pitch = null;

        [Serializable]
        public class EffectPitch
        {
            [SerializeReference]
            [SRLogicCard(typeof(PitchType))]
            public PitchType pitchType = null;

            public abstract class PitchType
            {
                public abstract int GetPitch(object register, MatchManager instance);
                [SRName("Pitch/Pitch with value")]
                public class PitchValue : PitchType
                {
                    public override int GetPitch(object register, MatchManager instance)
                    {
                        return 0;//TODO: Get value from target
                    }
                }
                [SRName("Pitch/Pitch with number count")]
                public class PitchCount : PitchType
                {
                    [SerializeReference]
                    [SRLogicCard(typeof(CardTarget), typeof(PlayerTarget), typeof(SpecifyCard), typeof(SpecifyCardPlayer))]
                    public AbstractTarget target = null;
                    public override int GetPitch(object register, MatchManager instance)
                    {
                        if(target is CardTarget cardTarget)
                        {
                            var cards = cardTarget.Execute(instance);
                            cards.RemoveAll(card => card == (CardBase)register);
                            return cards.Count;
                        }
                        else if(target is PlayerTarget playerTarget)
                        {
                            return playerTarget.Execute(instance).Count;
                        }
                        else if(target is SpecifyCard specifyCard)
                        {
                            var cards = specifyCard.Execute(instance);
                            return cards.Count;
                        }
                        else if(target is SpecifyCardPlayer specifyCardPlayer)
                        {
                            var players = specifyCardPlayer.Execute(instance);
                            return players != null ? 1 : 0;
                        }
                        else
                        {
                            Debug.Log(this.debug("Dose not find out this target", new
                            {
                                target.GetType().Name
                            }));
                        };
                        return 0;
                    }
                }
            }

        }

        public abstract void RevokeEffect(object register, MatchManager match);
        public abstract bool GainEffect(object register, EffectManager match);
    }
}
