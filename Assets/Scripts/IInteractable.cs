using UnityEngine;

public enum InteractionType { None, Take, Loot, Open, Pull, Press }

public interface IInteractable
{
    InteractionType interactionType { get; set; }

    public void Interaction(Creature source)
    {

    }
    public void OnSelect();
    public void OnDeselect();
}
