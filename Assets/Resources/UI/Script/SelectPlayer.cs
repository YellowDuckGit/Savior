using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectPlayer : MonoBehaviour
{
    public CardPlayer player;
  
    private void OnMouseDown()
    {
        player.IsSelected = true;
        print("Select" + player.side);

    }
}
