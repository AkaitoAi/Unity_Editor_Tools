using AkaitoAi.Extensions;
using UnityEngine;

public class BodyWheelsTransform : MonoBehaviour
{
    [SerializeField] private Transform bodyParent;
    [SerializeField] private Transform wheelCollidersParent;

    [SerializeField] private float chassisYOffset = -.03f;
    [SerializeField] private float wheelsYOffset = 0f;


    private void OnEnable()
    {
        DriveableCar.OnDriveableEnterAction += SetChassisAndWheels;
    }

    private void OnDisable()
    {
        DriveableCar.OnDriveableEnterAction -= SetChassisAndWheels;
    }
    private void SetChassisAndWheels(int id)
    {
        if (id != DriveableEnterExit.GetInstance().driveableInstanceID) return;

        if (!TryGetComponent(out Rigidbody rb)) return;

        rb.isKinematic = true;
        rb.useGravity = false;

        wheelCollidersParent.localPosition = new Vector3(wheelCollidersParent.localPosition.x,
            wheelsYOffset, wheelCollidersParent.localPosition.z);

        bodyParent.localPosition = new Vector3(bodyParent.localPosition.x,
            chassisYOffset, bodyParent.localPosition.z);

        StartCoroutine(AkaitoAiExtensions.SimpleDelay(.5f, () =>
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }));

    }
}
