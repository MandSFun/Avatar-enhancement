using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class NameCamera : MonoBehaviour
{   [SerializeField] private Camera sceneviewCamera;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineVirtualCamera thirdperson;
    
    private Camera lookingcam;
    private void Start(){
        sceneviewCamera = GameObject.FindGameObjectWithTag("Sceneview").GetComponent<Camera>();
        mainCamera  = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        thirdperson = GameObject.FindGameObjectWithTag("PlayerFollowCamera").GetComponent<CinemachineVirtualCamera>();
    }
    void LateUpdate(){
      
       
        if(sceneviewCamera.enabled){
            transform.LookAt(transform.position+ sceneviewCamera.transform.forward);
        }
        else{
             transform.LookAt(transform.position + Camera.main.transform.forward);
        }
        
   }
}
