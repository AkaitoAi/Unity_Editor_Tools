using UnityEngine;

public class TriggerDamager : MonoBehaviour
{
    [SerializeField] private float damageAmount;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<IDamageable>(out IDamageable damageable))
            return;

        damageable.Damage(damageAmount);
    }
}
