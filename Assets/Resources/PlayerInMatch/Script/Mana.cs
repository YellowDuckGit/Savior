using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Mana : MonoBehaviour
{
    int limit;
    int number;
    [SerializeField] TextMeshProUGUI textMeshPro;
    public int Number {

        get
        {
            return number;
        }
        set
        {
            number = value;
            textMeshPro.text = number.ToString();
        }
    
    }

    public int Limit
    {
        get
        {
            return limit;
        }
        set
        {
            limit = value;
            Number = limit;
        }
    }

    public void increase(int amount)
    {
        number += amount;
    }

    public void decrease(int amount)
    {
        number -= amount;
    }
}
