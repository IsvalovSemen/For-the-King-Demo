using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.Animations.Rigging;
using static UnityEngine.Rendering.DebugUI;

public abstract class Creature : MonoBehaviour, IEntityStats, IDamageable
{
    [Header("General:")]
    public string name;
    protected CharacterController controller;
    public Animator animator;
    public Inventory inventory;
    public SoundManager soundManager { get; set; }
    protected bool inCombat;
    public MultiRotationConstraint headConstraint;
    public Transform viewPoint;
    [SerializeField] protected Transform GroundCheckRight;
    [SerializeField] protected Transform GroundCheckLeft;

    [Header("Actions:")]
    public bool falling;
    public bool grounded;
    protected bool swimming;
    protected bool diving;
    protected bool suffocate;
    protected Vector3 velocity;
    [SerializeField] protected float jumpHeight = 10f;
    protected float jumpDistance = 10f;
    [SerializeField] protected float groundCheckDistance = 0.5f;
    protected float startFallPosition;
    public int maxFallHeight;
    public int minFallHeight;
    public float walkSpeed = 1f;
    public float runSpeed = 3f;
    public float sprintSpeed = 5f;
    public float crouchSpeed = 0.5f;
    public float strafeSpeed = 5f;
    public float swimSpeed = 0.5f;
    public float rotationSpeed = 5f;
    public int jumpStamina;
    public float sprintStamina;
    public int dodgeStamina;
    protected float dodgeDelay;
    public float dodgeDistance = 5f;
    protected float ThrowPower;
    [SerializeField] protected float MaxThrowPower = 30f;
    protected Collider[] hitColliders;
    protected bool _recoil;

    [Header("Stats:")]
    public bool isDead;
    [field: SerializeField] public GameObject indicators { get; set; }
    [field: SerializeField] public int strength { get; set; }
    [field: SerializeField] public int dexterity { get; set; }
    [field: SerializeField] public int intelligence { get; set; }
    [field: SerializeField] public float maxHealth { get; set; }
    [field: SerializeField] public float maxStamina { get; set; }
    [field: SerializeField] public float maxMana { get; set; }
    [field: SerializeField] public float currentHealth { get; set; }
    [field: SerializeField] public float currentStamina { get; set; }
    [field: SerializeField] public float currentMana { get; set; }
    [field: SerializeField] public float healthRecovery { get; set; }
    [field: SerializeField] public float staminaRecovery { get; set; }
    [field: SerializeField] public float manaRecovery { get; set; }
    public float healthRegenDelay;
    [SerializeField] private float _staminaRecoveryDelay = 1f;
    [SerializeField] private float _staminaRecoveryTick = .1f;
    private Coroutine _staminaRegenCoroutine;
    public float manaRegenDelay;
    public float maxEquipload;
    public float currentEquipLoad;
    [field: SerializeField] public int loadStage { get; set; }
    [field: SerializeField] public float maxOxygen { get; set; }
    public float curOxygen { get; set; }
    public List<Effect> activeEffects;

    public event Action<float, float> OnHealthChange;
    public event Action<float, float> OnStaminaChange;
    public event Action<float, float> OnManahange;
    public event Action<float, float> OnOxygenChange;

    public virtual void Awake()
    {
        if (GetComponentInChildren<Animator>() != null) animator = GetComponentInChildren<Animator>();
        else Debug.LogWarning("No Animator component found on this creature.", this.gameObject);

        if (GetComponentInChildren<Inventory>() != null) inventory = GetComponentInChildren<Inventory>();
        else Debug.LogWarning("No Inventory component found on this creature.", this.gameObject);
    }

    public virtual void Start()
    {
        if (GetComponentInChildren<SoundManager>() != null) soundManager = GetComponentInChildren<SoundManager>();
        else Debug.LogWarning("No Sound Manager component found on this creature.", this.gameObject);

        SetMaxHealth(maxHealth);

        ChangeCurrentHealth(maxHealth);

        SetMaxStamina(maxStamina);

        ChangeCurrentStamina(maxStamina);

        SetMaxMana(maxMana);

        ChangeCurrentMana(maxMana);

        SetMaxEquipload(maxEquipload);

        ChangeEquipload(currentEquipLoad);

        if (inventory != null) inventory.OnEquiploadChange += ChangeEquipload;

        loadStage = Mathf.Clamp(CalculateLoadStage(currentEquipLoad, maxEquipload), 1, 4);
    }

