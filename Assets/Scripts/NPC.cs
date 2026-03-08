using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPC : Humanoid, IInteractable
{
    public Transform indicatorsPrefab;
    private UnityEngine.AI.NavMeshAgent _agent;
    public InteractionType interactionType { get; set; }
    [SerializeField] private int _HPBarOffset;
    [SerializeField] private float _sightDistance = 50;
    [SerializeField] private float _aggroDistance = 30f;
    [SerializeField] private float _attackDistance = 2f;
    [SerializeField] private int _rays = 6;
    [SerializeField] private int _angle = 30;
    [SerializeField] private float _deagroDelay = 10f;
    [SerializeField] private float _dmgHideDelay = 2f;
    public float maxAlert = 100;
    public float alertVolume;
    protected float totalDmg;
    [SerializeField] private float _fadeTime;
    private GameObject _target;
    private Vector3 _saveTargetPos;
    private Vector3 _startPos;
    private Vector3 _dir;
    private Quaternion _startRot;
    float attackSpeed = 1f;

    public override void Awake()
    {
        interactionType = InteractionType.Loot;

        base.Awake();
    }

    public override void Start()
    {
        IndicatorsSetup();

        _agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        _startPos = transform.position;

        _startRot = transform.rotation;

        _target = Player.instance.gameObject;
        /*
        for (int i = 0; i <= GetComponent<Container>().inventoryPrefab.GetChild(0).GetComponent<Inventory>().storage.childCount; i++)
        {
            if (GetComponent<Container>().inventoryPrefab.GetChild(0).GetComponent<Inventory>().storage.GetChild(i).GetComponent<Cloth>() != null)
            {
                GetComponent<Container>().inventoryPrefab.GetChild(0).GetComponent<Inventory>().storage.GetChild(i).GetComponent<Item>().Equip(GetComponent<Container>().inventoryPrefab.GetChild(0).GetComponent<Inventory>().storage.GetChild(i).GetComponent<Item>().slotNum);
            }
        }
        */
        base.Start();
    }

    private void Update()
    {
        if (!dead)
        {
            _saveTargetPos = _target.transform.position;

            float distanceToTarget = Vector3.Distance(transform.position, _saveTargetPos);

            Vector3 screenPos = CameraControl.instance.mainCam.WorldToScreenPoint(_agent.transform.position); //Projects actor's position in world coordinates onto it's position on screen

            float angleToTarget = Vector3.Angle(CameraControl.instance.transform.forward, transform.position - _target.transform.position); //Angle between target and this actor, checks if Player faces this actor

            //float dotProduct = Vector3.Dot(transform.forward, _saveTargetPos - transform.position); //Alternate method that calculates cosinus of the angle between target and this actor, if it's below 0 then Player is behind the actor

            if (screenPos.x > 0f && screenPos.x < Screen.width && screenPos.y > 0f && screenPos.y < Screen.height && angleToTarget < 90)
            {
                indicators.GetComponent<Canvas>().enabled = true;

                indicators.transform.position = CameraControl.instance.mainCam.WorldToScreenPoint(new Vector3(transform.position.x, Mathf.Clamp(_HPBarOffset * distanceToTarget, 1, 2), transform.position.z));

                //indicators.transform.position = transform.position + new Vector3(0, _HPBarOffset, 0);

                //indicators.transform.LookAt(GameMaster.instance.player.transform);

                indicators.transform.localScale = new Vector2(Mathf.Clamp(1 / distanceToTarget, 1, 100), Mathf.Clamp(1 / distanceToTarget, 1, 100));
            }
            else indicators.GetComponent<Canvas>().enabled = false;

            if (Time.time >= _dmgHideDelay)
            {
                totalDmg = 0;

                indicators.GetComponent<ActorUI>().damageCount.GetComponent<Text>().text = null;

                _dmgHideDelay = Time.time + _fadeTime;
            }
            
            if (FielfOfView()) //If the actor sees the target or being alerted enough, he draws a weapon
            {
                Alarm(maxAlert);

                if (distanceToTarget > _attackDistance && distanceToTarget <= _aggroDistance)
                {
                    animator.SetInteger("Movement", 3);

                    _agent.SetDestination(_saveTargetPos);
                }
                
                if (distanceToTarget <= _attackDistance && inCombat) //If target in attack range, performs an attack
                {
                    int attackType = Random.Range(1, 5);

                    if (Time.time >= attackDelay)
                    {
                        if (WeaponDrawn) Attack(attackType);

                        attackDelay = Time.time + animator.GetCurrentAnimatorStateInfo(0).length + Random.Range(1f, attackSpeed);
                    }
                }
            }
            else if (alertVolume > 0)
            {
                if (!inCombat) alertVolume -= maxAlert / 10 * Time.deltaTime;
                else alertVolume -= maxAlert / 100 * Time.deltaTime;
                
                if (inCombat && distanceToTarget > _attackDistance && distanceToTarget <= _aggroDistance) //If lose sight of the target, agent walks towards to last place where target was seen
                {
                    _agent.SetDestination(_saveTargetPos);

                    if (distanceToTarget > _aggroDistance && distanceToTarget <= _sightDistance) animator.SetInteger("Movement", 4);

                    if (Vector3.Distance(_agent.transform.position, _saveTargetPos) <= 5f) animator.SetInteger("Movement", 0);
                    else animator.SetInteger("Movement", 3);
                }
            }

            if (distanceToTarget <= _attackDistance && alertVolume >= maxAlert)
            {
                Vector3 direction = (_target.transform.position - transform.position).normalized;

                Quaternion lookRotation = Quaternion.LookRotation(direction);

                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

                animator.SetInteger("Movement", 2);
            }

            if (animator.GetInteger("Movement") == 4) _agent.speed = sprintSpeed;

            if (animator.GetInteger("Movement") == 3) _agent.speed = runSpeed;

            if (animator.GetInteger("Movement") == 2) _agent.speed = walkSpeed;

            if (animator.GetInteger("Movement") == 1) _agent.speed = crouchSpeed;

            if (animator.GetInteger("Movement") == 0) _agent.speed = 0;
        }
    }

    private void IndicatorsSetup()
    {
        indicators = Instantiate(indicatorsPrefab.gameObject, GameMaster.instance.HUD.transform);

        //indicators = Instantiate(indicatorsPrefab.gameObject, transform); // alternative variant if using "world space" render mode of canvas

        //indicators.GetComponent<ActorUI>().lvlTxt.text = lvl.ToString();

        indicators.GetComponent<ActorUI>().nameTxt.text = transform.name;

        indicators.GetComponent<ActorUI>().healthBar.maxValue = maxHP;

        indicators.GetComponent<ActorUI>().healthBar.value = curHP;

        indicators.GetComponent<ActorUI>().curHealthTxt.text = curHP + " / " + maxHP;

        indicators.gameObject.SetActive(false);
    }

    public override void ChangeHealth(float value)
    {
        totalDmg += value;

        //dmgHideDelay = Time.time + fadeTime;

        indicators.GetComponent<ActorUI>().damageCount.GetComponent<Text>().text = totalDmg.ToString();

        base.ChangeHealth(value);
    }

    public void Alarm(float alert)
    {
        if (!dead)
        {
            alertVolume += alert;

            if (alertVolume > 0)
            {
                indicators.GetComponent<ActorUI>().alertIndicator.GetComponent<Image>().enabled = true;

                //indicators.GetComponent<ActorUI>().alertIndicator.GetComponent<Image>().sprite = indicators.GetComponent<ActorUI>().alertIndicatorSprites[(int)(((alertVolume * 100) / maxAlert) / 25)]; //Alternative if we using list of sprites instead of animation

                indicators.GetComponent<ActorUI>().alertIndicator.GetComponent<Animator>().Play("AlertIndicatorAnimation", 0, (alertVolume * 100) / maxAlert / 100);

                indicators.GetComponent<ActorUI>().alertIndicator.GetComponent<Animator>().speed = 0;
            }

            if (alertVolume <= 0)
            {
                indicators.GetComponent<ActorUI>().alertIndicator.GetComponent<Image>().enabled = false;

                alertVolume = 0;

                if (inCombat)
                {
                    indicators.gameObject.SetActive(false);

                    inCombat = !inCombat;
                }

                if (Vector3.Distance(_agent.transform.position, _startPos) <= 1f)
                {
                    animator.SetInteger("Movement", 0);

                    Vector3 direction = (_startPos - transform.position).normalized;

                    Quaternion lookRotation = Quaternion.LookRotation(direction);

                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

                    //agent.transform.rotation = startRot;

                    //agent.transform.position = startPos;
                }
                else
                {
                    animator.SetTrigger("DrawWeapon");

                    _agent.SetDestination(_startPos);

                    animator.SetInteger("Movement", 2);
                }
            }

            if (alertVolume >= maxAlert)
            {
                if (!inCombat)
                {
                    DrawOrSheathWeapon();

                    indicators.gameObject.SetActive(true);

                    inCombat = true;
                }
                if (alertVolume > maxAlert) alertVolume = maxAlert;

            }

            //Mathf.Clamp(alertVolume, 0, maxAlert);
        }
    }

    public override void Land()
    {
        float fallDistance = startFallPosition - transform.position.y;

        float healthDamage = 0;

        //PlaySound("Land");

        if (fallDistance > minFallHeight)
        {
            healthDamage = (int)Mathf.Clamp(maxHP * (fallDistance / maxFallHeight) - DEX, 0, maxHP);

            if (fallDistance >= maxFallHeight) healthDamage = maxHP;

            ChangeHealth((int)(healthDamage));

            UIManager.instance.PrintMessage(transform.name + "  fell from " + fallDistance + " units and took " + healthDamage + " damage");
        }

        if (!_agent.isOnNavMesh)
        {
            UnityEngine.AI.NavMeshHit hit;

            if (UnityEngine.AI.NavMesh.FindClosestEdge(transform.position, out hit, UnityEngine.AI.NavMesh.AllAreas)) ;

            _agent.transform.position = hit.position;

            //agent.enabled = false; //Fixes when actor falls through NavMesh when landing

            //agent.enabled = true;
        }

        falling = false;

        velocity = Vector3.zero;
    }

    private bool FielfOfView()
    {
        bool result = false;

        Vector3 pos = viewPoint.transform.position;

        float dist = _sightDistance;

        for (int k = -_rays; k <= _rays; k++)
        {
            for (int i = -_rays; i <= _rays; i++)
            {
                
                Vector3 dir = transform.TransformDirection(new Vector3(Mathf.Sin(i * (_angle / _rays) * Mathf.Deg2Rad), Mathf.Sin(k * (_angle / _rays) * Mathf.Deg2Rad) , Mathf.Cos(k * (_angle / _rays) * Mathf.Deg2Rad) * Mathf.Cos(i * (_angle / _rays) * Mathf.Deg2Rad)));

                RaycastHit hit = new RaycastHit();

                if (Physics.Raycast(pos, dir, out hit, dist))
                {
                    if (hit.transform == _target.transform)
                    {
                        result = true;

                        Debug.DrawLine(pos, hit.point, Color.green);
                    }
                    Debug.DrawLine(pos, hit.point, Color.green);
                }
                else Debug.DrawRay(pos, dir * dist, Color.red);

            }
        }

        return result;
    }

    public override void GetHit(float amount, DamageType type, Transform part)
    {
        Alarm(maxAlert - alertVolume);

        base.GetHit(amount, type, part);
    }

    public override void Death()
    {
        animator.SetTrigger("Dead");

        dead = true;

        //PlaySound("Death");

        animator.enabled = false;

        indicators.SetActive(false);

        //inventory[0].GetComponent<Item>().clone.transform.SetParent(clone.transform.GetChild(0).transform.GetChild(0));

        //Debug.Log(clone.transform.GetChild(0).gameObject.name);

        //healthBar.SetActive(false);

        //agent.destination = transform.position;

        _agent.speed = 0;

        //Invoke("DisableActor", animator.GetCurrentAnimatorStateInfo(0).length);

        //Invoke("DisableActor", 3f);

        base.Death();

        Sound sound = System.Array.Find(soundManager.sounds, sound => sound.name == "Death");

        Invoke("DisableActor", sound.source.clip.length);
    }

    private void DisableActor()
    {
        animator.enabled = false;

        this.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, _sightDistance);
    }
}
