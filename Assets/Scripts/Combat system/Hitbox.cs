using System.Collections.Generic;
using UnityEngine;

public class Hitbox : DamageArea
{
    private DamageInstance _damageData;

    [SerializeField] private DamageTypes _damageType = DamageTypes.Pure;

    // Targets already hit during this activation (for single-hit logic).
    private List<Hurtbox> _hittedTargets = new List <Hurtbox>();

    /// <summary>
    /// Additional overload of the constructor to configure hitbox with determined damage rigion (when equipping weapon).
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="region"></param>
    public void Init(Creature owner, RegionType region)
    {
        _owner = owner;
        _region = region;
        _owner.RegisterDamagebox(this);
    }

    /// <summary>
    /// Activate hitbox and assigne damage data to it.
    /// </summary>
    public void Activate(DamageInstance damage)
    {
        _damageData = damage;
        _hittedTargets.Clear();
        _collider.enabled = true;
    }

    /// <summary>
    /// Logic for dealing damage via trigger collider.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Try to get hurtbox on collided object.
        Hurtbox target = other.GetComponent<Hurtbox>();
        if (target == null) return;

        // Prohibit hitting self or hurtbox with same owner.
        if (target == this) return;
        if (target.Owner == _owner) return;

        // Prevent multiple hits if not multihit.
        if (_damageData != null && !_damageData.isMultihit)
        {
            if (_hittedTargets.Contains(target)) return;

            _hittedTargets.Add(target);
        }

        // Call hit registration on hitted hurtbox.
        target.GetHit(_damageData);
    }
}
