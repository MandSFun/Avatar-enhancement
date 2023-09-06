using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InventoryUpdateScript : MonoBehaviour
{

    [SerializeField] Item backpackItem; 
    // Start is called before the first frame update
    void Start()
    {
        int playerID;
        try
        {
            playerID = userdatapersist.Instance.verifiedUser.GetHashCode();
        }
        catch (System.Exception e)
        {
            playerID = NetworkManagerUI.instance.localPlayerID;
            Debug.Log("Unable to get playerID from SQL Server. Using default playerID from local username: " + playerID);
        }

        Item[] startingItems = DatabaseScript.instance.GetStartingItems(playerID);
        if (SQLConnection.instance.SQLServerConnected)
        {
            startingItems = SQLConnection.instance.GetStartingItems(playerID);
            Debug.Log("Successfully loaded data from SQL server");
        }
        
        bool hasBackpack = false;
        foreach (var item in startingItems)
        {
            if (item.name == backpackItem.name)
            {
                hasBackpack = true;
            }
            InventoryManager.instance.AddItem(item, false, playerID);
            Debug.Log("added item   " + item.name);
        }

        if (!hasBackpack)
        {
            //add the backpack item 
            InventoryManager.instance.AddItem(backpackItem, true, playerID);
        }
        InventoryManager.instance.ChangeSelectedSlot(1);
    }

    
}
