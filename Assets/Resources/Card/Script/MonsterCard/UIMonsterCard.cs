using Assets.GameComponent.Card.Logic.TargetObject.Target.CardTarget;
using Card;
using EPOOutline;
using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Assets.GameComponent.Card.CardComponents.Script.UI
{
    public class UIMonsterCard : UICardBase<MonsterCard>, IUIMonsterCard, IMonsterData
    {
        public CardAnimationController controller;

        [SerializeField] //for debug
        private TextMeshProUGUI _uiAttack;
        [SerializeField] //for debug
        private TextMeshProUGUI _uiHp;
        public TextMeshProUGUI UIAttack
        {
            get
            {
                return _uiAttack;
            }
            set
            {
                
                _uiAttack = value;
            }
        }

        public TextMeshProUGUI UIHp
        {
            get
            {
                return _uiHp;
            }
            set
            {
                _uiHp = value;
            }
        }
        public override TextMeshProUGUI UIName
        {
            get; set;
        }
        public override TextMeshProUGUI UICost
        {
            get; set;
        }
        public override TextMeshProUGUI UIDescription
        {
            get; set;
        }
        public override MeshRenderer UIAvatar
        {
            get; set;
        }
        public override MonsterCard CardTarget
        {
            get
            {
                return _cardTarget;
            }
            set
            {
                _cardTarget = value;
            }
        }
        public int Hp
        {
            get
            {
                return int.Parse(UIHp.text);
            }
            set
            {
                StartCoroutine(HPLerpCoroutine(Hp, value, 1f));

            }
        }
        // Property for Attack with getter and setter
        public int Attack
        {
            get
            {
                return int.Parse(UIAttack.text);
            }
            set
            {
                StartCoroutine(ATKLerpCoroutine(Attack, value, 1f));

            }
        }

        // Property for Id with getter and setter
        public string Id
        {
            set; get;
        }

        // Property for Name with getter and setter
        public string Name
        {
            get
            {
                return UIName.text;
            }
            set
            {
                UIName.text = value;
                print(this.debug($"Update ui for {Name}"));
            }
        }

        // Property for Cost with getter and setter
        public int Cost
        {
            get
            {
                return int.Parse(UICost.text);
            }
            set
            {
                UICost.text = value.ToString();
            }
        }

        // Property for Description with getter and setter
        public string Description
        {
            get
            {
                return UIDescription.text;
            }
            set
            {
                UIDescription.text = value;
            }
        }

        // Property for CardType with getter and setter
        public CardType CardType
        {
            set; get;
        }

        // Property for Avatar with getter and setter
        public Material Avatar
        {
            get
            {
                return UIAvatar.material;
            }
            set
            {
                UIAvatar.material = value;
            }
        }

        public Rarity RarityCard
        {
            get; set;
        }
        public RegionCard RegionCard
        {
            get; set;
        }
        public override Outlinable UIOutline
        {
            get => _outline; set => _outline = value;
        }
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
        public override CardAnimationController Controller
        {
            get => controller; set => controller = value;
        }

        public override void GetCardComponents()
        {
            GameObject canvas = transform.Find("Canvas_Text").gameObject;
            if(canvas != null)
            {
                while(this.UIName == null || this.UIDescription == null || this.UICost == null || this.UIAttack == null || this.UIHp == null || this.UIAvatar == null || CardTarget == null)
                {
                    this.UIName = canvas.transform.Find(nameof(Name)).GetComponent<TextMeshProUGUI>();
                    this.UIDescription = canvas.transform.Find("Description Container").transform.Find("Description").transform.Find(nameof(Description)).GetComponent<TextMeshProUGUI>();
                    this.UICost = canvas.transform.Find(nameof(Cost)).GetComponent<TextMeshProUGUI>();
                    this.UIAttack = canvas.transform.Find(nameof(Attack)).GetComponent<TextMeshProUGUI>();
                    this.UIHp = canvas.transform.Find(nameof(Hp)).GetComponent<TextMeshProUGUI>();
                    this.UIAvatar = transform.Find(nameof(Avatar)).GetComponent<MeshRenderer>();
                    this.UIOutline = transform.Find("OuterCard")?.gameObject.GetComponent<Outlinable>();

                    CardTarget = gameObject.GetComponent<MonsterCard>();
                    if(CardTarget != null)
                    {
                        CardTarget.PropertyChanged += OnCardUpdate;
                    }

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
            this.RegisterListener(EventID.OnCardDamaged, (param) => AnimationGetDamged(param as MonsterCard));
            this.RegisterListener(EventID.OnMoveToGraveyard, (param) => AnimationDie(param as MonsterCard));
            this.RegisterListener(EventID.OnCardAttack, (param) => AnimationAtk(param as AnimationAttackArgs));



        }

        private void SelectCardChange(bool value) => _ = value ? SelectCard() : UnSelectCard()/*OnClickOnCard()*/;

        private void OnMoveToGraveyard(MoveToGraveyardArgs args)
        {
            if(args.sender is MonsterCard monsterCard)
            {
                if(monsterCard == CardTarget)
                {
                    StartCoroutine(MoveToGraveyardAction());
                }
            }


        }
        public IEnumerator MoveToGraveyardAction()
        {
            yield return new WaitForSeconds(3);

            this.IsSelected = false;
            this.IsForcus = false;
            this.Interactable = false;

            //change card from hand to summon zone 
            CardTarget.Position = CardPosition.InGraveyard;
            CardTarget.RemoveCardFormParentPresent();
            CardTarget.MoveCardIntoNewParent(CardTarget.CardPlayer.graveyard.transform);
        }


        private void OnMoveToSummonZone(MoveToSummonZoneArgs args)
        {
            if(args.sender is MonsterCard monsterCard)
            {
                if(monsterCard == CardTarget)
                {
                    //SFX: Summon Monster
                    this.MoveToSummonZoneAction(args.fightZone, args.summonZone);
                }
            }

        }

        private void MoveToSummonZoneAction(FightZone fightZone, SummonZone summonZone)
        {
            this.IsSelected = false;
            this.IsForcus = false;

            //change card from hand to summon zone 
            summonZone.SetMonsterCard(CardTarget);
            this.PostEvent(EventID.OnMoveCardToSummonZone, MatchManager.instance.localPlayerSide);
        }

        private void OnMoveToFightZone(MoveToFightZoneArgs args)
        {
            if(args.sender is MonsterCard monsterCard)
            {
                if(monsterCard == CardTarget)
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
            fightZone.SetMonsterCard(CardTarget);
            this.UIOutline.enabled = false;
        }

        //public void updateAttack(int Attack) => this.UIAttack.text = Attack.ToString();
        //public void updateHp(int Hp) => this.UIHp.text = Hp.ToString();
        //public override void updateName(string Name) => this.UIName.text = Name;
        //public override void updateDescription(string Description) => this.UIDescription.text = Description;
        //public override void updateCost(int Cost) => this.UICost.text = Cost.ToString();
        //public override void updateAvatar(Material Avatar) => this.UIAvatar.material = Avatar;

        public override void OnCardUpdate(object sender, PropertyChangedEventArgs e)
        {
            if(sender is MonsterCard monsterCard && monsterCard == this.CardTarget)
            {
                print(this.debug($"Update card {monsterCard.ToString()}"));
                IMonsterData source = monsterCard;
                // Sử dụng cấu trúc switch để kiểm tra tên của thuộc tính đã thay đổi
                switch(e.PropertyName)
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
                    case nameof(NormalAvatar):
                        // Gán giá trị của thuộc tính Avatar của nguồn cho thuộc tính Avatar của đích
                        this.Avatar = source.NormalAvatar;
                        print("Gan NormalAvatar");
                        break;
                    case nameof(RarityCard):
                        // Gán giá trị của thuộc tính RarityCard của nguồn cho thuộc tính RarityCard của đích
                        this.RarityCard = source.RarityCard;
                        break;
                    // Nếu thuộc tính là RegionCard
                    case nameof(RegionCard):
                        // Gán giá trị của thuộc tính RegionCard của nguồn cho thuộc tính RegionCard của đích
                        this.RegionCard = source.RegionCard;
                        break;
                    // Nếu thuộc tính là Hp
                    case nameof(Hp):
                        // Gán giá trị của thuộc tính Hp của nguồn cho thuộc tính Hp của đích
                        this.Hp = source.Hp;
                        break;
                    // Nếu thuộc tính là Attack
                    case nameof(Attack):
                        // Gán giá trị của thuộc tính Attack của nguồn cho thuộc tính Attack của đích
                        this.Attack = source.Attack;
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
        }

        private void AnimationGetDamged(MonsterCard args)
        {
            if (args is MonsterCard monsterCard && args == CardTarget)
            {
                //SFX: CardGetDame
                controller.PlayGetDame();
            }
        }

        private void AnimationDie(MonsterCard args)
        {
            if (args is MonsterCard monsterCard && CardTarget == args)
            {
                //SFX: DestroyCard
                controller.PlayDestroyCard();
            }
        }

        private void AnimationAtk(AnimationAttackArgs args)
        {
            if (CardTarget == args.own)
            {
                print("AnimationAtk");
                controller.PlayATKCard(args.own,args.opponnet);
            }
            else
            {
                print("controller.Card != args.own");

            }
        }


        private IEnumerator HPLerpCoroutine(int fromValue, int toValue, float duration)
        {
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;

                int result = Mathf.RoundToInt(Mathf.Lerp(fromValue, toValue, t));

                UIHp.text = result.ToString();

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            UIHp.text = toValue.ToString();

        }

        private IEnumerator ATKLerpCoroutine(int fromValue, int toValue, float duration)
        {
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;

                int result = Mathf.RoundToInt(Mathf.Lerp(fromValue, toValue, t));

                UIAttack.text = result.ToString();

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            UIAttack.text = toValue.ToString();
        }
    }


}
