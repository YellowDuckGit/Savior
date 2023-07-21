using Assets.GameComponent.Card.Logic.Effect;
using Assets.GameComponent.Card.Logic.Effect.CreateCard;
using Assets.GameComponent.Card.Logic.Effect.Destroy;
using Assets.GameComponent.Card.Logic.Effect.Gain;
using Assets.GameComponent.Card.Logic.TargetObject.Select;
using Assets.GameComponent.Card.Logic.TargetObject.Target.CardTarget;
using Assets.GameComponent.Card.Logic.TargetObject.Target.PlayerTarget;
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
using Debug = UnityEngine.Debug;

public static class CommonFunction
{
    public static string getNewId()
    {
        Guid myuuid = Guid.NewGuid();
        return myuuid.ToString();
    }
    public static List<string> GetEnumValuesAsString<T>() where T : Enum
    {
        List<string> values = new List<string>();
        foreach(T enumValue in Enum.GetValues(typeof(T)))
        {
            values.Add(enumValue.ToString());
        }
        return values;
    }

    public static int FindClosestNumber(int[] numbers, int target)
    {
        int min = 0;
        int max = numbers.Length - 1;
        int closest = 0;

        while(min <= max)
        {
            int mid = (min + max) / 2;

            if(numbers[mid] == target)
            {
                return numbers[mid];
            }
            else if(numbers[mid] > target)
            {
                max = mid - 1;
            }
            else
            {
                min = mid + 1;
            }

            if(Math.Abs(numbers[mid] - target) < Math.Abs(numbers[closest] - target))
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

        while(min <= max)
        {
            int mid = (min + max) / 2;

            if(numbers[mid] == target)
            {
                return numbers[mid];
            }
            else if(numbers[mid] > target)
            {
                max = mid - 1;
            }
            else
            {
                min = mid + 1;
            }

            if(Math.Abs(numbers[mid] - target) < Math.Abs(numbers[closest] - target))
            {
                closest = mid;
            }
        }

        return closest;
    }

    public static bool isEqualsAttibutes(this IEffectAttributes a, IEffectAttributes b)
    {
        return a.IsCharming == b.IsCharming &&
            a.IsTreating == b.IsTreating &&
            a.IsDominating == b.IsDominating &&
            a.IsBlockAttack == b.IsBlockAttack &&
            a.IsBlockDefend == b.IsBlockDefend;
    }
    public static int IntegerLerp(int fromValue, int toValue, float t)
    {
        t = Mathf.Clamp01(t);


        int result = Mathf.RoundToInt(fromValue + (toValue - fromValue) * t);

        return result;
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
        for(int i = list.Count - 1; i > 0; i--)
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
        CardPosition cardPosition;
        switch((RaiseEvent)obj.Code)
        {
            case RaiseEvent.DRAW_CARD_EVENT:
                Console.WriteLine("Draw a card");
                return new
                {
                };
            case RaiseEvent.SET_DATA_CARD_EVENT:
                datas = (object[])obj.CustomData;
                cardDataId = (string)datas[0];
                photonviewID = (int)datas[1];
                Console.WriteLine("Set data monster");
                return new
                {
                    cardDataId,
                    photonviewID
                };

            case RaiseEvent.SUMMON_MONSTER:
                Console.WriteLine("Summon monster");
                zoneID = (int)datas[0];
                cardID = (int)datas[1];
                playerSide = (string)datas[2];
                cardPosition = (CardPosition)datas[3];
                bool isSpecialSummon = (int)datas[4] == 1;
                return new
                {
                    zoneID,
                    cardID,
                    playerSide,
                    cardPosition,
                    isSpecialSummon
                };
            case RaiseEvent.MoveCardInTriggerSpell:
                Console.WriteLine("MoveCardInTriggerSpell monster");
                playerSide = datas[0] as string;
                cardID = (int)datas[1];
                return new
                {
                    playerSide,
                    cardID
                };
            case RaiseEvent.SKIP_TURN:
                playerSide = (string)datas[0];

                return new
                {
                    playerSide
                };

            case RaiseEvent.SWITCH_TURN:
                return new
                {
                };

            case RaiseEvent.MOVE_FIGHTZONE:
                return new
                {
                };

            case RaiseEvent.MOVE_SUMMONZONE:
                fightZoneID = (int)datas[0];
                summonZoneID = (int)datas[1];
                cardID = (int)datas[2];
                playerSide = (string)datas[3];
                return new
                {
                    fightZoneID,
                    summonZoneID,
                    cardID,
                    playerSide
                };

            case RaiseEvent.ATTACK:
                playerSide = (string)datas[0];
                return new
                {
                    playerSide
                };

            case RaiseEvent.DEFENSE:
                playerSide = (string)datas[0];
                return new
                {
                    playerSide
                };
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
                switch(typeAsString)
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
                    case "Heal":
                        typeEffect = typeof(Heal);
                        break;
                    case "DestroyObject":
                        typeEffect = typeof(DestroyObject);
                        break;
                    case "CreateCard":
                        typeEffect = typeof(CreateCard);
                        break;
                }

                switch(selectType)
                {
                    case "PlayerTarget":
                        typeSelect = typeof(PlayerTarget);
                        break;
                    case "CardTarget":
                        typeSelect = typeof(CardTarget);
                        break;
                }

                if(typeEffect != null && typeSelect != null)
                {
                    var abstractEffect = JsonUtility.FromJson(json as string, typeEffect); //effect data
                    var selectTarget = JsonUtility.FromJson(selectJson as string, typeSelect); //select data
                    return new
                    {
                        senderPlayerSide,
                        abstractEffect,
                        selectTarget,
                        targetType,
                        targetID
                    };
                }
                else
                {
                    Debug.Log(typeEffect.debug("can not get type of effect", new
                    {
                        typeEffect.GetType().Name
                    }));
                }
                return null;
            case RaiseEvent.EFFECT_UPDATE_STATUS:
                EffectStatus status = (EffectStatus)datas[0];
                return new
                {
                    status
                };
            case RaiseEvent.NEXT_STEP:
                senderPlayerSide = datas[0] as string;
                bool isNEXT_STEP = (bool)datas[1];
                return new
                {
                    senderPlayerSide,
                    isNEXT_STEP
                };
            default:
                return new
                {
                };
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
        if(valuesToDisplay != null && valuesToDisplay.Length != 0)
        {
            for(int i = 0; i < valuesToDisplay.Length; i++)
            {
                if(valuesToDisplay[i] != null)
                {
                    Type type = valuesToDisplay[i].GetType();
                    var properties = type.GetProperties();
                    foreach(var property in properties)
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
        foreach(PropertyInfo property in sourceType.GetProperties())
        {
            // Check if the property can be read and written
            if(property.CanRead && property.CanWrite)
            {
                // Get the value of the property from the source object
                object value = property.GetValue(source, null);
                // Find the matching property in the destination object by name and type
                PropertyInfo destinationProperty = destinationType.GetProperty(property.Name, property.PropertyType);
                // Check if the matching property exists and can be written
                if(destinationProperty != null && destinationProperty.CanWrite)
                {
                    // Set the value of the property to the destination object
                    destinationProperty.SetValue(destination, value, null);
                }
            }
        }
    }
}