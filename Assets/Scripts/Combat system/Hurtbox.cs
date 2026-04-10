using UnityEngine;
using static UnityEditor.Progress;

public class Hurtbox : DamageArea
{
    /// <summary>
    /// Processes incoming damage instance and transmits it to the owner.
    /// </summary>
    /// <param name="damage"></param>
    public void GetHit(DamageInstance damage)
    {
        if (damage == null) return;

        // Apply local modifiers.
        int finalDamage = Mathf.RoundToInt(damage.dmgValue * _damageModifier);
        int finalImpact = Mathf.RoundToInt(damage.poiseDmg * _impactModifier);

        // Create modified instance (local copy).
        DamageInstance finalDamageInstance = new DamageInstance(damage.source, finalDamage, damage.dmgType, finalImpact, damage.objDmg, damage.dir);

        if (_owner != null)
        {
            _owner.TakeDamage(finalDamageInstance);
        }

        UIManager.instance.PrintMessage($"{damage.source.name} hit {_owner.name} for {finalDamage} {damage.dmgType} damage.");
    }
}
