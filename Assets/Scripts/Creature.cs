using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.Animations.Rigging;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using static UnityEditor.Timeline.Actions.MenuPriority;

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
    public bool dead;
    [field: SerializeField] public GameObject indicators { get; set; }
    [field: SerializeField] public int STR { get; set; }
    [field: SerializeField] public int DEX { get; set; }
    [field: SerializeField] public int INT { get; set; }
    [field: SerializeField] public float maxHP { get; set; }
    [field: SerializeField] public float maxSP { get; set; }
    [field: SerializeField] public float maxMP { get; set; }
    [field: SerializeField] public float curHP { get; set; }
    [field: SerializeField] public float curSP { get; set; }
    [field: SerializeField] public float curMP { get; set; }
    [field: SerializeField] public float healthRegen { get; set; }
    [field: SerializeField] public float staminaRegen { get; set; }
    [field: SerializeField] public float manaRegen { get; set; }
    public float healthRegenDelay;
    [SerializeField] private float _staminaRegenDelay = 1f;
    [SerializeField] private float _staminaRegenTick = .1f;
    private Coroutine _staminaRegenCoroutine;
    public float manaRegenDelay;
    public float carryWeight;
    public float equipLoad;
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
        else Debug.LogWarning("No Animator component found on this creature. Assign it manually than.", this.gameObject);

        if (GetComponentInChildren<Inventory>() != null) inventory = GetComponentInChildren<Inventory>();
        else Debug.LogWarning("No Inventory component found on this creature. Assign it manually than.", this.gameObject);
    }

    public virtual void Start()
    {
        if (GetComponentInChildren<SoundManager>() != null) soundManager = GetComponentInChildren<SoundManager>();
        else Debug.LogWarning("No Sound Manager component found on this creature. Assign it manually than.", this.gameObject);

        curHP = maxHP;

        OnHealthChange?.Invoke(curHP, maxHP);

        curSP = maxSP;

        curMP = maxMP;
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

    public virtual void ChangeHealth(float value)
    {
        if (value != 0)
        {
            if (value > 0 && (curHP + value) > maxHP) value = maxHP - curHP; //When resultant curHP after healing exceeds the maxHP, caps received healing to the difference between curHP and maxHP to avoid overhealing (mb remove this later).

            curHP += value;

            OnHealthChange?.Invoke(curHP, maxHP);

            if (value < 0)
            {
                healthRegenDelay = Time.time + 5f;

                if (Time.time >= healthRegenDelay) RegenHealth();

                if (curHP <= 0)
                {
                    curHP = 0;

                    Death();
                }
            }
        }
    }

    private IEnumerator RegenHealth()
    {
        yield return null;
    }

    public void ChangeStamina(float value)
    {
        if (value < 0)
        {
            if (_staminaRegenCoroutine != null) StopCoroutine(_staminaRegenCoroutine);

            _staminaRegenCoroutine = StartCoroutine(RegenStamina());
        }

        curSP += value;

        curSP = Mathf.Clamp(curSP, 0, maxSP);

        OnStaminaChange?.Invoke(curSP, maxSP);
    }

    private IEnumerator RegenStamina()
    {
        yield return new WaitForSeconds(_staminaRegenDelay); // Wait a delay after last stamina usage.

        while (curSP < maxSP) // While stamina is not full.
        {
            ChangeStamina(staminaRegen);

            yield return new WaitForSeconds(_staminaRegenTick);
        }

        _staminaRegenCoroutine = null; // When stamina is full replenished, forget this coroutine for further reusage.
    }

    public void ChangeMana(float value)
    {
        if (value < 0) curMP += value;
        else curMP += value;

        OnManahange?.Invoke(curMP, maxMP);

        if (curMP < 0) curMP = 0;
    }

    private IEnumerator RegenMana()
    {
        yield return null;
    }

    public void ChangePoise(float value)
    {

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
            
            ChangeHealth(maxHP / 60);
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
            fallDmg = (int)Mathf.Clamp(maxHP * (fallDistance / maxFallHeight) - DEX, 0, maxHP);

            if (fallDistance >= maxFallHeight) fallDmg = maxHP;

            ChangeHealth((int)(fallDmg));

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
        ChangeHealth(amount);

        soundManager.PlaySound("GetHit");
    }

    public virtual void Death()
    {
        soundManager.PlaySound("Death");

        UIManager.instance.PrintMessage(transform.name + " has died.");
    }
}