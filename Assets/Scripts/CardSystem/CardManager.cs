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
                        AddBuff("UltimateCooldownReduction", 20f);
                        AddBuff("StrongerLightBeam", 20f);  // Smarter = faster leveling
                        break;
                    case CardSuit.Bones:
                        AddBuff("ArmorUp", 20f);
                        AddBuff("KnockbackResistance", 20f);
                        break;
                    case CardSuit.Blood:
                        AddBuff("Lifesteal", 20f);
                        AddBuff("BleedOnHit", 20f);
                        break;
                    case CardSuit.RottenFlesh:
                        AddBuff("ToxicAura", 20f);
                        AddBuff("InfectionSpread", 20f);
                        break;
                }
                break;

            case "four":
                Debug.Log($"Four of a Kind {suit}: Large bonus to effect specific to {suit}");
                switch (suit)
                {
                    case CardSuit.Brains:
                        AddBuff("CooldownReduction", 15f);
                        AddBuff("EnergyRegen", 15f);
                        break;
                    case CardSuit.Bones:
                        AddBuff("HealthRegen", 15f);
                        AddBuff("ShieldBoost", 15f);
                        break;
                    case CardSuit.Blood:
                        AddBuff("AttackSpeed", 15f);
                        AddBuff("DamageBoost", 15f);
                        break;
                    case CardSuit.RottenFlesh:
                        AddBuff("PoisonOnHit", 15f);
                        AddBuff("RotShield", 15f); // absorbs damage and poisons attackers
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
            AddBuff("PoisonResistance", 12f);   // Rot synergy
            AddBuff("ArmorBoost", 12f);         // Tanky support
        }
        else if ((suit1 == CardSuit.Blood && suit2 == CardSuit.Brains) || (suit2 == CardSuit.Blood && suit1 == CardSuit.Brains))
        {
            AddBuff("CooldownReduction", 12f);  // Tactical offense
            AddBuff("DamageBoost", 10f);
        }
        else if ((suit1 == CardSuit.Bones && suit2 == CardSuit.Brains) || (suit2 == CardSuit.Bones && suit1 == CardSuit.Brains))
        {
            AddBuff("CooldownReduction", 10f);  // Defensive utility
            AddBuff("HealthRegen", 10f);
        }
        else if ((suit1 == CardSuit.Blood && suit2 == CardSuit.RottenFlesh) || (suit2 == CardSuit.Blood && suit1 == CardSuit.RottenFlesh))
        {
            AddBuff("PoisonOnHit", 12f);        // Damage-over-time
            AddBuff("MovementSpeedBoost", 10f); // Chase-down synergy
        }
    }


    private void ApplyFullHouseBuff(CardSuit threeSuit, CardSuit pairSuit)
    {
        Debug.Log($"Applying full house buff for {threeSuit} (3) and {pairSuit} (2)");

        if (threeSuit == CardSuit.Brains && pairSuit == CardSuit.Bones)
        {
            AddBuff("CooldownReduction", 20f);     // Faster ability use
            AddBuff("ArmorBoost", 20f);             // Defensive synergy
        }
        else if (threeSuit == CardSuit.Blood && pairSuit == CardSuit.RottenFlesh)
        {
            AddBuff("Lifesteal", 20f);              // Aggressive + decay synergy
            AddBuff("PoisonOnHit", 20f);            // Adds DoT to attacks
        }
        else if (threeSuit == CardSuit.Brains && pairSuit == CardSuit.RottenFlesh)
        {
            AddBuff("TrapSense", 25f);              // Maybe increases flashlight radius or visibility
            AddBuff("DecayAura", 20f);              // Nearby enemies take passive damage
        }
        else if (threeSuit == CardSuit.Brains && pairSuit == CardSuit.Blood)
        {
            AddBuff("CriticalChanceUp", 20f);       // Tactical aggression synergy
            AddBuff("CooldownReduction", 20f);
        }
        else if (threeSuit == CardSuit.Bones && pairSuit == CardSuit.RottenFlesh)
        {
            AddBuff("HealthRegen", 20f);            // Tanky sustain synergy
            AddBuff("PoisonResistance", 20f);
        }
        else if (threeSuit == CardSuit.Bones && pairSuit == CardSuit.Brains)
        {
            AddBuff("ShieldBoost", 25f);            // More defensive synergy
            AddBuff("CooldownReduction", 15f);
        }
        else if (threeSuit == CardSuit.Bones && pairSuit == CardSuit.Blood)
        {
            AddBuff("BerserkerMode", 20f);          // Boost dmg when under 50% health
            AddBuff("ArmorBoost", 20f);
        }
        else if (threeSuit == CardSuit.Blood && pairSuit == CardSuit.Brains)
        {
            AddBuff("Executioner", 20f);            // Bonus damage to low-health enemies
            AddBuff("CooldownReduction", 15f);
        }
        else if (threeSuit == CardSuit.Blood && pairSuit == CardSuit.Bones)
        {
            AddBuff("DamageBoost", 20f);
            AddBuff("StaggerResistance", 20f);
        }
        else if (threeSuit == CardSuit.RottenFlesh && pairSuit == CardSuit.Brains)
        {
            AddBuff("ConfuseEnemies", 15f);         // Chance for enemies to attack each other
            AddBuff("TrapSense", 15f);
        }
        else if (threeSuit == CardSuit.RottenFlesh && pairSuit == CardSuit.Blood)
        {
            AddBuff("PoisonOnHit", 25f);
            AddBuff("DamageBoost", 15f);
        }
        else if (threeSuit == CardSuit.RottenFlesh && pairSuit == CardSuit.Bones)
        {
            AddBuff("DecayAura", 20f);
            AddBuff("ArmorBoost", 15f);
        }
    }


    private void ApplyGeneralBuff()
    {
        Debug.Log("General Buff: +10% movement speed and +5% weapon damage.");
    }

    public void AddBuff(string buffName, float duration)
    {
        if (activeBuffs.ContainsKey(buffName))
            activeBuffs[buffName] = Mathf.Max(activeBuffs[buffName], duration);
        else
            activeBuffs.Add(buffName, duration);

        Debug.Log($"Buff Applied: {buffName} for {duration} seconds.");
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
        }
    }
}
