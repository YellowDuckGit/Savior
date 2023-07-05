using Cinemachine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCamera : MonoBehaviourPun, IPunObservable
{
    [SerializeField] public CinemachineVirtualCamera lookCardCamera;
    [SerializeField] public CinemachineVirtualCamera lookFieldCamera;

    private void OnEnable()
    {
        print("OnEnable");
        if (photonView.IsMine)
        {
            print("RightCLick");
            CameraSwitcher.Register(lookFieldCamera);
            CameraSwitcher.Register(lookCardCamera);
            CameraSwitcher.SwitchCamera(lookCardCamera);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (photonView.IsMine)
        {
            CameraSwitcher.UnRegister(lookFieldCamera);
            CameraSwitcher.UnRegister(lookCardCamera);
        }
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                print("RightClick");
                if (CameraSwitcher.IsActiveCamera(lookCardCamera))
                {
                    CameraSwitcher.SwitchCamera(lookFieldCamera);
                    print("Switch");

                }
                else if (CameraSwitcher.IsActiveCamera(lookFieldCamera))
                {
                    CameraSwitcher.SwitchCamera(lookCardCamera);
                    print("Switch");
                }
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
