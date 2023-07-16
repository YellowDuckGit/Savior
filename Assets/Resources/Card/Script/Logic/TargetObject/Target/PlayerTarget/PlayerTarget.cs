using Assets.GameComponent.Card.LogicCard;
using SerializeReferenceEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static Assets.GameComponent.Card.Logic.TargetObject.Target.AbstractTarget.AbstractTargetDataType;
using static Assets.GameComponent.Card.Logic.TargetObject.Target.AbstractTarget.AbstractTargetDataType.AbstractTargetDataTypeValue;
using static Assets.GameComponent.Card.Logic.TargetObject.Target.CardTarget.CardTarget.TargetCardType.TargetCardMonster;

namespace Assets.GameComponent.Card.Logic.TargetObject.Target.PlayerTarget
{
    [Serializable, SRName("Target type/Player")]
    public class PlayerTarget : AbstractTarget
    {
        //public enum TargetPlayerSide
        //{
        //    Any = 0, You = 1, Opponent = 2
        //}

        [Header("COMMON")]
        public CardOwner side;

        [Header("OPTION")]
        [SerializeReference]
        [SRLogicCard(typeof(TargetPlayerOption))]
        public List<TargetPlayerOption> playerOptions = null;


        public List<CardPlayer> Execute(MatchManager instance)
        {
            List<CardPlayer> players = new();
            if (side == CardOwner.Any)
            {
                players.Add(instance.redPlayer);
                players.Add(instance.bluePlayer);
            }
            else if (side == CardOwner.You)
            {
                players.Add(instance.LocalPlayer);
            }
            else if (side == CardOwner.Opponent)
            {
                players.Add(instance.OpponentPlayer);
            }
            else
            {
                Debug.LogError("PlayerTarget: side is not set");
            }

            if (playerOptions != null)
            {
                foreach (var option in playerOptions)
                {
                    option.Execute(players);
                }
            }
            return players;
        }

        [Serializable]
        public abstract class TargetPlayerOption
        {
            public abstract void Execute(List<CardPlayer> players);

            //public abstract bool Check(CardPlayer player);

            [SRName("Player Option/HP")]
            public class TargetPlayerOtpHP : TargetPlayerOption
            {
                [SerializeReference]
                [SRLogicCard(typeof(AbstractTargetDataTypeValue))]
                public AbstractTargetDataTypeValue value = null;

                public override void Execute(List<CardPlayer> players)
                {
                    if (value != null)
                    {
                        value.Execute(players, (CardPlayer player) => player.hp.Number);
                    }
                    else
                    {
                        Debug.Log("Value null");
                    }
                }
            }
            [SRName("Player Option/Mana")]
            public class TargetPlayerOtpMana : TargetPlayerOption
            {
                [SerializeReference]
                [SRLogicCard(typeof(AbstractTargetDataTypeValue))]
                public AbstractTargetDataTypeValue value = null;

                public override void Execute(List<CardPlayer> players)
                {
                    if (value != null)
                    {
                        value.Execute(players, (CardPlayer player) =>
                        player.mana.Number
                        );
                    }
                    else
                    {
                        Debug.Log("Value null");
                    }
                }
            }
        }

    }
}
