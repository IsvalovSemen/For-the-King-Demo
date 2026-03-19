using UnityEngine;
using System.Collections.Generic;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    private UIManager _UIManager;

    [Header("Grid setup:")]
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

    private int _pointerXPos;
    private int _pointerYPos;

    private Vector2Int lastMoveDir;
    private Item currentHoveredItem;

    public event Action OnItemPickupConfirmation;

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
    #region POINTER
    public void MovePointer(int dx, int dy)
    {
        Vector2Int dir = new Vector2Int(dx, dy);

        int newX = _pointerXPos + dx;
        int newY = _pointerYPos + dy;

        int maxX = grid.width;
        int maxY = grid.height;

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

                if (grid.CanPlaceHere(_draggedItem, _pointerXPos, _pointerYPos) == false) _UIManager.icons[_draggedItem].IconImg.color = Color.red;
                else _UIManager.icons[_draggedItem].IconImg.color = Color.white;

                _UIManager.icons[_draggedItem].transform.SetAsLastSibling();

                _UIManager.UpdateRect(_UIManager.icons[_draggedItem].GetComponent<RectTransform>(), _pointerXPos, _pointerYPos, _draggedItem.Width, _draggedItem.Height);
            }
            else
            {
                Item itemUnderneath = grid.GetItem(_pointerXPos, _pointerYPos);

                if (itemUnderneath != null)
                {
                    width = itemUnderneath.Width;
                    height = itemUnderneath.Height;

                    horizontalPos = itemUnderneath.x;
                    verticalPos = itemUnderneath.y;
                }
            }

            _UIManager.pointer.pivot = new Vector2(0f, 1f);

            _UIManager.UpdateRect(_UIManager.pointer, horizontalPos, verticalPos, width, height);

            _UIManager.pointer.transform.SetAsLastSibling();
        }
    }

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
            if (_selectingEquipSlot == true)
            {
                GoToPrevEquipSlot(_itemPendingEquip, _currentEquipIndex);
            }
            else MovePointer(-1, 0);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (_selectingEquipSlot == true)
            {
                GoToNextEquipSlot(_itemPendingEquip, _currentEquipIndex + 1);
            }
            else MovePointer(1, 0);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow)) MovePointer(0, -1);
        if (Input.GetKeyDown(KeyCode.DownArrow)) MovePointer(0, 1);

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_selectingEquipSlot == false)
            {
                if (_isDragging == false)
                {
                    BeginDrag(grid.GetItem(_pointerXPos, _pointerYPos));
                }
                else Place();
            }
            else ConfirmEquip(_itemPendingEquip);
        }

        if (Input.GetKeyDown(KeyCode.R) && _isDragging == true)
        {
            _draggedItem.Rotate();

            _UIManager.UpdateRect(_UIManager.icons[_draggedItem].GetComponent<RectTransform>(), _draggedItem.x, _draggedItem.y, _draggedItem.Width, _draggedItem.Height);

            _UIManager.icons[_draggedItem].UpdateVisuals();

            _UIManager.UpdateRect(_UIManager.pointer, _draggedItem.x, _draggedItem.y, _draggedItem.Width, _draggedItem.Height);

            Debug.Log($"{_draggedItem.Data.itemTitle} was rotated.");
        }

        if (Input.GetKeyDown(KeyCode.T) && _isDragging)
        {
            CancelDrag();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Item itemUnderneath = grid.GetItem(_pointerXPos, _pointerYPos);

            if (itemUnderneath != null) DropItem(itemUnderneath, false);
        }

        if (Input.GetKeyDown(KeyCode.Q) && _isDragging == false)
        {
            StartEquipSelection(grid.GetItem(_pointerXPos, _pointerYPos));
        }
    }
    #endregion

    #region ITEM PLACEMENT
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

    private void TakeFromSlot(Item item)
    {
        if (item == null) return;

        _originalItem = item;
        _originalX = item.x;
        _originalY = item.y;

        grid.Remove(item);

        Debug.Log($"{item.Data.itemTitle} was taken from grid.");
    }

    private void BeginDrag(Item item)
    {
        if (item == null) return;

        TakeFromSlot(item);

        _draggedItem = item;
        _isDragging = true;

        Debug.Log($"Now dragging {_draggedItem.Data.itemTitle}.");
    }

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
        }
        else CancelDrag();
    }

    private void Store(Item item)
    {
        Player.instance.ChangeEquipload(item.Data.weight);

        OnItemPickupConfirmation?.Invoke();

        _UIManager.PrintMessage($"{item.Data.itemTitle} was successfully stored in inventory.");
    }

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

            Debug.Log($"Drag of {_draggedItem.Data.itemTitle} was cancelled.");
        }
    }
    #endregion

    #region EQUIPMENT
    private void StartEquipSelection(Item item)
    {
        if (item == null) return;

        GoToNextEquipSlot(item, _currentEquipIndex);
        _itemPendingEquip = item;
        _selectingEquipSlot = true;

        TakeFromSlot(item);

        Debug.Log($"Started equipment slot selection for {item.Data.itemTitle}.");
    }

    private void GoToNextEquipSlot(Item item, int startIndex)
    {
        for (int i = startIndex; i < _equipSlots.Count; i++)
        {
            if (_equipSlots[i].CanPlace(item))
            {
                _currentEquipIndex = i;

                _UIManager.pointer.pivot = new Vector2(0.5f, 0.5f);

                _UIManager.pointer.position = _UIManager.equipSlots[_currentEquipIndex].transform.position;

                _UIManager.pointer.sizeDelta = _UIManager.equipSlots[_currentEquipIndex].GetComponent<RectTransform>().sizeDelta;

                return;
            }
        }
    }

    private void GoToPrevEquipSlot(Item item, int startIndex)
    {
        for (int i = startIndex - 1; i >= 0; i--)
        {
            if (_equipSlots[i].CanPlace(item))
            {
                _UIManager.pointer.pivot = new Vector2(0.5f, 0.5f);

                _UIManager.pointer.position = _UIManager.equipSlots[_currentEquipIndex].transform.position;

                _UIManager.pointer.sizeDelta = _UIManager.equipSlots[_currentEquipIndex].GetComponent<RectTransform>().sizeDelta;

                _currentEquipIndex = i;

                return;
            }
        }
    }

    public void ConfirmEquip(Item item)
    {
        EquipSlot slot = _equipSlots[_currentEquipIndex];

        if (slot.CanPlace(item) == false) return;

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

        _UIManager.PrintMessage($"{item.Data.itemTitle} was equipped in {_UIManager.equipSlots[_currentEquipIndex].transform.name}.");

        _itemPendingEquip = null;
        _selectingEquipSlot = false;
        _currentEquipIndex = 0;
    }
    #endregion

    #region ITEM DROP
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

        createdItem.GetComponent<Loot>().Item.SetCount(item.Count);

        _UIManager.DeleteItemIcon(item);

        _UIManager.PrintMessage($"{item.Data.itemTitle} dropped.");

        Player.instance.ChangeEquipload(-item.Data.weight * item.Count);

        grid.Remove(item);
    }
    #endregion
}