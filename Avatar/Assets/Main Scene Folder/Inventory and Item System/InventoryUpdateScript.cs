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
            playerID = LoginController.instance.verifiedUsername.GetHashCode();
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
        //add the backpack item 
        InventoryManager.instance.AddItem(backpackItem, false, playerID);
        foreach (var item in startingItems)
        {
            InventoryManager.instance.AddItem(item, false, playerID);
            Debug.Log("added item   " + item.name);
        }
        InventoryManager.instance.ChangeSelectedSlot(1);
    }

    
}
