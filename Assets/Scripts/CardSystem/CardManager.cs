using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;

    public List<Card> allCards = new();
    public List<Card> playerHand = new();

    public int startingHandSize = 5;

    private Dictionary<string, float> activeBuffs = new();

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
        EvaluatePokerHand(); // Evaluate initial hand
    }

    public void UseCard(int cardIndex)
    {
        if (cardIndex >= 0 && cardIndex < playerHand.Count)
        {
            playerHand[cardIndex].ActivateCard();

            if (playerHand[cardIndex].usageType == CardUsageType.SingleUse)
            {
                playerHand.RemoveAt(cardIndex);
                EvaluatePokerHand(); // Re-evaluate after card removal
            }
        }
    }

    public void AddCardToHand(Card newCard)
    {
        if (playerHand.Count < 5)
        {
            playerHand.Add(newCard);
            EvaluatePokerHand(); // Re-evaluate with new card
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

        // Check Flush (5 of same suit)
        foreach (var suit in suitCounts)
        {
            if (suit.Value == 5)
            {
                Debug.Log($"Flush of {suit.Key}: Ultimate buff!");
                BuffEffectsManager.Instance?.PlayPokerHandEffect("flush", suit.Key);
                ApplySuitBuff(suit.Key, "flush");
                return;
            }
        }

        // Check Four of a Kind
        foreach (var suit in suitCounts)
        {
            if (suit.Value == 4)
            {
                Debug.Log($"Four of a Kind ({suit.Key}): Major buff!");
                BuffEffectsManager.Instance?.PlayPokerHandEffect("four", suit.Key);
                ApplySuitBuff(suit.Key, "four");
                return;
            }
        }

        // Check pairs and three of a kind
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
        }

        // Check Full House (3 of one suit + 2 of another)
        if (hasThree && pairs >= 1)
        {
            Debug.Log($"Full House: {threeSuit} + {firstPairSuit}");
            BuffEffectsManager.Instance?.PlayPokerHandEffect("fullhouse", threeSuit, firstPairSuit);
            ApplyFullHouseBuff(threeSuit, firstPairSuit);
            return;
        }

        // Check Two Pair
        if (pairs == 2)
        {
            Debug.Log($"Two Pair: {firstPairSuit} and {secondPairSuit}");
            BuffEffectsManager.Instance?.PlayPokerHandEffect("twopair", firstPairSuit, secondPairSuit);
            ApplyTwoPairBuff(firstPairSuit, secondPairSuit);
            return;
        }

        // Check One Pair
        if (pairs == 1)
        {
            Debug.Log("One Pair: General minor buff.");
            BuffEffectsManager.Instance?.PlayPokerHandEffect("pair", firstPairSuit);
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
                        AddBuff("UltimateCooldownReduction", 20f, suit);
                        AddBuff("StrongerLightBeam", 20f, suit);
                        break;
                    case CardSuit.Bones:
                        AddBuff("ArmorUp", 20f, suit);
                        AddBuff("KnockbackResistance", 20f, suit);
                        break;
                    case CardSuit.Blood:
                        AddBuff("Lifesteal", 20f, suit);
                        AddBuff("BleedOnHit", 20f, suit);
                        break;
                    case CardSuit.RottenFlesh:
                        AddBuff("ToxicAura", 20f, suit);
                        AddBuff("InfectionSpread", 20f, suit);
                        break;
                }
                break;

            case "four":
                Debug.Log($"Four of a Kind {suit}: Large bonus to effect specific to {suit}");
                switch (suit)
                {
                    case CardSuit.Brains:
                        AddBuff("CooldownReduction", 15f, suit);
                        AddBuff("EnergyRegen", 15f, suit);
                        break;
                    case CardSuit.Bones:
                        AddBuff("HealthRegen", 15f, suit);
                        AddBuff("ShieldBoost", 15f, suit);
                        break;
                    case CardSuit.Blood:
                        AddBuff("AttackSpeed", 15f, suit);
                        AddBuff("DamageBoost", 15f, suit);
                        break;
                    case CardSuit.RottenFlesh:
                        AddBuff("PoisonOnHit", 15f, suit);
                        AddBuff("RotShield", 15f, suit);
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
            AddBuff("PoisonResistance", 12f, suit1);
            AddBuff("ArmorBoost", 12f, suit2);
        }
        else if ((suit1 == CardSuit.Blood && suit2 == CardSuit.Brains) || (suit2 == CardSuit.Blood && suit1 == CardSuit.Brains))
        {
            AddBuff("CooldownReduction", 12f, suit1);
            AddBuff("DamageBoost", 10f, suit2);
        }
        else if ((suit1 == CardSuit.Bones && suit2 == CardSuit.Brains) || (suit2 == CardSuit.Bones && suit1 == CardSuit.Brains))
        {
            AddBuff("CooldownReduction", 10f, suit1);
            AddBuff("HealthRegen", 10f, suit2);
        }
        else if ((suit1 == CardSuit.Blood && suit2 == CardSuit.RottenFlesh) || (suit2 == CardSuit.Blood && suit1 == CardSuit.RottenFlesh))
        {
            AddBuff("PoisonOnHit", 12f, suit1);
            AddBuff("MovementSpeedBoost", 10f, suit2);
        }
    }

    private void ApplyFullHouseBuff(CardSuit threeSuit, CardSuit pairSuit)
    {
        Debug.Log($"Applying full house buff for {threeSuit} (3) and {pairSuit} (2)");

        if (threeSuit == CardSuit.Brains && pairSuit == CardSuit.Bones)
        {
            AddBuff("CooldownReduction", 20f, threeSuit);
            AddBuff("ArmorBoost", 20f, pairSuit);
        }
        else if (threeSuit == CardSuit.Blood && pairSuit == CardSuit.RottenFlesh)
        {
            AddBuff("Lifesteal", 20f, threeSuit);
            AddBuff("PoisonOnHit", 20f, pairSuit);
        }
        else if (threeSuit == CardSuit.Brains && pairSuit == CardSuit.RottenFlesh)
        {
            AddBuff("TrapSense", 25f, threeSuit);
            AddBuff("DecayAura", 20f, pairSuit);
        }
        else if (threeSuit == CardSuit.Brains && pairSuit == CardSuit.Blood)
        {
            AddBuff("CriticalChanceUp", 20f, threeSuit);
            AddBuff("CooldownReduction", 20f, pairSuit);
        }
        else if (threeSuit == CardSuit.Bones && pairSuit == CardSuit.RottenFlesh)
        {
            AddBuff("HealthRegen", 20f, threeSuit);
            AddBuff("PoisonResistance", 20f, pairSuit);
        }
        else if (threeSuit == CardSuit.Bones && pairSuit == CardSuit.Brains)
        {
            AddBuff("ShieldBoost", 25f, threeSuit);
            AddBuff("CooldownReduction", 15f, pairSuit);
        }
        else if (threeSuit == CardSuit.Bones && pairSuit == CardSuit.Blood)
        {
            AddBuff("BerserkerMode", 20f, threeSuit);
            AddBuff("ArmorBoost", 20f, pairSuit);
        }
        else if (threeSuit == CardSuit.Blood && pairSuit == CardSuit.Brains)
        {
            AddBuff("Executioner", 20f, threeSuit);
            AddBuff("CooldownReduction", 15f, pairSuit);
        }
        else if (threeSuit == CardSuit.Blood && pairSuit == CardSuit.Bones)
        {
            AddBuff("DamageBoost", 20f, threeSuit);
            AddBuff("StaggerResistance", 20f, pairSuit);
        }
        else if (threeSuit == CardSuit.RottenFlesh && pairSuit == CardSuit.Brains)
        {
            AddBuff("ConfuseEnemies", 15f, threeSuit);
            AddBuff("TrapSense", 15f, pairSuit);
        }
        else if (threeSuit == CardSuit.RottenFlesh && pairSuit == CardSuit.Blood)
        {
            AddBuff("PoisonOnHit", 25f, threeSuit);
            AddBuff("DamageBoost", 15f, pairSuit);
        }
        else if (threeSuit == CardSuit.RottenFlesh && pairSuit == CardSuit.Bones)
        {
            AddBuff("DecayAura", 20f, threeSuit);
            AddBuff("ArmorBoost", 15f, pairSuit);
        }
    }

    private void ApplyGeneralBuff()
    {
        Debug.Log("General Buff: +10% movement speed and +5% weapon damage.");
        AddBuff("MovementSpeedBoost", 8f, CardSuit.Brains);
        AddBuff("DamageBoost", 8f, CardSuit.Blood);
    }

    public void AddBuff(string buffName, float duration, CardSuit primarySuit)
    {
        if (activeBuffs.ContainsKey(buffName))
            activeBuffs[buffName] = Mathf.Max(activeBuffs[buffName], duration);
        else
            activeBuffs.Add(buffName, duration);

        Debug.Log($"Buff Applied: {buffName} for {duration} seconds.");
        
        // Trigger visual effects
        BuffEffectsManager.Instance?.ActivateBuffEffect(buffName, duration, primarySuit);
    }

    private void Update()
    {
        List<string> expired = new();

        foreach (var buff in activeBuffs.Keys)
        {
            activeBuffs[buff] -= Time.deltaTime;
            if (activeBuffs[buff] <= 0)
                expired.Add(buff);
        }

        foreach (var buff in expired)
        {
            activeBuffs.Remove(buff);
            Debug.Log($"Buff expired: {buff}");
            BuffEffectsManager.Instance?.DeactivateBuffEffect(buff);
        }
    }

    // Public getter for other systems to check active buffs
    public bool HasActiveBuff(string buffName)
    {
        return activeBuffs.ContainsKey(buffName);
    }

    public float GetBuffRemainingTime(string buffName)
    {
        return activeBuffs.ContainsKey(buffName) ? activeBuffs[buffName] : 0f;
    }
}