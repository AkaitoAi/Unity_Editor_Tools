using UnityEngine;
using UnityEngine.UI;
using AkaitoAi.Singleton;
using Cinemachine;
using System;

public enum ControllerType 
{
    None,
    TPS,
    TPSDriveable,
    TPSDriveableBrain,
    RCC,
}
public class DriveableEnterExit : Singleton<DriveableEnterExit>
{
    [SerializeField] private ControllerType controller = ControllerType.TPS;

    [SerializeField] private DriveableButtons driveableButtons;

    [SerializeField] private TPSCharacter tpsCharacter;

    [SerializeField] private CameraCanvas cameraCanvas;

    internal int driveableInstanceID;
    internal Transform driveable;
    internal float distanceFromDriveable;
    internal Transform cloneCharacter;

    [Serializable]
    public struct DriveableButtons
    {
        public Button enterButton;
        public Button exitButton;
    }

    [Serializable]
    public struct TPSCharacter
    {
        public Transform tpsCharacterController;
        public Transform tpsGetToDriveableCharacter;
        public Transform tpsDriverCharacter;
        public CinemachineBrain brainFollowCamera;
        public CinemachineVirtualCamera vFollowCamera;
    }

    [Serializable]
    public struct CameraCanvas
    {
        public GameObject[] tps;
        public GameObject[] rcc;
    }

    private void Awake()
    {
        ChangeController(controller);
    }

    private void OnEnable()
    {
        DriveableFinder.OnDriveableFoundAction += DriveableFound;
        DriveableFinder.OnDriveableEnterAction += SetupEnterButton;
        NavigateToTarget.OnDestinationReachedAction += OnReachedDriveable;
        DriveableCar.OnControllerChangedAction += ChangeController;
    }

    private void OnDisable()
    {
        DriveableFinder.OnDriveableFoundAction -= DriveableFound;
        DriveableFinder.OnDriveableEnterAction -= SetupEnterButton;
        NavigateToTarget.OnDestinationReachedAction -= OnReachedDriveable;
        DriveableCar.OnControllerChangedAction -= ChangeController;
    }

    private void DriveableFound(bool isFound)
    {
        driveableButtons.enterButton.gameObject.SetActive(isFound);

        if (!isFound) driveableButtons.enterButton.onClick.RemoveAllListeners();
    }

    private void SetupEnterButton(int driveableID)
    {

        if (driveable.GetComponentInChildren<IDriveable>() == null) return;
        IDriveable drive = driveable.GetComponentInChildren<IDriveable>();

        driveableInstanceID = drive.DriveableInstanceID;

        if (driveableInstanceID != driveableID) return;

        driveableButtons.enterButton.onClick.AddListener(() =>
        {
            if (cloneCharacter != null) Destroy(cloneCharacter.gameObject);

            cloneCharacter = Instantiate(tpsCharacter.tpsGetToDriveableCharacter,
                tpsCharacter.tpsCharacterController.position,
                tpsCharacter.tpsCharacterController.rotation);

            tpsCharacter.tpsCharacterController.gameObject.SetActive(false);

            ChangeController(ControllerType.TPSDriveable);

            drive.NavigateTo.toMove = cloneCharacter;
            drive.NavigateTo.FindTheShortestPath();
        });
    }

    private void SetupExitButton()
    {
        if (cloneCharacter == null) return;

        driveableButtons.exitButton.onClick.AddListener(() =>
        {
            if (driveable.GetComponentInChildren<IDriveable>()
            is not DriveableCar car)
                return;

            car.carController.KillEngine();

            car.ExitDriveable(cloneCharacter);
        });
    }
    private void OnReachedDriveable()
    {
        if (driveable == null) return;

        if (driveable.GetComponentInChildren<IDriveable>() 
            is not DriveableCar car)
            return;

        if(driveableInstanceID != car.DriveableInstanceID) return;

        car.EnterDriveable(cloneCharacter);
    }

    private void ChangeController(ControllerType ct)
    {
        DisableAllCameraCanvas();

        if (ct == ControllerType.None) return;

        if (ct == ControllerType.TPSDriveable)
        {
            tpsCharacter.vFollowCamera.Follow = cloneCharacter;
            tpsCharacter.vFollowCamera.LookAt = cloneCharacter;

            tpsCharacter.brainFollowCamera.gameObject.SetActive(true);
            tpsCharacter.vFollowCamera.gameObject.SetActive(true);

            return;
        }

        if (ct == ControllerType.TPSDriveableBrain)
        {
            tpsCharacter.brainFollowCamera.gameObject.SetActive(true);

            return;
        }

        if (ct == ControllerType.TPS)
        {
            foreach (GameObject tpsControl in cameraCanvas.tps)
                tpsControl.SetActive(true);

            tpsCharacter.tpsCharacterController.gameObject.SetActive(true);

            return;
        }

        if (ct == ControllerType.RCC)
        {
            foreach (GameObject rccControl in cameraCanvas.rcc)
                rccControl.SetActive(true);

            SetupExitButton();

            return;
        }
    }

    private void DisableAllCameraCanvas()
    {
        foreach (GameObject tpsControl in cameraCanvas.tps)
            tpsControl.SetActive(false);

        foreach (GameObject rccControl in cameraCanvas.rcc)
            rccControl.SetActive(false);

        tpsCharacter.brainFollowCamera.gameObject.SetActive(false);
        tpsCharacter.vFollowCamera.gameObject.SetActive(false);
    }
}
