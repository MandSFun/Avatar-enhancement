using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryMenuScript : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public Item[] itemsToPickUp ;
    

    public void PickUpItem(int id)
    {
        int playerID = userdatapersist.Instance.verifiedUser.GetHashCode();
        Debug.Log(itemsToPickUp.Length);
       
    }

    public void GetSelectedItem()
    {
        Item receivedItem = inventoryManager.GetSelectedItem(false);
        if (receivedItem != null)
        {
            Debug.Log("Received item");
        }
        else
        {
            Debug.Log("Failed to receive item");
        }
    }

    public void UseSelectedItem()
    {
        Item receivedItem = inventoryManager.GetSelectedItem(true);
        if (receivedItem != null)
        {
            Debug.Log("Used item");
        }
        else
        {
            Debug.Log("Failed to use item");
        }
    }
}
