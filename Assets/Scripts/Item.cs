using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Item : Entity, IInteractable
{
    public InteractionType interactionType { get; set; }
    public List<Transform> hittedTargets = new List<Transform>();
    public ItemStats stats;
    [Range(1, 9999)] public int amount;

    public virtual void Awake()
    {
        SM = transform.GetChild(0).gameObject.AddComponent<SoundManager>();

        SM.sounds = new Sound[stats.sounds.Length];

        for (int i = 0; i < stats.sounds.Length; i++)
        {
            SM.sounds[i] = stats.sounds[i];
        }

        interactionType = InteractionType.Take;

        //bodyMesh = Player.instance.transform.Find("Body").gameObject;

        RB = GetComponent<Rigidbody>();

        RB.mass = stats.weight;

        currentDurability = stats.durability;

    }

    public virtual void Start()
    {

        amount = Mathf.Clamp(amount, 1, stats.maxStacksAmount);
    }

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

        //interactor.GetComponent<Humanoid>().StoreItem(this);
    }

    public virtual void Equip()
    {

    }
    
    public virtual void Unequip()
    {

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

        transform.root.GetComponent<Humanoid>().ChangeEquipload(-stats.weight);

        SM.PlaySound("ItemBroke");

        DestroyItem();
    }

    private void DestroyItem()
    {
        InventoryManager.instance.OnItemPickUpConfirmation -= DestroyItem;

        Destroy(gameObject);
    }

    void OnGUI() // Snaps an object to the mouse position.
    {
        /*
        if (Input.GetMouseButton(0) && equipped == false)
        {      
            Vector2 actualScreenPosition = new Vector2(Event.current.mousePosition.x, Screen.height - (Event.current.mousePosition.y + 25));

            imgObject.transform.position = actualScreenPosition; 
        }
        */
    }
}