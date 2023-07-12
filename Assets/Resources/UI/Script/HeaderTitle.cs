using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeaderTitle : MonoBehaviour
{
    public static string title
    {
        set
        {
            currentHeader.text = value;
        }
    }
    private static TextMeshProUGUI currentHeader;
    public TextMeshProUGUI headerTitle;

    private void OnEnable()
    {
        currentHeader = this.headerTitle;
    }
}
