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
    [SerializeField] private TextMeshProUGUI statusDescription;
    [SerializeField]
    public TextMeshProUGUI userName;
    [SerializeField]
    private GameObject options;
    [SerializeField]
    private Button Options;

    private int status = 0;
    private void Start()
    {
        Options.onClick.AddListener(() => CreateDrogdownOptions());
    }

    public int Status
    {
        get { return status; }
        set 
        { 
            status = value;

            switch (status)
            {
                case 0: //offline 
                    statusImage.color = Color.white;
                    statusDescription.text = "Offline";
                    break;
                case 1: //invisible : Be invisible to everyone
                    break;
                case 2: //online 
                    statusImage.color = Color.green;
                    statusDescription.text = "Online";

                    break;
                case 3: //away: Online but not available
                    statusImage.color = Color.yellow;
                    statusDescription.text = "Away";

                    break;
                case 4: //DND: Do not disturb.
                    statusImage.color = Color.yellow;
                    statusDescription.text = "DND";

                    break;
                case 5: //LFS:  Looking For Game/Group. Could be used when you want to be invited or do matchmaking. More...
                    statusImage.color = Color.blue;
                    statusDescription.text = "LFS";

                    break;
                case 6: //Playing:
                    statusImage.color = Color.blue;
                    statusDescription.text = "Playing";
                    break;
            }
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
