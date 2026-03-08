using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActorUI : MonoBehaviour
{
    public Text nameTxt;
    public Text classTxt;
    public Image experienceBar;
    public Slider healthBar;
    public Slider staminaBar;
    public Slider manaBar;
    public Text curHealthTxt;
    public Text curStaminaTxt;
    public Text curManaTxt;
    public Text lvlTxt;
    public Text expTxt;
    public Text strTxt;
    public Text dexTxt;
    public Text intTxt;
    public Slider throwBar;
    public Slider oxygenBar;
    public Text damageCount;
    public Text equipLoadTxt;
    public Slider equipLoadSlider;
    public Gradient loadGrad;
    public Transform statusEffectsPanel;
    public Image alertIndicator;
    public List<Sprite> alertIndicatorSprites = new List<Sprite>(4);
}
