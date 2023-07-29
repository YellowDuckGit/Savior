using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Mana : MonoBehaviour
{
    int limit = 0;
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
            print("Mana Number: " + value);

            StartCoroutine(IntegerLerpCoroutine(number, value, 1f));
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
        StartCoroutine(FloatLerpCoroutine(fromValue, toValue, duration));

        float elapsedTime = 0;
        SoundManager.instance.PlayPourMana();
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            int result = Mathf.RoundToInt(Mathf.Lerp(fromValue, toValue, t));
            textMeshPro.text = result.ToString();

            //if (toValue != 0)
            //    liquid.CompensateShapeAmount = (float)result / (float)MatchManager.instance.maxMana;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        SoundManager.instance.StopPourMana();

    }

    private IEnumerator FloatLerpCoroutine(float fromValue, float toValue, float duration)
    {
        float elapsedTime = 0;

        float a = fromValue / MatchManager.instance.maxMana;

        float b = toValue / MatchManager.instance.maxMana;

        print("a: " + a);
        print("b: " + b);

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            float result = (Mathf.Lerp(a, b, t));

            print("result: " + result.ToString());

            liquid.CompensateShapeAmount = result;
            print("CompensateShapeAmount: " + liquid.CompensateShapeAmount);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        print("CompensateShapeAmount: " + liquid.CompensateShapeAmount);

        liquid.CompensateShapeAmount =  b;
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
