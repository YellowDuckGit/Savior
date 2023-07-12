using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MMFeedbackSetRotation : MonoBehaviour
{
    [SerializeField]
    private Vector3 m_Rotation;

    public void Rotation()
    {
        transform.eulerAngles = m_Rotation;
    }
}
