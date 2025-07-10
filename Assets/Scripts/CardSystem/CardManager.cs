using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;

    public List<Card> allCards = new();
    public List<Card> playerHand = new();

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

        List<Card> commons = allCards.FindAll(card =>
            card.usageType == CardUsageType.Cooldown || card.usageType == CardUsageType.SingleUse);

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
                playerHand.RemoveAt(cardIndex);
        }
    }

    public void EvaluatePokerHand()
    {
        Dictionary<CardSuit, int> suitCounts = new();
        foreach (Card card in playerHand)
        {
            if (!suitCounts.ContainsKey(card.suit))
                suitCounts[card.suit] = 0;
            suitCounts[card.suit]++;
        }

        // Check Flush
        foreach (var suit in suitCounts)
        {
            if (suit.Value == 5)
            {
                Debug.Log($"Flush of {suit.Key}: Ultimate buff!");
                ApplySuitBuff(suit.Key, "flush");
                return;
            }
        }

        int pairs = 0;
        bool hasThree = false;
        CardSuit firstPairSuit = CardSuit.Brains;
        CardSuit secondPairSuit = CardSuit.Bones;
        CardSuit threeSuit = CardSuit.Blood;

        foreach (var suit in suitCounts)
        {
            if (suit.Value == 2)
            {
                if (pairs == 0)
                    firstPairSuit = suit.Key;
                else
                    secondPairSuit = suit.Key;
                pairs++;
            }
            else if (suit.Value == 3)
            {
                hasThree = true;
                threeSuit = suit.Key;
            }
            else if (suit.Value == 4)
            {
                Debug.Log($"Four of a Kind ({suit.Key}): Major buff!");
                ApplySuitBuff(suit.Key, "four");
                return;
            }
        }

        if (pairs == 2)
        {
            Debug.Log($"Two Pair: {firstPairSuit} and {secondPairSuit}");
            ApplyTwoPairBuff(firstPairSuit, secondPairSuit);
            return;
        }

        if (hasThree && pairs >= 1)
        {
            Debug.Log($"Full House: {threeSuit} + {firstPairSuit}");
            ApplyFullHouseBuff(threeSuit, firstPairSuit);
            return;
        }

        if (pairs == 1)
        {
            Debug.Log("One Pair: General minor buff.");
            ApplyGeneralBuff();
        }
    }

    private void ApplySuitBuff(CardSuit suit, string comboType)
    {
        switch (comboType)
        {
            case "flush":
                Debug.Log($"Flush of {suit}: Increased damage, speed, cooldown, AND flashlight.");
                switch (suit)
                {
                    case CardSuit.Brains:
                        //Add something here
                        break;
                    case CardSuit.Bones:
                        //Add something here
                        break;
                    case CardSuit.Blood:
                        //Add something here
                        break;
                    case CardSuit.RottenFlesh:
                        //Add something here
                        break;
                }
                break;

            case "four":
                Debug.Log($"Four of a Kind {suit}: Large bonus to effect specific to {suit}");
                switch (suit)
                {
                    case CardSuit.Brains:
                        //Add something here
                        break;
                    case CardSuit.Bones:
                        //Add something here
                        break;
                    case CardSuit.Blood:
                        //Add something here
                        break;
                    case CardSuit.RottenFlesh:
                        //Add something here
                        break;
                }
                break;
        }
    }

    private void ApplyTwoPairBuff(CardSuit suit1, CardSuit suit2)
    {
        Debug.Log($"Applying two pair bonus for {suit1} and {suit2}");

        if ((suit1 == CardSuit.Bones && suit2 == CardSuit.RottenFlesh) || (suit2 == CardSuit.Bones && suit1 == CardSuit.RottenFlesh))
        {
            //Add something for this combo
        }
        else if ((suit1 == CardSuit.Blood && suit2 == CardSuit.Brains) || (suit2 == CardSuit.Blood && suit1 == CardSuit.Brains))
        {
            //Add something for this combo
        }
        else if ((suit1 == CardSuit.Bones && suit2 == CardSuit.Brains) || (suit2 == CardSuit.Bones && suit1 == CardSuit.Brains))
        {
            //Add something for this combo
        }
        else if ((suit1 == CardSuit.Blood && suit2 == CardSuit.RottenFlesh) || (suit2 == CardSuit.Blood && suit1 == CardSuit.RottenFlesh))
        {
            //Add something for this combo
        }
    }

    private void ApplyFullHouseBuff(CardSuit threeSuit, CardSuit pairSuit)
    {
        Debug.Log($"Applying full house buff for {threeSuit} (3) and {pairSuit} (2)");

        if (threeSuit == CardSuit.Brains && pairSuit == CardSuit.Bones)
        {
            //Add something for this combo
        }
        else if (threeSuit == CardSuit.Blood && pairSuit == CardSuit.RottenFlesh)
        {
            //Add something for this combo
        }
        else if (threeSuit == CardSuit.Brains && pairSuit == CardSuit.RottenFlesh)
        {
            //Add something for this combo
        }
        else if (threeSuit == CardSuit.Brains && pairSuit == CardSuit.Blood)
        {
            //Add something for this combo
        }
        else if (threeSuit == CardSuit.Bones && pairSuit == CardSuit.RottenFlesh)
        {
            //Add something for this combo
        }
        else if (threeSuit == CardSuit.Bones && pairSuit == CardSuit.Brains)
        {
            //Add something for this combo
        }
        else if (threeSuit == CardSuit.Bones && pairSuit == CardSuit.Blood)
        {
            //Add something for this combo
        }
        else if (threeSuit == CardSuit.Blood && pairSuit == CardSuit.Brains)
        {
            //Add something for this combo
        }
        else if (threeSuit == CardSuit.Blood && pairSuit == CardSuit.Bones)
        {
            //Add something for this combo
        }
        else if (threeSuit == CardSuit.RottenFlesh && pairSuit == CardSuit.Brains)
        {
            //Add something for this combo
        }
        else if (threeSuit == CardSuit.RottenFlesh && pairSuit == CardSuit.Blood)
        {
            //Add something for this combo
        }
        else if (threeSuit == CardSuit.RottenFlesh && pairSuit == CardSuit.Bones)
        {
            //Add something for this combo
        }
    }

    private void ApplyGeneralBuff()
    {
        Debug.Log("General Buff: +10% movement speed and +5% weapon damage.");
    }
}
