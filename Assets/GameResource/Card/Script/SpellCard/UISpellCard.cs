using Assets.GameComponent.Card.CardComponents.Script;
using Assets.GameComponent.Card.CardComponents.Script.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class UISpellCard : UICardBase<SpellCard>, ISpellData
{
    public override TextMeshProUGUI UIName { get; set; }
    public override TextMeshProUGUI UICost { get; set; }
    public override TextMeshProUGUI UIDescription { get; set; }
    public override MeshRenderer UIAvatar { get; set; }
    public override SpellCard CardTarget { get { return _cardTarget; } set { _cardTarget = value; } }

    public string Id
    {
        set; get;
    }

    // Property for Name with getter and setter
    public string Name
    {
        get { return UIName.text; }
        set { UIName.text = value; }
    }

    // Property for Cost with getter and setter
    public int Cost
    {
        get { return int.Parse(UICost.text); }
        set { UICost.text = value.ToString(); }
    }

    // Property for Description with getter and setter
    public string Description
    {
        get { return UIDescription.text; }
        set { UIDescription.text = value; }
    }

    // Property for CardType with getter and setter
    public CardType CardType
    {
        set; get;
    }

    // Property for Avatar with getter and setter
    public Material Avatar
    {
        get { return UIAvatar.material; }
        set { UIAvatar.material = value; }
    }

    public Rarity RarityCard { get; set; }
    public RegionCard RegionCard { get; set; }
    public SpellType SpellType { get; set; }
    public override GameObject UIOutline { get => _outline; set => _outline = value; }

    public override void RegisLocalListener()
    {
    }
    public override void GetCardComponents()
    {
        GameObject canvas = transform.Find("Canvas_Text").gameObject;
        if (canvas != null)
        {
            while (this.UIName == null || this.UIDescription == null || this.UICost == null || this.UIAvatar == null || CardTarget == null)
            {
                this.UIName = canvas.transform.Find(nameof(Name)).GetComponent<TextMeshProUGUI>();
                this.UIDescription = canvas.transform.Find(nameof(Description)).GetComponent<TextMeshProUGUI>();
                this.UICost = canvas.transform.Find(nameof(Cost)).GetComponent<TextMeshProUGUI>();
                this.UIAvatar = transform.Find(nameof(Avatar)).GetComponent<MeshRenderer>();
                this.UIOutline = transform.Find("OutlineCard")?.gameObject;
                CardTarget = gameObject.GetComponent<SpellCard>();
            }
        }
        else
        {
            print(this.debug("Can not get UI Spell card"));
        }
    }

    public override void OnCardUpdate(SpellCard cardTarget)
    {
        if (cardTarget == this.CardTarget)
        {
            ISpellData source = cardTarget;
            ISpellData destination = this;
            destination.Id = source.Id;
            destination.Name = source.Name;
            destination.Cost = source.Cost;
            destination.Description = source.Description;
            destination.CardType = source.CardType;
            destination.Avatar = source.Avatar;
            destination.RarityCard = source.RarityCard;
            destination.RegionCard = source.RegionCard;
        }
    }


}
