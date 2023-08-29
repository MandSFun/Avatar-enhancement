using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterJet : MonoBehaviour
{
    // Flight physics parameters
    public float maxSpeed = 1000f;
    public float maxThrust = 20000f;
    public float liftCoefficient = 0.1f;
    public float dragCoefficient = 0.02f;
    public float rollSpeed = 100f;
    public float pitchSpeed = 100f;
    public float yawSpeed = 100f;
    public float maxBankAngle = 60f;

    // Input variables
    private float throttle;
    private float pitchInput;
    private float rollInput;
    private float yawInput;

    // Rigidbody reference
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Read inputs
        throttle = Input.GetAxis("Vertical");
        pitchInput = Input.GetAxis("Pitch");
        rollInput = Input.GetAxis("Roll");
        yawInput = Input.GetAxis("Horizontal");
    }

    private void FixedUpdate()
    {
        // Apply aerodynamic forces
        Vector3 lift = transform.up * (liftCoefficient * rb.velocity.sqrMagnitude);
        Vector3 drag = -rb.velocity.normalized * (dragCoefficient * rb.velocity.sqrMagnitude);

        rb.AddForce(lift + drag, ForceMode.Force);

        // Calculate thrust based on throttle input
        float currentThrust = maxThrust * throttle;
        rb.AddForce(transform.forward * currentThrust, ForceMode.Force);

        // Calculate torque for pitch, roll, and yaw control
        Vector3 pitchTorque = transform.right * pitchInput * pitchSpeed;
        Vector3 rollTorque = transform.forward * -rollInput * rollSpeed;
        Vector3 yawTorque = transform.up * yawInput * yawSpeed;

        rb.AddTorque(pitchTorque + rollTorque + yawTorque, ForceMode.Force);

        // Limit the bank angle to prevent unrealistic behavior
        Vector3 rotationAngles = rb.rotation.eulerAngles;
        rotationAngles.z = Mathf.Clamp(rotationAngles.z, -maxBankAngle, maxBankAngle);
        rb.rotation = Quaternion.Euler(rotationAngles);

        // Limit speed
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
    }
}
