using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CardData;
using Game.Player;

public class PokerHandManager : MonoBehaviour
{
    private PlayerController player;
    public string CurrentPokerHandDescription { get; private set; } = "No poker hand active.";


    private void Start()
    {
        player = PlayerController.Instance;
        if (player == null)
        {
            Debug.LogError("PlayerController not found in scene!");
        }
    }

    public void EvaluateHandAndApplyBuffs()
    {
        if (player == null) return;

        ResetPlayerBuffs();
        CurrentPokerHandDescription = "No poker hand active."; // default

        List<CardInformation> currentHand = HandManager.GetCurrentHand();
        if (currentHand == null || currentHand.Count == 0) return;

        Dictionary<CardInformation.CardType, int> suitCounts = new Dictionary<CardInformation.CardType, int>();

        foreach (var card in currentHand)
        {
            if (card.cardType == null || card.cardType.Count == 0)
                continue;

            CardInformation.CardType primarySuit = card.cardType[0];

            if (!suitCounts.ContainsKey(primarySuit))
                suitCounts[primarySuit] = 1;
            else
                suitCounts[primarySuit]++;
        }

        var counts = suitCounts.Values.ToList();

        if (counts.Contains(4))
        {
            player.DamageMultiplier = 2f;
            CurrentPokerHandDescription = "Four of a Kind: +100% Damage";
        }
        else if (counts.Contains(3) && counts.Contains(2))
        {
            player.MaxHealthMultiplier = 2f;
            CurrentPokerHandDescription = "Full House: +100% Max Health";
        }
        else if (counts.Contains(3))
        {
            player.DamageMultiplier = 1.5f;
            CurrentPokerHandDescription = "Three of a Kind: +50% Damage";
        }
        else if (counts.Count(c => c == 2) == 2)
        {
            player.MaxHealthMultiplier = 1.5f;
            CurrentPokerHandDescription = "Two Pair: +50% Max Health";
        }
        else if (counts.Contains(2))
        {
            player.MovementSpeedMultiplier = 1.5f;
            CurrentPokerHandDescription = "One Pair: +50% Movement Speed";
        }
    }


    void ResetPlayerBuffs()
    {
        player.MovementSpeedMultiplier = 1f;
        player.DamageMultiplier = 1f;
        player.MaxHealthMultiplier = 1f;
    }
}
