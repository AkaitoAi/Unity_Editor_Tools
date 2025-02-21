using UnityEngine;

public class CollisionDamager : MonoBehaviour
{
    [SerializeField] private float damageAmount;

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
            return;

        damageable.Damage(damageAmount);
    }
}
