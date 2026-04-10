using UnityEngine;

public abstract class DamageArea : MonoBehaviour
{
    protected Creature _owner;
    public Creature Owner => _owner;
    [SerializeField] protected RegionType _region;
    public RegionType Region => _region;
    [SerializeField] protected Collider _collider;

    [Header("Damage modifiers:")]
    [SerializeField] protected float _damageModifier = 1f;
    [SerializeField] protected float _impactModifier = 1f;

    /// <summary>
    /// Initialize this area and assign it to owner.
    /// </summary>
    public virtual void Init(Creature owner)
    {
        _owner = owner;

        _owner.RegisterDamagebox(this);
    }
    /// <summary>
    /// Activate linked collider.
    /// </summary>
    public virtual void Activate()
    {
        _collider.enabled = true;
    }
    /// <summary>
    /// Disable associated collider.
    /// </summary>
    public virtual void Deactivate()
    {
        _collider.enabled = false;
    }

    /// <summary>
    /// Cleanup when object is destroyed or unregistered.
    /// </summary>
    public void Unregister()
    {
        if (_owner != null)
        {
            _owner.UnregisterHitbox(this);
            _owner = null;
        }
    }
}
