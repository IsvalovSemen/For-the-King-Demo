using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : Item
{
    public void Use()
    {

    }

    public override void Break()
    {
        Destroy(transform.gameObject);
    }
}
