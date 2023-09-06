using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryBottomSlotController : MonoBehaviour
{

    public static InventoryBottomSlotController instance;
    [SerializeField] public GameObject inventorySlots;
    // Start is called before the first frame update
    private void Awake()
    {
        instance = this;
    }

    public void ActivateInventorySlots(bool val)
    {

        //GameObject InventorySlots = GameObject.FindGameObjectWithTag("BottomInventorySlot");

     


        if (inventorySlots != null)
        {
            // Set the child game object to active
            inventorySlots.SetActive(val);
        }
        else
        {
            Debug.LogError("Object with tag not found: BottomInventorySlot ");
        }
    }


}
