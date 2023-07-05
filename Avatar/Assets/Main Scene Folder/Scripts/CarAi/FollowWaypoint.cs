using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FollowWaypoint : MonoBehaviour
{
    public WaypointPlacer waypointPlacer;
    public PrometeoCarController carController;
    public float waypointThreshold = 0.2f;
    public WheelCollider frontLeftCollider;
    public float rotationThreshold = 1f; // Adjust this value to control the rotation threshold

    private List<Waypoint> waypoints;
    private int currentWaypointIndex;
    private float AbscarSpeed;
    private float moveSpeed = 5f; // Adjust this value to control the car's movement speed
    private bool isLastWaypoint;
    public bool isLooping;

    private void Start()
    {
        UpdateWaypoints();
        carController = GetComponent<PrometeoCarController>();
        currentWaypointIndex = 0;
        isLastWaypoint = false;
        isLooping = false;
        
    }

    private void Update()
    {
        AbscarSpeed = (2 * Mathf.PI * frontLeftCollider.radius * frontLeftCollider.rpm * 60) / 1000;

        if (waypoints == null || waypoints.Count == 0)
        {
            carController.setInputs(0, 0);
            return;
        }

        if (currentWaypointIndex >= waypoints.Count)
        {
            // No more waypoints, stop the car's steering input
            carController.setInputs(carController.throttle, 0);
            return;
        }

        Waypoint currentWaypoint = waypoints[currentWaypointIndex];
        Vector3 direction = currentWaypoint.transform.position - transform.position;

        Quaternion targetRotation = Quaternion.LookRotation(direction, transform.up);

        // Calculate the steering angle based on the direction to the waypoint
        float angle = Vector3.SignedAngle(transform.forward, direction, transform.up);
        float steering = angle * carController.steeringSpeed;

        // Set the steering value directly to the car controller
        carController.setInputs(carController.throttle, steering);

        // Check if the car is close enough to the target rotation
        if (!isLastWaypoint && Quaternion.Angle(transform.rotation, targetRotation) > rotationThreshold)
        {
            // Rotate the car towards the target rotation using interpolation
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * carController.steeringSpeed);
        }
        else
        {
            // Set the car's rotation directly to the target rotation
            transform.rotation = targetRotation;
        }

        float distanceToWaypoint = direction.magnitude;

        if (distanceToWaypoint < waypointThreshold)
        {
            // Reached the current waypoint, move to the next one
            if (currentWaypointIndex == waypoints.Count - 1)
            {
                if (!isLastWaypoint)
                {
                    // Reached the assumed last waypoint, allow rotation adjustment for new waypoints
                    isLastWaypoint = true;
                }
                else
                {
                    // Reached the actual last waypoint, stop the car's steering input
                    carController.setInputs(carController.throttle, 0);

                    // Check if looping is enabled
                    if (isLooping)
                    {
                        // Go back to the first waypoint
                        currentWaypointIndex = 0;
                    }

                    return;
                }
            }

            currentWaypointIndex++;
        }

        // Pass the throttle value to the car controller
        float throttle = Mathf.Clamp01(distanceToWaypoint / waypointThreshold) * carController.maxSpeed;

        if (carController.throttle > 0)
        {
            carController.setInputs(throttle, steering);
        }
        else if (carController.throttle < 0)
        {
            carController.setInputs(-throttle, -steering);
        }
        else
        {
            carController.setInputs(0, steering);
        }

        // Move the car forward
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }

    private void UpdateWaypoints()
    {
        GameObject waypointPlacerObject = GameObject.FindGameObjectWithTag("Sceneview");
        if (waypointPlacerObject != null)
        {
            waypointPlacer = waypointPlacerObject.GetComponent<WaypointPlacer>();
            if (waypointPlacer != null)
            {
                waypoints = waypointPlacer.waypoints;
            }
            else
            {
                Debug.LogWarning("WaypointPlacer component not found on the object with the 'WaypointPlacer' tag!");
            }
        }
        else
        {
            Debug.LogWarning("WaypointPlacer object not found in the scene with the 'WaypointPlacer' tag!");
        }
    }

    public void ToggleLoop()
    {
        isLooping = !isLooping;
    }
}
