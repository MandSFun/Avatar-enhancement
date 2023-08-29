using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorIKManager : MonoBehaviour
{
      private static Vector3 ikTargetPosition;

    public static void SetIKTargetPosition(Vector3 position)
    {
        ikTargetPosition = position;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetIKPosition(AvatarIKGoal.RightHand, ikTargetPosition);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
        }
    }
}
