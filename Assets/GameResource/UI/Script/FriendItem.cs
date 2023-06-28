using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FriendItem : MonoBehaviour
{
    [SerializeField]
    private Image avatar;
    [SerializeField]
    private Image statusImage;
    [SerializeField]
    public TextMeshProUGUI userName;
    [SerializeField]
    private GameObject options;
    [SerializeField]
    private Button Options;

    private bool status = false;
    private void Start()
    {
        Options.onClick.AddListener(() => CreateDrogdownOptions());

    }

    public bool Status
    {
        get { return status; }
        set 
        { 
            status = value;
            if (status)
                statusImage.color = Color.green;
            else statusImage.color = Color.red;
        }
    }
    public Image Avatar
    {
        get { return avatar; }
        set { avatar = value; }
    }

    public TextMeshProUGUI UserName
    {
        get { return userName; }
        set { userName = value; }
    }

    public void CreateDrogdownOptions()
    {
        Transform x = gameObject.transform.parent.Find("DropdownButton(Clone)");
        int index = 0;

        if (x != null)
        {
            index = x.transform.GetSiblingIndex();
            Destroy(x.gameObject);
        }

        if (!(index - 1 == gameObject.transform.GetSiblingIndex()))
        {
            GameObject a = GameObject.Instantiate(options);
            a.gameObject.transform.parent = gameObject.transform.parent;
            a.transform.localScale = Vector3.one;
            a.transform.localPosition = Vector3.one;
            a.transform.SetSiblingIndex(gameObject.transform.GetSiblingIndex() + 1);

            FriendItemDropdown friendItemDropdown = a.GetComponent<FriendItemDropdown>();
             a.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => {
                 ChatManager.instance.SendDirectMessage(userName.text, nameof(MessageType.RequestPlay) +"|"+ "null");
             });
            a.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => {
                friendItemDropdown.RemoveFriendInList(userName.text);
                ChatManager.instance.SendDirectMessage(userName.text, nameof(MessageType.DeleteFriend) + "|" + ChatManager.instance.nickName);
            });
        }
    }

  

  
}
