using UnityEngine;

public interface ICardBase : ICardData
{
    public bool IsSelectAble { get; set; }
    public bool IsSelected { get; set; }
    public bool Forcusable { get; set; }
    public bool IsFocus { get; set; }
    public CardPosition Position { get; set; }
    public CardOwner CardOwner { get; set; }
    public CardPlayer CardPlayer { get; set; }


}