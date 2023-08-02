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
    void Awake() 
    { ChatManager.Singleton = this; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        string message ="Player "+OwnerClientId+" has joined.";
        SendChatMessageServerRpc(message);
        

    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        string message ="Player "+OwnerClientId+" has left.";
        SendChatMessageServerRpc(message);
    }
    void Update() 
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            SendChatMessage(chatInput.text, NetworkManager.Singleton.LocalClientId );
            chatInput.text = "";
        }
    }

    public void SendChatMessage(string _message, ulong _fromWho)
    { 
        if(string.IsNullOrWhiteSpace(_message)) return;

        string S = "Player " + _fromWho + " > " +  _message;
        SendChatMessageServerRpc(S); 
    }
   
    void AddMessage(string msg)
    {
        ChatMessage CM = Instantiate(chatMessagePrefab, chatContent.transform);
        CM.SetText(msg);
    }

    [ServerRpc(RequireOwnership = false)]
    void SendChatMessageServerRpc(string message)
    {
        ReceiveChatMessageClientRpc(message);
    }

    [ClientRpc]
    void ReceiveChatMessageClientRpc(string message)
    {
        ChatManager.Singleton.AddMessage(message);
    }
}
