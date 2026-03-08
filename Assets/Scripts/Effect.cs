using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Effect : MonoBehaviour
{
    [SerializeField] private bool _isActive;
    public GameObject effectIcon;
    private float _duration;
    private float _internalTimer;
    private GameObject _target;

    public void Activate(float power, float tick, float duration, Sprite icon, GameObject target)
    {
        _isActive = true;

        target.GetComponent<Creature>().AddStatusEffect(icon, this);

        _target = target;

        StartCoroutine(EffectCountdown(power, tick, duration, icon, target));

        _duration = duration;

        _internalTimer = _duration;
    }

    public virtual void Affect(float power, GameObject target)
    {

    }

    public void Update()
    {
        //If you want more smooth fill change
        _internalTimer -= Time.deltaTime;

        float normalizedTime = Mathf.Clamp01(_internalTimer / _duration);

        effectIcon.transform.GetChild(0).GetComponent<Image>().fillAmount = normalizedTime;

        if (_internalTimer <= 0)
        {
            _internalTimer = 0;

            RemoveEffect();
        }
    }

    private IEnumerator EffectCountdown(float power, float tick, float duration, Sprite icon, GameObject target)
    {
        for (float i = 0f; i <= duration; i += tick)
        {
            Affect(power, target);

            float counter = i;

            //iconLink.fill.fillAmount =  (1 - (counter / duration)); // If you want to change the fill strictly at separate intervals

            yield return new WaitForSeconds(tick);
        }
    }

    private void RemoveEffect()
    {
        _isActive = false;

        Destroy(effectIcon);

        _target.GetComponent<Creature>().RemoveStatusEffect(this);

        Destroy(this);
    }
}
