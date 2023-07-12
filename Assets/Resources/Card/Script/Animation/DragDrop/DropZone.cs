using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropZone : MonoBehaviour
{
    private Transform position;

    public bool isAvaiable = true;
    public bool isOvering;

    private void OnEnable()
    {
        position = gameObject.transform;
    }


}
