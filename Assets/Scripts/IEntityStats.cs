using System.Collections.Generic;


public interface IEntityStats
{
    void ChangeCurrentHealth(float value);

    void ChangeCurrentStamina(float value);

    void ChangeCurrentMana(float value);

    void ChangePoise(float value);

    int strength { get; set; }
    int dexterity { get; set; }
    int intelligence { get; set; }
    float maxHealth { get; set; }
    float maxStamina { get; set; }
    float maxMana { get; set; }
    float currentHealth { get; set; }
    float currentStamina { get; set; }
    float currentMana { get; set; }
    float healthRecovery { get; set; }
    float staminaRecovery { get; set; }
    float manaRecovery { get; set; }
    int loadStage { get; set; }
    float maxOxygen { get; set; }
    float curOxygen { get; set; }
}
