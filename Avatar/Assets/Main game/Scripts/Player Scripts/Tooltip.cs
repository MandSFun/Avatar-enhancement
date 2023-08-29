using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tooltip : MonoBehaviour
{
    public string message;
    private void OnMouseEnter(){
        TooltipManager.instance.SetToolTip(message);
    }
    private void OnMouseExit(){
         TooltipManager.instance.HideToolTip();
    }
}
