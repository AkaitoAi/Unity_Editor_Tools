using UnityEngine;

public class ObjectHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float _maxHealth = 100f;

    public float Health { get; set; }

    private void Start() =>
        Health = _maxHealth;

    public void Damage(float damageAmount)
    {
        Health -= damageAmount;

        if (Health <= 0)
            Die();
    }

    public void Heal(float healAmount) =>
        Health = Mathf.Min(Health + healAmount, _maxHealth);

    private void Die() =>
        Destroy(this.gameObject);

    //public float regenerationRate = 2f;

    //void Update()
    //{
    //    currentHealth = Mathf.Min(currentHealth + regenerationRate * Time.deltaTime, maxHealth);
    //}
}
