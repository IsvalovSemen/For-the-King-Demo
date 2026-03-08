using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{
    [SerializeField] private string _skillTitle;
    [SerializeField] private int _skillPointsRequired;
    [SerializeField] private string _skillDescription;
    protected enum SkillTypes { Active, Passive }
    [SerializeField] protected SkillTypes Type;
    [SerializeField] private bool _acquired;
    [SerializeField] private Skill _previousSkill;

    public void Learn()
    {
        if (ConditionCheck())
        {
            _acquired = true;

            Player.instance.skillpointsAvailable--;

            if (Type == SkillTypes.Passive) MakeImpact();
        }
    }

    public virtual bool ConditionCheck()
    {
        bool available = false;

        if (Player.instance.skillpointsAvailable >= _skillPointsRequired & !_acquired) //If you have enough skill points available & skill yet not learned
        {
            if (_previousSkill == null || _previousSkill != null && _previousSkill._acquired) available = true; //If there's prior skill & that skill is learned, then you can learn this skill
        }

        return available;
    }

    public virtual void MakeImpact()
    {

    }
}
