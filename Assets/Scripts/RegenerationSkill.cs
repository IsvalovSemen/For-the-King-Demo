using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegenerationSkill : Skill
{
    [SerializeField] private EffectInfo _skillEffect;

    public override bool ConditionCheck()
    {
        bool available = false;

        if (Player.instance.lvl >= 1) available = true;

        available =  base.ConditionCheck();

        return available;
    }

    public override void MakeImpact()
    {
        var newEffect = Player.instance.gameObject.AddComponent<RegenerationEffect>();

        newEffect.Activate(_skillEffect.power, _skillEffect.tick, _skillEffect.duration, _skillEffect.icon, Player.instance.gameObject);
    }
}
