using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerData 
{
    // Start is called before the first frame update
     public string PlayerName { get; private set; }

        public PlayerData(string playerName)
        {
            PlayerName = playerName;
        }
}
