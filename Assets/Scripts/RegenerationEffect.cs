using UnityEngine;

public class RegenerationEffect : Effect
{
    public override void Affect(int power, GameObject target)
    {
        target.GetComponent<Creature>().ChangeCurrentHealth(power);
    }
}
