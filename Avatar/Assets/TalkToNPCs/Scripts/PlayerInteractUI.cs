using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
public class PlayerInteractUI : NetworkBehaviour {

    [SerializeField] private GameObject containerGameObject;
    [SerializeField] private PlayerInteract playerInteract;
    [SerializeField] private TextMeshProUGUI interactTextMeshProUGUI;
 public override void OnNetworkSpawn(){
playerInteract = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInteract>();
 }
    private void Update() {
        if (playerInteract.GetInteractableObject() != null) {
            Show(playerInteract.GetInteractableObject());
        } else {    
            Hide();
        }
    }

    private void Show(IInteractable interactable) {
        containerGameObject.SetActive(true);
        interactTextMeshProUGUI.text = interactable.GetInteractText();
    }

    private void Hide() {
        containerGameObject.SetActive(false);
    }

}