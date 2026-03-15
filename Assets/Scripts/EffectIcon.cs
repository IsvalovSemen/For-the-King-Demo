using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectIcon : MonoBehaviour
{
    private Effect _relatedEffect;
    public Image icon;
    public Image fill;
    public Text counter;

    public void Init(Effect effect)
    {
        _relatedEffect = effect;

        icon.sprite = effect.effectSprite;
    }

    private void Update()
    {
        float normalizedTime = Mathf.Clamp01(_relatedEffect.internalTimer / _relatedEffect.duration);

        fill.fillAmount = normalizedTime;
    }
}
