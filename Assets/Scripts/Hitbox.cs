using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField] private Collider _collider;
    private Collider _targetCollider;
    public List<IDamageable> hittedTargets = new List<IDamageable>();

    public void Enable()
    {
        _collider.enabled = true;
    }

    public void Disable()
    {
        _collider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger && other.GetComponentInChildren<IDamageable>() != null)
        {
            hittedTargets.Add(other.GetComponent<IDamageable>());

            _targetCollider = other;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == _targetCollider) _targetCollider = null;
    }

    public void DealDamage(int dmg, DamageType type, GameObject source)
    {
        if (_targetCollider != null)
        {
            _targetCollider.GetComponentInChildren<IDamageable>().GetHit(dmg, type, _targetCollider.transform);

            UIManager.instance.PrintMessage($"{_targetCollider.gameObject.name} got hit by {source.name} with {transform.name} taking {dmg} {type} damage.");
        }
    }

}
