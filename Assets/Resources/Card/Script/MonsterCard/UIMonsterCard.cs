using Card;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.GameComponent.Card.CardComponents.Script.UI
{
    public class UIMonsterCard : UICardBase<MonsterCard>, IUIMonsterCard, IMonsterData
    {
        [SerializeField] //for debug
        private TextMeshProUGUI _uiAttack;
        [SerializeField] //for debug
        private TextMeshProUGUI _uiHp;
        public TextMeshProUGUI UIAttack
        {
            get { return _uiAttack; }
            set { _uiAttack = value; }
        }

        public TextMeshProUGUI UIHp
        {
            get { return _uiHp; }
            set { _uiHp = value; }
        }
        public override TextMeshProUGUI UIName { get; set; }
        public override TextMeshProUGUI UICost { get; set; }
        public override TextMeshProUGUI UIDescription { get; set; }
        public override MeshRenderer UIAvatar { get; set; }
        public override MonsterCard CardTarget { get { return _cardTarget; } set { _cardTarget = value; } }

        public int Hp { get { return int.Parse(UIHp.text); } set { UIHp.text = value.ToString(); } }
        // Property for Attack with getter and setter
        public int Attack
        {
            get { return int.Parse(UIAttack.text); }
            set { UIAttack.text = value.ToString(); }
        }

        // Property for Id with getter and setter
        public string Id
        {
            set; get;
        }

        // Property for Name with getter and setter
        public string Name
        {
            get { return UIName.text; }
            set
            {
                UIName.text = value;
                print(this.debug($"Update ui for {Name}"));
            }
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
        public override GameObject UIOutline { get => _outline; set => _outline = value; }
        public Material NormalAvatar { get; set; }
        public Material InDeckAvatar { get; set; }
        public Material InBoardAvatar { get; set; }

        public override void GetCardComponents()
        {
            GameObject canvas = transform.Find("Canvas_Text").gameObject;
            if (canvas != null)
            {
                while (this.UIName == null || this.UIDescription == null || this.UICost == null || this.UIAttack == null || this.UIHp == null || this.UIAvatar == null || CardTarget == null)
                {
                    this.UIName = canvas.transform.Find(nameof(Name)).GetComponent<TextMeshProUGUI>();
                    this.UIDescription = canvas.transform.Find(nameof(Description)).GetComponent<TextMeshProUGUI>();
                    this.UICost = canvas.transform.Find(nameof(Cost)).GetComponent<TextMeshProUGUI>();
                    this.UIAttack = canvas.transform.Find(nameof(Attack)).GetComponent<TextMeshProUGUI>();
                    this.UIHp = canvas.transform.Find(nameof(Hp)).GetComponent<TextMeshProUGUI>();
                    this.UIAvatar = transform.Find(nameof(Avatar)).GetComponent<MeshRenderer>();
                    this.UIOutline = transform.Find("OutlineCard")?.gameObject;

                    CardTarget = gameObject.GetComponent<MonsterCard>();
                }
            }
            else
            {
                print(this.debug("Can not get UI Monster card"));
            }


            //this.Name = canvas.transform.Find(nameof(Name)).GetComponent<TextMeshProUGUI>();
            //this.Description = canvas.transform.Find(nameof(Description)).GetComponent<TextMeshProUGUI>();
            //this.Cost = canvas.transform.Find(nameof(Cost)).GetComponent<TextMeshProUGUI>();
            //this.Attack = canvas.transform.Find(nameof(Attack)).GetComponent<TextMeshProUGUI>();
            //this.Hp = canvas.transform.Find(nameof(Hp)).GetComponent<TextMeshProUGUI>();
            //this.Avatar = transform.Find(nameof(Avatar)).GetComponent<MeshRenderer>();
            //CardTarget = gameObject.GetComponent<MonsterCard>();
        }

        public override void RegisLocalListener()
        {
            this.RegisterListener(EventID.OnMoveToFightZone, (param) => OnMoveToFightZone(param as MoveToFightZoneArgs));
            this.RegisterListener(EventID.OnMoveToSummonZone, (param) => OnMoveToSummonZone(param as MoveToSummonZoneArgs));
            this.RegisterListener(EventID.OnMoveToGraveyard, (param) => OnMoveToGraveyard(param as MoveToGraveyardArgs));
        }

        private void SelectCardChange(bool value) => _ = value ? SelectCard() : UnSelectCard()/*OnClickOnCard()*/;

        private void OnMoveToGraveyard(MoveToGraveyardArgs args)
        {
            if (args.sender is MonsterCard monsterCard)
            {
                if (monsterCard == CardTarget)
                {
                    this.MoveToGraveyardAction();
                }
            }


        }
        private void MoveToGraveyardAction()
        {
            this.IsSelected = false;
            this.IsForcus = false;

            //change card from hand to summon zone 
            CardTarget.Position = CardPosition.InGraveyard;
            CardTarget.RemoveCardFormParentPresent();
            CardTarget.MoveCardIntoNewParent(CardTarget.CardPlayer.graveyard.transform);
        }

        private void OnMoveToSummonZone(MoveToSummonZoneArgs args)
        {
            if (args.sender is MonsterCard monsterCard)
            {
                if (monsterCard == CardTarget)
                {
                    this.MoveToSummonZoneAction(args.fightZone, args.summonZone);
                }
            }

        }

        private void MoveToSummonZoneAction(FightZone fightZone, SummonZone summonZone)
        {
            this.IsSelected = false;
            this.IsForcus = false;

            //change card from hand to summon zone 
            CardTarget.Position = CardPosition.InSummonField;
            CardTarget.RemoveCardFormParentPresent();
            CardTarget.MoveCardIntoNewParent(summonZone.transform);

            fightZone.monsterCard = null;
            print("UICardTarget>>MoveToSummonZone");
            summonZone.monsterCard = CardTarget;

            this.PostEvent(EventID.OnMoveCardToSummonZone, MatchManager.instance.localPlayerSide);
        }

        private void OnMoveToFightZone(MoveToFightZoneArgs args)
        {
            if (args.sender is MonsterCard monsterCard)
            {
                if (monsterCard == CardTarget)
                {
                    this.MoveToFightZoneAction(args.fightZone);
                }
            }
        }

        private void MoveToFightZoneAction(FightZone fightZone)
        {
            this.IsSelected = false;
            this.IsForcus = false;
            //change card from hand to summon zone 
            CardTarget.Position = CardPosition.InFightField;
            CardTarget.RemoveCardFormParentPresent();
            CardTarget.MoveCardIntoNewParent(fightZone.transform);
            this.UIOutline.SetActive(false);
        }


        //public void updateAttack(int Attack) => this.UIAttack.text = Attack.ToString();
        //public void updateHp(int Hp) => this.UIHp.text = Hp.ToString();
        //public override void updateName(string Name) => this.UIName.text = Name;
        //public override void updateDescription(string Description) => this.UIDescription.text = Description;
        //public override void updateCost(int Cost) => this.UICost.text = Cost.ToString();
        //public override void updateAvatar(Material Avatar) => this.UIAvatar.material = Avatar;

        public override void OnCardUpdate(MonsterCard cardTarget)
        {
            if (cardTarget == this.CardTarget)
            {
                print(this.debug($"Update card {cardTarget.ToString()}"));
                IMonsterData source = cardTarget;

                this.Id = source.Id;
                this.Name = source.Name;
                this.Cost = source.Cost;
                this.Description = source.Description;
                this.CardType = source.CardType;
                this.NormalAvatar = source.NormalAvatar;
                this.InDeckAvatar= source.InDeckAvatar;
                this.InBoardAvatar = source.InBoardAvatar;
                this.RarityCard = source.RarityCard;
                this.RegionCard = source.RegionCard;
                this.Hp = source.Hp;
                this.Attack = source.Attack;
            }
        }


    }
}
