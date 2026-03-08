using System.Collections.Generic;


public interface IEntityStats
{
    void ChangeHealth(float value);

    void ChangeStamina(float value);

    void ChangeMana(float value);

    void ChangePoise(float value);

    int STR { get; set; }
    int DEX { get; set; }
    int INT { get; set; }
    float maxHP { get; set; }
    float maxSP { get; set; }
    float maxMP { get; set; }
    float curHP { get; set; }
    float curSP { get; set; }
    float curMP { get; set; }
    float healthRegen { get; set; }
    float staminaRegen { get; set; }
    float manaRegen { get; set; }
    int loadStage { get; set; }
    float maxOxygen { get; set; }
    float curOxygen { get; set; }
}
