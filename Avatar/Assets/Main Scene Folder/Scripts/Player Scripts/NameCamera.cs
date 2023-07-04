using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameCamera : MonoBehaviour
{   [SerializeField] private Camera sceneviewCamera;
    [SerializeField] private Camera mainCamera;
    private Camera lookingcam;
    private void Start(){
        sceneviewCamera = GameObject.FindGameObjectWithTag("Sceneview").GetComponent<Camera>();
        mainCamera  = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }
    void LateUpdate(){
        if(sceneviewCamera.enabled)

        transform.LookAt(transform.position + sceneviewCamera.transform.forward);
        else{
            transform.LookAt(transform.position,mainCamera.transform.forward);
        }
   }
}
