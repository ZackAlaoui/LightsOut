
using System.Collections.Generic;
using UnityEngine;

public class TestCardGenerator : MonoBehaviour
{
    [Header("Card Testing")]
    public List<Card> testCards = new List<Card>();
    
    void Start()
    {
        Invoke(nameof(GenerateTestCards), 0.2f); // Delay so CardManager and SpriteGenerator run first
    }


    void GenerateTestCards()
    {
        testCards.Clear();

        // Card 1: Brains Suit - Cooldown Ability
        Card brainsCard = new Card
        {
            cardName = "Neural Boost",
            suit = CardSuit.Brains,
            usageType = CardUsageType.Cooldown,
            abilityDesc = "Reduces all ability cooldowns by 20% for 10 seconds",
            CardImage = SimpleCardSpriteGenerator.generatedSprites[CardSuit.Brains] // You'll need to assign sprites in inspector
        };
        testCards.Add(brainsCard);

        // Card 2: Bones Suit - Cooldown Ability
        Card bonesCard = new Card
        {
            cardName = "Bone Armor",
            suit = CardSuit.Bones,
            usageType = CardUsageType.Cooldown,
            abilityDesc = "Increases armor by 50% and provides knockback resistance",
            CardImage = SimpleCardSpriteGenerator.generatedSprites[CardSuit.Bones]
        };
        testCards.Add(bonesCard);

        // Card 3: Blood Suit - Single Use
        Card bloodCard = new Card
        {
            cardName = "Crimson Strike",
            suit = CardSuit.Blood,
            usageType = CardUsageType.SingleUse,
            abilityDesc = "Next attack deals 200% damage and heals for 25% of damage dealt",
            CardImage = SimpleCardSpriteGenerator.generatedSprites[CardSuit.Blood]
        };
        testCards.Add(bloodCard);

        // Card 4: RottenFlesh Suit - Cooldown Ability
        Card rottenCard = new Card
        {
            cardName = "Toxic Cloud",
            suit = CardSuit.RottenFlesh,
            usageType = CardUsageType.Cooldown,
            abilityDesc = "Creates a poison aura that damages nearby enemies",
            CardImage = SimpleCardSpriteGenerator.generatedSprites[CardSuit.RottenFlesh]
        };
        testCards.Add(rottenCard);

        // Card 5: Blood Suit - Cooldown Ability (for testing pairs)
        Card bloodCard2 = new Card
        {
            cardName = "Berserker Rage",
            suit = CardSuit.Blood,
            usageType = CardUsageType.Cooldown,
            abilityDesc = "Increases attack speed by 40% for 15 seconds",
            CardImage = SimpleCardSpriteGenerator.generatedSprites[CardSuit.Blood]
        };
        testCards.Add(bloodCard2);

        if (CardManager.Instance != null)
        {
            CardManager.Instance.allCards.AddRange(testCards);
            CardManager.Instance.GenerateStartingHand(); // <- Trigger after cards are added
            Debug.Log($"Added {testCards.Count} test cards to CardManager");
        }


        FindObjectOfType<HandUIManager>()?.UpdateHandDisplay();

        Debug.Log("Test cards generated!");
        LogCardCombinations();
        
        
    }
    
    void LogCardCombinations()
    {
        Debug.Log("=== Test Card Combinations ===");
        Debug.Log("Current hand will have:");
        Debug.Log("- 2 Blood cards (will trigger PAIR bonus)");
        Debug.Log("- 1 Brains card");
        Debug.Log("- 1 Bones card");
        Debug.Log("- 1 RottenFlesh card");
        Debug.Log("Expected result: One Pair of Blood cards");
        Debug.Log("================================");
    }
    
    // Method to manually set up specific test hands
    [ContextMenu("Test Flush Hand")]
    public void TestFlushHand()
    {
        if (CardManager.Instance == null) return;
        
        CardManager.Instance.playerHand.Clear();
        
        // Create 5 Brains cards for flush test
        for (int i = 0; i < 5; i++)
        {
            Card flushCard = new Card
            {
                cardName = $"Brain Card {i + 1}",
                suit = CardSuit.Brains,
                usageType = CardUsageType.Cooldown,
                abilityDesc = $"Test brain card {i + 1}",
                CardImage = null
            };
            CardManager.Instance.playerHand.Add(flushCard);
        }
        
        CardManager.Instance.EvaluatePokerHand();
        Debug.Log("Flush hand test complete!");
    }
    
