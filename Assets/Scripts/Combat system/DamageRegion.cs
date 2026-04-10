using System.Collections.Generic;
using UnityEngine;

public enum RegionType
{
    None,
    General,
    Head,
    WeaponRight,
    WeaponLeft,
    WeaponTwohanded,
    HandRight,
    HandLeft,
    Torso,
    LegRight,
    LegLeft
}

[System.Serializable]
public class DamageRegion
{
    [SerializeField] private RegionType _region;
    public RegionType Region => _region;
    public int baseDmg;
    public DamageTypes dmgType;
    public int impact;
    public int objectDmg;

    public List<Hitbox> hitboxes = new List<Hitbox>();

    public List<Hurtbox> hurtboxes = new List<Hurtbox>();
}
