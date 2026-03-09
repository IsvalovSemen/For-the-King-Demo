using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonResurrection : MonoBehaviour
{
    public bool startResurrection;

    void Update()
    {
        if (GetComponent<Creature>().isDead)
        {
            StartCoroutine(DestroySkeleton());

            startResurrection = true;
        }
        
        if (GetComponent<Creature>().isDead && startResurrection)
        {
            StartCoroutine(ResetSkeleton());

            startResurrection = false;
        }
    }

    IEnumerator DestroySkeleton()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(0).GetComponent<Bone>() != null)
            {
                transform.GetChild(0).GetComponent<Bone>().FallApart();

                transform.GetChild(0).GetComponent<Bone>().Dismember();
            }
            else break;
        }

        yield return null;
    }

    IEnumerator ResetSkeleton()
    {
        GetComponent<Creature>().isDead = false;

        GetComponent<Creature>().currentHealth = GetComponent<Creature>().maxHealth;

        GetComponent<Creature>().enabled = true;

        GetComponent<Animator>().enabled = true;

        yield return null;
    }
}
