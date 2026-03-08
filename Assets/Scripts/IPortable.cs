using UnityEngine;

public interface IPortable : IInteractable
{
    bool holding { get; set; }

    public void Interaction(Transform holdPoint)
    {

    }
}
