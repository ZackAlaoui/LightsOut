using System.Collections.Generic;
using UnityEngine;


public class CardManager : MonoBehaviour
{
    public static CardManager Instance;

    public List<Card> allCards = new();  // Your card pool
    public List<Card> playerHand = new();  // The current 5 cards

    public int startingHandSize = 5;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        GenerateStartingHand();
    }

    public void GenerateStartingHand()
    {
        playerHand.Clear();

        // Example logic: randomly draw 5 from common/uncommon pool
        List<Card> commons = allCards.FindAll(card =>
            card.usageType == CardUsageType.Cooldown || card.usageType == CardUsageType.SingleUse); // Filter if needed

        for (int i = 0; i < startingHandSize; i++)
        {
            int index = Random.Range(0, commons.Count);
            playerHand.Add(commons[index]);
        }

        Debug.Log("Player starting hand generated.");
    }

    public void UseCard(int cardIndex)
    {
        if (cardIndex >= 0 && cardIndex < playerHand.Count)
        {
            playerHand[cardIndex].ActivateCard();

            if (playerHand[cardIndex].usageType == CardUsageType.SingleUse)
                playerHand.RemoveAt(cardIndex); // One-time use
        }
    }

    public void PokerHand()
    {
        Dictionary<CardSuit, int> suitCounts = new();

        foreach (Card card in playerHand)
        {
            if (!suitCounts.ContainsKey(card.suit))
                suitCounts[card.suit] = 0;

            suitCounts[card.suit]++;
        }

        foreach (var entry in suitCounts)
        {
            int count = entry.Value;
            CardSuit suit = entry.Key;

            switch (count)
            {
                case 2:
                    Debug.Log($"Pair of {suit}: Granting minor buff.");
                    ApplySuitBuff(suit, "pair");
                    break;
                case 3:
                    Debug.Log($"Three of a Kind {suit}: Granting moderate buff.");
                    ApplySuitBuff(suit, "three");
                    break;
                case 4:
                    Debug.Log($"Four of a Kind {suit}: Granting major buff.");
                    ApplySuitBuff(suit, "four");
                    break;
                case 5:
                    Debug.Log($"Five of a Kind {suit}: Ultimate Buff!");
                    ApplySuitBuff(suit, "five");
                    break;

            }
        }
    }

    private void ApplySuitBuff(CardSuit suit, string comboType)
    {
        switch (suit)
        {
            case CardSuit.Brains:
                break;
            case CardSuit.Bones:
                break;
            case CardSuit.Blood:
                break;
            case CardSuit.RottenFlesh:
                break;
        }

        Debug.Log($"Applied {comboType} buff for {suit}.");
    }
}
