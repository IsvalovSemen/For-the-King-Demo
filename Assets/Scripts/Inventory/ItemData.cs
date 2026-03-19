using UnityEngine;

public enum ItemType
{
    Misc,
    Weapon,
    Head,
    Body,
    Arms,
    Legs,
    Feet,
    Ring,
    Necklace
}

public enum DamageType
{
    Slash,
    Blunt,
    Thrust,
    Fire,
    Cold,
    Poison,
    Light
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Item")]
public class ItemData : ScriptableObject
{
    public string ID;
    public GameObject prefab;
    public bool isStackable;
    public int maxStacksAmount;

    public ItemType itemType;
    public string itemTitle;
    public Sprite iconSprite;
    public int damage;

    public DamageType dmgType;
    public enum WeaponType
    {
        Striking1H,
        Thrusting1H,
        Striking2H,
        Thrusting2H
    }
    public WeaponType wpnType;
    public int slashRes;
    public int thrustRes;
    public int bluntRes;
    public int fireRes;
    public int coldRes;
    public int poisonRes;
    public int lightRes;
    public int strReq;
    public int dexReq;
    public int intReq;
    public int restoreAmount;
    public float weight;
    public int price;
    public int maxDurability;
    public int iconWidth;
    public int iconHeight;
    public string description;
    public enum WeaponPlacement
    {
        Back,
        Belt
    }

    public WeaponPlacement placement;
    public Sound[] sounds;
}