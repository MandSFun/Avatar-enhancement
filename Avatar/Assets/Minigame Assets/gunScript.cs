using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gunScript : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 15f;
    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    private object hit;
    public GameObject impactEffect;
    public float impactForce = 30f;
    private float nextTimeToFire = 0f;
    public AudioClip soundEffect;
    bool recoil = false;

    [SerializeField] private bool addBulletSpread = false;
    [SerializeField] private Vector3 bulletSpreadVariance = new Vector3(0, 0, 0);
    [SerializeField] private ParticleSystem shootingSystem;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] ParticleSystem impactParticleSystem;
    [SerializeField] private TrailRenderer bulletTrial;
    private Animator animator;



    private void Awake()
    {
        animator = GetComponent<Animator>();
    }


    // Update is called once per frame
    void Update()
    {

        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            shoot();

        }

    }

    void shoot()
    {
        
        shootingSystem.Play();
        muzzleFlash.Play();
        AudioSource.PlayClipAtPoint(soundEffect, transform.position);
        Vector3 direction = GetDirection();
        RaycastHit hit;

        //direction = fpsCam.transform.forward;

        //if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        if (Physics.Raycast(bulletSpawnPoint.position, direction, out hit)) 
        {

            //new stuff
            TrailRenderer trailRenderer = Instantiate(bulletTrial, bulletSpawnPoint.position, Quaternion.identity);
            StartCoroutine(SpawnTrail(trailRenderer,hit));
            Debug.Log(hit.transform.name);
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }


            if (hit.rigidbody != null)
            {

                hit.rigidbody.AddForce(-hit.normal * impactForce);

            }

            
           


        }

        GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
        Destroy(impactGO, 2f);


    }



    private Vector3 GetDirection()
    {
        Vector3 direction = fpsCam.transform.forward;
        if (addBulletSpread)
        {
            direction += new Vector3(Random.Range(-bulletSpreadVariance.x, bulletSpreadVariance.x),
                Random.Range(-bulletSpreadVariance.y, bulletSpreadVariance.y),
                Random.Range(-bulletSpreadVariance.z, bulletSpreadVariance.y)
                );


            direction.Normalize();
        }

        return direction;
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, RaycastHit hit)
    {
        float time = 0;
        Vector3 startPosition = trail.transform.position;
        while (time < 1)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hit.point, time);
            time += Time.deltaTime/trail.time;
            
            yield return null;
        }
 
        trail.transform.position = hit.point;
        Instantiate(impactParticleSystem, hit.point, Quaternion.LookRotation(hit.normal));
        Destroy(trail.gameObject, trail.time);

    }

}
