using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraSwitcher
{
   static List<CinemachineVirtualCamera> cameras = new List<CinemachineVirtualCamera>();

    public static CinemachineVirtualCamera ActiveCamera = null;

    public static bool IsActiveCamera(CinemachineVirtualCamera virtualCamera)
    {
        return virtualCamera == ActiveCamera;
    }

    public static void SwitchCamera(CinemachineVirtualCamera cam)
    {
        cam.Priority = 10;
        ActiveCamera = cam;

        foreach(CinemachineVirtualCamera c in cameras)
        {
            if(c != cam)
            {
                c.Priority = 0;
            }
        }
    }

    public static void Register(CinemachineVirtualCamera cam)
    {
        Debug.Log("Register");
        cameras.Add(cam);
    }

    public static void UnRegister(CinemachineVirtualCamera cam)
    {
        Debug.Log("UnRegister");
        cameras.Remove(cam);
    }
}
