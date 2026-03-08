using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using static Unity.Burst.Intrinsics.X86.Avx;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] private Text _notificationsWindow;
    private readonly List<string> _activeMessages = new List<string>();
    [SerializeField] private float _messageDisplayDelay = 5f;
    public GameObject effectIconPrefab;
    [SerializeField] private Transform _inventoryPointer;
    public event Action OnInventoryClosure;
    [Header("Cursor:")]
    [SerializeField] private Texture2D _cursorStandart;
    [SerializeField] private Texture2D _cursorDrag;
    [SerializeField] private Texture2D _cursorPick;
    [SerializeField] private Texture2D _cursorFether;
    private int _size = 50;
    private Vector2 _offset;
    public Transform crosshair;
    [Header("Menus sources:")]
    [SerializeField] private GameObject _inventoryMenu;
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private GameObject _characterSheetMenu;
    [SerializeField] private GameObject _mapMenu;
    [SerializeField] private GameObject _journalMenu;
    [SerializeField] private GameObject _questLogMenu;
    [SerializeField] private GameObject _settingsGeneralMenu;
    [SerializeField] private GameObject _controlsSettingsMenu;
    [SerializeField] private GameObject _graphicsSettingsMenu;
    [SerializeField] private GameObject _soundSettingsMenu;
    [SerializeField] private GameObject _exitConfirmWindow;
    [Header("Controls:")]
    [SerializeField] private KeyCode _mainMenuKey = KeyCode.Escape;
    [SerializeField] private KeyCode _inventoryMenuKey = KeyCode.I;
    [SerializeField] private KeyCode _journalMenuKey = KeyCode.J;
    [SerializeField] private KeyCode _mapMenuKey = KeyCode.M;
    [SerializeField] private KeyCode _characterMenuKey = KeyCode.C;
    [Header("Player status bar:")]
    [SerializeField] private Slider _playerHealthBar;
    [SerializeField] private TextMeshProUGUI _healthValueRatio;
    [SerializeField] private Slider _playerStaminaBar;
    [SerializeField] private TextMeshProUGUI _staminaValueRatio;
    [SerializeField] private Slider _playerManaBar;
    [SerializeField] private TextMeshProUGUI _manaValueRatio;
    [SerializeField] private Slider _playerOxygenBar;

    public enum MenuState
    {
        None,
        MainMenu,
        Inventory,
        Character,
        Journal,
        Map
    }

    [SerializeField] private MenuState currentMenu = MenuState.None;

    #region Singleton
    void Awake()
    {
        if (instance != null) Debug.LogWarning("More than one GameManager.");

        instance = this;
    }
    #endregion

    void Start()
    {
        //Cursor.visible = false;

        CloseAllMenus();

        Player.instance.OnHealthChange += UpdateHealthbar;

        Player.instance.OnStaminaChange += UpdateStaminaBar;

        Player.instance.OnManahange += UpdateManaBar;

        Player.instance.OnOxygenChange += UpdateOxygenBar;
    }

    void Update()
    {
        // Menu switching state machine.
        if (Input.GetKeyDown(_mainMenuKey))
        {
            if (currentMenu != MenuState.MainMenu) OpenMenu(MenuState.MainMenu);
            else CloseMenu(currentMenu);
        }
        else if (Input.GetKeyDown(_inventoryMenuKey))
        {
            if (currentMenu != MenuState.Inventory) OpenMenu(MenuState.Inventory);
            else CloseMenu (currentMenu);
        }
        else if (Input.GetKeyDown(_journalMenuKey))
        {
            if (currentMenu != MenuState.Journal) OpenMenu(MenuState.Journal);
            else CloseMenu(currentMenu);
        }
        else if (Input.GetKeyDown(_mapMenuKey))
        {
            if (currentMenu != MenuState.Map) OpenMenu(MenuState.Map);
            else CloseMenu(currentMenu);
        }
        else if (Input.GetKeyDown(_characterMenuKey))
        {
            if (currentMenu != MenuState.Character) OpenMenu(MenuState.Character);
            else CloseMenu(currentMenu);
        }
    }

    private void UpdateHealthbar(float currentHealth, float maxHealth)
    {
        _playerHealthBar.value = currentHealth;

        _playerHealthBar.maxValue = maxHealth;

        _staminaValueRatio.text = currentHealth + " / " + maxHealth;
    }

    private void UpdateStaminaBar(float currentStamina, float maxStamina)
    {
        _playerStaminaBar.value = currentStamina;

        _playerStaminaBar.maxValue = maxStamina;

        _staminaValueRatio.text = Mathf.Round(currentStamina) + " / " + maxStamina;
    }

    private void UpdateManaBar(float currentMana, float maxMana)
    {
        _playerManaBar.value = currentMana;

        _playerManaBar.maxValue = maxMana;

        _manaValueRatio.text = currentMana + " / " + maxMana;

        _playerManaBar.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 100, maxMana);
    }

    private void UpdateOxygenBar(float currentOxygen, float maxOxygen)
    {
        _playerOxygenBar.gameObject.SetActive(true);

        _playerOxygenBar.value = currentOxygen;

        _playerOxygenBar.maxValue = maxOxygen;
    }

    public void OpenMenu(MenuState menu)
    {
        CloseMenu(currentMenu);

        currentMenu = menu;

        switch (menu)
        {
            case MenuState.MainMenu:
                _mainMenu.SetActive(true);
                break;

            case MenuState.Inventory:
                _inventoryMenu.SetActive(true);
                _cursorStandart = _cursorPick;
                break;

            case MenuState.Character:
                _characterSheetMenu.SetActive(true);
                _questLogMenu.SetActive(false);
                break;

            case MenuState.Journal:
                _journalMenu.SetActive(true);
                _cursorStandart = _cursorFether;
                break;

            case MenuState.Map:
                _mapMenu.SetActive(true);
                _cursorStandart = _cursorFether;
                break;

            case MenuState.None:
            
            default:
                break;
        }
    }

    public void CloseMenu(MenuState menu)
    {
        switch (menu)
        {
            case MenuState.MainMenu:
                _mainMenu.SetActive(false);
                break;

            case MenuState.Inventory:
                _inventoryMenu.SetActive(false);
                OnInventoryClosure?.Invoke();
                break;

            case MenuState.Character:
                _characterSheetMenu.SetActive(false);
                break;

            case MenuState.Journal:
                _journalMenu.SetActive(false);
                break;

            case MenuState.Map:
                _mapMenu.SetActive(false);
                break;

            case MenuState.None:
            
            default:
                break;
        }

        currentMenu = MenuState.None;
    }

    /// <summary>
    /// Closes all menus and resets state machine.
    /// </summary>
    public void CloseAllMenus()
    {
        _mainMenu.SetActive(false);
        _inventoryMenu.SetActive(false);
        _mapMenu.SetActive(false);
        _characterSheetMenu.SetActive(false);
        _journalMenu.SetActive(false);
        _questLogMenu.SetActive(false);
        _settingsGeneralMenu.SetActive(false);
        _controlsSettingsMenu.SetActive(false);
        _graphicsSettingsMenu.SetActive(false);
        _soundSettingsMenu.SetActive(false);
        _exitConfirmWindow.SetActive(false);

        currentMenu = MenuState.None;
    }

    /// <returns>True if any menu is currently open.</returns>
    public bool IsAnyMenuOpen() => currentMenu != MenuState.None;

    /// <returns>Returns the currently active menu.</returns>
    public MenuState GetCurrentMenu() => currentMenu;

    public void PrintMessage(string messageTxt)
    {
        _activeMessages.Add(messageTxt);
        UpdateMessageText();
        StartCoroutine(RemoveMessageAfterDelay(messageTxt, _messageDisplayDelay));
    }

    public void UpdateInventoryPointerLocation(InventoryCell cell)
    {
        _inventoryPointer.transform.position = cell.transform.position;
    }

    private void UpdateMessageText()
    {
        _notificationsWindow.text = string.Join("\n", _activeMessages);
    }

    private IEnumerator RemoveMessageAfterDelay(string message, float delay)
    {
        yield return new WaitForSeconds(delay);

        _activeMessages.Remove(message);
        UpdateMessageText();
    }

    public void SetCursor(bool value)
    {
        _offset = new Vector2(-_size / 2, -_size / 2);

        if (_inventoryMenu.activeSelf == true)
        {
            if (value == true) _cursorStandart = _cursorPick;
            else _cursorStandart = _cursorDrag;
        }
    }

    void OnGUI()
    {
        Vector2 mousePos = Event.current.mousePosition;
        GUI.depth = 999;
        GUI.Label(new Rect(mousePos.x - _size / 2, mousePos.y - _size / 2, _size, _size), _cursorStandart);
    }
}
