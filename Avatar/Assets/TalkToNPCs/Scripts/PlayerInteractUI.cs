using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using System;
public class PlayerInteractUI : NetworkBehaviour
{
    [SerializeField ]private PlayerInteractUI playerInteractUI;
    [SerializeField] private GameObject containerGameObject;
    [SerializeField] private PlayerInteract playerInteract;
    [SerializeField] private TextMeshProUGUI interactTextMeshProUGUI;
    public override void OnNetworkSpawn()
    {  try{
         playerInteractUI.enabled = true; 
        playerInteract = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInteract>();
        containerGameObject.SetActive(true);}
        catch(Exception e){
            Debug.Log("hi22");
        }
    }
    private void Update()
    {   
        try{
        if (playerInteract.GetInteractableObject() != null)
        {
            Show(playerInteract.GetInteractableObject());
        }
        else
        {
            Hide();
        }}
         catch(Exception e){
         
        }
    }

    private void Show(IInteractable interactable)
    {
        containerGameObject.SetActive(true);
         interactTextMeshProUGUI.text = interactable.GetInteractText();
    }

    private void Hide()
    {
        containerGameObject.SetActive(false);
    }

}