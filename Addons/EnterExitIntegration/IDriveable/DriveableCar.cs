using DG.Tweening;
using System;
using UnityEngine;

public class DriveableCar : MonoBehaviour, IDriveable
{
    [SerializeField] private Transform target;

    [SerializeField] private Transform leftDoorTransform;

    [SerializeField] private Transform driverSeatTransform;

    [SerializeField] private NavigateToTarget navigateToTarget;

    [SerializeField] private Transform doorHandleIK;

    [SerializeField] private DriverIK driverIK;

    private RCC_CarControllerV3 carController;

    internal int driveableID;

    internal bool isLeftDoorOpen;

    public static event Action<int, Transform, 
        Transform, Transform, Transform> OnProvideIKAction;

    public static event Action<ControllerType> OnControllerChangedAction;

    public static event Action<Transform> OnControllerActiveAction;

    [Serializable]
    public struct DriverIK
    { 
        public Transform leftHandIK, rightHandIK, leftFootIK, rightFootIK;
    }

    public Transform Target
    {
        get => target;
        set => target = value;
    }

    public NavigateToTarget NavigateTo 
    { 
        get => navigateToTarget;
        set => navigateToTarget = value; 
    }

    public int DriveableInstanceID 
    { 
        get => driveableID = transform.root.GetInstanceID();
        set => driveableID = value;
    }

    private void OnEnable()
    {
        DriveableFinder.OnDriveableEnterAction += SetupNavigationTarget;
    }

    private void OnDisable()
    {
        DriveableFinder.OnDriveableEnterAction -= SetupNavigationTarget;
    }

    private void Awake()
    {
        carController = transform.root.GetComponent<RCC_CarControllerV3>();
    }
    private void SetupNavigationTarget(int id)
    { 
        if(DriveableInstanceID != id) return;

        NavigateTo.destination = Target;
    }

    public void EnterDriveable(Transform driver)
    {
        if (driver.TryGetComponent(out Animator animator))
        {
            animator.CrossFadeInFixedTime("EnteringCar", .5f);
            animator.SetBool("SitDirect", true);
        }

        leftDoorTransform.DOBlendableRotateBy(new Vector3(0, 45f, 0), 1f).SetDelay(.5f)
            .OnComplete( () =>
            {
                if (driver.TryGetComponent(out Animator animator))
                {
                    animator.CrossFadeInFixedTime("EnterCarSecondHalf", .5f);

                    leftDoorTransform.DOBlendableRotateBy(new Vector3(0, -45f, 0), 1f);

                    OnProvideIKAction?.Invoke(driver.GetInstanceID(),
                    null, null, null, null);
                }

                //driver.transform.position = navigateToTarget.destination.position;
                //driver.transform.rotation = navigateToTarget.destination.rotation;

                //driver.DOMove(new Vector3( navigateToTarget.destination.position.x,
                //    driver.position.y, navigateToTarget.destination.position.z), 1f).SetEase(Ease.Linear);

                driver.DOMove(driverSeatTransform.position, 1f).SetEase(Ease.Linear).
                OnPlay(() => {

                    driver.DORotate(driverSeatTransform.eulerAngles, 1f).SetEase(Ease.Linear);
                }).
                        OnComplete(() => {
                            
                            driver.transform.position = driverSeatTransform.position;
                            driver.transform.rotation = driverSeatTransform.rotation;

                            OnProvideIKAction?.Invoke(driver.GetInstanceID(),
                            driverIK.leftHandIK, driverIK.rightHandIK,
                            driverIK.leftFootIK, driverIK.rightFootIK);

                            driver.SetParent(driverSeatTransform);

                            OnControllerChangedAction?.Invoke(ControllerType.RCC);


                            carController.enabled = true;
                            carController.StartEngine();
                            carController.canControl = true;
                        });
            });
    }

    public void ExitDriveable(Transform driver)
    {
        if (driver.TryGetComponent(out Animator animator))
        {
            animator.CrossFadeInFixedTime("Exiting Car", 1f);

            OnProvideIKAction?.Invoke(driver.GetInstanceID(),
                    null, null, null, null);
        }

        leftDoorTransform.DOBlendableRotateBy(new Vector3(0, 45f, 0), 1f).SetDelay(.25f)
            .OnComplete(() =>
            {
                driver.DOMove(navigateToTarget.destination.position, 1f)
                    .SetEase(Ease.Linear)
                    .OnComplete(() => {

                        carController.enabled = false;
                        carController.KillEngine();
                        carController.canControl = false;

                        if (driver.TryGetComponent(out Animator animator))
                        {
                            //animator.CrossFadeInFixedTime("EnterCarSecondHalf", .5f);

                            leftDoorTransform.DOBlendableRotateBy(new Vector3(0, -45f, 0), 1f)
                                .OnComplete(() => {

                                    driver.parent = null;

                                    OnControllerChangedAction?.Invoke(ControllerType.TPS);

                                    OnControllerActiveAction?.Invoke(driver);

                                });
                        }
                    });
            });
    }
}
