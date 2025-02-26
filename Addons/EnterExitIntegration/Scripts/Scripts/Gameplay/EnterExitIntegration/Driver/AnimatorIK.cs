using System;
using UnityEngine;
using Driveable.Core;

public class AnimatorIK : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public AnimatorIKType animatorIKType = AnimatorIKType.None;

    internal Transform leftHandIK, rightHandIK, 
        leftFootIK, rightFootIK, lookAtIK;
    
    [Serializable]
    public enum AnimatorIKType
    {
        None,
        JustLook,
        JustHands,
        JustLegs,
        All
    }

    private void OnAnimatorIK()
    {
        if (animator == null) return;

        if (!animator.enabled) return;

        if (animatorIKType == AnimatorIKType.None) return;

        if (animatorIKType == AnimatorIKType.JustLook)
        {
            AnimatorLookIK(lookAtIK);

            return;
        }

        if (animatorIKType == AnimatorIKType.JustLegs)
        {
            AnimatorIK(AvatarIKGoal.LeftFoot, leftFootIK);

            AnimatorIK(AvatarIKGoal.RightFoot, rightFootIK);

            return;
        }

        if (animatorIKType == AnimatorIKType.JustHands)
        {
            AnimatorIK(AvatarIKGoal.LeftHand, leftHandIK);

            AnimatorIK(AvatarIKGoal.RightHand, rightHandIK);

            return;
        }

        if (animatorIKType == AnimatorIKType.All)
        {
            AnimatorLookIK(lookAtIK);

            AnimatorIK(AvatarIKGoal.LeftFoot, leftFootIK);

            AnimatorIK(AvatarIKGoal.RightFoot, rightFootIK);

            AnimatorIK(AvatarIKGoal.LeftHand, leftHandIK);

            AnimatorIK(AvatarIKGoal.RightHand, rightHandIK);

            return;
        }

        void AnimatorLookIK(Transform lookAt)
        {
            if (lookAt == null)
            {
                animator.SetLookAtWeight(0);

                return;
            }

            animator.SetLookAtWeight(1);
            animator.SetLookAtPosition(lookAt.position);
        }

        void AnimatorIK(AvatarIKGoal ikGoal, Transform ikTransform)
        {
            if (ikTransform == null) 
            {
                animator.SetIKPositionWeight(ikGoal, 0);
                animator.SetIKRotationWeight(ikGoal, 0);

                return; 
            }

            animator.SetIKPositionWeight(ikGoal, 1);
            animator.SetIKRotationWeight(ikGoal, 1);
            animator.SetIKPosition(ikGoal, ikTransform.position);
            animator.SetIKRotation(ikGoal, ikTransform.rotation);
        }
    }
    private void ProvideIK(int instanceID, Transform lookAt, Transform leftHand = null,
        Transform rightHand = null, Transform leftFoot = null, 
        Transform rightFoot = null)
    {
        if (transform.GetInstanceID() != instanceID) return;

        if (lookAt != null)
            animatorIKType = AnimatorIKType.JustLook;

        if (leftHand != null || rightHand != null)
            animatorIKType = AnimatorIKType.JustHands;

        if (leftFoot != null || rightFoot != null)
            animatorIKType = AnimatorIKType.JustLegs;

        if (leftHand == null || rightHand == null
            && leftFoot == null || rightFoot == null)
        {
            animatorIKType = AnimatorIKType.None;
        }
        else animatorIKType = AnimatorIKType.All;

        lookAtIK = lookAt;
        leftHandIK = leftHand;
        rightHandIK = rightHand;
        leftFootIK = leftFoot;
        rightFootIK = rightFoot;
    }
    
    private void ProvideIK(OnProvideIK ik)
    {
        ProvideIK(ik.instanceID, 
            ik.lookAt, 
            ik.leftHand, 
            ik.rightHand, 
            ik.leftFoot, 
            ik.rightFoot);
    }


    EventBinding<OnProvideIK> provideIKEventBinding;
    private void OnEnable()
    {
        provideIKEventBinding = new EventBinding<OnProvideIK>(ProvideIK);
        EventBus<OnProvideIK>.Register(provideIKEventBinding);
    }

    private void OnDisable()
    {
        EventBus<OnProvideIK>.Deregister(provideIKEventBinding);
    }
}
