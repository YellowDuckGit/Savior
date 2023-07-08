using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabInput : MonoBehaviour
{
    public List<TMP_InputField> list;

    private int inputSelect = 0;

    private void Start()
    {
        SelectInputField();
    }

    private void Update()
    {
    if (Input.GetKeyDown(KeyCode.Tab))
        {
            inputSelect++;
            inputSelect++;
            if (inputSelect > list.Count-1) { inputSelect = 0; }
            SelectInputField();
            // 0 1 
            // 
        }
    }

    void SelectInputField()
    {
        print(inputSelect);
        list[inputSelect].Select();
    }


    public void ClickInputField(TMP_InputField a)
    {
        inputSelect =  list.IndexOf(a) - 1;
        
    }
}
