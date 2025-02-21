using UnityEngine;

namespace Driveable
{
    public interface IDriveable
    {
        public Transform Root { get; set; }
        public int DriveableInstanceID { get; set; }
        public Transform Target { get; set; }
        public NavigateToTarget NavigateTo { get; set; }

        public void EnterDriveable(Transform driver);
        public void ExitDriveable(Transform driver);
    }

    // Extended Interface with Generic
    public interface IDriveable<T> : IDriveable where T : MonoBehaviour
    {
        T DriveableController { get; set; }
    }
}


