using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdowList : MonoBehaviour
{
    public Dropdown dropdown;

    [SerializeField]
    private bool isTypeCard;
    [SerializeField]
    private bool isRegion;
    [SerializeField]
    private bool isPart;
    [SerializeField]
    private bool isRarity;
    [SerializeField]
    private bool isCost;
    private List<Dropdown.OptionData> optionDatas;
    private void Start()
    {

        dropdown = GetComponent<Dropdown>();
        optionDatas = new List<Dropdown.OptionData>();
        dropdown.ClearOptions();

        if (isTypeCard)
        {
            List<string> cardType = CommonFunction.GetEnumValuesAsString<CardType>();
            foreach (string a in cardType)
            {
                Dropdown.OptionData optionData = new Dropdown.OptionData();
                optionData.text = a;
                optionDatas.Add(optionData);
            }
        }
        else if(isRegion)
        {
            List<string> cardType = CommonFunction.GetEnumValuesAsString<RegionCard>();
            foreach (string a in cardType)
            {
                Dropdown.OptionData optionData = new Dropdown.OptionData();
                optionData.text = a;
                optionDatas.Add(optionData);

            }
        }
        else if(isPart)
        {
            //List<string> cardType = CommonFunction.GetEnumValuesAsString<Part>();
            //foreach (string a in cardType)
            //{
            //    Dropdown.OptionData optionData = new Dropdown.OptionData();
            //    optionData.text = a;
            //}
        }
        else if (isCost)
        {
           for(int i=1;i<= 10; i++)
            {
                Dropdown.OptionData optionData = new Dropdown.OptionData();
                optionData.text = i.ToString();
                optionDatas.Add(optionData);

            }
        }
        else if (isRarity)
        {
            List<string> cardType = CommonFunction.GetEnumValuesAsString<Rarity>();
            foreach (string a in cardType)
            {
                Dropdown.OptionData optionData = new Dropdown.OptionData();
                optionData.text = a;
                optionDatas.Add(optionData);
            }
        }
    }

  
}
