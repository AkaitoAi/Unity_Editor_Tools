using AkaitoAi.Extensions;
using DG.Tweening;
using UnityEngine;
using Driveable.Core;
using Driveable;
using System;
public class DriveableCar : MonoBehaviour, IDriveable<RCC_CarControllerV3>
{
    public Transform root;

    [SerializeField] private Transform target;

    [SerializeField] private Transform leftDoorTransform;

    [SerializeField] private Transform driverSeatTransform;

    [SerializeField] private NavigateToTarget navigateToTarget;

    [SerializeField] private Transform doorHandleIK;

    [SerializeField] private GameObject enterExitCamera;

    [SerializeField] private float doorRotateAroundAngle = 45f;

    [SerializeField] private GameObject[] activateWithController;
    [SerializeField] private Collider[] wheelColliders;

    [SerializeField] private DoorOpenCloseSounds doorOpenCloseSounds;

    [SerializeField] private DriverIK driverIK;

    private RCC_SceneManager rccSceneManager;
    private Vector3 leftDoorInitialEuler;
    private Coroutine brakeCoroutine;
    private RCC_CarControllerV3 carController;

    private static readonly int idleHash = Animator.StringToHash("Idle");
    private static readonly int enteringCarHash = Animator.StringToHash("EnteringCar");
    private static readonly int enteringCarSecondHalfHash = Animator.StringToHash("EnterCarSecondHalf");
    private static readonly int exitingCarHash = Animator.StringToHash("Exiting Car");

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
        get => driveableID = root.GetInstanceID();
        set => driveableID = value;
    }
    public RCC_CarControllerV3 DriveableController 
    { 
        get => carController; 
        set => carController = value; }

    public Transform Root { get => root; set => root = value; }

    #endregion
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
        rccSceneManager = RCC_SceneManager.Instance;
        
        DriveableController = root.GetComponent<RCC_CarControllerV3>();
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

        if (driver.TryGetComponent(out Animator animator))
        {
            driver.DOMove(NavigateTo.destination.position, .25f).SetEase(Ease.Linear).
                    OnPlay(() => {

                        driver.DORotate(NavigateTo.destination.eulerAngles, 1f).SetEase(Ease.Linear);
                        
                        EventBus<OnControllerChanged>.Raise(new OnControllerChanged
                        { controller = ControllerType.TPSDriveableBrain });


                        enterExitCamera.SetActive(true);
                        
                        animator.CrossFadeInFixedTime(idleHash, .5f);
                    })
                    .OnComplete(() => { 

                        animator.CrossFadeInFixedTime(enteringCarHash, .5f);

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
        
        leftDoorTransform.DOKill();
        leftDoorTransform.localEulerAngles = leftDoorInitialEuler;

        leftDoorTransform.DOBlendableRotateBy(new Vector3(0, doorRotateAroundAngle, 0), 1f).SetDelay(.5f)
            .OnPlay(() => {

                doorOpenCloseSounds.PlayAudioClip(doorOpenCloseSounds.openAudioClip);
            })
            .OnComplete( () =>
            {
                if (driver.TryGetComponent(out Animator animator))
                {
                    animator.CrossFadeInFixedTime(enteringCarSecondHalfHash, .5f);


                    driver.DOMove(driverSeatTransform.position, .1f).SetEase(Ease.Linear).
                    OnPlay(() => {
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
                    OnPlay(() => { 

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

                    }).OnComplete(() => {

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

                        CarState();

                        OnDriveableEnterAction?.Invoke(DriveableInstanceID);

                    });
                }
            });
    }

    public void ExitDriveable(Transform driver)
    {
        brakeCoroutine = StartCoroutine(this.WaitWhile(DriveableController.enabled, () => {
            DriveableController.brakeInput += 1f;
        }));

        if (driver.TryGetComponent(out Animator animator))
        {
            animator.CrossFadeInFixedTime(exitingCarHash, 1f);

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
            { controller = ControllerType.TPSDriveableBrain });
            
            enterExitCamera.SetActive(true);
        }

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

                StartCoroutine(AkaitoAiExtensions.SimpleDelay(1f, () => {
                    
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
                    .OnComplete(() => {

                        DriveableController.KillEngine();
                        DriveableController.canControl = false; 
                        DriveableController.useCollisionParticles = false;
                        DriveableController.useCollisionAudio = false;
                        DriveableController.enabled = false;

                        CarState();

                        driver.transform.position = NavigateTo.destination.position;

                        if (driver.TryGetComponent(out Animator animator))
                        {
                            leftDoorTransform.DOBlendableRotateBy(new Vector3(0, -doorRotateAroundAngle, 0), 1f)
                            .OnPlay(() => {

                                doorOpenCloseSounds.PlayAudioClip(doorOpenCloseSounds.closeAudioClip);
                            })
                            .OnComplete(() => {

                                driver.parent = null;

                                enterExitCamera.SetActive(false);

                                EventBus<OnControllerChanged>.Raise(new OnControllerChanged 
                                { controller = ControllerType.TPS });

                                EventBus<OnTPSActive>.Raise(new OnTPSActive
                                { transform = driver });

                            });
                        }
                    });
            });
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
    }
}
