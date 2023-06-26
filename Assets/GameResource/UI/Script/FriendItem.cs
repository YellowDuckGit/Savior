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
    private TextMeshProUGUI name;
    [SerializeField]
    private GameObject options;
    private void Start()
    {
        GetComponent<BoxCollider>().size = gameObject.GetComponent<RectTransform>().sizeDelta;

    }
    public Image Avatar
    {
        get { return avatar; }
        set { avatar = value; }
    }

    public TextMeshProUGUI Name
    {
        get { return name; }
        set { name = value; }
    }

    private void OnMouseDown()
    {
        Transform x = gameObject.transform.parent.Find("DropdownButton(Clone)");
        int index = 0;

        if (x != null)
        {
            index = x.transform.GetSiblingIndex();
            Destroy(x.gameObject);
        }

        if ( !(index - 1 == gameObject.transform.GetSiblingIndex()))
        {
            GameObject a = GameObject.Instantiate(options);
            a.gameObject.transform.parent = gameObject.transform.parent;
            a.transform.localScale = Vector3.one;
            a.transform.localPosition = Vector3.one;
            a.transform.SetSiblingIndex(gameObject.transform.GetSiblingIndex() + 1);

            FriendItemDropdown friendItemDropdown = a.GetComponent<FriendItemDropdown>();   
            a.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => { 
               friendItemDropdown.RemoveFriendInList(name.text);
            });


        }
    }

  

  
}