    [ContextMenu("Test Four of a Kind")]
    public void TestFourOfAKind()
    {
        if (CardManager.Instance == null) return;
        
        CardManager.Instance.playerHand.Clear();
        
        // Create 4 Blood cards + 1 other
        for (int i = 0; i < 4; i++)
        {
            Card bloodCard = new Card
            {
                cardName = $"Blood Card {i + 1}",
                suit = CardSuit.Blood,
                usageType = CardUsageType.Cooldown,
                abilityDesc = $"Test blood card {i + 1}",
                CardImage = null
            };
            CardManager.Instance.playerHand.Add(bloodCard);
        }
        
        // Add one different suit
        Card otherCard = new Card
        {
            cardName = "Bones Card",
            suit = CardSuit.Bones,
            usageType = CardUsageType.Cooldown,
            abilityDesc = "Test bones card",
            CardImage = null
        };
        CardManager.Instance.playerHand.Add(otherCard);
        
        CardManager.Instance.EvaluatePokerHand();
        Debug.Log("Four of a kind test complete!");
    }
    
    [ContextMenu("Test Full House")]
    public void TestFullHouse()
    {
        if (CardManager.Instance == null) return;
        
        CardManager.Instance.playerHand.Clear();
        
        // Create 3 Brains cards
        for (int i = 0; i < 3; i++)
        {
            Card brainsCard = new Card
            {
                cardName = $"Brains Card {i + 1}",
                suit = CardSuit.Brains,
                usageType = CardUsageType.Cooldown,
                abilityDesc = $"Test brains card {i + 1}",
                CardImage = null
            };
            CardManager.Instance.playerHand.Add(brainsCard);
        }
        
        // Create 2 Blood cards
        for (int i = 0; i < 2; i++)
        {
            Card bloodCard = new Card
            {
                cardName = $"Blood Card {i + 1}",
                suit = CardSuit.Blood,
                usageType = CardUsageType.Cooldown,
                abilityDesc = $"Test blood card {i + 1}",
                CardImage = null
            };
            CardManager.Instance.playerHand.Add(bloodCard);
        }
        
        CardManager.Instance.EvaluatePokerHand();
        Debug.Log("Full house test complete!");
    }
    
    [ContextMenu("Test Two Pair")]
    public void TestTwoPair()
    {
        if (CardManager.Instance == null) return;
        
        CardManager.Instance.playerHand.Clear();
        
        // Create 2 Brains cards
        for (int i = 0; i < 2; i++)
        {
            Card brainsCard = new Card
            {
                cardName = $"Brains Card {i + 1}",
                suit = CardSuit.Brains,
                usageType = CardUsageType.Cooldown,
                abilityDesc = $"Test brains card {i + 1}",
                CardImage = null
            };
            CardManager.Instance.playerHand.Add(brainsCard);
        }
        
        // Create 2 Blood cards
        for (int i = 0; i < 2; i++)
        {
            Card bloodCard = new Card
            {
                cardName = $"Blood Card {i + 1}",
                suit = CardSuit.Blood,
                usageType = CardUsageType.Cooldown,
                abilityDesc = $"Test blood card {i + 1}",
                CardImage = null
            };
            CardManager.Instance.playerHand.Add(bloodCard);
        }
        
        // Add one different suit
        Card otherCard = new Card
        {
            cardName = "Bones Card",
            suit = CardSuit.Bones,
            usageType = CardUsageType.Cooldown,
            abilityDesc = "Test bones card",
            CardImage = null
        };
        CardManager.Instance.playerHand.Add(otherCard);
        
        CardManager.Instance.EvaluatePokerHand();
        Debug.Log("Two pair test complete!");
    }
    
    // Helper method to create a card of specific suit
    public Card CreateTestCard(CardSuit suit, string name = "Test Card")
    {
        return new Card
        {
            cardName = name,
            suit = suit,
            usageType = CardUsageType.Cooldown,
            abilityDesc = $"Test {suit} card ability",
            CardImage = null
        };
    }
}