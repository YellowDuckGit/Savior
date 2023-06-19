namespace Assets.GameComponent.Card.CardComponents.Script
{
    public interface ISpellData : ICardData
    {
        public SpellType SpellType { get; set; }
    }
}