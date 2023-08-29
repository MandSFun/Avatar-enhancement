using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class NPCgun : MonoBehaviour
{
    [SerializeField] LayerMask layer;
    [SerializeField] Rig aimingRig;
    public float aimDuration = 0.3f;
    private bool aimStance;

    [SerializeField] private Camera sceneviewCamera;

    [SerializeField] GameObject weaponOnHand;
    Shooting gun;

    private void Start()
    {
        gun = weaponOnHand.GetComponent<Shooting>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !sceneviewCamera.isActiveAndEnabled)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layer))
            {
                if (hit.collider.gameObject == gameObject) // Check if the raycast hit corresponds to the current GameObject
                {
                    if (aimingRig.weight != 1)
                    {
                        StartCoroutine(AimAtTarget());
                    }
                    else
                    {
                        StartCoroutine(StopAiming());
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.V) && aimingRig.weight == 1)
        {
            gun.StartFiring();
        }
        if (Input.GetKeyUp(KeyCode.V) && aimingRig.weight == 1)
        {
            gun.StopFiring();
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
