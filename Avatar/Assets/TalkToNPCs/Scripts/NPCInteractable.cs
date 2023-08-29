using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractable : MonoBehaviour, IInteractable {

    [SerializeField] private string interactText;

    private Animator animator;
    private NPCHeadLookAt npcHeadLookAt;
     public delegate void InteractStatus(bool value);
    public static event InteractStatus OnInteractStatusChange;
    bool status= false;
    bool onGoingstatus = false;

    private void Awake() {
        animator = GetComponent<Animator>();
        npcHeadLookAt = GetComponent<NPCHeadLookAt>();
    }

    public void Interact(Transform interactorTransform) {
        //ChatBubble3D.Create(transform.transform, new Vector3(-.3f, 1.7f, 0f), ChatBubble3D.IconType.Happy, "Hello there!");

        //animator.SetTrigger("Talk");
        int modelLayer = LayerMask.NameToLayer("Model");
        if(gameObject.layer== modelLayer){
            if(onGoingstatus){

            SetInteractStatus(true);
            }
            else{  SetInteractStatus(true);}
          
            
            return;
        }
       float playerHeight = 1.7f;
        npcHeadLookAt.LookAtPosition(interactorTransform.position + Vector3.up * playerHeight);
        
    }

    public string GetInteractText() {
        return interactText;
    }

    public Transform GetTransform() {
        return transform;
    }
     public void SetInteractStatus(bool value)
    {
        status = value;
        OnInteractStatusChange?.Invoke(status);
    }
    

}