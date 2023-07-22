using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Mana : MonoBehaviour
{
    int limit;
    int number = 0;
    [SerializeField] TextMeshProUGUI textMeshPro;
    [SerializeField] Liquid liquid;
    private int newNumber;
    private int presentNumber;

    private bool isChange;
    public int Number {

        get
        {
            return number;
        }
        set
        {

            //textMeshPro.text = number.ToString();
            //StartCoroutine(IntegerLerpCoroutine(number, value, 2f));

            StartCoroutine(IntegerLerpCoroutine(number, value, 2f));
            number = value;
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
        }
    }

    private IEnumerator IntegerLerpCoroutine(int fromValue, int toValue, float duration)
    {
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            int result = Mathf.RoundToInt(Mathf.Lerp(fromValue, toValue, t));
            textMeshPro.text = result.ToString();

            if (toValue != 0)
                liquid.CompensateShapeAmount = result / toValue;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public void DisplayLiquid()
    {
        if(isChange == true)
        {
            if (presentNumber == newNumber)
            {
                isChange = false;
                return;
            }
            else
            {
                //exute
                print("Presnet: " + presentNumber);
                print("newNumber: " + newNumber);
                int a = CommonFunction.IntegerLerp(presentNumber, newNumber, 0.1f);
                textMeshPro.text = a.ToString();
            }
        }
    }

    private void Update()
    {
     
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
