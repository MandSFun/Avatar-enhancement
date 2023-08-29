using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler
{

    public Image image;
    public Color selectedColor, unselectedColor;


    private void Awake()
    {
        Unselect();
    }

    public void Select()
    {
        image.color = selectedColor;
    }

    public void Unselect()
    {
        image.color = unselectedColor;
    }
   [HideInInspector] public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0)
        {
            //GameObject dropped =;
            InventoryItem draggableItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            draggableItem.parentAfterDrag = transform;
        }
        
    }
}
