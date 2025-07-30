using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardData;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;
    public List<CardInformation> allCards = new List<CardInformation>();

    private int currentIndex = 0;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
 
 
    void Start()
    {
        //Load all card assets
        CardInformation[] cards = Resources.LoadAll<CardInformation>("Cards");

        // --- ADD THIS LINE ---
        Debug.Log($"Found {cards.Length} cards in Resources/Cards folder.");

        //Add the loaded cards to the allCards list
        allCards.AddRange(cards);
        ShuffleDeck();
    }

    public static void DrawCard(HandManager handManager)
    {
        if (Instance.allCards.Count == 0)
        {
            return;
        }

        CardInformation nextCard = Instance.allCards[Instance.currentIndex];
        HandManager.AddCardToHand(nextCard);
        Instance.currentIndex = (Instance.currentIndex + 1) % Instance.allCards.Count;
    }

    private void ShuffleDeck()
    {
        for (int i = 0; i < allCards.Count; i++)
        {
            int randomIndex = Random.Range(i, allCards.Count);
            CardInformation temp = allCards[i];
            allCards[i] = allCards[randomIndex];
            allCards[randomIndex] = temp;
        }
        
        currentIndex = 0;
    }
}