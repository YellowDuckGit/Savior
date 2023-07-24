using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidenModel : MonoBehaviour
{
    public CardPlayer player;
    void Start()
    {
        if (player.photonView.IsMine)
        {
            this.gameObject.SetActive(false);
        }
    }
}
