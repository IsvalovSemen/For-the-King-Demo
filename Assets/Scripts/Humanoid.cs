using System;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.XR;

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
    /// <summary>
    /// FIXME: This method is weird. Logic need to be updated and transferred to the item itself that is being equipped (somehow).
    /// </summary>
    /// <param name="equipSlot"></param>
    /// <param name="item"></param>
    private void PutOn(EquipmentSlotType equipSlot, Item item)
    {
        Transform socket;

        if (equipSlot == EquipmentSlotType.HandLeft1) socket = holdPointLeft;
        else if (equipSlot == EquipmentSlotType.HandRight1) socket = holdPointRight;
        else socket = gearSocket;

        GameObject newObject = Instantiate(item.Stats.prefab, socket.transform.position, socket.transform.rotation, socket);

        if (item.Stats.itemType == ItemSlotType.Weapon)
        {
            foreach (Collider collider in newObject.GetComponentsInChildren<Collider>()) collider.enabled = false;

            var RB = newObject.GetComponent<Rigidbody>();

            RB.isKinematic = false;

            RB.useGravity = true;

            RB.constraints = RigidbodyConstraints.FreezeAll;

            RB.isKinematic = true;

            //RB.freezeRotation = true;

            //RB.constraints = RigidbodyConstraints.FreezePosition;

            if (equipSlot == EquipmentSlotType.HandLeft1) _weapon1L = newObject;
            else if (equipSlot == EquipmentSlotType.HandRight1) _weapon1R = newObject;
        }
        else if (item.Stats.itemType == ItemSlotType.Torso)
        {
            Destroy(newObject.GetComponentInChildren<Rigidbody>());

            foreach (var collider in newObject.GetComponentsInChildren<Collider>()) Destroy(collider);

            var targetSkinnedMesh = socket.GetComponentInChildren<SkinnedMeshRenderer>();

            newObject.GetComponentInChildren<SkinnedMeshRenderer>().bones = targetSkinnedMesh.bones;

            newObject.GetComponentInChildren<SkinnedMeshRenderer>().rootBone = targetSkinnedMesh.rootBone;
        }
    }

    private void TakeOff(EquipmentSlotType equipSlot)
    {
        Transform socket;

        if (equipSlot == EquipmentSlotType.HandLeft1) socket = holdPointLeft;
        else if (equipSlot == EquipmentSlotType.HandRight1) socket = holdPointRight;
        else socket = gearSocket;

        GameObject.Destroy(socket.GetChild(0).gameObject);
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

        //animator.SetTrigger("Reset");
    }

    public void AttackDelay(AnimationEvent animationEvent)
    {
        if (animationEvent.intParameter == 0) _attackAllowed = false;
        else if (animationEvent.intParameter == 1) _attackAllowed = true;
    }

    public virtual void Throw()
    {

    }
}
