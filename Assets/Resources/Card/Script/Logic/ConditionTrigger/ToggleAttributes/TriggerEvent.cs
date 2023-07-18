using Assets.GameComponent.Card.Logic.Effect;
using Assets.GameComponent.Card.LogicCard;
using SerializeReferenceEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.GameComponent.Card.Logic.ConditionTrigger.TriggerEvent
{
    [SRName("Logic/ConditionTrigger/ToggleAttributes")]

    public class ToggleAttributes : AbstractCondition, IEffectAttributes
    {
        [Header("Attributes")]
        [Space(10)]
        public bool _isCharming;
        public bool IsCharming { get { return _isCharming; } set { _isCharming = value; } }

        public bool _isTreating;
        public bool IsTreating { get { return _isTreating; } set { _isTreating = value; } }

        public bool _isDominating;
        public bool IsDominating { get { return _isDominating; } set { _isDominating = value; } }

        public bool _isBlockAttack;
        public bool IsBlockAttack { get { return _isBlockAttack; } set { _isBlockAttack = value; } }

        public bool _isBlockDefend;
        public bool IsBlockDefend { get { return _isBlockDefend; } set { _isBlockDefend = value; } }

        [Header("Toggle status")]
        [Space(10)]
        public bool toggleStatus = true;
    }
}
