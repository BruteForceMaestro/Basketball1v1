using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKController : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] float blockWeight;

    private Animator animator;
    bool blockingShot;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SetBlockIK(bool val)
    {
        blockingShot = val;
        
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (blockingShot)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, blockWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);

            animator.SetIKPosition(AvatarIKGoal.RightHand, Ball.Instance.transform.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, Ball.Instance.transform.rotation);
        }
    }
}
