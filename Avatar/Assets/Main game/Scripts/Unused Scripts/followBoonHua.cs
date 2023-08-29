using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class followBoonHua : MonoBehaviour
{private float Rotationspeed = 5.0f;
Vector3 offset = new Vector3(0,3,-1);
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {float horizontalinput = Input.GetAxis("Horizontal");
    transform.Rotate(Vector3.up,horizontalinput*Rotationspeed   *Time.deltaTime);
        
    }
}
