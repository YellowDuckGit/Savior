using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public class HP : MonoBehaviour
{
    int number = 0;
    int limit;
    [SerializeField] TextMeshProUGUI textMeshPro;
    [SerializeField] Liquid liquid;

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
            StartCoroutine(IntegerLerpCoroutine(number, value, 1f));
            number = value;

            if (number <= 0)
            {
                //SFX: Out of Blood Tube
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

        }
    }

    private IEnumerator IntegerLerpCoroutine(int fromValue, int toValue, float duration)
    {
        if (toValue != 0)
        {
            StartCoroutine(FloatLerpCoroutine(fromValue, toValue, duration));
        }

        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            int result = Mathf.RoundToInt(Mathf.Lerp(fromValue, toValue, t));


            textMeshPro.text = result.ToString();
        
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        textMeshPro.text = toValue.ToString();

    }

    private IEnumerator FloatLerpCoroutine(float fromValue, float toValue, float duration)
    {
        float elapsedTime = 0;

        float a = fromValue / MatchManager.instance.maxHP;

        float b = toValue / MatchManager.instance.maxHP;


        print("a: " + a);
        print("b: " + b);

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            float result = (Mathf.Lerp(a, b, t));
            print("result: "+result.ToString());

            liquid.CompensateShapeAmount = result;
            //liquid.CompensateShapeAmount += result;

            print("CompensateShapeAmount: " + liquid.CompensateShapeAmount);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        liquid.CompensateShapeAmount = b;

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
