using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class GameMaster : MonoBehaviour
{
    public static GameMaster instance;
    public GameObject HUD;
    public SoundManager SM;
    public LayerMask environmentMask;
    public LayerMask actorsMask;
    public LayerMask waterMask;
    public Color fogColor;
    public float fogDensity;
    public bool fogEnabled;
    public FogMode fogMode;
    public const int MaxFallTime = 5;
    public Transform deathScreen;
    public float gravity;
    [SerializeField] private Transform _sun;
    [SerializeField] private float _timeSpeed = .1f;
    [SerializeField] private Gradient _sunColor;
    [SerializeField] private bool _day;
    [SerializeField] private bool _dayCycleActive;

    [Range(0f, 1f)] public float soundOverall = 1f;
    [Range(0f, 1f)] public float musicOverall = 1f;
    [Range(0f, 1f)] public float effectsOverall = 1f;
    [Range(0f, 1f)] public float dialogueOverall = 1f;

    [Header("Controls:")]
    public float mouseSensitivity = 100f;
    public KeyCode mainMenuKey;
    public KeyCode playerMenuKey;
    public KeyCode moveForward;
    public KeyCode moveBackward;
    public KeyCode moveRight;
    public KeyCode moveLeft;
    public KeyCode sprintKey;
    public KeyCode walkKey;
    public KeyCode sneakKey;
    public KeyCode jumpKey;
    public KeyCode dodgeKey;
    public KeyCode kickKey;
    public KeyCode drawWeaponKey;
    public KeyCode interactionKey;
    public KeyCode holdKey;
    public KeyCode throwKey;
    public float interactionDistance = 5f;
    public float keyHoldTime;
    // Makes sure that there's always only one Game Master in the scene.
    #region Singleton
    void Awake()
    {
        if (instance != null) Debug.LogWarning("More than one Game Mannager.");

        instance = this;
    }
    #endregion

    void Start()
    {
        SM = GetComponent<SoundManager>();

        if (SceneManager.GetActiveScene().buildIndex == 0) SM.PlaySound("MainMenuTheme");
        //else SM.PlaySound("Ambient");
    }

    void Update()
    {
        if (_dayCycleActive) DayNightCycle();

        /*
        if (paused == false && Time.timeScale != 1f)
        {
            Time.timeScale = 1f;

            //Time.fixedDeltaTime = Time.timeScale / 0.02f;
        }
        else if (paused == true && Player.instance.GetComponent<PlayerStats>().curHP > 0)
        {
            Time.timeScale = 0.33f;

            //Time.fixedDeltaTime = Time.timeScale * 0.02f;
        }*/

    }

    void DayNightCycle() //  FIXME: change it to coroutine and move to separate class.
    {
        //if (_sun.localRotation.x < 0) _sun.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        
        if (_sun.transform.rotation.x < 1)
        {
            _sun.transform.Rotate(_timeSpeed, 0f, 0f, Space.World);

            //_timeWidget.transform.Rotate(0f, 0f, -_timeSpeed, Space.World);

            //_timeWidget.rotation = new Quaternion(0f, 0f, _sun.transform.rotation.x, 1f);
        }
        else
        {
            _sun.transform.rotation = Quaternion.identity;

            //_timeWidget.transform.rotation = Quaternion.identity;

            //if (_day) _timeWidget.transform.rotation = new Quaternion(0f, 0f, _timeWidget.transform.rotation.z - 180, 1f);

            _day = !_day;
        }

        if (_day)
        {
            _sun.GetComponent<Light>().color = _sunColor.Evaluate(_sun.transform.localRotation.x / 2);

            _sun.GetComponent<Light>().intensity = (1 / _sunColor.Evaluate(_sun.transform.localRotation.x)[0]) - 1;
        }
        else
        {
            _sun.GetComponent<Light>().color = _sunColor.Evaluate(0.51f);

            _sun.GetComponent<Light>().intensity = 0.5f;
        }
    }

    public void NewGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void GameOver()
    {
        deathScreen.gameObject.SetActive(true);

        deathScreen.GetComponent<Animation>().Play("DeathScreen");

        StartCoroutine(GameOverDelay(deathScreen.GetComponent<Animation>().clip.length, "GameOver"));
    }

    IEnumerator GameOverDelay(float delay, string levelTitle)
    {
        yield return new WaitForSeconds(delay);

        ReloadLevel();
    }

    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ToMainMenu(string sceneTitle)
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}