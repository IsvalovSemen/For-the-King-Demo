using UnityEngine;

public interface IDamageable
{
    public void GetHit(float damage, DamageType type, Transform part);
}
