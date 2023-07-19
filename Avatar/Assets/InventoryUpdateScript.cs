using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUpdateScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int playerID = NetworkManagerUI.instance.playerID;
        Item[] startingItems = DatabaseScript.instance.GetStartingItems(playerID);

        foreach (var item in startingItems)
        {
            InventoryManager.instance.AddItem(item, false, playerID);
            Debug.Log("added item   " + item.name);
        }
        InventoryManager.instance.ChangeSelectedSlot(1);
    }

    
}
