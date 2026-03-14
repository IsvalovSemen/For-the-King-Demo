using UnityEngine;

public interface IDamageable
{
    public void GetHit(int damage, DamageType type, Transform part);
}
