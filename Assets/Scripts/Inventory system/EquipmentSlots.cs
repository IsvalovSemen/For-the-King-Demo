public enum EquipSlotType
{
    WeaponLeft,
    Head,
    WeaponRight,
    RingLeft,
    Necklace,
    RingRight,
    Body,
    Arms,
    Legs,
    Feet
}

[System.Serializable]
public class EquipSlot
{
    public EquipSlotType slotType;
    public Item item;

    public bool CanPlaceHere(Item item)
    {
        switch (slotType)
        {
            case EquipSlotType.WeaponLeft:
            case EquipSlotType.WeaponRight:
                return item.Data.itemType == ItemType.Weapon;

            case EquipSlotType.RingLeft:
            case EquipSlotType.RingRight:
                return item.Data.itemType == ItemType.Ring;

            case EquipSlotType.Head:
                return item.Data.itemType == ItemType.Head;

            case EquipSlotType.Body:
                return item.Data.itemType == ItemType.Body;

            case EquipSlotType.Arms:
                return item.Data.itemType == ItemType.Arms;

            case EquipSlotType.Legs:
                return item.Data.itemType == ItemType.Legs;

            case EquipSlotType.Feet:
                return item.Data.itemType == ItemType.Feet;

            case EquipSlotType.Necklace:
                return item.Data.itemType == ItemType.Necklace;
        }

        return false;
    }
}