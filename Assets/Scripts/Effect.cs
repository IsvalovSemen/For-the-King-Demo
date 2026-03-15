using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Effect : MonoBehaviour
{
    [SerializeField] private bool _isActive;
    public Sprite effectSprite;
    public float duration { get; private set; }
    public float internalTimer { get; private set; }
    private GameObject _target;

    public void Activate(int power, float tick, float duration, Sprite icon, GameObject target)
    {
        _isActive = true;

        target.GetComponent<Creature>().AddStatusEffect(icon, this);

        _target = target;

        StartCoroutine(EffectCountdown(power, tick, duration, icon, target));

        this.duration = duration;

        internalTimer = this.duration;
    }

    public virtual void Affect(int power, GameObject target)
    {

    }

    private void Update()
    {
        
        internalTimer -= Time.deltaTime; //If you want more smooth fill change.



        if (internalTimer <= 0)
        {
            internalTimer = 0;

            RemoveEffect();
        }
    }

    private IEnumerator EffectCountdown(int power, float tick, float duration, Sprite icon, GameObject target)
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

        Destroy(effectSprite);

        _target.GetComponent<Creature>().RemoveStatusEffect(this);

        Destroy(this);
    }
}
