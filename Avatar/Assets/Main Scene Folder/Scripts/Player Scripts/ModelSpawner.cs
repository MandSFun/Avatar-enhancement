using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using StarterAssets;

public class ModelSpawner : NetworkBehaviour
{
    public GameObject modelPrefab;
    private List<GameObject> spawnedPrefabs = new List<GameObject>();
    [SerializeField] private Vector3 spawnPosition = new Vector3(0.18f, -0.74f, 4.41f);
    [SerializeField] private LayerMask layer;
    
    public delegate void VehicleSpawned(bool value);
    public static event VehicleSpawned OnVehicleSpawned;

    public bool spawned = false;
    private ThirdPersonController playerController;
       [SerializeField] private GameObject sceneviewCamera;
       Camera svcCamera;
    private void Start(){
         sceneviewCamera  = GameObject.FindGameObjectWithTag("Sceneview");
          svcCamera = sceneviewCamera.GetComponent<Camera>();
    }
    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && !svcCamera.isActiveAndEnabled)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layer))
            {
                if (spawnedPrefabs.Count == 0)
                {
                    SpawnModelServerRPC();
                    spawned = true;
                    isSpawned(spawned);
                }
                else
                {
                    DestroyModelServerRPC();
                    spawned = false;
                    isSpawned(spawned);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = true)]
    private void SpawnModelServerRPC()
    {
        Collider[] colliders = Physics.OverlapSphere(spawnPosition, 1f, layer);

        if (colliders.Length > 0)
        {
            // Collision detected, choose an alternative position or handle it as desired
            Debug.Log("Collision detected at the spawn point.");
            return;
        }

        GameObject spawnedModel = Instantiate(modelPrefab, spawnPosition, Quaternion.Euler(0f, -63f, 0f));
        spawnedPrefabs.Add(spawnedModel);

        PrometeoCarController carController = spawnedModel.GetComponent<PrometeoCarController>();
      
        spawnedModel.GetComponent<NetworkObject>().Spawn();
   

    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyModelServerRPC()
    {
        if (spawnedPrefabs.Count > 0)
        {
            GameObject destroy = spawnedPrefabs[0];
            Destroy(destroy);
            destroy.GetComponent<NetworkObject>().Despawn();
    
            spawnedPrefabs.RemoveAt(0);
            Destroy(destroy);
        }
    }
    public void SetPlayer(GameObject player)
{
    playerController = player.GetComponent<ThirdPersonController>();
}
 public void isSpawned(bool value)
    {
        spawned = value;
        OnVehicleSpawned?.Invoke(spawned);
    }

}
