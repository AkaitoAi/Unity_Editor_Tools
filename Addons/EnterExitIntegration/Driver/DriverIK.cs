using UnityEngine;

public class DriverIK : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform walkableCharacter;
    public DriverIKType driverIKType = DriverIKType.None;

    internal Transform leftHandIK, rightHandIK, leftFootIK, rightFootIK;
    
    [System.Serializable]
    public enum DriverIKType
    {
        None,
        JustHands,
        JustLegs,
        All
    }

    private void OnAnimatorIK()
    {
        if (animator == null) return;

        if (!animator.enabled) return;

        if (driverIKType == DriverIKType.None) return;

        if (driverIKType == DriverIKType.JustLegs)
        {
            if (leftFootIK) AnimatorIK(AvatarIKGoal.LeftFoot, leftFootIK);

            if (rightFootIK) AnimatorIK(AvatarIKGoal.RightFoot, rightFootIK);

            return;
        }

        if (driverIKType == DriverIKType.JustHands)
        {
            if (leftHandIK) AnimatorIK(AvatarIKGoal.LeftHand, leftHandIK);

            if (rightHandIK) AnimatorIK(AvatarIKGoal.RightHand, rightHandIK);

            return;
        }

        if (driverIKType == DriverIKType.All)
        {
            if (leftFootIK) AnimatorIK(AvatarIKGoal.LeftFoot, leftFootIK);

            if (rightFootIK) AnimatorIK(AvatarIKGoal.RightFoot, rightFootIK);

            if (leftHandIK) AnimatorIK(AvatarIKGoal.LeftHand, leftHandIK);

            if (rightHandIK) AnimatorIK(AvatarIKGoal.RightHand, rightHandIK);

            return;
        }

        void AnimatorIK(AvatarIKGoal ikGoal, Transform ikTransform)
        {
            animator.SetIKPositionWeight(ikGoal, 1);
            animator.SetIKRotationWeight(ikGoal, 1);
            animator.SetIKPosition(ikGoal, ikTransform.position);
            animator.SetIKRotation(ikGoal, ikTransform.rotation);
        }
    }
    private void ProvideIK(int instanceID,Transform leftHand = null, 
        Transform rightHand = null, Transform leftFoot = null, 
        Transform rightFoot = null)
    {
        if (transform.GetInstanceID() != instanceID) return;

        if (leftHand != null && rightHand != null)
            driverIKType = DriverIKType.JustHands;

        if (leftFoot != null && rightFoot != null)
            driverIKType = DriverIKType.JustLegs;

        if (leftHand == null && rightHand == null
            && leftFoot == null && rightFoot == null)
        {
            driverIKType = DriverIKType.None;
        }
        else driverIKType = DriverIKType.All;

        leftHandIK = leftHand;
        rightHandIK = rightHand;
        leftFootIK = leftFoot;
        rightFootIK = rightFoot;
    }

    private void OnEnable()
    {
        DriveableCar.OnProvideIKAction += ProvideIK;
    }

    private void OnDisable()
    {
        DriveableCar.OnProvideIKAction -= ProvideIK;
    }
}
