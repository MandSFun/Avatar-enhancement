using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
public class NPCpistol : MonoBehaviour
{


    [SerializeField] LayerMask layer;
    [SerializeField] Rig aimingRig;
    public float aimDuration = 0.3f;
    private bool aimStance;

    [SerializeField] private Camera sceneviewCamera;//Redundant, can remove if there is no camera.

    [SerializeField] GameObject weaponOnHand;
    [SerializeField] private Shooting gun;
    private void Start()
    {
       
    }
    private void Update()
    {

        //sceneviewcamera, ie.drone.
        //prevent Mouse input when sceneviewcam is active
        if (Input.GetMouseButtonDown(0) && !sceneviewCamera.isActiveAndEnabled)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layer))
            {   if (hit.collider.gameObject == gameObject) // Check if the raycast hit corresponds to the current GameObject
                {
                if (aimingRig.weight != 1)
                {

                    StartCoroutine(AimAtTarget());
                }
                else StartCoroutine(StopAiming());
                }
            }
        }
  


    }

    private IEnumerator AimAtTarget()
    {

        // Build the rig after modifying the source objects
        yield return new WaitForEndOfFrame();

        float elapsedTime = 0f;
        float startWeight = aimingRig.weight;
        float targetWeight = 1f;

        while (elapsedTime < aimDuration)
        {
            float t = elapsedTime / aimDuration;
            aimingRig.weight = Mathf.Lerp(startWeight, targetWeight, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        aimingRig.weight = targetWeight;

    }
    private IEnumerator StopAiming()
    {
        // Decrease the rig weight
        float elapsedTime = 0f;
        float startWeight = aimingRig.weight;
        float targetWeight = 0f;

        while (elapsedTime < aimDuration)
        {
            float t = elapsedTime / aimDuration;
            aimingRig.weight = Mathf.Lerp(startWeight, targetWeight, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        aimingRig.weight = targetWeight;

        // Wait for a short delay before clearing the source objects
        yield return new WaitForSeconds(0.1f);
    }
}
