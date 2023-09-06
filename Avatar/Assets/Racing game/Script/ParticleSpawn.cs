using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class ParticleSpawn : MonoBehaviour
{
    
     public ParticleSystem[] muzzleflash;
    public ParticleSystem impactEffect;
    private void Start(){
    
    }
    [ServerRpc(RequireOwnership = true)]
    public void ParticlestartServerRPC(){
        foreach (var particle in muzzleflash)
        {
            particle.Emit(1);
            particle.GetComponent<NetworkObject>().Spawn();

        }
        
    }
}
