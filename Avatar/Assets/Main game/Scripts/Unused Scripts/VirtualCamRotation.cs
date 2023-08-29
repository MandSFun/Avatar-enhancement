using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.Netcode;
public class VirtualCamRotation : NetworkBehaviour
{
    [SerializeField] private float rotationSpeed = 1f;

    private Transform cameraTransform;
    private CinemachineVirtualCamera virtualCamera;
    private float mouseX, mouseY;

    private void Start()
    {
        
            // Find the virtual camera and its transform component
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
            cameraTransform = virtualCamera.transform;
        
    }

    private void LateUpdate()
    {
        

        // Get mouse input
        mouseX += Input.GetAxis("Mouse X") * rotationSpeed;
        mouseY -= Input.GetAxis("Mouse Y") * rotationSpeed;
        mouseY = Mathf.Clamp(mouseY, -90f, 90f);

        // Rotate the virtual camera based on mouse input
        cameraTransform.rotation = Quaternion.Euler(mouseY, mouseX, 0f).normalized;

        // Sync virtual camera position and rotation across the network
        CmdUpdateVirtualCamera(cameraTransform.position, cameraTransform.rotation);
    }

    
    private void CmdUpdateVirtualCamera(Vector3 position, Quaternion rotation)
    {
        cameraTransform.position = position;
        cameraTransform.rotation = rotation;
        RpcUpdateVirtualCamera(position, rotation);
    }

    
    private void RpcUpdateVirtualCamera(Vector3 position, Quaternion rotation)
    {
            cameraTransform.position = position;
            cameraTransform.rotation = rotation;
        
    }
}
