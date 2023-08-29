using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : MovementBase {
    public override void EnterState(ThirdPersonMovement player) {
        // Set animation state to idle
        
    }

    public override void UpdateState(ThirdPersonMovement player) {
        // Check if player is moving
        if (player.direction.magnitude >= 0.1f) {
            // If so, switch to the Walking state
            player.SwitchState(player.walkstate);
        }
    }
}

