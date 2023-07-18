using Assets.GameComponent.Card.CardComponents.Script;
using Assets.GameComponent.Card.CardComponents.Script.UI;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
    public Material NormalAvatar
    {
        get;
        set;
    }
    public Material InDeckAvatar
    {
        get;
        set;
    }
    public Material InBoardAvatar
    {
        get;
        set;
    }
    public Sprite NormalAvatar2D
    {
        get;
        set;
    }
    public Sprite InDeckAvatar2D
    {
        get;
        set;
    }

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
                this.UIDescription = canvas.transform.Find("Description Container").transform.Find("Description").transform.Find(nameof(Description)).GetComponent<TextMeshProUGUI>();
                this.UICost = canvas.transform.Find(nameof(Cost)).GetComponent<TextMeshProUGUI>();
                this.UIAvatar = transform.Find(nameof(Avatar)).GetComponent<MeshRenderer>();
                this.UIOutline = transform.Find("OutlineCard")?.gameObject;
                CardTarget = gameObject.GetComponent<SpellCard>();
                if(CardTarget != null)
                {
                    CardTarget.PropertyChanged += OnCardUpdate;
                }
            }
        }
        else
        {
            print(this.debug("Can not get UI Spell card"));
        }
    }

    public override void OnCardUpdate(object sender, PropertyChangedEventArgs e)
    {

        if (sender is SpellCard spellCard && spellCard == this.CardTarget)
        {
            print(this.debug($"Update card {spellCard.ToString()}"));
            ISpellData source = spellCard;
            // Sử dụng cấu trúc switch để kiểm tra tên của thuộc tính đã thay đổi
            switch (e.PropertyName)
            {
                // Nếu thuộc tính là Id
                case nameof(Id):
                    // Gán giá trị của thuộc tính Id của nguồn cho thuộc tính Id của đích
                    this.Id = source.Id;
                    break;
                // Nếu thuộc tính là Name
                case nameof(Name):
                    // Gán giá trị của thuộc tính Name của nguồn cho thuộc tính Name của đích
                    this.Name = source.Name;
                    break;
                // Nếu thuộc tính là Cost
                case nameof(Cost):
                    // Gán giá trị của thuộc tính Cost của nguồn cho thuộc tính Cost của đích
                    this.Cost = source.Cost;
                    break;
                // Nếu thuộc tính là Description
                case nameof(Description):
                    // Gán giá trị của thuộc tính Description của nguồn cho thuộc tính Description của đích
                    this.Description = source.Description;
                    break;
                // Nếu thuộc tính là CardType
                case nameof(CardType):
                    // Gán giá trị của thuộc tính CardType của nguồn cho thuộc tính CardType của đích
                    this.CardType = source.CardType;
                    break;
         
                // Nếu thuộc tính là RarityCard
                case nameof(RarityCard):
                    // Gán giá trị của thuộc tính RarityCard của nguồn cho thuộc tính RarityCard của đích
                    this.RarityCard = source.RarityCard;
                    break;
                // Nếu thuộc tính là RegionCard
                case nameof(RegionCard):
                    // Gán giá trị của thuộc tính RegionCard của nguồn cho thuộc tính RegionCard của đích
                    this.RegionCard = source.RegionCard;
                    break;
                case nameof(NormalAvatar):
                    this.NormalAvatar = source.NormalAvatar;
                    break;
                case nameof(InDeckAvatar):
                    this.InDeckAvatar = source.InDeckAvatar;
                    break;
                case nameof(NormalAvatar2D):
                    this.NormalAvatar2D = source.NormalAvatar2D;
                    break;
                case nameof(InDeckAvatar2D):
                    this.InDeckAvatar2D = source.InDeckAvatar2D;
                    break;
            }
            //this.Id = source.Id;
            //this.Name = source.Name;
            //this.Cost = source.Cost;
            //this.Description = source.Description;
            //this.CardType = source.CardType;
            //this.Avatar = source.Avatar;
            //this.RarityCard = source.RarityCard;
            //this.RegionCard = source.RegionCard;
            //this.Hp = source.Hp;
            //this.Attack = source.Attack;
        }
        //if (cardTarget == this.CardTarget)
        //{
        //    ISpellData source = cardTarget;
        //    ISpellData destination = this;
        //    destination.Id = source.Id;
        //    destination.Name = source.Name;
        //    destination.Cost = source.Cost;
        //    destination.Description = source.Description;
        //    destination.CardType = source.CardType;
        //    destination.Avatar = source.Avatar;
        //    destination.RarityCard = source.RarityCard;
        //    destination.RegionCard = source.RegionCard;
        //}
    }


}
