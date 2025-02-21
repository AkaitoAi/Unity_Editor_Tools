using Invector.vShooter;
using UnityEngine;

public class AlwayAimToggle : MonoBehaviour
{
    [SerializeField] private vShooterManager vShooterManager;

    private bool isAiming = false;

    public void ToggleAlwaysAim()
    {
        isAiming = !isAiming;

        vShooterManager.alwaysAiming = isAiming;
    }

    public void AimState()
    {
        if (isAiming) return;

        vShooterManager.alwaysAiming = isAiming;
    }
}
