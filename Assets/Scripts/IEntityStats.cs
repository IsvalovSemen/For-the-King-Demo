using System.Collections.Generic;


public interface IEntityStats
{
    void ChangeCurrentHealth(int value);

    void ChangeCurrentStamina(int value);

    void ChangeCurrentMana(int value);

    void ChangePoise(int value);

    int strength { get; set; }
    int dexterity { get; set; }
    int intelligence { get; set; }
    int maxHealth { get; set; }
    int maxStamina { get; set; }
    int maxMana { get; set; }
    int currentHealth { get; set; }
    int currentStamina { get; set; }
    int currentMana { get; set; }
    int healthRecovery { get; set; }
    int staminaRecovery { get; set; }
    int manaRecovery { get; set; }
    int loadStage { get; set; }
    int maxOxygen { get; set; }
    int curOxygen { get; set; }
}
