using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;
public class Car : NetworkBehaviour
{
     [SerializeField] private CinemachineVirtualCamera playerCamera;
        [SerializeField] private float positionrange = 10f;
     public override void OnNetworkSpawn(){
        if (playerCamera == null)
            { if(!IsOwner)return;
                playerCamera = GameObject.FindGameObjectWithTag("CarCamera").GetComponent<CinemachineVirtualCamera>();
                // playerCamera.gameObject.SetActive(false);
                playerCamera.Follow = transform.GetChild(0).transform;
                
            }

            transform.position= new Vector3(Random.RandomRange(positionrange,-positionrange),0,Random.RandomRange(positionrange,-positionrange));
            transform.rotation = new Quaternion(0,180,0,0);
     }
     
}
