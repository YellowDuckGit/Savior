using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.GameComponent.UI.CreateDeck.UI.Script
{
    public class PackItem : MonoBehaviour, IPointerClickHandler
    {
        public TextMeshProUGUI text_packName;
        public Image avatar;
        Data_Pack data;
        private string id = CommonFunction.getNewId();
        public TextMeshProUGUI price;

        public string ID
        {
            get { return id; }
            set { id = value; }
        }

        public Data_Pack Data
        {
            get { return this.data; }
            set
            {
                this.data = value;

                ////Avatar
                //data.cardItemsId.ForEach(a => print(a.ToString()));
                //string idSavaior = data.cardItemsId.SingleOrDefault(a => a.Contains("SA"));

                //if (idSavaior != null)
                //{
                //    ICardData carData = GameData.instance.listCardDataInGame.SingleOrDefault(a => a.Id.Equals(idSavaior));
                //    if(carData != null)
                //        avatar.sprite = carData.NormalAvatar2D;
                //}
                //else
                //{
                //    ICardData carData = GameData.instance.listCardDataInGame.SingleOrDefault(a => a.Id == data.cardItemsId[0]);
                //     if(carData != null)
                //    avatar.sprite = carData.NormalAvatar2D;
                //}

            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //SOUND
            SoundManager.instance.PlayClick_Normal();
            //truyen droptableid of data -> getdroptable()
            StartCoroutine(LoadDataDropTable());
            print("onpointerclick");
        }

        public IEnumerator LoadDataDropTable()
        {
            if(data != null && data.dropTableId.Count() > 0) 
            {
                yield return StartCoroutine(PlayfabManager.instance.GetDropTable(data.dropTableId));
                yield return StartCoroutine(UIManager.instance.ShowPopupPackDetailed(this));
                // TUTORIAL
                TutorialManager.instance.PlayTutorialChain();
            } else
            {
                print("LOAD DATA DROP TABLE EMPTY");
            }
            yield return null;
        }
    }
}
//=====//
[Serializable]
public class Data_Pack
{
    public string id;
    public string packName;
    public string price;
    public List<string> cardItemsId = new List<string>();
    public List<string> dropTableId = new List<string>();

    public Data_Pack(string id, string packName, string price)
    {
        this.id = id;
        this.packName = packName;
        this.price = price;
    }

}
public class DropTableInfor
{
    public string id;
    public Dictionary<string, int> items;
    public string ItemsToString()
    {
        return string.Join(", ", items.Keys);
    }
}