    public virtual void Update()
    {
        grounded = Physics.CheckSphere(GroundCheckRight.position, groundCheckDistance, GameMaster.instance.environmentMask) | Physics.CheckSphere(GroundCheckLeft.transform.position, groundCheckDistance, GameMaster.instance.environmentMask);

        velocity.y += -GameMaster.instance.gravity * Time.deltaTime; //Acceleration of gravity

        if (velocity.y < -3)
        {
            if (grounded)
            {
                if (falling)
                {
                    Land();
                }
                else velocity.y = -1; //Resets free fall acceleration after hitting the ground/other objects
            }
            else
            {
                if (!falling)
                {
                    falling = true;

                    startFallPosition = transform.position.y;
                }
            }
        }

        Movement();

        Swimming();

        Jump();

        Dodge();
    }

    private void SetMaxHealth(float value)
    {
        maxHealth = value;
    }

    public virtual void ChangeCurrentHealth(float value)
    {
        if (value != 0)
        {
            if (value > 0 && (currentHealth + value) > maxHealth) value = maxHealth - currentHealth; //When resultant curHP after healing exceeds the maxHP, caps received healing to the difference between curHP and maxHP to avoid overhealing (mb remove this later).

            currentHealth += value;

            OnHealthChange?.Invoke(currentHealth, maxHealth);

            if (value < 0)
            {
                healthRegenDelay = Time.time + 5f;

                if (Time.time >= healthRegenDelay) RestoreHealth();

                if (currentHealth <= 0)
                {
                    currentHealth = 0;

                    Death();
                }
            }
        }
    }

    private IEnumerator RestoreHealth()
    {
        yield return null;
    }

    private void SetMaxStamina(float value)
    {
        maxStamina = value;
    }

    public void ChangeCurrentStamina(float value)
    {
        if (value < 0)
        {
            if (_staminaRegenCoroutine != null) StopCoroutine(_staminaRegenCoroutine);

            _staminaRegenCoroutine = StartCoroutine(RecoverStamina());
        }

        currentStamina += value;

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        OnStaminaChange?.Invoke(currentStamina, maxStamina);
    }

    private IEnumerator RecoverStamina()
    {
        yield return new WaitForSeconds(_staminaRecoveryDelay); // Wait a delay after last stamina usage.

        while (currentStamina < maxStamina) // While stamina is not full.
        {
            ChangeCurrentStamina(staminaRecovery);

            yield return new WaitForSeconds(_staminaRecoveryTick);
        }

        _staminaRegenCoroutine = null; // When stamina is full replenished, forget this coroutine for further reusage.
    }

    private void SetMaxMana(float value)
    {
        maxMana = value;
    }

    public void ChangeCurrentMana(float value)
    {
        if (value < 0) currentMana += value;
        else currentMana += value;

        OnManahange?.Invoke(currentMana, maxMana);

        if (currentMana < 0) currentMana = 0;
    }

    private IEnumerator RegenMana()
    {
        yield return null;
    }

    public void ChangePoise(float value)
    {

    }

    private void SetMaxEquipload(float value)
    {
        maxEquipload = value;
    }

    public virtual void ChangeEquipload(float value)
    {

    }

    protected int CalculateLoadStage(float equipLoad, float carryWeight)
    {
        if (carryWeight <= 0) throw new ArgumentOutOfRangeException(nameof(carryWeight));

        float ratio = equipLoad / carryWeight;

        switch (ratio)
        {
            case < 0.33f:
                return 1;

            case >= 0.33f and < 0.66f:
                return 2;

            case >= 0.66f and < 1f:
                return 3;

            case >= 1f:
                return 4;

            default: throw new InvalidOperationException("Unexpected load stage.");
        }
    }


    public virtual void AddStatusEffect(Sprite icon, Effect effect)
    {
        activeEffects.Add(effect);
    }

    public virtual void RemoveStatusEffect(Effect effect)
    {
        activeEffects.Remove(effect);
    }

    public void HitBodyPart(int part)
    {
        //if (curHP <= 0 && !_isPlayerChar) parts[part].GetComponent<BodyPart>().Dismember();
    }
    
    public virtual void Movement()
    {

    }

