using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class planeCameraController : MonoBehaviour
{
    [SerializeField] Transform[] povs;
    [SerializeField] float speed;

    public int index = -1;
    private Vector3 target;
    public static planeCameraController instance;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))index = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2))index = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3))index = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4))index = 3;
        else index = 4;

        target = povs[index].position;

    }

    private void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * speed);
        transform.forward = povs[index].forward;
    }
}
