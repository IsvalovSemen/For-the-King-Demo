using UnityEngine;

public interface IDamageable
{
    void GetHit(float amount, DamageType type, Transform part);
}
