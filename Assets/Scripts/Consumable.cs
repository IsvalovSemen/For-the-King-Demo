using System;
using UnityEngine;

public class Consumable : Item
{
    public override void Start()
    {
        base.Start();
    }
    /*
    public override void Store(int slotType)
    {
        if (slotType == -1)
        {
            if (_stored == false)
            {
                GM.player.GetComponent<IEntityStats>().ChangeEquipload(stats.weight);

                _stored = true;
            }

            transform.SetParent(card.transform.GetComponentInParent<Inventory>().storage.transform);

            transform.position = card.transform.GetComponentInParent<Inventory>().storage.transform.position;

            transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;

            transform.GetChild(0).GetComponent<Collider>().enabled = false;
        }
    }
    */
    public void Use()
    {
        Consume();
    }

    private void Consume()
    {
        /*
        switch (stats.type)
        {
            case CunsumableType.RestoreHealth:
                {
                    transform.root.GetComponent<Creature>().ChangeHealth(stats.restoreAmount);
                }
                break;

            case CunsumableType.RestoreStamina:
                {
                    transform.root.GetComponent<Creature>().ChangeStamina(stats.restoreAmount);
                }
                break;

            case CunsumableType.RestoreMana:
                {
                    transform.root.GetComponent<Creature>().ChangeMana(stats.restoreAmount);
                }
                break;

            default:

                break;
        }
        /*
        SM.PlaySound("Consume");

        Sound sound = Array.Find(SM.sounds, sound => sound.name == "Consume");

        Invoke("DestroyItem", sound.source.clip.length);
        */
        GameMaster.instance.SM.PlaySound("EatVegetable");

        Break();
    }

    public override void Break()
    {
        //transform.root.GetComponent<Creature>().ChangeEquipload(-stats.weight);

        Destroy(transform.gameObject);
    }
}
