using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UIManager;

public class Door : MonoBehaviour, IInteractable, IDamageable
{
    public SoundManager SM { get; set; }
    GameObject _player;
    public InteractionType interactionType { get; set; }
    [SerializeField] bool _opened;
    Animator _animator;
    [SerializeField] bool _locked;
    [SerializeField] string _keyID;
    [SerializeField] Creature _interactor;

    private void Awake()
    {
        interactionType = InteractionType.Open;
    }

    private void Start()
    {
        SM = GetComponent<SoundManager>();

        _animator = GetComponent<Animator>();

        _animator.SetFloat("Speed", 0);
    }

    private void Update()
    {
        //GetComponent<Animation>()["Open"].speed = 1 * -1;
    }

    public void Interaction(Creature interactor)
    {
        if (interactor == _interactor)
        {
            if (_locked)
            {
                UIManager.instance.OpenMenu(MenuState.Inventory);
            }
            else
            {
                UIManager.instance.PrintMessage("Door has opened.");

                if (!_opened) _animator.SetFloat("Speed", 1);
                else _animator.SetFloat("Speed", -1);

                _animator.SetTrigger("Interacted");

                _opened = !_opened;
            }
        }
    }

    public void GetHit(float amount, DamageType type, Transform part)
    {
        SM.PlaySound("GetHit");

        //GetComponent<Rigidbody>().AddForce(GetComponent<Rigidbody>().velocity * 10f, ForceMode.Impulse); Need to finish it later
    }

    public void Unlock(string key)
    {
        if (_locked)
        {
            if (_keyID == key)
            {
                UIManager.instance.PrintMessage("Door was unlocked.");

                UIManager.instance.CloseAllMenus();

                SM.PlaySound("Unlock");

                _locked = false;
            }
            else UIManager.instance.PrintMessage("Oops wrong key!");
        }
    }

    public void Open(AnimationEvent animationEvent)
    {
        if (_opened)
        {
            SM.PlaySound("Open");

            SM.StopSound("Creak");

            SM.PlaySound("Creak");
        }
        else
        {
            SM.PlaySound("Close");

            SM.StopSound("Creak");
        }
    }

    public void Close(AnimationEvent animationEvent)
    {
        if (_opened)
        {
            SM.PlaySound("Close");

            SM.StopSound("Creak");
        }
        else
        {
            SM.StopSound("Creak");

            SM.PlaySound("Creak");
        }
    }

    void OnTriggerStay(Collider trigger)
    {
        _interactor = trigger.GetComponentInParent<Creature>();
    }
}
