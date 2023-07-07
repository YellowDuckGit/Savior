using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AmountCardDisplay : MonoBehaviour
{
    public List<Image> gameObjects = new List<Image>();

    public Sprite exits;

    public Sprite unExits;

    public int amount = 0;

    public void SetAmount(int amount)
    {
        this.amount = amount;
        foreach (Image go in gameObjects)
        {
            if (amount > 0)
            {
                go.sprite = exits;
                amount--;
            }
            else
            {
                go.sprite = unExits;

            }
        }
    }
}
