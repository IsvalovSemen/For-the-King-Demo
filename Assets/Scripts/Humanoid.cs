using System;
using UnityEditor.Rendering;
using UnityEngine;

public class Humanoid : Creature
{
    [SerializeField] private GameObject _weapon1R;
    [SerializeField] private GameObject _weapon1L;
    [SerializeField] private GameObject _weapon2R;
    [SerializeField] private GameObject _weapon2L;
    public Transform holdPointRight;
    public Transform holdPointLeft;
    public Transform holdPointMiddle;
    public Transform sheathRight;
    public Transform sheathLeft;
    public Transform sheathBack;
    public Transform gearSocket;
    protected float attackDelay;
    [SerializeField] protected bool _attackAllowed;
    [SerializeField] protected bool _weaponDrawn;

    protected override void Start()
    {
        _attackAllowed = true;

        if (inventory != null)
        {
            inventory.OnEquiploadChange += ChangeEquipload;

            inventory.OnItemEquip += PutOn;

            inventory.OnItemUnequip += TakeOff;
        }

        loadStage = Mathf.Clamp(CalculateLoadStage(currentEquipLoad, maxEquipload), 1, 4);

        base.Start();
    }
    protected override void Update()
    {
        Throw();

        base.Update();
    }

    private void PutOn(EquipmentSlotType equipSlot, ItemInstance item)
    {
        Transform socket;

        if (equipSlot == EquipmentSlotType.HandLeft1) socket = holdPointLeft;
        else if (equipSlot == EquipmentSlotType.HandRight1) socket = holdPointRight;
        else socket = gearSocket;

        GameObject newObject = Instantiate(item.Stats.prefab, socket.transform.position, socket.rotation, socket);

        newObject.GetComponent<IEquipment>().Equip();

        if (socket == holdPointLeft) _weapon1L = newObject;
        else if (socket == holdPointRight) _weapon1R = newObject;
    }

    private void TakeOff(EquipmentSlotType equipSlot)
    {

    }
    /// <summary>
    /// TODO: Update this method to work with weapons draw/sheath animations.
    /// </summary>
    protected void DrawWeapon()
    {
        _weaponDrawn = true;
    }
    /// <summary>
    /// TODO: Update this method to work with weapons draw/sheath animations.
    /// </summary>
    protected void SheatheWeapon()
    {
        _weaponDrawn = false;
    }

    protected override void EnableHitbox(AnimationEvent animationEvent)
    {
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
                _weapon1R.GetComponentInChildren<Hitbox>().Enable();

                _weapon1R.GetComponentInChildren<Hitbox>().DealDamage(inventory.equipment[EquipmentSlotType.HandRight1].Stats.damage, inventory.equipment[EquipmentSlotType.HandRight1].Stats.dmgType, this.gameObject);

                _weapon1R.GetComponentInChildren<SoundManager>().PlaySound("Sway");
            }

            if (animationEvent.stringParameter == "left")
            {
                _weapon1L.GetComponentInChildren<Hitbox>().Enable();

                _weapon1L.GetComponentInChildren<Hitbox>().DealDamage(inventory.equipment[EquipmentSlotType.HandLeft1].Stats.damage, inventory.equipment[EquipmentSlotType.HandLeft1].Stats.dmgType, this.gameObject);

                _weapon1L.GetComponentInChildren<SoundManager>().PlaySound("Sway");
            }
            
            //dmg = animationEvent.intParameter;

            //weapon.GetComponent<Item>().attackType = animationEvent.stringParameter;

            //weapon.GetComponent<Collider>().enabled = true;
        }
    }

    protected override void DisableHitbox(AnimationEvent animationEvent)
    {
        if (animationEvent.stringParameter == "right")
        {
            _weapon1R.GetComponentInChildren<Hitbox>().hittedTargets.Clear();

            _weapon1R.GetComponentInChildren<Hitbox>().Disable();
        }
        else if (animationEvent.stringParameter == "left")
        {
            _weapon1L.GetComponentInChildren<Hitbox>().hittedTargets.Clear();

            _weapon1L.GetComponentInChildren<Hitbox>().Disable();
        }

        //weapon.GetComponent<Item>().hittedTarget = null;

        //weapon.GetComponent<Collider>().enabled = false;

        //dmg = 0;

        //animator.SetTrigger("Reset");
    }

    public void AttackDelay(AnimationEvent animationEvent)
    {
        if (animationEvent.intParameter == 0) _attackAllowed = false;
        else if (animationEvent.intParameter == 1) _attackAllowed = true;
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
