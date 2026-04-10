using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Attack
{
    public string title;
    [SerializeField] private float _motionMdf = 1f;
    public Direction dir;
    public RegionType region;
}

