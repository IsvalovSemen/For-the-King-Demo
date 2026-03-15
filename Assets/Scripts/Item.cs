using System.Collections.Generic;

using UnityEngine;
using static UIManager;

public interface IItem
{
    ItemStats Stats { get; }
    int Count { get; }
    float CurrentDurability { get; }
}

public class Item : MonoBehaviour, IInteractable, IItem
{
    public InteractionType interactionType { get; set; }
    [SerializeField] protected ItemStats _stats;
    [SerializeField] protected float _currentDurability;
    [SerializeField] [Range(1, 9999)] private int _count;
    protected SoundManager SoundManager { get; set; }
    protected Rigidbody RigidBody;

    public virtual void Awake()
    {
        SoundManager = transform.GetChild(0).gameObject.AddComponent<SoundManager>();

        SoundManager.sounds = new Sound[Stats.sounds.Length];

        for (int i = 0; i < Stats.sounds.Length; i++)
        {
            SoundManager.sounds[i] = Stats.sounds[i];
        }

        interactionType = InteractionType.Take;

        //bodyMesh = Player.instance.transform.Find("Body").gameObject;

        RigidBody = GetComponent<Rigidbody>();

        RigidBody.mass = Stats.weight;

        _currentDurability = Stats.maxDurability;

        _count = Mathf.Clamp(_count, 1, Stats.maxStacksAmount);
    }

    public ItemStats Stats => _stats;
    public int Count => _count;
    public float CurrentDurability => _currentDurability;

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

    private void OnMouseDown()
    {
        if (UIManager.instance.GetCurrentMenu == MenuState.Inventory && DoubleClick.IsDoubleClick() == true) Take(Player.instance);
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
        _currentDurability += amount;

        SoundManager.PlaySound("GetHit");

        if (_currentDurability <= 0) Break();
    }

    public virtual void Break()
    {
        UIManager.instance.PrintMessage(transform.name + " broke");

        transform.root.GetComponent<Humanoid>().ChangeEquipload(-Stats.weight);

        SoundManager.PlaySound("ItemBroke");

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