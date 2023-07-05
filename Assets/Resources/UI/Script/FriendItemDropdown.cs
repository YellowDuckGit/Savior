using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendItemDropdown : MonoBehaviour
{
    public void RemoveFriendInList(string name)
    {
        StartCoroutine(IERemoveFriendInList(name)); 
    }

    public IEnumerator IERemoveFriendInList(string name)
    {
        yield return StartCoroutine(PlayfabManager.instance.RemoveFriend(name));
        Destroy(gameObject);
    }
}
