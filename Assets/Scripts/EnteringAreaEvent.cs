using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnteringAreaEvent : MonoBehaviour
{
    public UnityEvent ActivateOnEnter;
    [SerializeField] private bool _activated;

    private void OnTriggerEnter()
    {
        if (!_activated)
        {
            _activated = !_activated;

            ActivateOnEnter.Invoke();
        }
    }
}
