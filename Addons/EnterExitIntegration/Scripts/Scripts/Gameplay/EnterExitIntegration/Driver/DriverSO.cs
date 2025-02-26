using UnityEngine;


[CreateAssetMenu(fileName = "Drivers", menuName = "Driveable/Drivers", order = 1)]
public class DriverSO : ScriptableObject
{
    [SerializeField] private GameObject[] drivers;

    public GameObject GetRandomDriver()
    {
        int driverIndex = Random.Range(0, drivers.Length);

        return drivers[driverIndex];
    }

    public GameObject GetDriverByIndex(int index)
    {
        return drivers[index];
    }

    public int GetTotalDrivers()
    {
        return drivers.Length;
    }

    public int GetRandomDriverIndex()
    { 
        return Random.Range(0, drivers.Length);
    }

    public Animator GetAnimatorByIndex(int index)
    {
        return drivers[index].GetComponent<Animator>();
    }


    //public void Ragdoll(bool active)
    //{
    //    if (driverGO == null) return;

    //    if (driverGO.TryGetComponent(out Animator animator)) animator.enabled = !active;
        
    //    Component[] Rigidbodys = playerRoot.GetComponentsInChildren(typeof(Rigidbody));
    //    Component[] Colliders = playerRoot.GetComponentsInChildren(typeof(Collider));

    //    foreach (Rigidbody rigidbody in Rigidbodys)
    //        rigidbody.isKinematic = !active;


    //    foreach (Collider collider in Colliders)
    //        collider.enabled = active;

    //}
}
