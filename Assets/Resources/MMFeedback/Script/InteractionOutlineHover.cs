using EPOOutline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionOutlineHover : MonoBehaviour
{
    public List<Outlinable> outlines;

    private void OnMouseEnter()
    {
        outlines.ForEach(a=>a.enabled = true);
    }

    private void OnMouseExit()
    {
        outlines.ForEach(a => a.enabled = false);

    }


}
