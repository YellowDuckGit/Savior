using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CameraManager;

public class SwitchCamera : MonoBehaviour
{
    public ChanelCamera chanel;
    public CardBase card;
    private void OnMouseOver()
    {
        
        if (Input.GetMouseButtonDown(1))
        {
            if (card.Position.Equals(CardPosition.InSummonField)
            || card.Position.Equals(CardPosition.InFightField)
            || card.Position.Equals(CardPosition.InTriggerSpellField)
            || card.Position.Equals(CardPosition.InGraveyard))

            {
                if (card != null) print("Card != null");

                if (chanel == ChanelCamera.Card)
                    CameraManager.instance.SwitchCamera(chanel, card);
            }
            
        }
    }

}
