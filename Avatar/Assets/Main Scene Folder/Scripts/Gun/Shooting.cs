using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Shooting : MonoBehaviour
{
    public ParticleSystem[] muzzleflash;
    public ParticleSystem impactEffect;
    public GameObject shootOrigin;
    public void isFiring()
    {
        foreach (var particle in muzzleflash)
        {
            particle.Emit(1);
        }
        Shoot();
    }
    private void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(shootOrigin.transform.position, shootOrigin.transform.forward, out hit))
        {
            impactEffect.transform.position = hit.point;
            impactEffect.transform.rotation = Quaternion.LookRotation(hit.normal);
            impactEffect.Emit(1);
        }
    }



}
