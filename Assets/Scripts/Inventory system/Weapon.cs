using UnityEngine;

public class Weapon : Loot, IEquipment
{
    private Transform _sheatheSocket;
    private int _handIndex;

    void OnCollisionEnter(Collision coll)
    {
        if (coll.transform.GetComponent<Collider>().gameObject.layer == 3) transform.GetComponentInParent<Creature>().Knockback(this);
    }

    public void Equip()
    {
        foreach (Collider coll in GetComponentsInChildren<Collider>()) coll.enabled = false;

        GetComponent<Rigidbody>().isKinematic = false;

        RigidBody.useGravity = true;

        RigidBody.constraints = RigidbodyConstraints.FreezeAll;

        transform.GetComponent<Rigidbody>().isKinematic = true;

        //weapon.GetComponent<Rigidbody>().freezeRotation = true;

        //weapon.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;

        transform.GetComponentInChildren<MeshRenderer>().enabled = true;
    }
    /*
    public void Draw(Humanoid owner)
    {
        RigidBody.isKinematic = true;

        RigidBody.useGravity = false;

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

        RigidBody.isKinematic = false;

        RigidBody.useGravity = true;

        RigidBody.constraints = RigidbodyConstraints.FreezeAll;

        //RB.freezeRotation = true;

        //RB.constraints = RigidbodyConstraints.FreezePosition;

        transform.GetComponentInChildren<MeshRenderer>().enabled = true;

        transform.position = _sheatheSocket.position;

        transform.rotation = _sheatheSocket.rotation;

        //transform.SetParent(sheatheSocket);

        transform.parent = _sheatheSocket;
    }
    */
    public void Unequip()
    {
        Destroy(this.gameObject);
    }
}