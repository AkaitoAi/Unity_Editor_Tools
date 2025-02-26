using Driveable.Core;
using UnityEngine;

public class TrafficDriver : MonoBehaviour
{
    [SerializeField] private DriverSO driverSO;

    [SerializeField] private DriverIK driverIK;

    private GameObject driverClone;
    private int driverID;
    
    internal int driverIndex;
    private void Start()
    {
        if (driverSO == null) return;

        if (driverSO.GetTotalDrivers() == 0) return;

        driverIndex = driverSO.GetRandomDriverIndex();

        driverClone = Instantiate(driverSO.GetDriverByIndex(driverIndex),
            transform.position, transform.rotation);

        driverClone.transform.parent = transform;

        driverID = driverClone.transform.GetInstanceID();

        EventBus<OnProvideIK>.Raise(new OnProvideIK
        {
            instanceID = driverID,
            lookAt = null,
            leftHand = driverIK.leftHandIK,
            rightHand = driverIK.rightHandIK,
            leftFoot = driverIK.leftFootIK,
            rightFoot = driverIK.rightFootIK
        });
    }
}
