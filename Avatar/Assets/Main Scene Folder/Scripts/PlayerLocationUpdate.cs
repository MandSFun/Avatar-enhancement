using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerLocationUpdate : MonoBehaviour
{

    private int playerID;
    // Start is called before the first frame update
    void Start()
    {
        playerID = NetworkManagerUI.instance.playerID;
        int[] startingCoordinates = DatabaseScript.instance.getStartingLocation(playerID);
        int startingx = startingCoordinates[0];
        int startingy = startingCoordinates[1];
        int startingz = startingCoordinates[2];
        transform.position = new Vector3(startingx, startingy, startingz);
    }

    // Update is called once per frame
    void Update()
    {
     int xpos = (int)Math.Ceiling(transform.position.x);
     int ypos = (int)Math.Ceiling(transform.position.y);
     int zpos = (int)Math.Ceiling(transform.position.z);
     DatabaseScript.instance.UpdatePlayerLocation(playerID, xpos, ypos, zpos);
    }
}
