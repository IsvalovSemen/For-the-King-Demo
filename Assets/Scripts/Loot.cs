using UnityEngine;
using static UIManager;

public class Loot : MonoBehaviour, IInteractable
{
    [SerializeField] private Item _item;
    public Item Item => _item;
    public InteractionType interactionType { get; set; }
    private ItemPreview _preview;
    protected SoundManager SoundManager { get; set; }
    protected Rigidbody RigidBody;

    public virtual void Awake()
    {
        SoundManager = transform.GetChild(0).gameObject.AddComponent<SoundManager>();

        SoundManager.sounds = new Sound[_item.Stats.sounds.Length];

        for (int i = 0; i < _item.Stats.sounds.Length; i++)
        {
            SoundManager.sounds[i] = _item.Stats.sounds[i];
        }

        interactionType = InteractionType.Take;

        //bodyMesh = Player.instance.transform.Find("Body").gameObject;

        RigidBody = GetComponent<Rigidbody>();

        RigidBody.mass = _item.Stats.weight;

        _item.SetCount(Mathf.Clamp(_item.Count, 1, 999));
    }

    private void Update()
    {/*
        if (IsSelected == true)
        {
            _preview = Instantiate(UIManager.instance.itemPreviewPrefab, UIManager.instance.transform);
        }
        else Destroy(_preview);*/
    }

    public void Interaction(Creature interactor)
    {
        Take(interactor);
    }

    public void OnSelect()
    {
        UIManager.instance.EnableInteractionPrompt(interactionType);

        _preview = Instantiate(UIManager.instance.itemPreviewPrefab, UIManager.instance.transform);

        _preview.Init(_item);
    }

    public void OnDeselect()
    {
        UIManager.instance.DisableInteractionPrompt();

        _preview.DestroyPreview();
    }

    public void Take(Creature interactor)
    {
        InventoryManager.instance.OnItemPickUpConfirmation += DestroyItem;

        InventoryManager.instance.PickUpItem(interactor.inventory, _item);
    }

    private void OnMouseDown()
    {
        if (UIManager.instance.GetCurrentMenu == MenuState.Inventory && DoubleClick.IsDoubleClick() == true) Take(Player.instance);
    }

    private void OnMouseEnter()
    {
        if (UIManager.instance.GetCurrentMenu == MenuState.Inventory) UIManager.instance.ShowItemTooltip(_item);
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

    private void DestroyItem()
    {
        InventoryManager.instance.OnItemPickUpConfirmation -= DestroyItem;

        Destroy(gameObject);
    }
}