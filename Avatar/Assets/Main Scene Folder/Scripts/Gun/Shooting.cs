using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    public ParticleSystem[] muzzleflash;
    public ParticleSystem impactEffect;
    public GameObject shootOrigin;
    float accumulatedTime;
    bool isFiringEnabled;
    public int fireRate = 25;

    private void Update()
    {
        if (isFiringEnabled)
        {
            accumulatedTime += Time.deltaTime;
            float fireInterval = 1f / fireRate;

            while (accumulatedTime >= fireInterval)
            {
                Shoot();
                accumulatedTime -= fireInterval;
            }
        }
    }

    public void StartFiring()
    {
        isFiringEnabled = true;
    }

    public void StopFiring()
    {
        isFiringEnabled = false;
    }

    private void Shoot()
    {
        foreach (var particle in muzzleflash)
        {
            particle.Emit(1);
        }

        RaycastHit hit;
        if (Physics.Raycast(shootOrigin.transform.position, shootOrigin.transform.forward, out hit))
        {
            impactEffect.transform.position = hit.point;
            impactEffect.transform.rotation = Quaternion.LookRotation(hit.normal);
            impactEffect.Emit(1);
        }
    }
}
