using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public bool isTriggered = false;
    public float delay = 5f;
    public float blastRadius = 10;
    public float explosionForce = 10000;
    public int type;

    public GameObject explosionEffect;
    float countdown;
    bool isExploeded = false;
    public AudioClip soundEffect;
    // Start is called before the first frame update
    void Start()
    {
        countdown = delay;
    }

    // Update is called once per frame
    void Update()
    {
        if (isTriggered)
        {
            countdown -= Time.deltaTime;
            if (countdown <= 0 && !isExploeded)
            {
                Explode();
                isExploeded = true;
            }
        }
       
    }

    void Explode()
    {
        // show effect
        Instantiate(explosionEffect, transform.position, transform.rotation);
        AudioSource.PlayClipAtPoint(soundEffect, transform.position);

        // get nearby objects
        Collider[] colliders = Physics.OverlapSphere(transform.position, blastRadius);
        foreach (Collider nearbyObject in colliders)
        {
            // add force
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, blastRadius);
                
            }
        }


        //remove grenade

        Destroy(gameObject);

        Debug.Log("Grenade exploeded");
        
    }
}
