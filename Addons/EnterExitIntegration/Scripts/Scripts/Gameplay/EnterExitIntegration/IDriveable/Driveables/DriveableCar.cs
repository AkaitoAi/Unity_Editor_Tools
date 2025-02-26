using AkaitoAi.Extensions;
using DG.Tweening;
using UnityEngine;
using Driveable.Core;
using Driveable;
using System;
using UnityEngine.Events;
public class DriveableCar : MonoBehaviour, IDriveable<RCC_CarControllerV3>
{
    [SerializeField] private Transform root;

    [SerializeField] private Transform target;

    [SerializeField] private Transform leftDoorTransform;

    [SerializeField] private Transform driverSeatTransform;
    [SerializeField] private Transform npcDriverDropTransform;
    [SerializeField] private Transform npcDriverGrabPointTransform;

    [SerializeField] private Transform doorHandleIK;

    [SerializeField] private GameObject enterExitCamera;

    [SerializeField] private float doorRotateAroundAngle = 45f;

    [SerializeField] private Driver driver;
    
    [SerializeField] private NavigateToTarget navigateToTarget;

    [Space]
    [SerializeField] private GameObject[] activateWithController;
    [SerializeField] private Collider[] wheelColliders;

    [Space]
    [SerializeField] private DoorOpenCloseSounds doorOpenCloseSounds;

    [SerializeField] private DriverIK driverIK;

    [Space]
    [SerializeField] private UnityEvent OnDriveableEntered;
    [SerializeField] private UnityEvent OnDriveableExited;

    private RCC_SceneManager rccSceneManager;
    private Vector3 leftDoorInitialEuler;
    private Coroutine brakeCoroutine;
    private RCC_CarControllerV3 carController;
    private Transform tpsCameraTransform;
    private Animator tpsDriverAnimator;

    private GameObject npcDriverClone;
    private Animator npcDriverAnimator;
    private AnimationEvents npcAnimationEvents;

    private static readonly int idleHash = Animator.StringToHash("Idle");
    private static readonly int enteringCarHash = Animator.StringToHash("EnteringCar");
    private static readonly int enteringCarSecondHalfHash = Animator.StringToHash("EnterCarSecondHalf");
    private static readonly int exitingCarHash = Animator.StringToHash("Exiting Car");
    private static readonly string pulledFromSeatString = "Pulled From Seat";
    private static readonly string startPullingString = "Pull Heavy Object Start";
    private static readonly string pullingString = "Pull Heavy Object";
    private static readonly string stopPullingString = "Pull Heavy Object Stop";

    internal int driveableID;

    public static event Action<int> OnDriveableEnterAction;

