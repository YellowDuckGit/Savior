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

namespace Assets.GameComponent.UI.CreateDeck.UI.Script
{
    public class PackItem : MonoBehaviour, IPointerClickHandler
    {
        public TextMeshProUGUI text_packName;
        Data_Pack data;
        private string id = CommonFunction.getNewId();

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
                text_packName.text = id;

            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //truyen droptableid of data -> getdroptable()
           StartCoroutine(LoadDataDropTable());
            print("onpointerclick");
        }

        public IEnumerator LoadDataDropTable()
        {
            if(data != null && data.dropTableId.Count() > 0) 
            {
                yield return StartCoroutine(PlayfabManager.instance.GetDropTable(data.dropTableId));
                UIManager.instance.ShowPopupPackDetailed(this);
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
    public List<string> cardItemsId = new List<string>();
    public List<string> dropTableId = new List<string>();

    public Data_Pack(string id)
    {
        this.id = id;
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
