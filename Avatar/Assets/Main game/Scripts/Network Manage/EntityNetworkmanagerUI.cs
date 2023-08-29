using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

using UnityEngine.UI;

public class EntityNetworkmanagerUI : NetworkBehaviour
{
     [SerializeField] private Button executePhysicsButton;
      [SerializeField] private TextMeshProUGUI EntityCount;
      private EntitySpawner count;
     private NetworkVariable<int> EntityNum = new NetworkVariable<int>(0,NetworkVariableReadPermission.Everyone);


    private bool hasServerStarted;
    void Start()
    {
        
        NetworkManager.Singleton.OnServerStarted += () =>
        {
            hasServerStarted = true;
        };

        executePhysicsButton.onClick.AddListener(() => 
        {
            if (!hasServerStarted)
            {
                Debug.Log("Server has not started...");
                return;
            }
            EntitySpawner.Instance.SpawnObjects();
        });
    }
    private void Update(){
        EntityCount.text = "Entities: " + EntityNum.Value.ToString();
        if(!IsOwner)return;
        EntityNum.Value = EntitySpawner.Instance.UpdateEntityCount();
    }

}
