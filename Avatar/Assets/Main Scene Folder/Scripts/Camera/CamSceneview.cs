using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using Unity.Netcode;

public class CamSceneview : NetworkBehaviour
{
    [SerializeField] private Camera sceneviewCamera;
    [SerializeField] private GameObject thirdpersoncam;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private WaypointPlacer waypointPlacer;
    [SerializeField] private FreeFlyCamera freefly;
    GameObject player;
    ThirdPersonController controller;

    PrometeoCarController carController;
    GameObject car;
    private bool carSpawned = false;

    void Start()
    {
        sceneviewCamera.enabled = false;
        thirdpersoncam = GameObject.FindGameObjectWithTag("PlayerFollowCamera");
        waypointPlacer.enabled = false;
        carSpawned = false;
        freefly.enabled=false;  
    }


    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        player = GameObject.FindGameObjectWithTag("Player");
        controller = player.GetComponent<ThirdPersonController>();

    }
    private void OnEnable()
    {
        ModelSpawner.OnVehicleSpawned += HandleCarSpawned;
    }

    private void OnDisable()
    {
        ModelSpawner.OnVehicleSpawned -= HandleCarSpawned;
    }

    private void HandleCarSpawned(bool value)
    {
        carSpawned = value;
        Debug.Log("Car has spawned: " + carSpawned);
        if (carSpawned)
        {
            car = GameObject.FindGameObjectWithTag("Car");
            carController = car?.GetComponent<PrometeoCarController>();
        }


    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H) &&IsSpawned)
        { 
            if (sceneviewCamera.enabled)
            {   
                sceneviewCamera.enabled = false;
                mainCamera.SetActive(true);
                controller.enabled = true;
                freefly.enabled = false;
                waypointPlacer.enabled = false;
                if (carController != null)
                {
                    carController.enabled = true;
                }

            }
            else
            {
                sceneviewCamera.enabled = true;
                mainCamera.SetActive(false);
                controller.enabled = false;
                freefly.enabled = true;
                waypointPlacer.enabled = true;
                if (carController != null)
                {
                    carController.enabled = false;
                }

            }
        }
    }
}
