using UnityEngine;

public class Waypoint : MonoBehaviour
{ [SerializeField] private float groundHeight;
    [SerializeField] private int order;

    public float GroundHeight { get { return groundHeight; } }
    public int Order { get { return order; } }

    public void SetGroundHeight(float height)
    {
        groundHeight = height;
    }

    public void SetOrder(int newOrder)
    {
        order = newOrder;
    }
    
}
