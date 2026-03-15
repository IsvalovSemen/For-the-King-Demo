using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.U2D;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] private Text _notificationsWindow;
    private readonly List<string> _activeMessages = new List<string>();
    [SerializeField] private float _messageDisplayDelay = 5f;
    [SerializeField] private MenuState currentMenu = MenuState.None;
    [SerializeField] private ActorStatus actorStatusPanelPrefab;

    [Header("Cursor:")]
    [SerializeField] private Texture2D _cursorStandart;
    [SerializeField] private Texture2D _cursorDrag;
    [SerializeField] private Texture2D _cursorPick;
    [SerializeField] private Texture2D _cursorFether;
    private int _size = 50;
    private Vector2 _offset;
    [SerializeField] private Transform crosshair;

    [Header("Context prompt:")]
    public GameObject interactionPrompt;
    public Text promptText;
    [SerializeField] private Text promptKey;

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
    [SerializeField] private Slider _playerEquiploadMeter;
    [SerializeField] private TextMeshProUGUI _playerEquiploadRatio;
    [SerializeField] private Slider _playerOxygenBar;
    public Slider throwMeter;
    public Slider oxygenMeter;
    public Image experienceMeter;
    [SerializeField] private EffectIcon _effectIconPrefab;
    [SerializeField] private RectTransform _playerEffectsPanel;

    [Header("Inventory:")]
    [SerializeField] private Transform _inventoryPointer;
    [SerializeField] private GameObject _tooltipWindow;
    [SerializeField] private Image _tooltipIcon;
    [SerializeField] private Text _tooltipTitle;
    [SerializeField] private Text _tooltipWeight;
    [SerializeField] private Text _tooltipPrice;
    [SerializeField] private Slider _durabilityMeter;
    [SerializeField] private Text _durabilityRatio;
    [SerializeField] private Text _tooltipDescription;
    public event Action OnInventoryClosure;

    public enum MenuState
    {
        None,
        MainMenu,
        Inventory,
        Character,
        Journal,
        Map
    }

    #region Singleton
    private void Awake()
    {
        if (instance != null) Debug.LogWarning("More than one GameManager.");

        instance = this;
    }
    #endregion

    private void Start()
    {
        //Cursor.visible = false;

        Player.instance.OnHealthChange += UpdateHealthbar;

        Player.instance.OnStaminaChange += UpdateStaminaBar;

        Player.instance.OnManahange += UpdateManaBar;

        Player.instance.OnEquiploadChange += UpdateEquiploadMeter;

        Player.instance.OnOxygenChange += UpdateOxygenBar;

        CloseAllMenus();

        promptKey.text = GameMaster.instance.interactionKey.ToString();

        HideItemTooltip();
    }

    private void Update()
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
        _playerHealthBar.maxValue = maxHealth;

        _playerHealthBar.value = currentHealth;

        _healthValueRatio.text = currentHealth + " / " + maxHealth;

        _playerHealthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(maxHealth, _playerHealthBar.GetComponent<RectTransform>().sizeDelta.y); // Sets width of the slider to atribute's max value.
    }

    private void UpdateStaminaBar(float currentStamina, float maxStamina)
    {
        _playerStaminaBar.maxValue = maxStamina;

        _playerStaminaBar.value = currentStamina;

        _staminaValueRatio.text = Mathf.Round(currentStamina) + " / " + maxStamina;

        _playerStaminaBar.GetComponent<RectTransform>().sizeDelta = new Vector2(maxStamina, _playerStaminaBar.GetComponent<RectTransform>().sizeDelta.y);
    }

    private void UpdateManaBar(float currentMana, float maxMana)
    {
        _playerManaBar.maxValue = maxMana;

        _playerManaBar.value = currentMana;

        _manaValueRatio.text = currentMana + " / " + maxMana;

        _playerManaBar.GetComponent<RectTransform>().sizeDelta = new Vector2(maxMana, _playerManaBar.GetComponent<RectTransform>().sizeDelta.y);

        //_playerManaBar.GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 100, maxMana); // OUTDATED: Sets the width and offset from the left edge of the parent.
    }

    private void UpdateEquiploadMeter(float currentWeight, float maxWeight, int loadStage)
    {
        _playerEquiploadMeter.maxValue = maxWeight;

        _playerEquiploadMeter.value = currentWeight;

        string stage = loadStage switch
        {
            1 => "Light",
            2 => "Madium",
            3 => "Heavy",
            4 => "Overweight"
        };

        _playerEquiploadRatio.text = $"{currentWeight} / {maxWeight} ({stage} load).";
    }

    private void UpdateOxygenBar(float currentOxygen, float maxOxygen)
    {
        _playerOxygenBar.gameObject.SetActive(true);

        _playerOxygenBar.maxValue = maxOxygen;

        _playerOxygenBar.value = currentOxygen;
    }

    public void UpdateThrowMeter(float curPower, float maxPower)
    {
        if (curPower > 0) throwMeter.gameObject.SetActive(true);
        else throwMeter.gameObject.SetActive(false);

        throwMeter.maxValue = maxPower;

        throwMeter.value = curPower;
    }

    public void AddEffectIcon(Effect effect)
    {
        var effectIcon = Instantiate(_effectIconPrefab, _playerEffectsPanel);

        effectIcon.Init(effect);
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
    /// Close all menus and reset state machine.
    /// </summary>
    public void CloseAllMenus()
    {
        _mainMenu.SetActive(false);
        _inventoryMenu.SetActive(false);
        _mapMenu.SetActive(false);
        _characterSheetMenu.SetActive(false);
        _journalMenu.SetActive(false);
        //_questLogMenu.SetActive(false);
        //_settingsGeneralMenu.SetActive(false);
        //_controlsSettingsMenu.SetActive(false);
        //_graphicsSettingsMenu.SetActive(false);
        //_soundSettingsMenu.SetActive(false);
        //_exitConfirmWindow.SetActive(false);

        currentMenu = MenuState.None;
    }

    /// <returns>True if any menu is currently open.</returns>
    public bool IsAnyMenuOpen => currentMenu != MenuState.None;

    /// <returns>Returns the currently active menu.</returns>
    public MenuState GetCurrentMenu => currentMenu;

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

    public void ShowItemTooltip(IItem item)
    {
        _tooltipWindow.SetActive(true);
        _tooltipIcon.sprite = item.Stats.iconSprite;
        _tooltipTitle.text = item.Stats.itemTitle;
        _tooltipWeight.text = item.Stats.weight.ToString();
        _tooltipPrice.text = item.Stats.price.ToString();
        _durabilityMeter.maxValue = item.Stats.maxDurability;
        _durabilityMeter.value = item.CurrentDurability;
        _durabilityRatio.text = $"{item.CurrentDurability}/{item.Stats.maxDurability}";
        _tooltipDescription.text = item.Stats.description;
    }

    public void HideItemTooltip()
    {
        _tooltipWindow.SetActive(false);
        _tooltipIcon.sprite = null;
        _tooltipTitle.text = string.Empty;
        _tooltipWeight.text = string.Empty;
        _tooltipPrice.text = string.Empty;
        _durabilityMeter.maxValue = 100;
        _durabilityMeter.value = 0;
        _durabilityRatio.text = string.Empty;
        _tooltipDescription.text = string.Empty;
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

    public ActorStatus CreateActorStatusPanel(Creature actor)
    {
        var newPanel = Instantiate(UIManager.instance.actorStatusPanelPrefab, UIManager.instance.transform);

        newPanel.Init(actor);

        return newPanel;
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