using SerializeReferenceEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GameComponent.Card.Logic.TargetObject.Select
{
    [SRName("SelectFilter/Target/Card")]
    public class SelectTargetCard : AbstractSelectTargetObject
    {
        public CardOwner owner;

        public CardPosition cardPosition;

        public CardType cardType;
        public bool isDamed;

        public SelectTargetCard()
        {
        }

        public SelectTargetCard(string input)
        {
            // Split the input by the '-' character
            string[] parts = input.Split('-');
            // Check if the input has exactly three parts
            if (parts.Length == 5)
            {
                // Parse each part into the corresponding enum value
                owner = (CardOwner)Enum.Parse(typeof(CardOwner), parts[1]);
                cardPosition = (CardPosition)Enum.Parse(typeof(CardPosition), parts[2]);
                cardType = (CardType)Enum.Parse(typeof(CardType), parts[3]);
            }
            else
            {
                // Throw an exception if the input is invalid
                throw new ArgumentException("Invalid input format");
            }
        }

        public override string ToString()
        {
            return string.Format("Select a card {0} from {1} {2}", (this.cardType == CardType.Monster ? "monster" : "spell"), (this.owner == CardOwner.You ? "your" : "opponent's"), this.cardPosition.ToString());
        }

        internal bool isMatch(CardBase cardBase)
        {
            if (cardBase == null)
            {
                return false;
            }
            if (cardBase.CardType != this.cardType)
            {
                return false;
            }
            if (cardBase.Position != this.cardPosition)
            {
                return false;
            }

            if (this.owner == CardOwner.You)
            {
                return MatchManager.instance.LocalPlayer == cardBase.CardPlayer;
            }
            else
            {
                return MatchManager.instance.LocalPlayer != cardBase.CardPlayer;
            }
        }
    }
}
