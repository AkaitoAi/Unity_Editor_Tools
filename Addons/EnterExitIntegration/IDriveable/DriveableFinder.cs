using System;
using UnityEngine;

public class DriveableFinder : MonoBehaviour
{
    [SerializeField] private DriveableEnterExit enterExit;
    [SerializeField] private float spherecastRadius = 2.5f;
    [SerializeField] private float spherecastDistance = 1f;

    private bool driveableFound = false;
    private float delayToCastSphere;

    public static event Action<bool> OnDriveableFoundAction;
    public static event Action<int> OnDriveableEnterAction;

    private void Update()
    {
        delayToCastSphere += Time.deltaTime;

        if (delayToCastSphere < 1f) return;

        delayToCastSphere = 0f;

        driveableFound = false;

        OnDriveableFoundAction?.Invoke(driveableFound);

        RaycastHit[] hits;
        Vector3 trans = transform.position;

        hits = Physics.SphereCastAll(trans, spherecastRadius, transform.forward, spherecastDistance);

        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.transform.TryGetComponent(out IDriveable iDriveable)) continue;
            
            IDriveable driveable = null;

            driveable = iDriveable;

            enterExit.distanceFromDriveable = Vector3.Distance(transform.position, hit.collider.transform.position);
            enterExit.driveable = hit.collider.transform.root;

            driveableFound = true;

            OnDriveableFoundAction?.Invoke(driveableFound);
            OnDriveableEnterAction?.Invoke(driveable.DriveableInstanceID);

            break;
        }

        if (!driveableFound)
        {
            enterExit.distanceFromDriveable = 0f;
            enterExit.driveable = null;

            OnDriveableFoundAction?.Invoke(driveableFound);
        }
    }

    private void SetTransform(Transform transform)
    {
        this.transform.position = transform.position;
        this.transform.rotation = transform.rotation;

        Destroy(transform.gameObject);
    }
    private void OnEnable()
    {
        DriveableCar.OnControllerActiveAction += SetTransform;
    }

    private void OnDisable()
    {
        DriveableCar.OnControllerActiveAction -= SetTransform;
    }
}
