using System.Collections.Generic;
using UnityEngine;

public class CarAi : MonoBehaviour
{
        [SerializeField] private Transform targetPositionTranform;

    private PrometeoCarController carDriver;
    private Vector3 targetPosition;

    private void Awake() {
        carDriver = GetComponent<PrometeoCarController>();
    }
    private void Update(){
        carDriver.setInputs(0.5f,0.5f);
    }

}