using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class EntitySpawner : NetworkSingleton<EntitySpawner>
{
    [SerializeField]
    private GameObject objectPrefab;

   
    [SerializeField]
    private int maxObjectInstanceCount = 3;
    private int currentEntityCount = 0;

    private void Awake()
    {
        NetworkManager.Singleton.OnServerStarted += () =>
        {   
           // NetworkObjectPool.Instance.InitializePool();
        };
    }

    public void SpawnObjects()

    {
        if (!IsServer) return;

        for (int i = 0; i < maxObjectInstanceCount; i++)
        {
            //GameObject go = Instantiate(objectPrefab, 
            //    new Vector3(Random.Range(-10, 10), 10.0f, Random.Range(-10, 10)), Quaternion.identity);
            GameObject go = Instantiate(objectPrefab, new Vector3(Random.Range(-10,10), 10.0f, Random.Range(-10,10)), Quaternion.identity);
           // go.transform.position = new Vector3(Random.Range(-10, 10), 10.0f, Random.Range(-10, 10));
            go.GetComponent<NetworkObject>().Spawn();
             currentEntityCount++;
        }
    }
    public int UpdateEntityCount(){
        return currentEntityCount;
    }
}
