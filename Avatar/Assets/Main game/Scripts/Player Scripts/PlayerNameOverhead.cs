using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Text;
using TMPro;
public class PlayerNameOverhead : NetworkBehaviour
{

    [SerializeField]
    private NetworkVariable<NetworkString> playerNetworkName = new NetworkVariable<NetworkString>("",NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);
    [SerializeField]
    private TextMeshProUGUI localPlayerOverlay;
    private bool overlaySet = false;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerNetworkName.Value = $"Player {OwnerClientId}";
        }
    }

    public void SetOverlay()
    {

        localPlayerOverlay.text = $"{playerNetworkName.Value}";
    }

    public void Update()
    {
        if (!overlaySet && !string.IsNullOrEmpty(playerNetworkName.Value) && IsOwner)
        {
            SetOverlay();
            overlaySet = true;
        }
        else return;
    }
}

