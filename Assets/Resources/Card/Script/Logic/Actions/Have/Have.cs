using Assets.GameComponent.Card.Logic.Effect;
using Assets.GameComponent.Card.Logic.TargetObject;
using Assets.GameComponent.Card.Logic.TargetObject.Select;
using Assets.GameComponent.Card.Logic.TargetObject.Target;
using Assets.GameComponent.Card.LogicCard;
using SerializeReferenceEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.GameComponent.Card.Logic.TargetObject.Target.AbstractTarget.AbstractTargetDataType.AbstractTargetDataTypeValue.ValueNumber;
//using static Assets.GameComponent.Card.Logic.Have.Have.AbstractHaveTargetData.HavePlayerTarget;
//using static Assets.GameComponent.Card.Logic.TargetObject.Select.SelectTargetPlayer;

namespace Assets.GameComponent.Card.Logic.Have
{
    [SRName("Logic/Have")]
    public class Have : AbstractAction
    {
        public bool _not = false;
        public GameCircle circle;
        public compareType comepare = compareType.more;
        public int number = 0;
        public enum GameCircle
        {
            Match, Round
        }

        [Header("Target")]
        [SerializeReference]
        [SRLogicCard(typeof(AbstractTarget))]
        public AbstractTarget target = null;

        [Header("Action")]
        [SerializeReference]
        [SRLogicCard(typeof(AbstractAction))]
        public List<AbstractAction> Actions = null;
    }
}
