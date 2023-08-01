using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class InventoryManager : MonoBehaviour
{

    public static InventoryManager instance;
    
    public InventorySlot[] inventorySlots;
    public int maxStackedItems = 10;
    public GameObject inventoryItemPrefab;
    public Item selectedItem;
    public List<GameObject> hiddenInventoryBackpackItems;
    public int playerID = -1;
    
    int selectedSlot = 0;


    private void Awake()
    {
        instance = this;
    }


    private void Start()
    {
        playerID = NetworkManagerUI.instance.playerID;
        //Debug.Log("inveotry playerid is   " + playerID);
        
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Comma) && selectedSlot != 0)
        {  // user changes selcted slot 

            ChangeSelectedSlot(selectedSlot - 1);
            selectedItem = GetSelectedItem(false);
            selectedItem = GetSelectedItem(false);

        }

        if (Input.GetKeyDown(KeyCode.Period) && selectedSlot != 5)
        {

            ChangeSelectedSlot(selectedSlot + 1);
            selectedItem = GetSelectedItem(false);
        }



    }

    public Item GetSelectedItem(bool used)
    {
        
        InventorySlot slot = inventorySlots[selectedSlot];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
        if (itemInSlot != null) //there exists an item in the selcted slot 
        {
            Item item = itemInSlot.item;

            if (used)
            {
                itemInSlot.count--;
                if (itemInSlot.count <= 0)
                {
                    Destroy(itemInSlot.gameObject);
                }
                else
                {
                    itemInSlot.RefreshCount();
                }
                int weaponID = ItemToHash(item); 
                int playerID = NetworkManagerUI.instance.playerID;
                DatabaseScript.instance.RemoveWeapon(playerID, weaponID, 1);
            }


            return item;

        }
        return null;
    }

    public void ChangeSelectedSlot(int newValue)
    {
        if (selectedSlot >= 0)
        {
            inventorySlots[selectedSlot].Unselect();
        }

        inventorySlots[newValue].Select();
        selectedSlot = newValue;
    }


    public bool AddItem(Item item, bool isNewItem, int playerID)
    {
        if (isNewItem)
        {
            AddItemInventoryDB(item, playerID);
        }
        

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null && itemInSlot.item == item && itemInSlot.count < maxStackedItems && itemInSlot.item.stackable)
            {
                itemInSlot.count++;
                itemInSlot.RefreshCount();
                return true;
            }

        }


        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
            if (itemInSlot == null)
            {
                SpawnNewItemInSlot(item, slot);
                ChangeSelectedSlot(i);

                return true;
            }

        }
        return false;
    }

    public void SpawnNewItemInSlot(Item item, InventorySlot slot)
    {
        GameObject newItemGameObject = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGameObject.GetComponent<InventoryItem>();
        inventoryItem.InitialiseItem(item);
    }


    public void AddItemInventoryDB(Item item, int playerID)
    {
        int quantity = 1;
        int weaponID = ItemToHash(item);
        DatabaseScript.instance.AddWeapon(playerID, weaponID, quantity);
    }

    public int ItemToHash(Item item)
    {
        int weaponID = -1;
        switch (item.name)
        {
            case "Ak47":
                weaponID = 1;
                break;
            case "Dynamite":
                weaponID = 2;
                break;
            case "M4 Rifle":
                weaponID = 3;
                break;
            case "Smg":
                weaponID = 4;
                break;


        }
        return weaponID;
    }




}

