using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    // Declare the static Instance property
    public static CardManager Instance { get; private set; } // This makes it accessible from anywhere, but only settable from within this class

    public List<Card> allCards = new();
    public List<Card> playerHand = new();

    public int startingHandSize = 5;

    private void Awake()
    {
        // This is where you assign the current instance to the static Instance property
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject); // Destroy duplicate instances if one already exists
    }

    private void Start()
    {
        GenerateStartingHand();
    }

    public void GenerateStartingHand()
    {
        playerHand.Clear();

        List<Card> commons = allCards.FindAll(card =>
            card.usageType == CardUsageType.Cooldown || card.usageType == CardUsageType.SingleUse);

        for (int i = 0; i < startingHandSize; i++)
        {
            if (commons.Count > 0)
            {
                int index = Random.Range(0, commons.Count);
                playerHand.Add(commons[index]);
            }
            else
            {
                Debug.LogWarning("No common cards found to generate starting hand!");
                break;
            }
        }
    }

    public void UseCard(int cardIndex)
    {
        if (cardIndex >= 0 && cardIndex < playerHand.Count)
        {
            playerHand[cardIndex].ActivateEffect();

            if (playerHand[cardIndex].usageType == CardUsageType.SingleUse)
                playerHand.RemoveAt(cardIndex);
        }
    }
}