using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Entity, IPortable
{
    public InteractionType interactionType { get; set; }
    [SerializeField] private bool _inRadius;
    [SerializeField] public bool holding { get; set; }
    [SerializeField] private Vector3 _holdOffset;
    [SerializeField] private Transform _holdPoint;

    public override void Awake()
    {
        interactionType = InteractionType.Take;

        base.Awake();
    }

    private void LateUpdate()
    {
        if (holding)
        {
            if (transform.position != _holdPoint.transform.position) transform.position = Vector3.Slerp(transform.position, _holdPoint.transform.position, .1f);
            
            //if (transform.rotation != _holdPoint.transform.rotation) transform.rotation = Quaternion.Slerp(transform.rotation, _holdPoint.transform.rotation, .01f);
        }
        else if (RB.velocity.magnitude >= dealDmgThreshold) foreach (Collider coll in GetComponentsInChildren<Collider>()) coll.enabled = true;
    }

    public void Interaction(Humanoid source)
    {
        _holdPoint = source.holdPointMiddle;

        if (_inRadius) Hold();
    }

    private void Hold()
    {
        holding = true;

        transform.position = _holdPoint.transform.position;

        transform.rotation = _holdPoint.transform.rotation;

        transform.SetParent(_holdPoint);

        foreach (Collider coll in GetComponentsInChildren<Collider>()) coll.enabled = false;

        // DungeonMaster.instance.player.GetComponent<Entity>().animator.SetTrigger("Throw");

        // DungeonMaster.instance.player.GetComponent<Entity>().animator.SetFloat("Speed", 0);

        RB.isKinematic = true;

        //RB.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void Throw(Vector3 force)
    {
        RB.isKinematic = false;

        RB.AddForce(force, ForceMode.Impulse);

        transform.SetParent(null);

        holding = false;
    }

    private void OnTriggerStay(Collider trigger)
    {
        if (trigger.gameObject.layer == 6) _inRadius = true;
    }

    private void OnTriggerExit(Collider trigger)
    {
        if (trigger.gameObject.layer == 6) _inRadius = false;
    }
}
