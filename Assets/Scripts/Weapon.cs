using UnityEngine.UI;
using UnityEngine;
using static UnityEditor.Progress;
using UnityEngine.XR;
using UnityEngine.Animations.Rigging;

public class Weapon : Item
{
    private Transform _sheatheSocket;
    private int _handIndex;

    void OnCollisionEnter(Collision coll)
    {
        DealDamage(coll);
    }

    void DealDamage(Collision coll)
    {
        Transform part = coll.transform.GetComponent<Collider>().transform;

        if (coll.transform.GetComponent<Collider>().gameObject.layer == 6)
        {
            //Collider myCollider = collision.GetContact(0).thisCollider;

            if (coll.transform.GetComponentInParent<Creature>() != transform.GetComponentInParent<Creature>() & coll.transform.GetComponent<Collider>().GetComponentInParent<IDamageable>() != null & !hittedTargets.Contains(coll.transform.GetComponent<Collider>().transform.root.transform))
            {
                hittedTargets.Add(coll.transform.GetComponent<Collider>().transform.root.transform);

                coll.transform.GetComponentInParent<IDamageable>().GetHit(-stats.damage, stats.dmgType, part);


                UIManager.instance.PrintMessage(coll.transform.GetComponentInParent<Creature>().gameObject.name + " got hit by " + transform.GetComponentInParent<Creature>().gameObject.name + " with " + transform.name + ", taking " + stats.damage + " " + stats.dmgType.ToString() + " damage to " + part.name);

                
            }
        }

        if (coll.transform.GetComponent<Collider>().gameObject.layer == 3)
        {
            transform.root.GetComponent<Creature>().Knockback(transform);

            //GameMaster.instance.player.GetComponent<Player>().Noise(collision.contacts[0].point, 10 * _RB.mass);

            UIManager.instance.PrintMessage(transform.root.name + " hits " + coll.transform.GetComponent<Collider>().transform.name);

            if (coll.transform.GetComponent<IDamageable>() != null) coll.transform.GetComponent<IDamageable>().GetHit(-stats.damage, stats.dmgType, part);
            else SM.PlaySound("Hit");
        }
    }

    public override void Equip()
    {
        foreach (Collider coll in GetComponentsInChildren<Collider>()) coll.enabled = false;

        GetComponent<Rigidbody>().isKinematic = false;

        RB.useGravity = true;

        RB.constraints = RigidbodyConstraints.FreezeAll;

        transform.GetComponent<Rigidbody>().isKinematic = true;

        //weapon.GetComponent<Rigidbody>().freezeRotation = true;

        //weapon.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;

        transform.GetComponentInChildren<MeshRenderer>().enabled = true;

        //owner.OnDrawWeapon += Draw;

        //owner.OnSheathWeapon += Sheath;

        UIManager.instance.PrintMessage(transform.name + " was equiped");
    }

    public override void Unequip()
    {
        transform.SetParent(null);

        RB.isKinematic = false;

        RB.useGravity = true;

        RB.freezeRotation = false;

        RB.isKinematic = false;
    }

    public void Draw(Humanoid owner)
    {
        RB.isKinematic = true;

        RB.useGravity = false;

        if (_handIndex == 0)
        {
            transform.position = owner.holdPointRight.position;

            transform.rotation = owner.holdPointRight.rotation;

            transform.parent = owner.holdPointRight;

            owner.animator.SetTrigger("Draw");

            owner.animator.SetInteger("Hand", 1);
        }
        else if (_handIndex == 1)
        {
            transform.position = owner.holdPointLeft.position;

            transform.rotation = owner.holdPointLeft.rotation;

            transform.parent = owner.holdPointLeft;

            owner.animator.SetTrigger("Draw");

            owner.animator.SetInteger("Hand", 2);
        }
    }

    public void Sheath(Humanoid owner)
    {
        foreach (Collider coll in GetComponentsInChildren<Collider>()) coll.enabled = false;

        RB.isKinematic = false;

        RB.useGravity = true;

        RB.constraints = RigidbodyConstraints.FreezeAll;

        //RB.freezeRotation = true;

        //RB.constraints = RigidbodyConstraints.FreezePosition;

        transform.GetComponentInChildren<MeshRenderer>().enabled = true;

        transform.position = _sheatheSocket.position;

        transform.rotation = _sheatheSocket.rotation;

        //transform.SetParent(sheatheSocket);

        transform.parent = _sheatheSocket;
    }
}