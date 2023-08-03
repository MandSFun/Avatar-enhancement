using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Singleton;

    [SerializeField] ChatMessage chatMessagePrefab;
    [SerializeField] CanvasGroup chatContent;
    [SerializeField] TMP_InputField chatInput;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        string message ="Player "+NetworkManager.Singleton.LocalClientId+" has joined.";
        SendChatMessageServerRpc(message,NetworkManager.Singleton.LocalClientId);
        

    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        string message ="Player "+NetworkManager.Singleton.LocalClientId+" has left.";
        SendChatMessageServerRpc(message,NetworkManager.Singleton.LocalClientId);
    }
    void Update() 
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            SendChatMessage(chatInput.text );
            chatInput.text = "";
        }
    }

    public void SendChatMessage(string _message)
    { 
        if(string.IsNullOrWhiteSpace(_message)) return;

        //string S = "Player " + NetworkManager.Singleton.LocalClientId + " > " +  _message;
        SendChatMessageServerRpc(_message,NetworkManager.Singleton.LocalClientId); 
    }
   
    void AddMessage(string msg,ulong senderID)
    {
        ChatMessage CM = Instantiate(chatMessagePrefab, chatContent.transform);
        CM.SetMessage(senderID,msg);
    }

    [ServerRpc(RequireOwnership = false)]
    void SendChatMessageServerRpc(string message,ulong senderID)
    {
        ReceiveChatMessageClientRpc(message,senderID);
    }

    [ClientRpc]
    void ReceiveChatMessageClientRpc(string message,ulong senderID)
    {
        //ChatManager.Singleton.AddMessage(message,senderID);
        AddMessage(message,senderID);
    }
}
