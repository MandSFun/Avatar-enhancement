using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class planeController : MonoBehaviour
{

    [SerializeField] public TMP_Text airSpeed;
 

    public float throttleIncrement = 0.1f;
    public float maxThrust = 200f;
    public float responsiveness = 10f;
    public float lift = 145f;



    private float throttle;
    private float roll;
    private float pitch;
    private float yaw;

    Rigidbody rb;
    private float responseModifier
    {
        get
        {
            return (rb.mass / 10f) * responsiveness;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void HandleInputs()
    {
        roll = Input.GetAxis("Horizontal");
        pitch = Input.GetAxis("Vertical");
        yaw = Input.GetAxis("Yaw");

        if (Input.GetKey(KeyCode.Space))
        {
            throttle += throttleIncrement;
        }else if (Input.GetKey(KeyCode.LeftShift))
        {
            throttle -= throttleIncrement;
        }
            
        throttle = Mathf.Clamp(throttle, 0f, 1000f);
    }

    
    private void Update()
    {

        if (planeCameraController.instance.index != 4)
        {
            HandleInputs();
            airSpeed.text = throttle.ToString();
        }
        

    }

    private void FixedUpdate()
    {
        rb.AddForce(-transform.forward * maxThrust * throttle);
        rb.AddTorque(transform.up * yaw * responseModifier * 3);
        rb.AddTorque(transform.right * pitch * responseModifier);
        rb.AddTorque(transform.forward * roll * responseModifier /2);


        rb.AddForce(Vector3.up * rb.velocity.magnitude * lift);
    }
}
