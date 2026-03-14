using System.Collections;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class ActorStatus : MonoBehaviour
{
    public Canvas canvas;
    [SerializeField] private TMP_Text _nameTMP;
    [SerializeField] private Slider _healthSlider;
    public float dmgHideDelay = 2f;
    public float totalDmg;
    public float fadeTime;
    public TMP_Text dmgCountTMP;
    public Image alertIndicator;

    public void Init(Creature actor)
    {
        _nameTMP.text = actor.name;
        _healthSlider.maxValue = actor.maxHealth;
        _healthSlider.value = actor.currentMana;

        actor.OnHealthChange += UpdateHealthBar;
    }

    private void UpdateHealthBar(float curHP, float maxHP)
    {

    }

    public IEnumerator ResetDamageCounter()
    {
        yield return new WaitForSeconds(dmgHideDelay);

        totalDmg = 0;

        dmgCountTMP.text = string.Empty;
    }
}
