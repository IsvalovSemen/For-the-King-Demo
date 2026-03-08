using UnityEngine;

public class RegenerationEffect : Effect
{
    public override void Affect(float power, GameObject target)
    {
        target.GetComponent<Creature>().ChangeHealth(power);
    }
}
