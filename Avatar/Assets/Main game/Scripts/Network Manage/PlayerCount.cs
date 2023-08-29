using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerCount : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI playerCount;
      private NetworkVariable<int> playerNum = new NetworkVariable<int>(0,NetworkVariableReadPermission.Everyone);
      private void Start(){
        playerCount.gameObject.SetActive(false);
      }
      public override void OnNetworkSpawn(){
       if(!IsOwner){return;}
       playerCount.gameObject.SetActive(true);
      }
      private void Update(){
        playerCount.text = "Players: "+ playerNum.Value.ToString();
        if(!IsOwner)return;
        playerNum.Value = NetworkManager.Singleton.ConnectedClients.Count;
        
    }
   
}
