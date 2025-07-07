using UnityEngine;

public enum CardSuit { Brains, Bones, Blood, RottenFlesh }
public enum CardUsageType { Cooldown, SingleUse }

public class Card
{
    public string cardName;
    public CardSuit suit;
    public CardUsageType usageType;
    public string abilityDescription;
    public Sprite cardImage;

    public void ActivateEffect()
    {
        Debug.Log("Activating Card!");
    }
}
