using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Surrender : MonoBehaviour
{
    [SerializeField] private Rig surrenderRig;
    [SerializeField] private float transitionDuration = 0.5f;

    private Coroutine surrenderCoroutine;

    private bool surrenderActive = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleSurrender();
        }
    }

    private void ToggleSurrender()
    {
        surrenderActive = !surrenderActive;

        if (surrenderCoroutine != null)
        {
            StopCoroutine(surrenderCoroutine);
        }

        surrenderCoroutine = StartCoroutine(SurrenderTransition(surrenderActive));
    }

    private IEnumerator SurrenderTransition(bool activate)
    {
        float targetWeight = activate ? 1f : 0f;
        float currentWeight = surrenderRig.weight;

        float startTime = Time.time;
        float endTime = startTime + transitionDuration;

        while (Time.time < endTime)
        {
            float normalizedTime = (Time.time - startTime) / transitionDuration;
            surrenderRig.weight = Mathf.Lerp(currentWeight, targetWeight, normalizedTime);
            yield return null;
        }

        surrenderRig.weight = targetWeight;
        surrenderCoroutine = null;
    }
}
