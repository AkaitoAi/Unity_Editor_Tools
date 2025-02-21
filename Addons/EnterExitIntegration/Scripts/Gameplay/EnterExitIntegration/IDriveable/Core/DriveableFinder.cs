using System;
using UnityEngine;
using Driveable.Core;
using Driveable;
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
            //enterExit.driveable = hit.collider.transform.root;
            enterExit.driveable = driveable.Root;

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
    
    private void SetTransform(OnTPSActive tpsTransform)
    {
        SetTransform(tpsTransform.transform);
    }

    EventBinding<OnTPSActive> tpsEventBinding;
    private void OnEnable()
    {
        tpsEventBinding = new EventBinding<OnTPSActive>(SetTransform);
        EventBus<OnTPSActive>.Register(tpsEventBinding);
    }

    private void OnDisable()
    {
        EventBus<OnTPSActive>.Deregister(tpsEventBinding);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
    
        // Draw sphere at the origin point
        Gizmos.DrawWireSphere(transform.position, spherecastRadius);
    
        // Draw the direction of the sphere cast
        Vector3 endPosition = transform.position + transform.forward * spherecastDistance;
        Gizmos.DrawLine(transform.position, endPosition);
        
        // Draw sphere at the end position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(endPosition, spherecastRadius);
    }
#endif
}
