using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEditor.PackageManager;
using UnityEditor.Rendering;
using UnityEngine;
using static UIManager;

public interface IItem
{
    ItemStats Stats { get; }
    int Count { get; }
    float CurrentDurability { get; }
}

public class Item : Entity, IInteractable, IItem
{
    public InteractionType interactionType { get; set; }
    public List<Transform> hittedTargets = new List<Transform>();
    [SerializeField] protected ItemStats _stats;
    [SerializeField] [Range(1, 9999)] private int _count;

    public virtual void Awake()
    {
        SM = transform.GetChild(0).gameObject.AddComponent<SoundManager>();

        SM.sounds = new Sound[_stats.sounds.Length];

        for (int i = 0; i < _stats.sounds.Length; i++)
        {
            SM.sounds[i] = _stats.sounds[i];
        }

        interactionType = InteractionType.Take;

        //bodyMesh = Player.instance.transform.Find("Body").gameObject;

        RB = GetComponent<Rigidbody>();

        RB.mass = _stats.weight;

        currentDurability = _stats.maxDurability;

        _count = Mathf.Clamp(_count, 1, _stats.maxStacksAmount);
    }

    public ItemStats Stats => _stats;
    public int Count => _count;
    public float CurrentDurability => currentDurability;

    public virtual void Use()
    {

    }

    public void Interaction(Creature interactor)
    {
        Take(interactor);
    }

    public void Take(Creature interactor)
    {
        InventoryManager.instance.OnItemPickUpConfirmation += DestroyItem;

        InventoryManager.instance.PickUpItem(interactor.inventory, this);
    }

    public virtual void Equip()
    {

    }
    
    public virtual void Unequip()
    {

    }

    private void OnMouseEnter()
    {
        if (UIManager.instance.GetCurrentMenu == MenuState.Inventory) UIManager.instance.ShowItemTooltip(this);
    }

    private void OnMouseDrag()
    {
        if (UIManager.instance.GetCurrentMenu == MenuState.Inventory)
        {
            Ray ray = CameraControl.instance.mainCam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, GameMaster.instance.interactionDistance, LayerMask.GetMask("Environment")))
            {
                transform.position = hitInfo.point;
            }
        }
    }

    private void OnMouseExit()
    {
        if (UIManager.instance.GetCurrentMenu == MenuState.Inventory) UIManager.instance.HideItemTooltip();
    }

    public void SetCount(int value)
    {
        _count = value;
    }

    void ChangeDurability(int amount)
    {
        currentDurability += amount;

        SM.PlaySound("GetHit");

        if (currentDurability <= 0) Break();
    }

    public virtual void Break()
    {
        UIManager.instance.PrintMessage(transform.name + " broke");

        transform.root.GetComponent<Humanoid>().ChangeEquipload(-_stats.weight);

        SM.PlaySound("ItemBroke");

        DestroyItem();
    }

    private void DestroyItem()
    {
        InventoryManager.instance.OnItemPickUpConfirmation -= DestroyItem;

        Destroy(gameObject);
    }
    /*
    void OnGUI() // Snaps an object to the mouse position.
    {
        
        if (Input.GetMouseButton(0) && equipped == false)
        {      
            Vector2 actualScreenPosition = new Vector2(Event.current.mousePosition.x, Screen.height - (Event.current.mousePosition.y + 25));

            imgObject.transform.position = actualScreenPosition;
        }
        
    }*/
}