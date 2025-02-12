using UnityEngine;

public interface IDriveable
{
    public int DriveableInstanceID { get; set; }

    public Transform Target { get; set; }

    public NavigateToTarget NavigateTo { get; set; }

    public void EnterDriveable(Transform driver);
    public void ExitDriveable(Transform driver);
}

