using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardData;

public class DeckManager : MonoBehaviour
{
    public List<CardInformation> allCards = new List<CardInformation>();

    private int currentIndex = 0;

    void Start()
    {
        //Load all card assets
        CardInformation[] cards = Resources.LoadAll<CardInformation>("Cards");

        //Add the loaded cards to the allCards list
        allCards.AddRange(cards);
    }

    public void DrawCard(HandManager handManager)
    {
        if (allCards.Count == 0)
        {
            return;
        }

        CardInformation nextCard = allCards[currentIndex];
        handManager.AddCardToHand(nextCard);
        currentIndex = (currentIndex + 1) % allCards.Count;
    }
}