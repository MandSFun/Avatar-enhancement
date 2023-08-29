using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Walking : MovementBase {
    public override void EnterState(ThirdPersonMovement player) {
        // Set animation state to walking
        player.anim.SetBool("walking", true);
    }

    public override void UpdateState(ThirdPersonMovement player) {
        // Move the player in the direction they are facing
        player.controller.Move(player.direction * player.movementSpeed * Time.fixedDeltaTime);

        // Check if player is moving
        if (player.direction.magnitude > 0) {
            // If so, continue in the Walking state
        }
        else {
            // If not, switch back to the Idle state
            player.SwitchState(player.basestate);
        }
    }
  
}
