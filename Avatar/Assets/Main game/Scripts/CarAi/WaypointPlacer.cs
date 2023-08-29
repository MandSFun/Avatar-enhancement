using UnityEngine;
using System.Collections.Generic;

public class WaypointPlacer : MonoBehaviour
{
    [SerializeField] private LayerMask roadLayerMask; // Layer mask for the road
    [SerializeField] private GameObject waypointPrefab;
    [SerializeField] public List<Waypoint> waypoints = new List<Waypoint>();
    private Camera freefly; // Declare the camera variable

    private void Start()
    {
        FreeFlyCamera freeFlyCamera = GetComponent<FreeFlyCamera>();

        // Check if the FreeFlyCamera component exists
        if (freeFlyCamera == null)
        {
            Debug.LogError("FreeFlyCamera component not found!");
            return;
        }

        // Get the camera from the FreeFlyCamera component
        freefly = freeFlyCamera.GetComponent<Camera>();

        // Check if the camera exists
        if (freefly == null)
        {
            Debug.LogError("Camera component not found in FreeFlyCamera!");
            return;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = freefly.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                if (hit.collider.CompareTag("road"))
                {
                    AddWaypoint(hit.point);
                }

            }
        }
    }

    public void AddWaypoint(Vector3 position)
    {
        position.y = GetGroundHeight(position);

        // Instantiate the waypoint prefab at the specified position
        GameObject waypointObject = Instantiate(waypointPrefab, position, Quaternion.identity);

        // Attach a waypoint component if not already present
        Waypoint waypoint = waypointObject.GetComponent<Waypoint>();
        if (waypoint == null)
        {
            waypoint = waypointObject.AddComponent<Waypoint>();
        }

        waypoint.SetGroundHeight(position.y);

        // Set the order of the waypoint based on the number of existing waypoints
        int newOrder = waypoints.Count + 1;
        waypoint.SetOrder(newOrder);

        // Add the waypoint to the list
        waypoints.Add(waypoint);
    }

    private float GetGroundHeight(Vector3 position)
    {
        if (Physics.Raycast(position, Vector3.down, out RaycastHit hit, Mathf.Infinity))
        {
            return hit.point.y;
        }

        return position.y;
    }
}
