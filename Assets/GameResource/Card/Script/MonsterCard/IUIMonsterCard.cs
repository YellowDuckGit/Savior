using TMPro;

namespace Assets.GameComponent.Card.CardComponents.Script.UI
{
    internal interface IUIMonsterCard:IUICardBase
    {
        public TextMeshProUGUI UIAttack { get; set;}
        public TextMeshProUGUI UIHp{get;}
    }
}