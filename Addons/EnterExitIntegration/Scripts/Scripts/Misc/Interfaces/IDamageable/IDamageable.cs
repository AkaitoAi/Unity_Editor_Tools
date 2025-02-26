public interface IDamageable
{
    public float Health { get; set; }
    public void Damage(float damageAmount);
    public void Heal(float healAmount);
}
