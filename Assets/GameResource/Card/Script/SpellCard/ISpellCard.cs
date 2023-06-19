using Assets.GameComponent.Card.CardComponents.Script;

public interface ISpellCard: ISpellData
{
    public SpellData BaseSpellData { get; set; }
}