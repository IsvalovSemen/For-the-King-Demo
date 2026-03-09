using System;
using UnityEngine;

public class Humanoid : Creature
{
    public Transform holdPointRight;
    public Transform holdPointLeft;
    public Transform holdPointMiddle;
    public Transform sheathRight;
    public Transform sheathLeft;
    public Transform sheathBack;
    protected bool WeaponDrawn;
    public event Action<Humanoid> OnDrawWeapon;
    public event Action<Humanoid> OnSheathWeapon;
    protected float attackDelay;
    public bool attackAllowed;

    public override void Start()
    {
        attackAllowed = true;

        base.Start();

        loadStage = Mathf.Clamp(CalculateLoadStage(currentEquipLoad, maxEquipload), 1, 4);
    }
    public override void Update()
    {
        Throw();

        base.Update();
    }

    protected void DrawOrSheathWeapon()
    {
        if (WeaponDrawn == false) OnDrawWeapon?.Invoke(this);
        else OnSheathWeapon?.Invoke(this);

        WeaponDrawn = !WeaponDrawn;
    }

    public void EnableHitbox(AnimationEvent animationEvent)
    {/*
        if (_recoil)
        {
            animator.SetTrigger("Stop");

            animator.ResetTrigger("Attack");

            _recoil = false;
        }
        else
        {
            if (animationEvent.stringParameter == "right")
            {
                GameObject weaponR = slots.Find(x => x.inventorySlot == EquipmentSlotType.HandRight).itemLink;

                foreach (Collider coll in weaponR.transform.GetComponentsInChildren<Collider>()) coll.enabled = true;

                weaponR.GetComponentInChildren<SoundManager>().PlaySound("Sway");
            }
            else if (animationEvent.stringParameter == "left")
            {
                GameObject weaponL = slots.Find(x => x.inventorySlot == EquipmentSlotType.HandLeft).itemLink;

                foreach (Collider coll in weaponL.transform.GetComponentsInChildren<Collider>()) coll.enabled = true;

                weaponL.GetComponentInChildren<SoundManager>().PlaySound("Sway");
            }
            
            //dmg = animationEvent.intParameter;

            //weapon.GetComponent<Item>().attackType = animationEvent.stringParameter;

            //weapon.GetComponent<Collider>().enabled = true;
        }*/
    }

    public void DisableHitbox(AnimationEvent animationEvent)
    {
        /*
        if (animationEvent.stringParameter == "right")
        {
            GameObject weaponR = slots.Find(x => x.inventorySlot == EquipmentSlotType.HandRight).itemLink;

            weaponR.transform.GetComponentInChildren<Item>().hittedTargets.Clear();

            foreach (Collider coll in weaponR.transform.GetComponentsInChildren<Collider>()) coll.enabled = false;
        }
        else if (animationEvent.stringParameter == "left")
        {
            GameObject weaponL = slots.Find(x => x.inventorySlot == EquipmentSlotType.HandLeft).itemLink;

            weaponL.transform.GetComponentInChildren<Item>().hittedTargets.Clear();

            foreach (Collider coll in weaponL.transform.GetComponentsInChildren<Collider>()) coll.enabled = false;
        }
        */
        //weapon.GetComponent<Item>().hittedTarget = null;

        //weapon.GetComponent<Collider>().enabled = false;

        //dmg = 0;

        //animator.SetTrigger("Reset");
    }

    public void AttackDelay(AnimationEvent animationEvent)
    {
        if (animationEvent.intParameter == 0) attackAllowed = false;
        else if (animationEvent.intParameter == 1) attackAllowed = true;
    }

    public void StoreItem(Item item)
    {
        ChangeEquipload(item.stats.weight);
    }

    /*public void DropItem(ItemSlot slot)
    {
        GameObject item;

        RaycastHit hit;

        Ray ray = CameraControl.instance.mainCam.ScreenPointToRay(Input.mousePosition); // Fires a beam from the camera to the point where the cursor is located on the screen.
        
        if (Physics.Raycast(ray, out hit) && hit.transform.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
            float placingDistance = UnityEngine.Vector3.Distance(Player.instance.transform.position, hit.point); // Calculate the distance to the point where the object can be placed.

            if (hit.transform != null && placingDistance <= GameMaster.instance.interactionDistance && placingDistance > 0.5f) // Only if the distance to the surface is adequate.
            {
                item = Instantiate(InventoryManager.instance.GetItemByID(slot.storedItemID).prefab, hit.transform.position, hit.transform.rotation);

                item.transform.position = hit.point; // The object appears in the place where the beam hits.
            }
        }
        else UIManager.instance.PrintMessage("Cannot drop the item!");

        UIManager.instance.PrintMessage((InventoryManager.instance.GetItemByID(slot.storedItemID).itemTitle) + " was dropped.");

        slot.storedItemID = "";
    }*/

    public virtual void Throw()
    {

    }
}
