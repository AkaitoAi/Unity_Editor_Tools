using AkaitoAi.Extensions;
using ArcadeBP_Pro;
using DG.Tweening;
using UnityEngine;
using Driveable.Core;
using Driveable;

public class DriveableBike : MonoBehaviour, IDriveable<ArcadeBikeControllerPro>
{
    [SerializeField] private Transform root;

    [SerializeField] private Transform target;
    
    [SerializeField] private NavigateToTarget navigateToTarget;

    [SerializeField] private GameObject enterExitCamera;

    [SerializeField] private Transform driverSeatTransform;

    [SerializeField] private GameObject[] driverAndIK;
    [SerializeField] private Collider[] wheelColliders;
    [SerializeField] private DriverIK driverIK;

    private ArcadeBikeControllerPro bikeController;
    private Coroutine brakeCoroutine;
    private int driverID;

    private static readonly int getOnBikeHash = Animator.StringToHash("Get On Bike");
    private static readonly int getOffBikeHash = Animator.StringToHash("Get Off Bike");

    internal int driveableID;

    #region Properties
    public int DriveableInstanceID
    {
        get => driveableID = root.GetInstanceID();
        set => driveableID = value;
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
    public ArcadeBikeControllerPro DriveableController 
    { 
        get => bikeController; 
        set => bikeController = value; 
    }
    public Transform Root { get => root; set => root = value; }
    #endregion
    private void Awake()
    {
        DriveableController = root.GetComponent<ArcadeBikeControllerPro>();
        BikeState();
    }

    private void OnEnable()
    {
        DriveableFinder.OnDriveableEnterAction += SetupNavigationTarget;

        AnimationEvents.OnAnimationEventTriggered += AssignIKDuringAnimation;
    }

    private void OnDisable()
    {
        DriveableFinder.OnDriveableEnterAction -= SetupNavigationTarget;
        AnimationEvents.OnAnimationEventTriggered -= AssignIKDuringAnimation;
    }

    private void SetupNavigationTarget(int id)
    {
        if (DriveableInstanceID != id) return;

        NavigateTo.destination = Target;
    }
    
    public void EnterDriveable(Transform driver)
    {
        driverID = driver.GetInstanceID();

        if (brakeCoroutine != null) StopCoroutine(brakeCoroutine);

        if (driver.TryGetComponent(out Animator animator))
        {
            animator.CrossFadeInFixedTime(getOnBikeHash, .5f);

            driver.position = target.position;


            driver.DOMove(driverSeatTransform.position, .1f).SetEase(Ease.Linear).
            OnPlay(() => {

                driver.DORotate(driverSeatTransform.eulerAngles, .1f).SetEase(Ease.Linear);
                EventBus<OnControllerChanged>.Raise(new OnControllerChanged
                { controller = ControllerType.TPSDriveableBrain });

                enterExitCamera.SetActive(true);

            }).OnComplete(() => {

                driver.transform.position = driverSeatTransform.position;
                driver.transform.rotation = driverSeatTransform.rotation;

                driver.SetParent(driverSeatTransform);

                StartCoroutine(AkaitoAiExtensions.SimpleDelay(1.5f, () => {
                    
                    enterExitCamera.SetActive(false);

                    driver.position = target.position;
                    driver.rotation = target.rotation;
                    driver.gameObject.SetActive(false);

                    DriveableController.enabled = true;
                    BikeState();
                    DriveableController.StartBike();

                    DriveableController.bikeReferences.BikeRb.drag = 0f;

                    EventBus<OnControllerChanged>.Raise(new OnControllerChanged
                    { controller = ControllerType.Bike });
                
                }));
            });
        }
    }
    
    public void ExitDriveable(Transform driver)
    {
        driverID = driver.GetInstanceID();

        StartCoroutine(AkaitoAiExtensions.SimpleDelay(1f, () =>
        {
            DriveableController.enabled = false;
            BikeState();
        }));

        brakeCoroutine = StartCoroutine(this.WaitWhile(() => DriveableController.enabled, () =>
        {
            DriveableController.bikeReferences.BikeRb.drag = 5f;
        }, () => { 

            if (driver.TryGetComponent(out Animator animator))
            {
                driver.transform.position = driverSeatTransform.position;
                driver.transform.rotation = driverSeatTransform.rotation;

                driver.gameObject.SetActive(true);

                DriveableController.enabled = false;
                BikeState();

                animator.CrossFadeInFixedTime(getOffBikeHash, 1f);

                EventBus<OnControllerChanged>.Raise(new OnControllerChanged
                { controller = ControllerType.TPSDriveableBrain });

                enterExitCamera.SetActive(true);
            }


            StartCoroutine(AkaitoAiExtensions.SimpleDelay(2f, () =>
            {
                driver.position = target.position;
                driver.rotation = target.rotation;
                driver.parent = null;

                enterExitCamera.SetActive(false);

                EventBus<OnControllerChanged>.Raise(new OnControllerChanged
                { controller = ControllerType.TPS });

                EventBus<OnTPSActive>.Raise(new OnTPSActive
                { transform = driver });

            }));
        }));
    }
    
    private void BikeState()
    {
        bool isBikeActive = DriveableController.enabled;

        foreach (Collider cl in wheelColliders)
            cl.enabled = !isBikeActive;

        foreach (GameObject go in driverAndIK)
            go.SetActive(isBikeActive);
    }
    
    private void AssignIKDuringAnimation(string eventName)
    {
        switch (eventName)
        {
            case "OnLeftHandIK": //2nd

                EventBus<OnProvideIK>.Raise(new OnProvideIK
                {
                    instanceID = driverID,
                    lookAt = null,
                    leftHand = driverIK.leftHandIK,
                    rightHand = null,
                    leftFoot = driverIK.leftFootIK,
                    rightFoot = null
                });


                root.DORotate(new Vector3(root.eulerAngles.x,
                    root.eulerAngles.y, 0), 1f).SetEase(Ease.Linear);


                break;

            case "OnRightHandIK": //4th

                EventBus<OnProvideIK>.Raise(new OnProvideIK
                {
                    instanceID = driverID,
                    lookAt = null,
                    leftHand = driverIK.leftHandIK,
                    rightHand = driverIK.rightHandIK,
                    leftFoot = driverIK.leftFootIK,
                    rightFoot = driverIK.rightFootIK
                });

                break;

            case "OnLeftFootIK": //1st

                EventBus<OnProvideIK>.Raise(new OnProvideIK
                {
                    instanceID = driverID,
                    lookAt = null,
                    leftHand = null,
                    rightHand = null,
                    leftFoot = driverIK.leftFootIK,
                    rightFoot = null
                });

                break;

            case "OnRightFootIk": //3rd

                EventBus<OnProvideIK>.Raise(new OnProvideIK
                {
                    instanceID = driverID,
                    lookAt = null,
                    leftHand = driverIK.leftHandIK,
                    rightHand = null,
                    leftFoot = driverIK.leftFootIK,
                    rightFoot = driverIK.rightFootIK
                });

                break;

            case "OffRightSideIK": //1st

                EventBus<OnProvideIK>.Raise(new OnProvideIK
                {
                    instanceID = driverID,
                    lookAt = null,
                    leftHand = driverIK.leftHandIK,
                    rightHand = null,
                    leftFoot = driverIK.leftFootIK,
                    rightFoot = null
                });

                break;

            case "OffLeftHandIK": //2nd

                EventBus<OnProvideIK>.Raise(new OnProvideIK
                {
                    instanceID = driverID,
                    lookAt = null,
                    leftHand = null,
                    rightHand = null,
                    leftFoot = driverIK.leftFootIK,
                    rightFoot = null
                });

                break;

            case "OffLeftFootIK": //3rd

                EventBus<OnProvideIK>.Raise(new OnProvideIK
                {
                    instanceID = driverID,
                    lookAt = null,
                    leftHand = null,
                    rightHand = null,
                    leftFoot = null,
                    rightFoot = null
                });


                break;
        }
    }
}