    public void Swimming()
    {
        //if (parts[5].transform.position.y <= _waterLine) _swimming = true;

        //if (parts[5].transform.position.y > _waterLine) _swimming = false;

        //if (_swimming && _cam.transform.position.y < _waterLine) _diving = true;

        if (swimming)
        {
            //if (_diving && _cam.transform.position.y >= _waterLine) _diving = false;

            if (falling && !grounded ) swimming = true;

            if (!grounded && !diving && Input.GetKeyDown(GameMaster.instance.jumpKey)) diving = true;

            falling = false;

            grounded = false;

            //velocity.y -= master.gravity * Time.deltaTime;

            velocity.y = 0;

            if (Input.GetKey(GameMaster.instance.moveForward))
            {
                animator.SetInteger("Movement", 6);

                controller.Move(transform.forward * Input.GetAxis("Vertical") * (swimSpeed / loadStage) * Time.deltaTime);
            }

            if (Input.GetKey(GameMaster.instance.moveBackward))
            {
                animator.SetInteger("Movement", 6);

                controller.Move(-transform.forward * -Input.GetAxis("Vertical") * (swimSpeed / loadStage) * Time.deltaTime);

            }
            else animator.SetInteger("Movement", 0);
        }

        if (diving)
        {
            velocity.y = -1f;

            falling = false;

            grounded = false;

            curOxygen -= 10 * Time.deltaTime;

            OnOxygenChange?.Invoke(curOxygen, maxOxygen); // TODO: add coroutine to disable oxygen meter in UI after it's regenerates to full.

            if (Input.GetKey(GameMaster.instance.moveForward))
            {
                animator.SetInteger("Movement", 5);

                controller.Move(CameraControl.instance.transform.forward * Input.GetAxis("Vertical") * (swimSpeed / loadStage) * Time.deltaTime);
            }

            if (Input.GetKey(GameMaster.instance.moveBackward))
            {
                animator.SetInteger("Movement", 5);

                controller.Move(-CameraControl.instance.transform.forward * -Input.GetAxis("Vertical") * (swimSpeed / loadStage) * Time.deltaTime);
            }
            else animator.SetInteger("Movement", 0);

            if (!suffocate && curOxygen <= 0) StartCoroutine(Suffocate());
        }
    }

    public IEnumerator Suffocate()
    {
        suffocate = true;

        while (diving)
        {
            yield return new WaitForSeconds(1);
            
            ChangeCurrentHealth(maxHealth / 60);
        }
    }

    public virtual void Jump()
    {

    }

    public virtual void Dodge()
    {

    }

    public virtual void Land()
    {
        float fallDistance = startFallPosition - transform.position.y;

        float fallDmg = 0;

        //PlaySound("Land");

        if (fallDistance > minFallHeight)
        {
            fallDmg = (int)Mathf.Clamp(maxHealth * (fallDistance / maxFallHeight) - dexterity, 0, maxHealth);

            if (fallDistance >= maxFallHeight) fallDmg = maxHealth;

            ChangeCurrentHealth((int)(fallDmg));

            UIManager.instance.PrintMessage(transform.name + " fell from " + fallDistance + " units and took " + fallDmg + " damage");
        }

        falling = false;

        velocity = Vector3.zero;
    }

    public virtual void Footsteps(AnimationEvent animationEvent)
    {
        soundManager.PlaySound("Footsteps");
    }

    public virtual void Attack(int attackType)
    {
        animator.SetTrigger("Attack");

        animator.SetFloat("Speed", 1);

        animator.SetInteger("AttackType", attackType);

        animator.SetTrigger("Stop");
    }

    public void Knockback(Transform weapon)
    {
        animator.SetFloat("Speed", -1f);

        foreach (Collider coll in weapon.transform.GetComponentsInChildren<Collider>()) coll.enabled = false;

        weapon.transform.GetComponentInChildren<Item>().hittedTargets.Clear();

        _recoil = true;
    }

    private void OnTriggerExit(Collider col)
    {
        //if (col.gameObject.layer == 4 && parts[0].transform.position.y < col.transform.position.y) _diving = true;
        //if (col.gameObject.layer == 4 && parts[0].transform.position.y >= col.transform.position.y) _diving = false;

        if (col.gameObject.layer == 4)
        {
            swimming = false;
        }
    }

    public virtual void GetHit(float amount, DamageType type, Transform part)
    {
        ChangeCurrentHealth(amount);

        soundManager.PlaySound("GetHit");
    }

    public virtual void Death()
    {
        soundManager.PlaySound("Death");

        UIManager.instance.PrintMessage(transform.name + " has died.");
    }
}