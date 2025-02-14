using AkaitoAi.Extensions;
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

    [SerializeField] private GameObject enterExitCamera;

    [SerializeField] private float doorRotateAroundAngle = 45f;

    [SerializeField] private DoorOpenCloseSounds doorOpenCloseSounds;

    [SerializeField] private DriverIK driverIK;

    private RCC_SceneManager rccSceneManager;
    private Vector3 leftDoorInitialEuler;
    private Coroutine brakeCoroutine;
    
    internal RCC_CarControllerV3 carController;

    internal int driveableID;

    public static event Action<int, Transform, Transform, 
        Transform, Transform, Transform> OnProvideIKAction;

    public static event Action<ControllerType> OnControllerChangedAction;

    public static event Action<Transform> OnControllerActiveAction;

    [Serializable]
    public struct DriverIK
    { 
        public Transform leftHandIK, rightHandIK, leftFootIK, rightFootIK;
    }

    [Serializable]
    public struct DoorOpenCloseSounds
    {
        public AudioClip openAudioClip;
        public AudioClip closeAudioClip;
        public AudioSource audioSource;

        public void PlayAudioClip(AudioClip audioClip)
        {
            if(audioSource == null) return;

            if(audioClip == null) return;

            audioSource.PlayOneShot(audioClip);
        }
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
        rccSceneManager = RCC_SceneManager.Instance;
        
        carController = transform.root.GetComponent<RCC_CarControllerV3>(); 
        carController.enabled = false;
        
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
                        
                        OnControllerChangedAction?.Invoke(ControllerType.TPSDriveableBrain);
                        enterExitCamera.SetActive(true);
                        
                        animator.CrossFadeInFixedTime("Idle", .5f);
                    })
                    .OnComplete(() => { 

                        animator.CrossFadeInFixedTime("EnteringCar", .5f);

                        OnProvideIKAction?.Invoke(driver.GetInstanceID(),
                               null, doorHandleIK, null, null, null);
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
                    animator.CrossFadeInFixedTime("EnterCarSecondHalf", .5f);


                    driver.DOMove(driverSeatTransform.position, .1f).SetEase(Ease.Linear).
                    OnPlay(() => {
                        driver.DORotate(driverSeatTransform.eulerAngles, .1f).SetEase(Ease.Linear);

                        OnProvideIKAction?.Invoke(driver.GetInstanceID(),
                            null, null, null, driverIK.leftFootIK, driverIK.rightFootIK);
                    });

                    leftDoorTransform.DOBlendableRotateBy(new Vector3(0, -doorRotateAroundAngle, 0), 1f).SetDelay(.25f).
                    OnPlay(() => { 
                    
                        OnProvideIKAction?.Invoke(driver.GetInstanceID(),
                            null, doorHandleIK, null, driverIK.leftFootIK, driverIK.rightFootIK);

                        doorOpenCloseSounds.PlayAudioClip(doorOpenCloseSounds.closeAudioClip);

                    }).OnComplete(() => {

                        driver.transform.position = driverSeatTransform.position;
                        driver.transform.rotation = driverSeatTransform.rotation;

                        OnProvideIKAction?.Invoke(driver.GetInstanceID(), null,
                        driverIK.leftHandIK, driverIK.rightHandIK,
                        driverIK.leftFootIK, driverIK.rightFootIK);

                        driver.SetParent(driverSeatTransform);

                        enterExitCamera.SetActive(false);

                        OnControllerChangedAction?.Invoke(ControllerType.RCC);


                        rccSceneManager.activePlayerCamera.SetTarget(carController);
                        rccSceneManager.activePlayerVehicle = carController;

                        carController.enabled = true;
                        carController.canControl = true;
                        carController.StartEngine();
                    });
                }
            });
    }

    public void ExitDriveable(Transform driver)
    {
        brakeCoroutine = StartCoroutine(this.WaitWhile(carController.enabled, () => {
            carController.brakeInput += 1f;
        }));

        if (driver.TryGetComponent(out Animator animator))
        {
            animator.CrossFadeInFixedTime("Exiting Car", 1f);

            OnProvideIKAction?.Invoke(driver.GetInstanceID(), null,
                        driverIK.leftHandIK, driverIK.rightHandIK,
                        driverIK.leftFootIK, driverIK.rightFootIK);

            OnControllerChangedAction?.Invoke(ControllerType.TPSDriveableBrain);
            enterExitCamera.SetActive(true);
        }

        leftDoorTransform.DOKill();
        leftDoorTransform.localEulerAngles = leftDoorInitialEuler;

        leftDoorTransform.DOBlendableRotateBy(new Vector3(0, doorRotateAroundAngle, 0), 1f).SetDelay(.25f)
            .OnPlay(() =>
            {
                doorOpenCloseSounds.PlayAudioClip(doorOpenCloseSounds.openAudioClip);

                OnProvideIKAction?.Invoke(driver.GetInstanceID(), null,
                    doorHandleIK, null, driverIK.leftFootIK, driverIK.rightFootIK);

                StartCoroutine(AkaitoAiExtensions.SimpleDelay(1f, () => {
                    
                    OnProvideIKAction?.Invoke(driver.GetInstanceID(),
                                null, doorHandleIK, null, null, null);

                }));

            }).OnComplete(() =>
            {
                OnProvideIKAction?.Invoke(driver.GetInstanceID(),
                                null, null, null, null, null);

                driver.DOMove(NavigateTo.destination.position, 1f)
                    .SetEase(Ease.Linear)
                    .OnComplete(() => {

                        carController.KillEngine();
                        carController.enabled = false;
                        carController.canControl = false;

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

                                OnControllerChangedAction?.Invoke(ControllerType.TPS);

                                OnControllerActiveAction?.Invoke(driver);

                            });
                        }
                    });
            });
    }
}
