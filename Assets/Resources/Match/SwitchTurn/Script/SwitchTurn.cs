using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchTurn : MonoBehaviour
{

    void OnMouseDown()
    {
        UIMatchManager.instance.ClickSkipTurnModel();
    }
}
