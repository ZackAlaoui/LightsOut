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

    public void DrawCard(HandManager handManager)
    {
        if (allCards.Count == 0) return;
    
        for (int attempts = 0; attempts < allCards.Count; attempts++)
        {
            CardInformation nextCard = allCards[currentIndex];
            int previousIndex = currentIndex;
    
            currentIndex = (currentIndex + 1) % allCards.Count;
    
            bool success = handManager.TryAddCardToHand(nextCard);
            if (success) break;
        }
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