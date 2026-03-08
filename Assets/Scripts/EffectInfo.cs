using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEffect", menuName = "Effect")]
public class EffectInfo : ScriptableObject
{
    public string effectTitle;
    public Sprite icon;
    public float power;
    public float tick;
    public float duration;
}
