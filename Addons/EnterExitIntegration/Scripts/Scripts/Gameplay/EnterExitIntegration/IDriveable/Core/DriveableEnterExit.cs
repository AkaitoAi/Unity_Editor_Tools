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
    internal Transform controllerCameraTransform;

    private void Start()
    {
        driveableButtons.enterButton.gameObject.SetActive(false);
        driveableButtons.exitButton.gameObject.SetActive(false);

        EventBus<OnControllerChanged>.Raise(new OnControllerChanged { controller = this.controller });
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
        if (driveable.GetComponentInChildren<TrafficVehicleDriveable>() != null)
        {
            SetupTrafficDriveable();
        }
        else if (driveable.GetComponentInChildren<IDriveable>() != null)
        {
            SetupDriveable(driveableID);
        }

        void SetupTrafficDriveable()
        {
            TrafficVehicleDriveable trafficVehicle = driveable.GetComponentInChildren<TrafficVehicleDriveable>();

            if (trafficVehicle == null) return;

            driveableButtons.enterButton?.onClick.AddListener(() =>
            {
                driveableButtons.enterButton.gameObject.SetActive(false);

                Vector3 aiCarPosition = driveable.position;
                Quaternion aiCarRotation = driveable.rotation;

                IDriveable newDriveable = trafficVehicle.SpawnRCCDriveableAndRespawnAI(aiCarPosition, aiCarRotation);
                
                if (newDriveable == null) return;

                driveableInstanceID = newDriveable.DriveableInstanceID;
                driveable = newDriveable.Root;
                distanceFromDriveable = Vector3.Distance(tpsCharacter.tpsCharacterController.position, driveable.position);

                NavigateToTarget nav = newDriveable.NavigateTo;

                if (cloneCharacter != null) Destroy(cloneCharacter.gameObject);

                //Vector3 spawnPosition = newDriveable.Target.position - newDriveable.Root.forward * 1f;
                //if (Physics.Raycast(spawnPosition + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f))
                //{
                //    spawnPosition.y = hit.point.y + 1f; 
                //}
                //else
                //{
                //    spawnPosition.y = newDriveable.Target.position.y + 1f; 
                //}

                cloneCharacter = Instantiate(tpsCharacter.tpsGetToDriveableCharacter,
                    tpsCharacter.tpsCharacterController.position, 
                    Quaternion.LookRotation(newDriveable.Root.forward));

                if (cloneCharacter.TryGetComponent(out Rigidbody rb))
                    rb.isKinematic = true; 

                if (cloneCharacter.TryGetComponent(out Animator animator))
                    animator.CrossFadeInFixedTime(runHash, .5f);

                tpsCharacter.tpsCharacterController.gameObject.SetActive(false);

                EventBus<OnControllerChanged>.Raise(new OnControllerChanged
                { controller = ControllerType.TPSDriveable });

                nav.toMove = cloneCharacter;
                nav.destination = newDriveable.Target;
                nav.FindTheShortestPath();
            });
        }

        void SetupDriveable(int driveableID)
        {
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

                NavigateToTarget nav = drive.NavigateTo;
                nav.toMove = cloneCharacter;
                nav.destination = drive.Target;
                nav.FindTheShortestPath();
            });
        }
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

            driveable.GetComponentInChildren<IDriveable>()?.ExitDriveable(cloneCharacter);
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
            //tpsCharacter.vFollowCamera.LookAt = cloneCharacter;

            TPSCameraTransformToDriveableCameraTransform();

            tpsCharacter.brainFollowCamera.gameObject.SetActive(true);
            tpsCharacter.vFollowCamera.gameObject.SetActive(true);

            return;
        }

        if (onController.controller == ControllerType.TPSDriveableBrain)
        {
            TPSCameraTransformToDriveableCameraTransform();

            tpsCharacter.brainFollowCamera.gameObject.SetActive(true);

            return;
        }
        
        if (onController.controller == ControllerType.TPSExitDriveable)
        {
            tpsCharacter.vEnterExitCamera.Follow = driveable;
            tpsCharacter.vEnterExitCamera.LookAt = driveable;

            tpsCharacter.vEnterExitCamera.gameObject.SetActive(true);

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

        void TPSCameraTransformToDriveableCameraTransform()
        {
            Transform controllerCameraTransform =
                            cameraCanvas.tps[1].transform;

            tpsCharacter.vFollowCamera.transform.position =
                controllerCameraTransform.position;
            tpsCharacter.vFollowCamera.transform.rotation =
                controllerCameraTransform.rotation;

            EventBus<OnDriveableCameraSwitch>.Raise(new OnDriveableCameraSwitch
            { tpsCameraTransform = controllerCameraTransform });
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
        tpsCharacter.vEnterExitCamera.gameObject.SetActive(false);
    }
    #endregion

    #endregion
}
