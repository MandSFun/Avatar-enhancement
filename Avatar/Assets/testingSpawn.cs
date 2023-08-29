using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class testingSpawn : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
       
    }
 public override void OnNetworkSpawn(){
 transform.position = new Vector3(177,204, 0);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
