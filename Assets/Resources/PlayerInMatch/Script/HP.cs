using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HP : MonoBehaviour
{
    int number;
    int limit;
    [SerializeField] TextMeshProUGUI textMeshPro;

    private void Start()
    {
    }

    public int Number
    {

        get
        {
            return number;
        }
        set
        {
            number = value;
            textMeshPro.text = number.ToString();
            if (number <= 0)
            {
                MatchManager.instance.ResultMatch(WinCondition.EnemyLoseAllHp);
            }
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
        Number += amount;
    }

    public void decrease(int amount)
    {
        Number -= amount;
    }
}
