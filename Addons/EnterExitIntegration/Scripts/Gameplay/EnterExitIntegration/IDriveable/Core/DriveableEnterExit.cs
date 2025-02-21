using UnityEngine;
using AkaitoAi.Singleton;
using AkaitoAi.Extensions;
using Driveable.Core;
using Driveable;
public class DriveableEnterExit : Singleton<DriveableEnterExit>
{
    [SerializeField] private ControllerType controller = ControllerType.TPS;

    [SerializeField] private DriveableButtons driveableButtons;

    [SerializeField] private TPSCharacter tpsCharacter;

    [SerializeField] private CameraCanvas cameraCanvas;

    private static readonly int runHash = Animator.StringToHash("Run");

    internal int driveableInstanceID;
    internal Transform driveable;
    internal float distanceFromDriveable;
    internal Transform cloneCharacter;
    private void Start()
    {
        driveableButtons.enterButton.gameObject.SetActive(false);
        driveableButtons.exitButton.gameObject.SetActive(false);

        EventBus<OnControllerChanged>.Raise(new OnControllerChanged
        { controller = this.controller });
    }

    EventBinding<OnControllerChanged> controllerChangedEventBinding;
    private void OnEnable()
    {
        DriveableFinder.OnDriveableFoundAction += DriveableFound;
        DriveableFinder.OnDriveableEnterAction += SetupEnterButton;
        NavigateToTarget.OnDestinationReachedAction += OnReachedDriveable;

        controllerChangedEventBinding = new EventBinding<OnControllerChanged>(ChangeController);
        EventBus<OnControllerChanged>.Register(controllerChangedEventBinding);
    }
    private void OnDisable()
    {
        DriveableFinder.OnDriveableFoundAction -= DriveableFound;
        DriveableFinder.OnDriveableEnterAction -= SetupEnterButton;
        NavigateToTarget.OnDestinationReachedAction -= OnReachedDriveable;

        EventBus<OnControllerChanged>.Deregister(controllerChangedEventBinding);
    }

    #region Enter/Exit Driveable

    #region Enter Driveable
    private void SetupEnterButton(int driveableID)
    {
        if (driveable.GetComponentInChildren<IDriveable>() == null) return;
        IDriveable drive = driveable.GetComponentInChildren<IDriveable>();

        driveableInstanceID = drive.DriveableInstanceID;

        if (driveableInstanceID != driveableID) return;

        driveableButtons.enterButton?.onClick.AddListener(() =>
        {
            driveableButtons.enterButton.gameObject.SetActive(false);
            
            if (cloneCharacter != null) Destroy(cloneCharacter.gameObject);

            cloneCharacter = Instantiate(tpsCharacter.tpsGetToDriveableCharacter,
                tpsCharacter.tpsCharacterController.position,
                tpsCharacter.tpsCharacterController.rotation);

            if (cloneCharacter.TryGetComponent(out Animator animator))
                animator.CrossFadeInFixedTime(runHash, .5f);

            tpsCharacter.tpsCharacterController.gameObject.SetActive(false);

            EventBus<OnControllerChanged>.Raise(new OnControllerChanged
            { controller = ControllerType.TPSDriveable });

            drive.NavigateTo.toMove = cloneCharacter;
            drive.NavigateTo.FindTheShortestPath();
        });
    }
    #endregion

    #region Exit Driveable
    private void SetupExitButton()
    {
        if (cloneCharacter == null) return;

        if (driveableButtons.exitButton == null) return;
        
        driveableButtons.exitButton.gameObject.SetActive(true);

        driveableButtons.exitButton.onClick.AddListener(() =>
        {
            driveableButtons.exitButton.gameObject.SetActive(false);

            if (driveable.GetComponentInChildren<IDriveable>()
            is DriveableCar car)
            {

                car.DriveableController.KillEngine();

                car.ExitDriveable(cloneCharacter);

                return;
            }
            
            if (driveable.GetComponentInChildren<IDriveable>()
            is DriveableBike bike)
            {

                bike.DriveableController.StopBike();

                bike.ExitDriveable(cloneCharacter);

                return;
            }
        });
    }
    #endregion

    #endregion

    #region Events Callbacks
    private void DriveableFound(bool isFound)
    {
        driveableButtons.enterButton?.gameObject.SetActive(isFound);

        if (!isFound) driveableButtons.enterButton.onClick.RemoveAllListeners();
    }
    private void OnReachedDriveable()
    {
        if (driveable == null) return;

        if (driveable.GetComponentInChildren<IDriveable>()
            is DriveableCar car)
        {
            if (driveableInstanceID != car.DriveableInstanceID) return;

            car.EnterDriveable(cloneCharacter);

            return;
        }

        if (driveable.GetComponentInChildren<IDriveable>()
            is DriveableBike bike)
        {
            if (driveableInstanceID != bike.DriveableInstanceID) return;

            bike.EnterDriveable(cloneCharacter);

            return;
        }
    }

    #region Change Controller
    private void ChangeController(OnControllerChanged onController)
    {
        DisableAllCameraCanvas();

        if (onController.controller == ControllerType.None) return;

        if (onController.controller == ControllerType.TPSDriveable)
        {
            tpsCharacter.vFollowCamera.Follow = cloneCharacter;
            tpsCharacter.vFollowCamera.LookAt = cloneCharacter;

            tpsCharacter.brainFollowCamera.gameObject.SetActive(true);
            tpsCharacter.vFollowCamera.gameObject.SetActive(true);

            return;
        }

        if (onController.controller == ControllerType.TPSDriveableBrain)
        {
            tpsCharacter.brainFollowCamera.gameObject.SetActive(true);

            return;
        }

        if (onController.controller == ControllerType.TPS)
        {
            foreach (GameObject tpsControl in cameraCanvas.tps)
                tpsControl.SetActive(true);

            tpsCharacter.tpsCharacterController.gameObject.SetActive(true);

            return;
        }

        StartCoroutine(AkaitoAiExtensions.SimpleDelay(2f, () =>
        {
            SetupExitButton();
        }));

        if (onController.controller == ControllerType.Car)
        {
            foreach (GameObject rccControl in cameraCanvas.rcc)
                rccControl.SetActive(true);

            return;
        }

        if (onController.controller == ControllerType.Bike)
        {
            foreach (GameObject abp in cameraCanvas.abp)
                abp.SetActive(true);

            return;
        }
    }
    private void DisableAllCameraCanvas()
    {
        foreach (GameObject tpsControl in cameraCanvas.tps)
            tpsControl.SetActive(false);

        foreach (GameObject rccControl in cameraCanvas.rcc)
            rccControl.SetActive(false);

        foreach (GameObject bikeControl in cameraCanvas.abp)
            bikeControl.SetActive(false);

        tpsCharacter.brainFollowCamera.gameObject.SetActive(false);
        tpsCharacter.vFollowCamera.gameObject.SetActive(false);
    }
    #endregion

    #endregion
}