    #region Properties
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
        get => driveableID = Root.GetInstanceID();
        set => driveableID = value;
    }
    public RCC_CarControllerV3 DriveableController 
    { 
        get => carController; 
        set => carController = value; }

    public Transform Root { get => root; set => root = value; }

    #endregion

    EventBinding<OnDriveableCameraSwitch> driveableCameraSwitchEventBinding;
    EventBinding<OnTrafficToRCC> trafficToRCCEventBinding;

    private void OnEnable()
    {
        DriveableFinder.OnDriveableEnterAction += SetupNavigationTarget;

        driveableCameraSwitchEventBinding = new EventBinding<OnDriveableCameraSwitch>(StoreTPSCameraTransform);
        EventBus<OnDriveableCameraSwitch>.Register(driveableCameraSwitchEventBinding);
        
        trafficToRCCEventBinding = new EventBinding<OnTrafficToRCC>(TrafficToRCC);
        EventBus<OnTrafficToRCC>.Register(trafficToRCCEventBinding);
    }

    private void OnDisable()
    {
        DriveableFinder.OnDriveableEnterAction -= SetupNavigationTarget;

        EventBus<OnDriveableCameraSwitch>.Deregister(driveableCameraSwitchEventBinding);

        EventBus<OnTrafficToRCC>.Deregister(trafficToRCCEventBinding);
    }

    private void Awake()
    {
        rccSceneManager = RCC_SceneManager.Instance;
        
        DriveableController = Root.GetComponent<RCC_CarControllerV3>();
        DriveableController.useCollisionParticles = false;
        DriveableController.useCollisionAudio = false;
        DriveableController.enabled = false;
        
        leftDoorInitialEuler = leftDoorTransform.localEulerAngles;

        doorOpenCloseSounds.audioSource = AkaitoAiExtensions.GetOrAddComponent<AudioSource>(gameObject);
        doorOpenCloseSounds.audioSource.playOnAwake = false;
    }
    
    private void SetupNavigationTarget(int id)
    { 
        if(DriveableInstanceID != id) return;

        NavigateTo.destination = Target;
    }

    public void EnterDriveable(Transform driver)
    {
        if (brakeCoroutine != null) StopCoroutine(brakeCoroutine);
        
        DoorOpeningProcees(driver);

        leftDoorTransform.DOKill();
        leftDoorTransform.localEulerAngles = leftDoorInitialEuler;

        leftDoorTransform.DOBlendableRotateBy(new Vector3(0, doorRotateAroundAngle, 0), 1f).SetDelay(.5f)
            .OnPlay(() =>
            {
                doorOpenCloseSounds.PlayAudioClip(doorOpenCloseSounds.openAudioClip);
            })
            .OnComplete(() =>
            {
                if (this.driver.driverSO == null) SittingProcess(driver);
                else RemoveNPCDriver(driver);
            });
        
        void RemoveNPCDriver(Transform driver)
        {
            driver.DOMove(npcDriverGrabPointTransform.position, 1f).SetEase(Ease.Linear);
            driver.DORotate(npcDriverGrabPointTransform.eulerAngles, 1f).SetEase(Ease.Linear);

            tpsDriverAnimator.CrossFadeInFixedTime(startPullingString, 1f);
            StartCoroutine(this.WaitForAnimation(tpsDriverAnimator, startPullingString, 0, () =>
            {

                tpsDriverAnimator.CrossFadeInFixedTime(pullingString, 1f);

                EventBus<OnProvideIK>.Raise(new OnProvideIK
                {
                    instanceID = driver.GetInstanceID(),
                    lookAt = npcDriverClone.transform,
                    leftHand = npcAnimationEvents.bodyIK.leftHandIK,
                    rightHand = npcAnimationEvents.bodyIK.rightHandIK,
                    leftFoot = null,
                    rightFoot = null
                });

                npcDriverAnimator.CrossFadeInFixedTime(pulledFromSeatString, .5f);
                npcDriverClone.transform.DOMove(npcDriverDropTransform.position, 1.5f).SetEase(Ease.Linear).SetDelay(1.25f);
                npcDriverClone.transform.DORotate(npcDriverDropTransform.eulerAngles, 1.5f).SetEase(Ease.Linear).SetDelay(1.25f);

                StartCoroutine(this.WaitForAnimation(tpsDriverAnimator, pullingString, 0, () =>
                {

                    tpsDriverAnimator.CrossFadeInFixedTime(stopPullingString, 1f);

                    StartCoroutine(this.WaitForAnimation(tpsDriverAnimator, stopPullingString, 0, () =>
                    {
                        this.driver.driverSO = null;

                        npcDriverClone.transform.parent = null;

                        Destroy(npcDriverClone, 2f);

                        SittingProcess(driver);
                    }));

                }));
            }));
        }

        void SittingProcess(Transform driver)
        {
            
             tpsDriverAnimator?.CrossFadeInFixedTime(enteringCarSecondHalfHash, .5f);


             driver.DOMove(driverSeatTransform.position, .1f).SetEase(Ease.Linear).
             OnPlay(() =>
             {
                 driver.DORotate(driverSeatTransform.eulerAngles, .1f).SetEase(Ease.Linear);

                 EventBus<OnProvideIK>.Raise(new OnProvideIK
                 {
                     instanceID = driver.GetInstanceID(),
                     lookAt = null,
                     leftHand = null,
                     rightHand = null,
                     leftFoot = driverIK.leftFootIK,
                     rightFoot = driverIK.rightFootIK
                 });
             });

             leftDoorTransform.DOBlendableRotateBy(new Vector3(0, -doorRotateAroundAngle, 0), 1f).SetDelay(.25f).
             OnPlay(() =>
             {

                 EventBus<OnProvideIK>.Raise(new OnProvideIK
                 {
                     instanceID = driver.GetInstanceID(),
                     lookAt = null,
                     leftHand = doorHandleIK,
                     rightHand = null,
                     leftFoot = driverIK.leftFootIK,
                     rightFoot = driverIK.rightFootIK
                 });

                 doorOpenCloseSounds.PlayAudioClip(doorOpenCloseSounds.closeAudioClip);

             }).OnComplete(() =>
             {

                 driver.transform.position = driverSeatTransform.position;
                 driver.transform.rotation = driverSeatTransform.rotation;

                 EventBus<OnProvideIK>.Raise(new OnProvideIK
                 {
                     instanceID = driver.GetInstanceID(),
                     lookAt = null,
                     leftHand = driverIK.leftHandIK,
                     rightHand = driverIK.rightHandIK,
                     leftFoot = driverIK.leftFootIK,
                     rightFoot = driverIK.rightFootIK
                 });

                 driver.SetParent(driverSeatTransform);

                 enterExitCamera.SetActive(false);

                 EventBus<OnControllerChanged>.Raise(new OnControllerChanged
                 { controller = ControllerType.Car });


                 rccSceneManager.activePlayerCamera.SetTarget(DriveableController);
                 rccSceneManager.activePlayerVehicle = DriveableController;

                 DriveableController.enabled = true;
                 DriveableController.canControl = true;
                 DriveableController.useCollisionParticles = true;
                 DriveableController.useCollisionAudio = true;
                 DriveableController.StartEngine();

                 OnDriveableEntered?.Invoke();

                 CarState();

                 OnDriveableEnterAction?.Invoke(DriveableInstanceID);

             });
        }

        void DoorOpeningProcees(Transform driver)
        {
            if (driver.TryGetComponent(out Animator animator)) tpsDriverAnimator = animator;

            driver.DOMove(NavigateTo.destination.position, .25f).SetEase(Ease.Linear).
                    OnPlay(() =>
                    {
                        driver.DORotate(NavigateTo.destination.eulerAngles, 1f).SetEase(Ease.Linear);

                        EventBus<OnControllerChanged>.Raise(new OnControllerChanged
                        { controller = ControllerType.TPSDriveableBrain });

                        enterExitCamera.SetActive(true);

                        LerpToDriveableCamera();

                        tpsDriverAnimator?.CrossFadeInFixedTime(idleHash, .5f);
                    })
                    .OnComplete(() =>
                    {

                        tpsDriverAnimator?.CrossFadeInFixedTime(enteringCarHash, .5f);

                        EventBus<OnProvideIK>.Raise(new OnProvideIK
                        {
                            instanceID = driver.GetInstanceID(),
                            lookAt = null,
                            leftHand = doorHandleIK,
                            rightHand = null,
                            leftFoot = null,
                            rightFoot = null
                        });
                    });
        }
    }

    public void ExitDriveable(Transform driver)
    {
        DriveableController.KillEngine();

        brakeCoroutine = StartCoroutine(this.WaitWhile(DriveableController.enabled, () =>
        {
            DriveableController.brakeInput += 1f;
        }));

        if (IsUpsideDown()) DriveableExitWithoutAnimation(driver);
        else DriveableExitWithAnimation(driver);

        void DriveableExitWithAnimation(Transform driver)
        {
            if (driver.TryGetComponent(out Animator animator)) tpsDriverAnimator = animator;

            tpsDriverAnimator?.CrossFadeInFixedTime(exitingCarHash, 1f);

            EventBus<OnProvideIK>.Raise(new OnProvideIK
            {
                instanceID = driver.GetInstanceID(),
                lookAt = null,
                leftHand = driverIK.leftHandIK,
                rightHand = driverIK.rightHandIK,
                leftFoot = driverIK.leftFootIK,
                rightFoot = driverIK.rightFootIK
            });

            EventBus<OnControllerChanged>.Raise(new OnControllerChanged
            { controller = ControllerType.TPSExitDriveable });

            leftDoorTransform.DOKill();
            leftDoorTransform.localEulerAngles = leftDoorInitialEuler;

            leftDoorTransform.DOBlendableRotateBy(new Vector3(0, doorRotateAroundAngle, 0), 1f).SetDelay(.25f)
                .OnPlay(() =>
                {
                    doorOpenCloseSounds.PlayAudioClip(doorOpenCloseSounds.openAudioClip);

                    EventBus<OnProvideIK>.Raise(new OnProvideIK
                    {
                        instanceID = driver.GetInstanceID(),
                        lookAt = null,
                        leftHand = doorHandleIK,
                        rightHand = null,
                        leftFoot = driverIK.leftFootIK,
                        rightFoot = driverIK.rightFootIK
                    });

                    StartCoroutine(AkaitoAiExtensions.SimpleDelay(1f, () =>
                    {

                        EventBus<OnProvideIK>.Raise(new OnProvideIK
                        {
                            instanceID = driver.GetInstanceID(),
                            lookAt = null,
                            leftHand = doorHandleIK,
                            rightHand = null,
                            leftFoot = null,
                            rightFoot = null
                        });

                    }));

                }).OnComplete(() =>
                {
                    EventBus<OnProvideIK>.Raise(new OnProvideIK
                    {
                        instanceID = driver.GetInstanceID(),
                        lookAt = null,
                        leftHand = null,
                        rightHand = null,
                        leftFoot = null,
                        rightFoot = null
                    });

                    driver.DOMove(NavigateTo.destination.position, 1f)
                        .SetEase(Ease.Linear)
                        .OnComplete(() =>
                        {

                            DriveableController.KillEngine();
                            DriveableController.canControl = false;
                            DriveableController.useCollisionParticles = false;
                            DriveableController.useCollisionAudio = false;
                            DriveableController.enabled = false; 

                            CarState();

                            driver.transform.position = NavigateTo.destination.position;

                            leftDoorTransform.DOBlendableRotateBy(new Vector3(0, -doorRotateAroundAngle, 0), 1f)
                               .OnPlay(() =>
                               {
                                   doorOpenCloseSounds.PlayAudioClip(doorOpenCloseSounds.closeAudioClip);
                               })
                               .OnComplete(() =>
                               {

                                   driver.parent = null;

                                   OnDriveableExited?.Invoke();

                                   EventBus<OnControllerChanged>.Raise(new OnControllerChanged
                                   { controller = ControllerType.TPS });

                                   EventBus<OnTPSActive>.Raise(new OnTPSActive
                                   { transform = driver });

                               });
                        });
                });
        }

        bool IsUpsideDown()
        {
            return Root.eulerAngles.z < 300 && Root.eulerAngles.z > 60;
        }

        void DriveableExitWithoutAnimation(Transform driver)
        {
            EventBus<OnProvideIK>.Raise(new OnProvideIK
            {
                instanceID = driver.GetInstanceID(),
                lookAt = null,
                leftHand = null,
                rightHand = null,
                leftFoot = null,
                rightFoot = null
            });

            EventBus<OnControllerChanged>.Raise(new OnControllerChanged
            { controller = ControllerType.TPSExitDriveable });

            leftDoorTransform.DOKill();
            leftDoorTransform.localEulerAngles = leftDoorInitialEuler;

            DriveableController.KillEngine();
            DriveableController.canControl = false;
            DriveableController.useCollisionParticles = false;
            DriveableController.useCollisionAudio = false;
            DriveableController.enabled = false;

            CarState();

            driver.transform.position = NavigateTo.destination.position;
            driver.transform.eulerAngles = new Vector3(0f, driver.transform.eulerAngles.y, 0f);


            driver.parent = null;

            OnDriveableExited?.Invoke();

            EventBus<OnControllerChanged>.Raise(new OnControllerChanged
            { controller = ControllerType.TPS });

            EventBus<OnTPSActive>.Raise(new OnTPSActive
            { transform = driver });
        }
    }

    private void CarState()
    {
        bool isCarActive = DriveableController.enabled;

        if (wheelColliders != null || wheelColliders.Length > 0)
        {
            foreach (Collider cl in wheelColliders)
                cl.enabled = !isCarActive;
        }

        if (activateWithController != null || activateWithController.Length > 0)
        {
            foreach (GameObject go in activateWithController)
                go.SetActive(isCarActive);
        }

        DriveableController.Rigid.isKinematic = !isCarActive;
    }

    private void LerpToDriveableCamera()
    { 
        enterExitCamera.transform.DOMove(tpsCameraTransform.position, 1f).From();
        enterExitCamera.transform.DORotate(tpsCameraTransform.eulerAngles, 1f).From();
    }

    private void StoreTPSCameraTransform(OnDriveableCameraSwitch dcs)
    { 
        tpsCameraTransform = dcs.tpsCameraTransform;
    }

    private void TrafficToRCC(OnTrafficToRCC trafficToRCC)
    {
        if (driver.driverSO == null) return;

        if(driver.driverSO.GetTotalDrivers() == 0) return;

        npcDriverClone = Instantiate(driver.driverSO.GetDriverByIndex(trafficToRCC.driverIndex),
             driver.driverTransform.position, driver.driverTransform.rotation);

        npcDriverClone.transform.parent = driver.driverTransform.transform;

        npcDriverAnimator = npcDriverClone.GetComponentInChildren<Animator>();
        npcAnimationEvents = npcDriverClone.GetComponentInChildren<AnimationEvents>();

        npcAnimationEvents.drivingIK = driverIK;

        EventBus<OnProvideIK>.Raise(new OnProvideIK
        {
            instanceID = npcDriverClone.transform.GetInstanceID(),
            lookAt = null,
            leftHand = driverIK.leftHandIK,
            rightHand = driverIK.rightHandIK,
            leftFoot = driverIK.leftFootIK,
            rightFoot = driverIK.rightFootIK
        });
    }
}
