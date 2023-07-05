using Assets.GameComponent.Card.Logic;
using Assets.GameComponent.Card.Logic.TargetObject.Select;
using SerializeReferenceEditor;
using System;

namespace Assets.GameComponent.Card.LogicCard
{
    public class SRLogicCardAttribute : SRAttribute

    {
        public SRLogicCardAttribute() : base()
        {
        }

        public SRLogicCardAttribute(Type baseType) : base(baseType)
        {
        }

        public SRLogicCardAttribute(params Type[] types) : base(types)
        {
        }

        public override void OnCreate(object instance)
        {
            //if (instance is AbstractData)
            //{
            //    ((AbstractData)instance).name = instance.GetType().Name; //set variable name to name of class
            //}
        }
    }

}
