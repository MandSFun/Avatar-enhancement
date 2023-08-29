using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{   public static TooltipManager instance;
    
    public TMP_Text tooltipui;
    // Start is called before the first frame update
    private void Awake(){
        if(instance != null && instance != this){
            Destroy(this.gameObject);
        }
        else{
            instance = this;
        }
    }
    void Start()
    { //Cursor.visible = true;
    gameObject.SetActive(false);
        
    }

    // Update is called once per frame
    void Update()
{
    transform.position = Input.mousePosition;

    // Calculate the distance between the TooltipManager's position and the mouse position
 
}
    public void SetToolTip(string message){
        gameObject.SetActive(true);
        tooltipui.text = message;
    }
    public void HideToolTip(){
        gameObject.SetActive(false);
        tooltipui.text = string.Empty;  
    }
}
