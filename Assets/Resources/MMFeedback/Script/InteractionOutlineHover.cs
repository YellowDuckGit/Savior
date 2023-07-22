using EPOOutline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionOutlineHover : MonoBehaviour
{
    public Outlinable outline;

    private void OnMouseEnter()
    {
        outline.enabled = true;
    }

    private void OnMouseExit()
    {
        outline.enabled = false;

    }
}
