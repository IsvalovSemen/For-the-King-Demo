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

    [Header("Movement:")]
    [SerializeField] protected Transform GroundCheckRight;
    [SerializeField] protected Transform GroundCheckLeft;
    public bool falling;
    public bool wasFalling;
    public bool grounded;
    [SerializeField] private int _fallingTimer;
    protected bool swimming;
    protected bool diving;
    protected bool suffocate;
    protected Vector3 velocity;
    [SerializeField] protected float jumpHeight = 10f;
    [SerializeField] protected float jumpDistance = 10f;
    [SerializeField] protected float groundCheckDistance = 0.5f;
    protected float startFallPosition;
    public int maxSafeFallHeight;
    public int minSafeFallHeight;
    public float walkSpeed = 1f;
    public float runSpeed = 3f;
    public float sprintSpeed = 5f;
    public float crouchSpeed = 0.5f;
    public float strafeSpeed = 5f;
    public float swimSpeed = 0.5f;
    public float rotationSpeed = 5f;
    protected float dodgeDelay;
    public float dodgeDistance = 5f;
    protected float ThrowPower;
    [SerializeField] protected float MaxThrowPower = 30f;
    protected Collider[] hitColliders;
    protected bool _recoil;
    [SerializeField] protected int _jumpStaminaCost;
    [SerializeField] protected float _sprintStaminaCost;
    [SerializeField] protected int _dodgeStaminaCost;

    [Header("Stats:")]
    public bool isDead;
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
    private Coroutine _infiniteFallCoroutine;

    public event Action<float, float> OnHealthChange;
    public event Action<float, float> OnStaminaChange;
    public event Action<float, float> OnManahange;
    public event Action<float, float> OnOxygenChange;
    public event Action<float> OnRecieveDamage;
    public event Action<Creature> OnDeath;

    protected virtual void Awake()
    {
        if (GetComponentInChildren<Animator>() != null) animator = GetComponentInChildren<Animator>();
        else Debug.LogWarning("No Animator component found on this creature.", this.gameObject);

        if (GetComponentInChildren<Inventory>() != null) inventory = GetComponentInChildren<Inventory>();
        else Debug.LogWarning("No Inventory component found on this creature.", this.gameObject);
    }

    protected virtual void Start()
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
    }

    protected virtual void Update()
    {


        Movement();

        Swimming();

        Jump();

        Dodge();
    }

    public virtual void Land()
    {
        
        float fallDistance = startFallPosition - transform.position.y;

        //PlaySound("Land");

        if (fallDistance > minSafeFallHeight)
        {
            //float fallDmg = Mathf.Clamp(maxHealth * (fallDistance / maxFallHeight), 0, maxHealth);

            float x = Mathf.InverseLerp(minSafeFallHeight, maxSafeFallHeight, fallDistance); // If value <= a  returns 0 | if value >= b returns 1 | if value between a and b returns 0..1.

            float fallDmg = x * maxHealth;

            UIManager.instance.PrintMessage($"{transform.name} fell from {fallDistance} units and took {fallDmg} damage.");

            ChangeCurrentHealth(-fallDmg);
        }

        velocity = Vector3.zero;
    }

    IEnumerator InfiniteFallCountdown()
    {
        while (falling == true)
        {
            yield return new WaitForSeconds(1);

            _fallingTimer++;

            if (_fallingTimer > GameMaster.MaxFallTime)
            {
                Kill();

                Debug.Log($"{this.gameObject} was killed via infinite fall.");
            }
        }

        _fallingTimer = 0;
    }

    private void SetMaxHealth(float value)
    {
        maxHealth = value;
    }

    public virtual void ChangeCurrentHealth(float value)
    {
        if (value != 0)
        {
            if (value > 0 && (currentHealth + value) > maxHealth)
            {
                value = maxHealth - currentHealth; //When resultant curHP after healing exceeds the maxHP, caps received healing to avoid overhealing.
            }

            if (value < 0)
            {
                healthRegenDelay = Time.time + 5f;

                if (Time.time >= healthRegenDelay) RestoreHealth();

                OnRecieveDamage?.Invoke(Mathf.Abs(value));
            }

            currentHealth += value;

            OnHealthChange?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                currentHealth = 0;

                Kill();
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
    
    protected virtual void Movement()
    {
        wasFalling = falling;

        grounded = Physics.CheckSphere(GroundCheckRight.position, groundCheckDistance, GameMaster.instance.environmentMask) | Physics.CheckSphere(GroundCheckLeft.transform.position, groundCheckDistance, GameMaster.instance.environmentMask);

        velocity.y += -GameMaster.instance.gravity * Time.deltaTime; //Acceleration of gravity.

        if (velocity.y < 0)
        {
            if (grounded)
            {
                if (falling == true)
                {
                    falling = false;

                    Land();
                }
                else velocity.y = -1; //Resets free fall acceleration after hitting the ground/other objects
            }
            else
            {
                if (falling == false)
                {
                    falling = true;

                    startFallPosition = transform.position.y;

                    if (_infiniteFallCoroutine == null) _infiniteFallCoroutine = StartCoroutine(InfiniteFallCountdown());
                    else StopCoroutine(_infiniteFallCoroutine);
                }
            }
        }
    }

    protected virtual void Swimming()
    {
        //if (parts[5].transform.position.y <= _waterLine) _swimming = true;

        //if (parts[5].transform.position.y > _waterLine) _swimming = false;

        //if (_swimming && _cam.transform.position.y < _waterLine) _diving = true;

        if (swimming)
        {
            //if (_diving && _cam.transform.position.y >= _waterLine) _diving = false;

            if (falling && !grounded) swimming = true;

            falling = false;

            grounded = false;

            //velocity.y -= master.gravity * Time.deltaTime;

            velocity.y = 0;
        }

        if (diving)
        {
            velocity.y = -1f;

            falling = false;

            grounded = false;

            curOxygen -= (int)(10 * Time.deltaTime);

            OnOxygenChange?.Invoke(curOxygen, maxOxygen); // TODO: add coroutine to disable oxygen meter in UI after it's regenerates to full.

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

    protected virtual void EnableHitbox(AnimationEvent animationEvent)
    {

    }

    protected virtual void DisableHitbox(AnimationEvent animationEvent)
    {

    }

    public void Knockback(Weapon weapon)
    {
        animator.SetFloat("Speed", -1f);

        weapon.GetComponentInChildren<Hitbox>().Disable();

        weapon.GetComponentInChildren<Hitbox>().hittedTargets.Clear();

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
        ChangeCurrentHealth(-amount);

        soundManager.PlaySound("GetHit");
    }

    public virtual void Kill()
    {
        soundManager.PlaySound("Death");

        OnDeath?.Invoke(this);

        UIManager.instance.PrintMessage($"{this.gameObject.name} died.");
    }
}