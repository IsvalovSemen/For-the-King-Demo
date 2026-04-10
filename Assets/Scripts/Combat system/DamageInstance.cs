using System.Collections.Generic;
using UnityEngine;

public enum DamageTypes
{
    Slash,
    Blunt,
    Pierce,
    Fire,
    Ice,
    Lightning,
    Pure // Ignores any resistances.
}

public class DamageInstance
{
    public GameObject source;
    public int dmgValue;
    public DamageTypes dmgType; // Type of damage for resistances, effects, or VFX.
    public int poiseDmg;
    public int objDmg;
    public Direction dir;
    public bool isMultihit;
    public bool ignoreInvulnerability;  // Whether to bypass invulnerability frames (useful for AoE or projectiles).
    public float lifetime;              // Optional lifetime for delayed damage (e.g., fire effect, falling rock).
    public float radius;                // Optional AoE radius for explosions or magic spells.
    public bool isCritical;             // Marks if this hit is critical (could modify damage or trigger effects).
    public float force;                 // Optional force applied to target for knockback or physics reactions.
    public GameObject[] ignoredTargets; // Optional list of targets to ignore (e.g., already hit targets or owner).
    public string effectName;           // Optional visual/audio effect to spawn on hit.

    /// <summary>
    /// Constructor for basic damage.
    /// </summary>
    public DamageInstance(GameObject source, int dmgValue, DamageTypes dmgType, int poiseDmg, int objectDmg, Direction direction)
    {
        this.source = source;
        this.dmgValue = dmgValue;
        this.dmgType = dmgType;
        this.poiseDmg = poiseDmg;
        this.objDmg = objectDmg;
        this.dir = direction;
        isMultihit = false;
        ignoreInvulnerability = false;
        lifetime = 0f;
        radius = 0f;
        isCritical = false;
        force = 0f;
        ignoredTargets = null;
        effectName = null;
    }
}