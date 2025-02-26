using UnityEngine;
using Gley.TrafficSystem;
using Driveable;
using Driveable.Core;

public class TrafficVehicleDriveable : MonoBehaviour
{
    [SerializeField] private Transform root;
    [SerializeField] private GameObject rccDriveablePrefab;
    [SerializeField] private TrafficDriver trafficDriver;

    public GameObject RCCDriveablePrefab => rccDriveablePrefab;

    private void Awake()
    {
        if (rccDriveablePrefab == null)
            Debug.LogError("RCC Driveable Prefab not assigned to " + gameObject.name, this);
        if (rccDriveablePrefab.GetComponentInChildren<IDriveable>() == null)
            Debug.LogError("RCC Driveable Prefab does not implement IDriveable!", rccDriveablePrefab);
    }

    public IDriveable SpawnRCCDriveableAndRespawnAI(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        GameObject newDriveableObj = Instantiate(rccDriveablePrefab, spawnPosition, spawnRotation);
        IDriveable newDriveable = newDriveableObj.GetComponentInChildren<IDriveable>();

        if (newDriveable == null)
        {
            Debug.LogError("Spawned RCC Driveable does not implement IDriveable!", newDriveableObj);
            Destroy(newDriveableObj);
            return null;
        }

        API.RemoveVehicle(root.gameObject);
        root.gameObject.SetActive(false);

        Vector3 respawnPosition = spawnPosition + Random.onUnitSphere * 100f; 
        respawnPosition.y = spawnPosition.y; 
        API.AddVehicle(respawnPosition, root.GetComponent<VehicleComponent>().vehicleType);

        EventBus<OnTrafficToRCC>.Raise(new OnTrafficToRCC
        { driverIndex =  trafficDriver.driverIndex});

        return newDriveable;
    }
}