using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable, IDamageable
{
    private SoundManager _soundManager;
    public InteractionType interactionType { get; set; }
    [SerializeField] private bool _open;
    [SerializeField] private bool _locked;
    [SerializeField] private Animator _animator;
    private bool _inRange = false;
    [SerializeField] private string _keyID;
    [SerializeField] private List<Creature> _interactors = new List<Creature>();

    private void Awake()
    {
        interactionType = InteractionType.Open;
    }

    private void Start()
    {
        _soundManager = GetComponent<SoundManager>();

        _animator = GetComponent<Animator>();

        _animator.SetFloat("Speed", 0);
    }

    private void Update()
    {
        //GetComponent<Animation>()["Open"].speed = 1 * -1;
    }

    private void OnTriggerEnter(Collider trigger)
    {
        if (!_interactors.Contains(trigger.GetComponentInParent<Creature>()))
        {
            _interactors.Add(trigger.GetComponentInParent<Creature>());
        }
    }

    private void OnTriggerExit(Collider trigger)
    {
        if (_interactors.Contains(trigger.GetComponentInParent<Creature>()))
        {
            _interactors.Remove(trigger.GetComponentInParent<Creature>());
        }
    }

    public void Interaction(Creature interactor)
    {
        for (int i = 0; i < _interactors.Count; i++)
        {
            if (_interactors.Contains(interactor))
            {
                if (_locked == true)
                {
                    if (interactor.inventory.SearchForItem(_keyID) == true) Unlock();
                    else UIManager.instance.PrintMessage("You don't have the right key.");
                }
                else
                {
                    _animator.SetTrigger("Interacted");

                    if (!_open) Open();
                    else Close();
                }
            }

            break;
        }
    }

    public void OnSelect()
    {
        if (_interactors.Contains(Player.instance)) UIManager.instance.EnableInteractionPrompt(interactionType);
    }

    public void OnDeselect()
    {
        UIManager.instance.DisableInteractionPrompt();
    }

    public void GetHit(float amount, DamageType type, Transform part)
    {
        _soundManager.PlaySound("GetHit");

        //GetComponent<Rigidbody>().AddForce(GetComponent<Rigidbody>().velocity * 10f, ForceMode.Impulse); Need to finish it later
    }

    public void Unlock()
    {
        UIManager.instance.PrintMessage("Door was unlocked.");

        _locked = false;

        if (_soundManager != null) _soundManager.PlaySound("Unlock");
    }

    public void Open()
    {
        _animator.SetFloat("Speed", 1);

        _open = true;

        UIManager.instance.PrintMessage($"{this.gameObject} has been opened.");
    }

    public void Close()
    {
        _animator.SetFloat("Speed", -1);

        _open = false;

        UIManager.instance.PrintMessage($"{this.gameObject} has been closed.");
    }

    public void PlaySound(AnimationEvent animationEvent)
    {
        if (_soundManager != null)
        {
            if (animationEvent.stringParameter == "Start")
            {
                _soundManager.PlaySound("Creak");
            }

            if (animationEvent.stringParameter == "End")
            {
                _soundManager.StopSound("Creak");

                if (_open == true)
                {
                    _soundManager.PlaySound("Open");
                }
                else
                {
                    _soundManager.PlaySound("Close");
                }
            }
        }
    }
}
