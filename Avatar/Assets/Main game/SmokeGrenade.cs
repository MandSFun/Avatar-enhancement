using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeGrenade : MonoBehaviour
{
    public bool isTriggered = false;
    public float delay = 5f;


    public GameObject smokeEffect;
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
                TriggerSmoke();
                isExploeded = true;
            }
        }

    }

    void TriggerSmoke()
    {
        // show effect
        Instantiate(smokeEffect, transform.position, transform.rotation);
        AudioSource.PlayClipAtPoint(soundEffect, transform.position);
        Destroy(gameObject);
        Debug.Log("Smoke Grenade exploeded");

    }
}
