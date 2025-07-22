using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CardData;
using Game.Player;

public class PokerHandManager : MonoBehaviour
{
    public HandManager handManager;
    //public PlayerController player;

    private void Update()
    {
        EvaluateHandAndApplyBuffs();
    }

    public void EvaluateHandAndApplyBuffs()
    {
        ResetPlayerBuffs();

        List<CardInformation> currentHand = handManager.GetCurrentHand();
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
            Debug.Log("Four of a Kind (Suit): Damage Buff Applied");
        }
        else if (counts.Contains(3) && counts.Contains(2))
        {
            Debug.Log("Full House (Suit): Max Health Buff Applied");
        }
        else if (counts.Contains(3))
        {
            Debug.Log("Three of a Kind (Suit): Damage Buff Applied");
        }
        else if (counts.Count(c => c == 2) == 2)
        {
            Debug.Log("Two Pair (Suit): Max Health Buff Applied");
        }
        else if (counts.Contains(2))
        {
            Debug.Log("Pair (Suit): Speed Buff Applied");
        }
        else
        {
            Debug.Log("No matching poker hand found.");
        }

        Debug.Log($"Evaluating hand with {currentHand.Count} cards.");
    }


    void ResetPlayerBuffs()
    {
        //player.MovementSpeedMultiplier = 1f;
        //player.DamageMultiplier = 1f;
        //player.MaxHealthMultiplier = 1f;
    }
}