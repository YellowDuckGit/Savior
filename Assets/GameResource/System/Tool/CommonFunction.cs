using Assets.GameComponent.Card.Logic.Effect.Gain;
using Assets.GameComponent.Card.Logic.TargetObject.Select;
using Assets.GameComponent.Card.LogicCard;
using Assets.GameComponent.Card.LogicCard.ListLogic.Effect;
using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
//using static Assets.GameComponent.Card.Logic.TargetObject.SelectTargetObject;
using static EffectManager;
using static EventGameHandler;

public static class CommonFunction
{
    public static string getNewId()
    {
        Guid myuuid = Guid.NewGuid();
        return myuuid.ToString();
    }

    public static int FindClosestNumber(int[] numbers, int target)
    {
        int min = 0;
        int max = numbers.Length - 1;
        int closest = 0;

        while (min <= max)
        {
            int mid = (min + max) / 2;

            if (numbers[mid] == target)
            {
                return numbers[mid];
            }
            else if (numbers[mid] > target)
            {
                max = mid - 1;
            }
            else
            {
                min = mid + 1;
            }

            if (Math.Abs(numbers[mid] - target) < Math.Abs(numbers[closest] - target))
            {
                closest = mid;
            }
        }

        return numbers[closest];
    }

    public static int FindIndexClosestNumber(int[] numbers, int target)
    {
        int min = 0;
        int max = numbers.Length - 1;
        int closest = 0;

        while (min <= max)
        {
            int mid = (min + max) / 2;

            if (numbers[mid] == target)
            {
                return numbers[mid];
            }
            else if (numbers[mid] > target)
            {
                max = mid - 1;
            }
            else
            {
                min = mid + 1;
            }

            if (Math.Abs(numbers[mid] - target) < Math.Abs(numbers[closest] - target))
            {
                closest = mid;
            }
        }

        return closest;
    }
}
public static class ListExtensions
{
    // Shuffle the list using Fisher-Yates algorithm
    public static void Shuffle<T>(this List<T> list)
    {
        // Get a random number generator
        System.Random random = new System.Random();
        // Loop from the last element to the second one
        for (int i = list.Count - 1; i > 0; i--)
        {
            // Pick a random index from 0 to i
            int j = random.Next(i + 1);
            // Swap the elements at i and j
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
    //c# extension
    public static dynamic GetData(this EventData obj)
    {
        object[] datas = obj.CustomData as object[];
        int zoneID;
        int cardID;
        string playerSide;
        int fightZoneID;
        int summonZoneID;
        string cardDataId;
        int photonviewID;
        switch ((RaiseEvent)obj.Code)
        {
            case RaiseEvent.DRAW_CARD_EVENT:
                Console.WriteLine("Draw a card");
                return new { };
            case RaiseEvent.SET_DATA_CARD_EVENT:
                datas = (object[])obj.CustomData;
                cardDataId = (string)datas[0];
                photonviewID = (int)datas[1];
                Console.WriteLine("Set data monster");
                return new { cardDataId, photonviewID };
            case RaiseEvent.UPDATE_DATA_MONSTER_EVENT:
                datas = (object[])obj.CustomData;
                MonsterCard card = datas[0] as MonsterCard;
                photonviewID = (int)datas[1];
                Console.WriteLine("Set data monster");
                return new { card, photonviewID };
            case RaiseEvent.MoveCardInTriggerSpell:
                Console.WriteLine("Summon monster");
                zoneID = (int)datas[0];
                cardID = (int)datas[1];
                playerSide = (string)datas[2];
                return new { zoneID, cardID, playerSide };
            case RaiseEvent.SKIP_TURN:
                playerSide = (string)datas[0];

                return new { playerSide };

            case RaiseEvent.SWITCH_TURN:
                return new { };

            case RaiseEvent.MOVE_FIGHTZONE:
                return new { };

            case RaiseEvent.MOVE_SUMMONZONE:
                fightZoneID = (int)datas[0];
                summonZoneID = (int)datas[1];
                cardID = (int)datas[2];
                playerSide = (string)datas[3];
                return new { fightZoneID, summonZoneID, cardID, playerSide };

            case RaiseEvent.ATTACK:
                playerSide = (string)datas[0];
                return new { playerSide };

            case RaiseEvent.DEFENSE:
                playerSide = (string)datas[0];
                return new { playerSide };
            case RaiseEvent.EFFECT_EXCUTE:

                Type typeEffect = null;
                Type typeSelect = null;
                var senderPlayerSide = datas[0] as string;

                var typeAsString = datas[1] as string; //type effect
                var json = datas[2] as string;// json string

                var targetType = datas[3] as string;//target type
                var targetID = (int)datas[4]; //target photon id

                var selectType = datas[5] as string; //select type
                var selectJson = datas[6] as string; //select json
                switch (typeAsString)
                {
                    case "BuffStats":
                        typeEffect = typeof(BuffStats);
                        break;
                    case "Dame":
                        typeEffect = typeof(Dame);
                        break;
                    case "Gain":
                        typeEffect = typeof(Gain);
                        break;
                }

                switch (selectType)
                {
                    case "SelectTargetPlayer":
                        typeSelect = typeof(SelectTargetPlayer);
                        break;
                    case "SelectTargetCard":
                        typeSelect = typeof(SelectTargetCard);
                        break;
                }

                if (typeEffect != null && typeSelect != null)
                {
                    var abstractEffect = JsonUtility.FromJson(json as string, typeEffect); //effect data
                    var selectTarget = JsonUtility.FromJson(selectJson as string, typeSelect); //select data
                    return new { senderPlayerSide, abstractEffect, selectTarget, targetType, targetID };
                }
                else
                {
                    UnityEngine.Debug.Log("can not get type of effect");
                }
                return null;
            case RaiseEvent.EFFECT_UPDATE_STATUS:
                EffectStatus status = (EffectStatus)datas[0];
                return new { status };
            default:
                return new { };
        }
    }
}
public static class DebugHelper
{
    public static string debug(this object args, string des = null, params object[] valuesToDisplay)
    {
        StringBuilder stringBuilder = new StringBuilder();
        string className = args.GetType().Name;
        var track = new System.Diagnostics.StackTrace(true);
        string methodName = track.GetFrame(1).GetMethod().Name;
        int line = track.GetFrame(1).GetFileLineNumber();
        string time = DateTime.Now.ToString("HH:mm:ss");
        string v1 = string.Format("{0}|{1}>>{2}::{3} {4}", time, className, methodName, line, string.IsNullOrEmpty(des) ? "" : $">>{des}");
        stringBuilder.Append(v1);
        if (valuesToDisplay != null && valuesToDisplay.Length != 0)
        {
            for (int i = 0; i < valuesToDisplay.Length; i++)
            {
                if (valuesToDisplay[i] != null)
                {
                    Type type = valuesToDisplay[i].GetType();
                    var properties = type.GetProperties();
                    foreach (var property in properties)
                    {
                        var value = property.GetValue(valuesToDisplay[i]);
                        stringBuilder.Append(string.Format("\n${0}: {1}", property.Name, (value ?? "null").ToString()));
                    }
                }
            }
        }
        return stringBuilder.ToString();
    }
}

public static class DataExtension
{
    public static void CopyAttributes(this object source, object destination)
    {
        // Get the type of the source object
        Type sourceType = source.GetType();
        // Get the type of the destination object
        Type destinationType = destination.GetType();
        // Loop through all the properties of the source object
        foreach (PropertyInfo property in sourceType.GetProperties())
        {
            // Check if the property can be read and written
            if (property.CanRead && property.CanWrite)
            {
                // Get the value of the property from the source object
                object value = property.GetValue(source, null);
                // Find the matching property in the destination object by name and type
                PropertyInfo destinationProperty = destinationType.GetProperty(property.Name, property.PropertyType);
                // Check if the matching property exists and can be written
                if (destinationProperty != null && destinationProperty.CanWrite)
                {
                    // Set the value of the property to the destination object
                    destinationProperty.SetValue(destination, value, null);
                }
            }
        }
    }
}