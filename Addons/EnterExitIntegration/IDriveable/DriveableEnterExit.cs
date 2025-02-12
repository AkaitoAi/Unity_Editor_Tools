using UnityEngine;
using UnityEngine.UI;


public enum ControllerType 
{
    None,
    TPS,
    RCC,
}
public class DriveableEnterExit : MonoBehaviour
{
    [SerializeField] private Button enterButton, exitButton;

    [SerializeField] private TPSCharacter tpsCharacter;

    [SerializeField] private GameObject[] tpsControls, rccControls;

    internal int driveableInstanceID;
    internal Transform driveable;
    internal float distanceFromDriveable;
    internal Transform cloneCharacter;

    [System.Serializable]
    public struct TPSCharacter
    {
        public Transform tpsCharacterController;
        public Transform tpsGetToDriveableCharacter;
        public Transform tpsDriverCharacter;
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
        enterButton.gameObject.SetActive(isFound);

        if (!isFound) enterButton.onClick.RemoveAllListeners();
    }

    private void SetupEnterButton(int driveableID)
    {
        //if (!driveable.TryGetComponent(out IDriveable drive)) return;

        if (driveable.GetComponentInChildren<IDriveable>() == null) return;
        IDriveable drive = driveable.GetComponentInChildren<IDriveable>();

        driveableInstanceID = drive.DriveableInstanceID;

        if (driveableInstanceID != driveableID) return;

        enterButton.onClick.AddListener(() =>
        {
            if (cloneCharacter != null) Destroy(cloneCharacter.gameObject);

            cloneCharacter = Instantiate(tpsCharacter.tpsGetToDriveableCharacter,
                tpsCharacter.tpsCharacterController.position,
                tpsCharacter.tpsCharacterController.rotation);

            tpsCharacter.tpsCharacterController.gameObject.SetActive(false);

            drive.NavigateTo.toMove = cloneCharacter;
            drive.NavigateTo.FindTheShortestPath();
        });
    }

    private void SetupExitButton()
    {
        if (cloneCharacter == null) return;

        exitButton.onClick.AddListener(() =>
        {
            if (driveable.GetComponentInChildren<IDriveable>()
            is not DriveableCar car)
                return;

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

        //if (cloneCharacter != null) Destroy(cloneCharacter.gameObject);

        //cloneCharacter = Instantiate(tpsCharacter.tpsDriverCharacter);

        car.EnterDriveable(cloneCharacter);
    }

    private void ChangeController(ControllerType ct)
    {
        foreach (GameObject tpsControl in tpsControls)
            tpsControl.SetActive(false);

        foreach (GameObject rccControl in rccControls)
            rccControl.SetActive(false);

        if (ct == ControllerType.None) return;

        if (ct == ControllerType.TPS)
        {
            foreach (GameObject tpsControl in tpsControls)
                tpsControl.SetActive(true);

            tpsCharacter.tpsCharacterController.gameObject.SetActive(true);

            return;
        }

        if (ct == ControllerType.RCC)
        {
            foreach (GameObject rccControl in rccControls)
                rccControl.SetActive(true);

            SetupExitButton();

            return;
        }
    }
}
