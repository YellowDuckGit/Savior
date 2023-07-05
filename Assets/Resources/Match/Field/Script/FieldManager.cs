using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FieldManager : MonoBehaviour
{
    [Header("Red Player")]
    public FightZone.FieldType typeActionRed;
    public List <FightZone> actionZonesRed;
    public List<SummonZone> summonZonesRed;

    [Space(20f)]
    [Header("Blue Player")]
    public FightZone.FieldType typeActionBlue;
    public List <FightZone> actionZonesBlue;
    public List<SummonZone> summonZonesBlue;


    private void Start()
    {
        //when user set role blue or red side -- assign player to this 

        //foreach(SummonField field in summonFieldsBlue)
        //{
        //    field.hand = PlayerBlue.hand;
        //}

        //if (actionFieldsBlue[0].type != typeActionBlue)
        //{
        //    foreach (ActionField actionField in actionFieldsBlue)
        //    {
        //        actionField.type = typeActionBlue;
        //    }
        //}

        //foreach (SummonField field in summonFieldsRed)
        //{
        //    field.hand = PlayerRed.hand;
        //}

        //if (actionFieldsRed[0].type != typeActionRed)
        //{
        //    foreach (ActionField actionField in actionFieldsRed)
        //    {
        //        actionField.type = typeActionRed;
        //    }
        //}
       
    }


}
