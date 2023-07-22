using System.Collections;
using System.Collections.Generic;
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
            StartCoroutine(IntegerLerpCoroutine(number, value, 2f));
            number = value;

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

    public void increase(int amount)
    {
        Number += amount;
    }

    public void decrease(int amount)
    {
        Number -= amount;
    }
}
