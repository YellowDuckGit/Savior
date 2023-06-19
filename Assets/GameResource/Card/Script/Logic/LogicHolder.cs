using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Assets.GameComponent.Card.LogicCard
{
    public class LogicHolder : MonoBehaviour
    {
        //All Type Data
        //[SerializeReference]
        //[SRLogicCard(typeof(AbstractData))]
        //public AbstractData[] List;

        [SerializeReference]
        [SRLogicCard(typeof(AbstractCondition))]
        public AbstractCondition[] LogicCard;
    }
}
