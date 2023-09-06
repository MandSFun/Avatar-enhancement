using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering;
using System.Net;
using System;
using Unity.VisualScripting;

public class CameraFirstPerson : MonoBehaviour
{
    [SerializeField] private float sensitivity = 50f;
    
    
    private float xRotation = 0f;
    private float yRotation = 0f;
    public float defaultFOV = 56f;
    public float zoomedFOV = 30f;
    public bool isZoomed = false;
    public bool isCrouching = false;
    [SerializeField] float movementSpeed = 2f;
    private Vector3 oldAngles;
    private bool revertedPosition = true;
    [SerializeField] GameObject hitBox;
    [SerializeField] GameObject machineGunObj;
    void Start()
    {
        machineGunObj.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        
    }

    void Update()
    {
        if (planeCameraController.instance == null || planeCameraController.instance.index == 4)
        {
            HandleInput();
        }
        else
        {
            machineGunObj.SetActive(false);
        }
        

    }

    public void HandleInput()
    {

        machineGunObj.SetActive(true) ;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isCrouching = !isCrouching;
        }

        if (isCrouching)
        {
            revertedPosition = false;
            transform.eulerAngles = new Vector3(
                 -20, -180, 0);
            hitBox.GetComponent<Collider>().enabled = false;

        }
        else
        {

            if (!revertedPosition)
            {
                transform.eulerAngles = oldAngles;
                revertedPosition = true;
                hitBox.GetComponent<Collider>().enabled = true;
            }

            if (Input.GetKey(KeyCode.W))
            {
                transform.position = transform.position + transform.forward * Time.deltaTime * movementSpeed;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                transform.position = transform.position - transform.forward * Time.deltaTime * movementSpeed;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                transform.position = transform.position + transform.right * Time.deltaTime * movementSpeed;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                transform.position = transform.position - transform.right * Time.deltaTime * movementSpeed;
            }



            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                isZoomed = !isZoomed;
            }

            float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            yRotation += mouseX;
            yRotation = Mathf.Clamp(90f, yRotation, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
            //playerBody.Rotate(Vector3.up * mouseX);
            Camera cam = this.GetComponent<Camera>();
            if (isZoomed)
            {
                cam.fieldOfView = zoomedFOV;
            }
            else
            {
                cam.fieldOfView = defaultFOV;
            }
        }

    
}


}
