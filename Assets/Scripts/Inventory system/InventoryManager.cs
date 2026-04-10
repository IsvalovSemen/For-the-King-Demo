using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Net.Sockets;
using UnityEditor.Rendering;
using static Creature;
using static Hitbox;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    private UIManager _UIManager;

    [Header("Grid:")]
    public InventoryGrid grid;
    [SerializeField] private int _gridWidth = 4;
    [SerializeField] private int _gridHeight = 3;
    [SerializeField] private List<EquipSlot> _equipSlots;

    private Item _draggedItem;
    private bool _isDragging;

    private bool _isPickingUp;
    private Item _originalItem;
    private int _originalX, _originalY;

    private Item _itemPendingEquip;
    private bool _selectingEquipSlot;
    private int _currentEquipIndex;

    private int _pointerXPos, _pointerYPos;

    private Vector2Int lastMoveDir;
    private Item currentHoveredItem;

    public event Action OnItemPickupConfirmation;
    public enum InventoryState
    {
        Default,
        Dragging,
        EquipNavigation
    }

    private InventoryState _state = InventoryState.Default;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("More than one InventoryManager.", this); // Proper way to throw an error with a link to the cause.

            Destroy(gameObject);
        }

        grid = new InventoryGrid(_gridWidth, _gridHeight);

        _UIManager = UIManager.instance;
    }

    private void Update()
    {
        HandleInputs();
    }

    public bool HasItem(string ID)
    {
        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                Item item = grid.GetItem(x, y);

                if (item != null && item.Data.ID == ID) return true;
            }
        }

        foreach (var slot in _equipSlots)
        {
            if (slot.item != null && slot.item.Data.ID == ID) return true;
        }

        return false;
    }
    #region POINTER
    public void MovePointer(int dx, int dy)
    {
        Vector2Int dir = new Vector2Int(dx, dy);

        int newX = _pointerXPos + dx;
        int newY = _pointerYPos + dy;

        int maxX = grid.width;
        int maxY = grid.height;
        // the position of the icon so that it cannot go outside the grid bounds.
        if (_isDragging == true)
        {
            maxX -= _draggedItem.Width;
            maxY -= _draggedItem.Height;
        }
        else
        {
            maxX -= 1;
            maxY -= 1;
        }

        newX = Mathf.Clamp(newX, 0, maxX);
        newY = Mathf.Clamp(newY, 0, maxY);
        // Checks whether the item will appear after the pointer changes its position.
        Item nextItem = grid.GetItem(newX, newY);

        if (_isDragging == false && currentHoveredItem != null && currentHoveredItem == grid.GetItem(_pointerXPos, _pointerYPos))
        {
            JumpOverItem(currentHoveredItem, dx, dy, ref newX, ref newY);

            nextItem = grid.GetItem(newX, newY);
        }

        _pointerXPos = newX;
        _pointerYPos = newY;

        currentHoveredItem = nextItem;
        lastMoveDir = dir;

        if (_selectingEquipSlot == false)
        {
            int horizontalPos = _pointerXPos;
            int verticalPos = _pointerYPos;
            int width = 1;
            int height = 1;

            if (_isDragging == true)
            {
                width = _draggedItem.Width;
                height = _draggedItem.Height;
                // Changes the color of the icon to red if it is currently over another item.
                if (grid.CanPlaceHere(_draggedItem, _pointerXPos, _pointerYPos) == false) _UIManager.icons[_draggedItem].IconImg.color = Color.red;
                else _UIManager.icons[_draggedItem].IconImg.color = Color.white;

                _UIManager.icons[_draggedItem].transform.SetAsLastSibling(); // Moves the icon of the dragged icon down the hierarchy so that it is not overlapped by others.

                _UIManager.UpdateRect(_UIManager.icons[_draggedItem].GetComponent<RectTransform>(), _pointerXPos, _pointerYPos, _draggedItem.Width, _draggedItem.Height);
            }
            else
            {
                Item item = grid.GetItem(_pointerXPos, _pointerYPos); // Searches for item underneath pointer.

                if (item != null)
                {
                    width = item.Width;
                    height = item.Height;

                    horizontalPos = item.x;
                    verticalPos = item.y;
                }
            }

            _UIManager.pointer.pivot = new Vector2(0f, 1f);

            _UIManager.UpdateRect(_UIManager.pointer, horizontalPos, verticalPos, width, height);

            _UIManager.pointer.transform.SetAsLastSibling();
        }
    }
    /// <summary>
    /// Snap pointer along the direction in which it entered the item from one of the sides (yes, it's complicated).
    /// </summary>
    /// <param name="item"></param>
    /// <param name="dx"></param>
    /// <param name="dy"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private void JumpOverItem(Item item, int dx, int dy, ref int x, ref int y)
    {
        // Item boundaries.
        int minX = item.x;
        int minY = item.y;
        int maxX = item.x + item.Width - 1;
        int maxY = item.y + item.Height - 1;

        if (dx > 0) // Right.
            x = maxX + 1;

        else if (dx < 0) // Left.
            x = minX - 1;

        else if (dy > 0) // Down.
            y = maxY + 1;

        else if (dy < 0) // Up.
            y = minY - 1;

        x = Mathf.Clamp(x, 0, grid.width - 1);
        y = Mathf.Clamp(y, 0, grid.height - 1);
    }
    #endregion

    #region INPUTS
    private void HandleInputs()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (_state == InventoryState.Default || _state == InventoryState.Dragging) MovePointer(-1, 0);

            if (_state == InventoryState.EquipNavigation) SwitchToEquipSlot(_currentEquipIndex - 1);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (_state == InventoryState.Default || _state == InventoryState.Dragging) MovePointer(1, 0);

            if (_state == InventoryState.EquipNavigation) SwitchToEquipSlot(_currentEquipIndex + 1);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (_state == InventoryState.Default || _state == InventoryState.Dragging) MovePointer(0, -1);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (_state == InventoryState.Default || _state == InventoryState.Dragging) MovePointer(0, 1);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_state == InventoryState.Default)
            {
                Item selectedItem = grid.GetItem(_pointerXPos, _pointerYPos);

                if (selectedItem != null)
                {
                    BeginDrag(selectedItem);

                    return;
                }
            }

            if (_state == InventoryState.Dragging) Place();

            if (_state == InventoryState.EquipNavigation) TryEquip(_itemPendingEquip);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (_state == InventoryState.Dragging)
            {
                _draggedItem.Rotate();
                // Push item back inbounds if icon sticks of the grid frame after rotation.
                _pointerXPos = Mathf.Clamp(_pointerXPos, 0, grid.width - _draggedItem.Width);
                _pointerYPos = Mathf.Clamp(_pointerYPos, 0, grid.height - _draggedItem.Height);

                _UIManager.UpdateRect(_UIManager.icons[_draggedItem].GetComponent<RectTransform>(), _draggedItem.x, _draggedItem.y, _draggedItem.Width, _draggedItem.Height);

                _UIManager.icons[_draggedItem].UpdateVisuals();

                _UIManager.UpdateRect(_UIManager.pointer, _draggedItem.x, _draggedItem.y, _draggedItem.Width, _draggedItem.Height);

                Debug.Log($"{_draggedItem.Data.itemTitle} was rotated.");
            }
        }
        // Cnacel drag.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_state == InventoryState.Dragging) CancelDrag();
        }
        // Drop item.
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (_state == InventoryState.Default)
            {
                Item itemToDrop = grid.GetItem(_pointerXPos, _pointerYPos);

                if (itemToDrop != null) DropItem(itemToDrop, false);

                return;
            }

            if (_state == InventoryState.EquipNavigation)
            {
                Item itemToDrop = _equipSlots[_currentEquipIndex].item;

                if (itemToDrop != null) DropItem(itemToDrop, false);

                return;
            }
        }
        // Switch to equipment browsing.
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (_state == InventoryState.Default)
            {
                SwitchToEquipSlot(0);

                _state = InventoryState.EquipNavigation;

                return;
            }

            if (_state == InventoryState.EquipNavigation)
            {
                MovePointer(-_pointerXPos, -_pointerYPos);

                _state = InventoryState.Default;

                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (_state == InventoryState.Default)
            {
                Item itemToEquip = grid.GetItem(_pointerXPos, _pointerYPos);

                if (itemToEquip != null)
                {
                    SwitchToEquipSlot(0);

                    StartEquipSlotSelection(itemToEquip);

                    TakeFromGrid(itemToEquip);

                    _state = InventoryState.EquipNavigation;

                    return;
                }
            }

            if (_state == InventoryState.EquipNavigation)
            {
                Item itemToEquip = _equipSlots[_currentEquipIndex].item;

                if (itemToEquip != null)
                {
                    StartEquipSlotSelection(itemToEquip);

                    TakeFromEquipslot(_currentEquipIndex);

                    SwitchToEquipSlot(0);

                    return;
                }
            }
        }
    }
    #endregion

    #region ITEM PICKUP
    /// <summary>
    /// Activated externally on loot when picked up from scene.
    /// </summary>
    /// <param name="item"></param>
    public void StartPickUp(Item item)
    {
        _UIManager.OpenMenu(UIManager.MenuState.Inventory);

        _UIManager.CreateItemIcon(item);

        if (TryAutoPlace(item) == true)
        {
            Store(item);

            return;
        }

        _draggedItem = item;

        _isDragging = true;

        _isPickingUp = true;

        Debug.Log($"Begun placement selection of {item.Data.itemTitle}.");
    }
    /// <summary>
    /// Attempt to automatically place an item in the grid if there is space available to do so.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool TryAutoPlace(Item item)
    {
        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                if (grid.CanPlaceHere(item, x, y))
                {
                    grid.Place(item, x, y);

                    MovePointer(x, y);

                    Debug.Log($"{item.Data.itemTitle} was automatically placed in inventory.");

                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region ITEM PLACEMENT
    /// <summary>
    /// Temporarily remove item from the grid and cash its original position.
    /// </summary>
    /// <param name="item"></param>
    private void TakeFromGrid(Item item)
    {
        if (item == null) return;

        _originalItem = item;
        _originalX = item.x;
        _originalY = item.y;

        grid.Remove(item);

        Debug.Log($"{item.Data.itemTitle} was taken from grid.");
    }
    /// <summary>
    /// Switch inventory to the "drag" state.
    /// </summary>
    /// <param name="item"></param>
    private void BeginDrag(Item item)
    {
        if (item == null) return;

        TakeFromGrid(item);

        _draggedItem = item;
        _isDragging = true;

        _state = InventoryState.Dragging;

        Debug.Log($"Now dragging {_draggedItem.Data.itemTitle}.");
    }
    /// <summary>
    /// Try place dragged item on the grid.
    /// </summary>
    private void Place()
    {
        if (grid.CanPlaceHere(_draggedItem, _pointerXPos, _pointerYPos) == true)
        {
            if (_isPickingUp == true) // If pick up item from scene.
            {
                Store(_draggedItem);

                _isPickingUp = false;
            }

            Debug.Log($"{_draggedItem.Data.itemTitle} was placed on grid.");

            grid.Place(_draggedItem, _pointerXPos, _pointerYPos);
            _draggedItem = null;
            _isDragging = false;
            _state= InventoryState.Default;
        }
        else CancelDrag();
    }
    /// <summary>
    /// If item was taken from world, ping it and add it's weight to Player.
    /// </summary>
    /// <param name="item"></param>
    private void Store(Item item)
    {
        Player.instance.ChangeEquipload(item.Data.weight * item.Quantity);

        OnItemPickupConfirmation?.Invoke();

        _UIManager.PrintMessage($"{item.Data.itemTitle} was successfully stored in inventory.");
    }
    /// <summary>
    /// Stop drag and clear all dragging info.
    /// </summary>
    private void CancelDrag()
    {
        if (_originalItem != null)
        {
            _UIManager.icons[_draggedItem].IconImg.color = Color.white;

            _draggedItem = null;

            _isDragging = false;

            grid.Place(_originalItem, _originalX, _originalY); // Swaps icon and pointer back to position on grid where item was taken from.

            MovePointer(_originalX, _originalY);

            _originalItem = null;

            _state = InventoryState.Default;

            Debug.Log($"Drag of {_draggedItem.Data.itemTitle} was cancelled.");
        }
    }
    #endregion

    #region ITEM DROP
    /// <summary>
    /// Create item model on scene after drop. If dropping by mouse drag, place item in place where cursor is pointing, else if via key, place item at the surface, camera is facing.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="viaCursor"></param>
    public void DropItem(Item item, bool viaCursor)
    {
        RaycastHit hitInfo;

        if (viaCursor == true)
        {
            Ray ray = CameraControl.instance.mainCam.ScreenPointToRay(Input.mousePosition);

            Physics.Raycast(ray, out hitInfo, GameMaster.instance.interactionDistance, LayerMask.GetMask("Environment"));

        }
        else
        {
            Ray ray = new Ray(CameraControl.instance.mainCam.transform.position, CameraControl.instance.mainCam.transform.forward);

            Physics.Raycast(ray, out hitInfo, GameMaster.instance.interactionDistance, LayerMask.GetMask("Environment"));

        }

        GameObject createdItem = Instantiate(item.Data.prefab, hitInfo.point, Quaternion.identity); // Spawns a prefab of dropped item in the point camera facing.

        createdItem.GetComponent<Loot>().Item.SetCount(item.Quantity);

        _UIManager.DeleteItemIcon(item);

        _UIManager.PrintMessage($"{item.Data.itemTitle} dropped.");

        Player.instance.ChangeEquipload(-item.Data.weight * item.Quantity);

        grid.Remove(item);
    }
    #endregion

    #region EQUIPMENT
    private void TakeFromEquipslot(int index)
    {
        Debug.Log($"{_equipSlots[index].item.Data.itemTitle} was taken from equip slot.");

        _equipSlots[index].item = null;
    }
    /// <summary>
    /// Switch to selection of avaliable equipment slot.
    /// </summary>
    /// <param name="item"></param>
    private void StartEquipSlotSelection(Item item)
    {
        if (item == null) return;

        _itemPendingEquip = item;

        _selectingEquipSlot = true;

        Debug.Log($"Started equipment slot selection for {item.Data.itemTitle}.");
    }
    /// <summary>
    /// Snap to the equipslot by given index.
    /// </summary>
    /// <param name="index"></param>
    private void SwitchToEquipSlot(int index)
    {
        if (index < 0 || index >= _equipSlots.Count) return;

        _currentEquipIndex = index;

        _UIManager.pointer.pivot = new Vector2(0.5f, 0.5f);

        _UIManager.pointer.position = _UIManager.equipSlots[_currentEquipIndex].transform.position;

        _UIManager.pointer.sizeDelta = _UIManager.equipSlots[_currentEquipIndex].GetComponent<RectTransform>().sizeDelta;
    }
    /// <summary>
    /// Try to equip item in selected equipslot.
    /// </summary>
    /// <param name="item"></param>
    public void TryEquip(Item item)
    {
        EquipSlot slot = _equipSlots[_currentEquipIndex];

        if (slot.CanPlaceHere(item) == false)
        {
            Debug.LogWarning($"Wrong equipment type.");

            return;
        }

        if (item.rotated == true)
        {
            item.Rotate();

            _UIManager.UpdateRect(_UIManager.icons[item].GetComponent<RectTransform>(), item.x, item.y, item.Width, item.Height);

            _UIManager.icons[item].UpdateVisuals();
        }

        _UIManager.icons[item].GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        _UIManager.icons[item].GetComponent<RectTransform>().position = _UIManager.equipSlots[_currentEquipIndex].transform.position;

        grid.Remove(item);

        slot.item = item;

        switch (item.Data.itemType)
        {
            case ItemType.Weapon:
                {
                    GameObject weapon = Instantiate(item.Data.prefab);

                    Transform parent = null;
                    RegionType slotType = RegionType.None;

                    // Determine slot and parent.
                    if (slot.slotType == EquipSlotType.WeaponLeft)
                    {
                        parent = Player.instance.holdPointLeft;
                        slotType = RegionType.WeaponLeft;
                    }
                    else if (slot.slotType == EquipSlotType.WeaponRight)
                    {
                        parent = Player.instance.holdPointRight;
                        slotType = RegionType.WeaponRight;
                    }

                    // Attach weapon to hand.
                    weapon.transform.SetParent(parent);
                    weapon.transform.localPosition = Vector3.zero;
                    weapon.transform.localRotation = Quaternion.identity;

                    // Disable all colliders initially.
                    foreach (Collider collider in weapon.GetComponentsInChildren<Collider>())
                        collider.enabled = false;

                    // Setup rigidbody (if needed).
                    var rb = weapon.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = true;
                        rb.useGravity = false;
                        rb.constraints = RigidbodyConstraints.FreezeAll;
                    }

                    // Initialize and register hitboxes.
                    Hitbox[] hitboxes = weapon.GetComponentsInChildren<Hitbox>(true);

                    foreach (var hitbox in hitboxes)
                    {
                        hitbox.Init(Player.instance, slotType);
                    }

                    break;
                }
                /*
                else if (item.Data.itemType == ItemSlots.Torso)
                {
                    Destroy(newObject.GetComponentInChildren<Rigidbody>());

                    foreach (var collider in newObject.GetComponentsInChildren<Collider>()) Destroy(collider);

                    var targetSkinnedMesh = socket.GetComponentInChildren<SkinnedMeshRenderer>();

                    newObject.GetComponentInChildren<SkinnedMeshRenderer>().bones = targetSkinnedMesh.bones;

                    newObject.GetComponentInChildren<SkinnedMeshRenderer>().rootBone = targetSkinnedMesh.rootBone;
                }*/
        }

        _UIManager.PrintMessage($"{item.Data.itemTitle} was equipped in {_UIManager.equipSlots[_currentEquipIndex].transform.name}.");

        _itemPendingEquip = null;
        _selectingEquipSlot = false;
        _currentEquipIndex = 0;
    }

    public EquipSlot GetEquipslotByType(EquipSlotType type)
    {
        return _equipSlots.FirstOrDefault(s => s.slotType == type);
    }
    #endregion
}