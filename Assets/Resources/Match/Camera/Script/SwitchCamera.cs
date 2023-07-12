using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CameraManager;

public class SwitchCamera : MonoBehaviour
{
    public ChanelCamera chanel;
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if(chanel == ChanelCamera.Card)
                CameraManager.instance.SwitchCamera(chanel, gameObject.transform);
            else
                CameraManager.instance.SwitchCamera(chanel, null);
        }
    }

}
