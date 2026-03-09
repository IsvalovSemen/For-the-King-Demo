using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : Humanoid
{
    public static Player instance;
    [field: SerializeField] public int lvl { get; set; }
    [field: SerializeField] public int exp { get; set; }
    [field: SerializeField] public int nextLvlExp { get; set; }
    [field: SerializeField] public int skillpointsAvailable { get; set; }

    public event Action<float, float, int> OnEquiploadChange;

    #region Singleton
    public Player()
    {
        // If there's more than one Player ion the scene.
        if (instance != null && instance != this) throw new Exception($"Multiple Player instances detected! Existing: {instance.name}, New: {name}");

        instance = this;

        DontDestroyOnLoad(gameObject); // Do not destroy when transitioning between scenes.
    }
    #endregion

    public override void Start()
    {
        //InventoryManager.instance.OnPickUpConfirmation += ChangeEquipload;

        controller = GetComponent<CharacterController>();

        indicators.GetComponent<ActorUI>().strTxt.text = strength.ToString();

        indicators.GetComponent<ActorUI>().dexTxt.text = dexterity.ToString();

        indicators.GetComponent<ActorUI>().intTxt.text = intelligence.ToString();

        indicators.GetComponent<ActorUI>().nameTxt.text = transform.name;

        indicators.GetComponent<ActorUI>().lvlTxt.text = lvl.ToString();

        indicators.GetComponent<ActorUI>().experienceBar.fillAmount = (float)exp / (float)nextLvlExp;

        indicators.GetComponent<ActorUI>().expTxt.text = exp.ToString() + " / " + nextLvlExp.ToString();

        curOxygen = maxOxygen;

        indicators.GetComponent<ActorUI>().oxygenBar.maxValue = maxOxygen;

        indicators.GetComponent<ActorUI>().oxygenBar.value = maxOxygen;

        base.Start();
    }

    public override void Update()
    {
        if (WeaponDrawn) Attack(0);

        if (Input.GetKeyUp(GameMaster.instance.drawWeaponKey)) DrawOrSheathWeapon();

        RotateProjectile();

        Interact();

        if (attackDelay > 0f) attackDelay -= Time.deltaTime;

        base.Update();
    }

    public override void ChangeEquipload(float value)
    {
        currentEquipLoad += value;

        loadStage = CalculateLoadStage(currentEquipLoad, maxEquipload);

        OnEquiploadChange?.Invoke(currentEquipLoad, maxEquipload, loadStage);
    }

    private void ChangeExperience(int expGain)
    {
        exp += expGain;

        indicators.GetComponent<ActorUI>().experienceBar.fillAmount = (float)exp / (float)nextLvlExp;

        if (exp >= nextLvlExp) LevelUp();
    }

    private void LevelUp()
    {
        exp = 0;

        nextLvlExp += nextLvlExp * lvl;

        lvl++;
    }

    public override void Movement()
    {
        if (!UIManager.instance.IsAnyMenuOpen() && grounded)
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                transform.rotation = Quaternion.Euler(0, CameraControl.instance.transform.rotation.eulerAngles.y, 0);

                if (Input.GetKey(GameMaster.instance.sprintKey) && Input.GetKey(GameMaster.instance.moveForward) && currentStamina >= sprintStamina)
                {
                    controller.Move(CameraControl.instance.transform.forward * Input.GetAxis("Vertical") * (sprintSpeed / loadStage) * Time.deltaTime + CameraControl.instance.transform.right * Input.GetAxis("Horizontal") * (sprintSpeed / loadStage) * Time.deltaTime);

                    animator.SetInteger("Movement", 4);

                    ChangeCurrentStamina(-sprintSpeed * loadStage * Time.deltaTime);
                }
                else if (Input.GetKey(GameMaster.instance.walkKey))
                {
                    controller.Move(CameraControl.instance.transform.forward * Input.GetAxis("Vertical") * (walkSpeed / loadStage) * Time.deltaTime + CameraControl.instance.transform.right * Input.GetAxis("Horizontal") * (walkSpeed / loadStage) * Time.deltaTime);

                    animator.SetInteger("Movement", 2);
                }
                else if (Input.GetKey(GameMaster.instance.sneakKey))
                {
                    controller.Move(CameraControl.instance.transform.forward * Input.GetAxis("Vertical") * (crouchSpeed / loadStage) * Time.deltaTime + CameraControl.instance.transform.right * Input.GetAxis("Horizontal") * (crouchSpeed / loadStage) * Time.deltaTime);

                    animator.SetInteger("Movement", 1);
                }
                else
                {
                    controller.Move(CameraControl.instance.transform.forward * Input.GetAxis("Vertical") * (runSpeed / loadStage) * Time.deltaTime + CameraControl.instance.transform.right * Input.GetAxis("Horizontal") * (runSpeed / loadStage) * Time.deltaTime);

                    animator.SetInteger("Movement", 3);
                }


                /*var camRotation = CameraControl.instance.transform.rotation;

                camRotation.x = 0;

                camRotation.z = 0;

                transform.rotation = Quaternion.Slerp(transform.rotation, camRotation, rotationSpeed * Time.deltaTime);

                //transform.Rotate(0, CameraControl.instance.transform.rotation.eulerAngles.y * Time.deltaTime, 0, Space.World);

                //transform.Rotate(transform.rotation.x, Quaternion.Slerp(transform.rotation,CameraControl.instance.transform.rotation, rotationSpeed * Time.deltaTime), transform.rotation.z, Space.Self);*/
            }
            else animator.SetInteger("Movement", 0);
        }
    }

    public override void AddStatusEffect(Sprite sprite, Effect effect)
    {
        var effectIcon = Instantiate(UIManager.instance.effectIconPrefab, indicators.GetComponent<ActorUI>().statusEffectsPanel);

        base.AddStatusEffect(sprite, effect);

        activeEffects[activeEffects.Count - 1].effectIcon = effectIcon;

        effectIcon.GetComponent<EffectIcon>().icon.sprite = sprite;
    }

    public override void Attack(int attackType)
    {
        float staminaUsage = 0;

        string rightWeaponID = "jopa"; //  slots.Find(x => x.inventorySlot == EquipmentSlotType.HandLeft).storedItemID;

        string leftWeaponID = "govno"; // slots.Find(x => x.inventorySlot == EquipmentSlotType.HandRight).storedItemID;

        if (attackAllowed)
        {
            float mouseX = Input.GetAxis("Mouse X");

            float mouseY = Input.GetAxis("Mouse Y");

            if (Input.GetMouseButton(0) && rightWeaponID != null)
            {
                staminaUsage = InventoryManager.instance.GetItemByID(rightWeaponID).weight * 10;

                //if (mouseX == 0 && mouseY == 0) return; // Preventing the attack if there's no mouse movement at all.

                if (currentStamina >= staminaUsage)
                {
                    /*
                    if (Mathf.Abs(mouseX) > Mathf.Abs(mouseY)) attackType = mouseX > 0 ? 1 : 2;
                    else attackType = mouseY > 0 ? 3 : 4;
                    
                    Debug.Log(attackType);
                    */
                    ChangeCurrentStamina(-staminaUsage);

                    if (InventoryManager.instance.GetItemByID(rightWeaponID).wpnType == WeaponType.Striking1H) animator.SetTrigger("AttackStrikeR");
                    else if (InventoryManager.instance.GetItemByID(rightWeaponID).wpnType == WeaponType.Thrusting1H) animator.SetTrigger("AttackThrustR");

                    //animator.SetFloat("Speed", 0);
                }
            }
            else if (Input.GetMouseButton(1) && leftWeaponID != null)
            {
                staminaUsage = InventoryManager.instance.GetItemByID(leftWeaponID).weight * 10;

                //if (mouseX == 0 && mouseY == 0) return;

                if (currentStamina >= staminaUsage)
                {
                    /*
                    if (Mathf.Abs(mouseX) > Mathf.Abs(mouseY)) attackType = mouseX > 0 ? 5 : 6;
                    else attackType = mouseY > 0 ? 7 : 8;

                    Debug.Log(attackType);
                    */
                    ChangeCurrentStamina(-staminaUsage);

                    if (InventoryManager.instance.GetItemByID(leftWeaponID).wpnType == WeaponType.Striking1H) animator.SetTrigger("AttackStrikeL");
                    else if (InventoryManager.instance.GetItemByID(leftWeaponID).wpnType == WeaponType.Thrusting1H) animator.SetTrigger("AttackThrustL");

                    //animator.SetFloat("Speed", 0);
                }
            }
        }

        base.Attack(attackType);
    }

    public override void Footsteps(AnimationEvent animationEvent)
    {
        if (grounded)
        {
            Collider[] surface;

            surface = Physics.OverlapSphere((GroundCheckRight.transform.position + GroundCheckLeft.transform.position) / 2, groundCheckDistance, GameMaster.instance.environmentMask);

            if (surface != null)
            {
                switch (surface[0].tag)
                {
                    case "Stone":
                        {
                            GameMaster.instance.SM.PlaySound("FootstepsStone");
                        }
                        break;

                    case "Wood":
                        {
                            GameMaster.instance.SM.PlaySound("FootstepsWood");
                        }
                        break;
                }
            }

            Noise((GroundCheckRight.transform.position + GroundCheckLeft.transform.position) / 2, 50 * animationEvent.intParameter * loadStage);
        }

        base.Footsteps(animationEvent);
    }

    public override void Jump()
    {
        controller.Move(velocity * Time.deltaTime);

        if (grounded & Input.GetKeyDown(GameMaster.instance.jumpKey) & currentStamina >= jumpStamina + (jumpStamina * (loadStage / 4)))
        {
            if (currentStamina >= dodgeStamina + (dodgeStamina * (loadStage / 4)) & Time.time >= dodgeDelay)
            {
                if (Input.GetKey(GameMaster.instance.moveForward))
                {
                    velocity = transform.forward * jumpDistance + new Vector3(0, jumpHeight / 2, 0);

                    if (animator.GetInteger("Movement") == 4) velocity = transform.forward * jumpDistance + new Vector3(0, jumpHeight / 2, 0);

                    animator.SetTrigger("Jump");

                    animator.SetTrigger("Ahead");

                    dodgeDelay = Time.time + animator.GetCurrentAnimatorStateInfo(0).length;

                    ChangeCurrentStamina(-jumpStamina * loadStage);
                }
                else
                {
                    animator.SetTrigger("Jump");

                    animator.SetTrigger("Up");

                    dodgeDelay = Time.time + animator.GetCurrentAnimatorStateInfo(0).length;

                    velocity.y = Mathf.Sqrt(jumpHeight * 1f * GameMaster.instance.gravity);

                    ChangeCurrentStamina(-jumpStamina * loadStage);
                }
            }
        }
    }

    public override void Dodge()
    {
        if (grounded & Input.GetKeyDown(GameMaster.instance.dodgeKey) & currentStamina >= dodgeStamina + (dodgeStamina * (loadStage / 4)))

        if (Input.GetKey(GameMaster.instance.moveLeft))
        {
            animator.SetTrigger("Dodge");

            animator.SetTrigger("Left");

            StartCoroutine(Strafe(-1));

            Noise((GroundCheckRight.transform.position + GroundCheckLeft.transform.position) / 2, 100 * loadStage);
        }
        else if (Input.GetKey(GameMaster.instance.moveRight))
        {
            animator.SetTrigger("Dodge");

            animator.SetTrigger("Right");

            StartCoroutine(Strafe(1));

            Noise((GroundCheckRight.transform.position + GroundCheckLeft.transform.position) / 2, 100 * loadStage);
        }
        else if (Input.GetKey(GameMaster.instance.moveBackward))
        {
            animator.SetTrigger("Dodge");

            animator.SetTrigger("Back");

            StartCoroutine(Strafe(0));

            Noise((GroundCheckRight.transform.position + GroundCheckLeft.transform.position) / 2, 100 * loadStage);
        }
    }

    private IEnumerator Strafe(int strafeType)
    {
        dodgeDelay = Time.time + animator.GetCurrentAnimatorStateInfo(0).length;

        ChangeCurrentStamina(-dodgeStamina * loadStage);

        float startTime = Time.time;

        while (Time.time < startTime + animator.GetCurrentAnimatorStateInfo(0).length)
        {
            switch (strafeType)
            {
                case -1:
                    controller.Move(transform.right * -dodgeDistance * Time.deltaTime);
                    break;

                case 0:
                    controller.Move(transform.forward * -dodgeDistance * Time.deltaTime);
                    break;

                case 1:
                    controller.Move(transform.right * dodgeDistance * Time.deltaTime);
                    break;
            }

            yield return null;
        }
    }

    public override void Land()
    {
        Noise((GroundCheckRight.transform.position + GroundCheckLeft.transform.position) / 2, -velocity.y * loadStage * 2);

        base.Land();
    }

    public void Noise(Vector3 pos, float volume)
    {
        hitColliders = Physics.OverlapSphere(pos, volume / 10, GameMaster.instance.actorsMask);
        // FIXME: make sound wave contact with head body part of actor
        foreach (Collider coll in hitColliders) if (coll.transform.root.GetComponent<Creature>() != null && coll.transform.root.GetComponent<Creature>().isDead == false && coll.transform == coll.transform.root.GetComponent<Creature>().viewPoint)
        {
            coll.transform.root.GetComponent<NPC>().Alarm(volume / Vector3.Distance(coll.transform.position, pos));
        }
    }

    private void Interact()
    {
        if (CameraControl.instance.InteractionCheck() != InteractionType.None)
        {
            RaycastHit target = CameraControl.instance.hit;

            UIManager.instance.crosshair.GetChild(0).gameObject.SetActive(true);

            UIManager.instance.crosshair.GetComponentInChildren<Text>().text = CameraControl.instance.InteractionCheck().ToString();

            if (Input.GetKey(GameMaster.instance.interactionKey))
            {
                if (target.transform.GetComponent<IPortable>() != null && !target.transform.GetComponent<IPortable>().holding)
                {
                    GameMaster.instance.keyHoldTime += Time.deltaTime;

                    if (GameMaster.instance.keyHoldTime > .5f)
                    {
                        target.transform.GetComponent<IPortable>().Interaction(transform);

                        GameMaster.instance.keyHoldTime = .5f;
                    }
                }
            }
            else if (Input.GetKeyUp(GameMaster.instance.interactionKey))
            {
                if (GameMaster.instance.keyHoldTime >= .5f)
                {
                    if (target.transform.GetComponent<IPortable>() != null & !target.transform.GetComponent<IPortable>().holding)
                    {
                        target.transform.GetComponent<IPortable>().Interaction(transform);

                        GameMaster.instance.keyHoldTime = 0f;
                    }

                }
                else
                {
                    target.transform.GetComponentInParent<IInteractable>().Interaction(this);

                    GameMaster.instance.keyHoldTime = 0f;
                }
            }
        }
        else UIManager.instance.crosshair.GetChild(0).gameObject.SetActive(false);
    }

    private void RotateProjectile() // FIXME: don't forget remove it in future
    {
        if (holdPointMiddle.childCount > 0)
        {
            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                holdPointMiddle.GetChild(0).GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;

                if (Input.GetAxis("Mouse ScrollWheel") > 0) holdPointMiddle.GetChild(0).transform.Rotate(0, Input.GetAxis("Mouse ScrollWheel") * 100, 0, Space.World);

                if (Input.GetAxis("Mouse ScrollWheel") < 0) holdPointMiddle.GetChild(0).transform.Rotate(Input.GetAxis("Mouse ScrollWheel") * 100, 0, 0, Space.World);
            }
            else holdPointMiddle.GetChild(0).GetComponent<Rigidbody>().freezeRotation = false;
        }
    }

    public override void Throw()
    {
        if (holdPointMiddle.childCount > 0)
        {
            Transform projectile = holdPointMiddle.GetChild(0);

            if (Input.GetKeyDown(GameMaster.instance.interactionKey)) indicators.GetComponent<ActorUI>().throwBar.gameObject.SetActive(true);

            if (indicators.GetComponent<ActorUI>().throwBar.gameObject.activeSelf == true)
            {
                if (Input.GetKey(GameMaster.instance.interactionKey))
                {
                    ThrowPower += 100f * Time.deltaTime;

                    if (ThrowPower > MaxThrowPower) ThrowPower = MaxThrowPower;

                    ThrowMeter(ThrowPower, MaxThrowPower);
                }

                float staminaUsage = (projectile.GetComponent<Rigidbody>().mass * ThrowPower) / strength;

                if (currentStamina < staminaUsage)
                {
                    ThrowPower = 0;

                    projectile.GetComponent<Projectile>().Throw(CameraControl.instance.transform.forward * ThrowPower);

                    ThrowMeter(ThrowPower, MaxThrowPower);

                    GameMaster.instance.keyHoldTime = 0f;

                    projectile = null;
                }

                if (Input.GetKeyUp(GameMaster.instance.interactionKey) || ThrowPower >= MaxThrowPower)
                {
                    animator.SetFloat("Speed", 1);

                    ChangeCurrentStamina(-staminaUsage);

                    projectile.GetComponent<Projectile>().Throw(CameraControl.instance.transform.forward * ThrowPower);

                    ThrowPower = 0;

                    ThrowMeter(ThrowPower, MaxThrowPower);

                    GameMaster.instance.keyHoldTime = 0f;

                    projectile = null;
                }
            }
            
        }
    }

    private void ThrowMeter(float currPower, float maxPower)
    {
        if (currPower > 0) indicators.GetComponent<ActorUI>().throwBar.gameObject.SetActive(true);
        else indicators.GetComponent<ActorUI>().throwBar.gameObject.SetActive(false);

        indicators.GetComponent<ActorUI>().throwBar.maxValue = maxPower;

        indicators.GetComponent<ActorUI>().throwBar.value = currPower;
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == 4)
        {
            if (diving && !swimming)
            {
                swimming = true;

                diving = false;

                StopCoroutine(Suffocate());

                if (curOxygen < maxOxygen) curOxygen = maxOxygen;

                suffocate = false;

                indicators.GetComponent<ActorUI>().oxygenBar.gameObject.SetActive(false);

                GameMaster.instance.fogEnabled = false;

                RenderSettings.fog = GameMaster.instance.fogEnabled;
            }
        }
    }

    private void OnTriggerStay(Collider col)
    {
        /*
        if (col.gameObject.layer == 4)
        {
            if (parts[5].transform.position.y <= col.transform.position.y) _swimming = true;
            if (parts[5].transform.position.y > col.transform.position.y) _swimming = false;

            if (_swimming && parts[0].transform.position.y < col.transform.position.y) _diving = true;
            if (_swimming && parts[0].transform.position.y >= col.transform.position.y) _diving = false;
        }
        */
        if (col.gameObject.layer == 4)
        {
            if (diving)
            {
                GameMaster.instance.fogEnabled = true;

                RenderSettings.fogColor = GameMaster.instance.fogColor;

                RenderSettings.fogDensity = GameMaster.instance.fogDensity;

                RenderSettings.fog = GameMaster.instance.fogEnabled;

                RenderSettings.fogMode = GameMaster.instance.fogMode;
            }

            if (viewPoint.transform.position.y <= col.transform.position.y) swimming = true;
            else swimming = false;
        }
        //if (animator.GetInteger("Movement") == 5) agent.speed = swimSpeed;
    }

    public override void Death()
    {
        GameMaster.instance.deathScreen.gameObject.SetActive(true);

        GameMaster.instance.deathScreen.GetComponent<Animation>().Play("DeathScreen");

        GameMaster.instance.Invoke("GameOver", GameMaster.instance.deathScreen.GetComponent<Animation>().clip.length);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(GroundCheckRight.transform.position, groundCheckDistance);

        Gizmos.DrawWireSphere(GroundCheckLeft.transform.position, groundCheckDistance);

        Gizmos.color = Color.yellow;

        float MS = 0;

        switch (animator.GetInteger("Movement")) // FIXME: need to clear this mess.
        {
            case 4:
                {
                    MS = sprintSpeed;
                }
                break;

            case 3:
                {
                    MS = runSpeed;
                }
                break;

            case 2:
                {
                    MS = walkSpeed;
                }
                break;

            case 1:
                {
                    MS = crouchSpeed;
                }
                break;
        }

        Gizmos.DrawWireSphere((GroundCheckRight.transform.position + GroundCheckLeft.transform.position) / 2, MS * loadStage);
    }
}
