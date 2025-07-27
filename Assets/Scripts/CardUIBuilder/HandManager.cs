using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardData;

public class HandManager : MonoBehaviour
{
    public PokerHandManager pokerHandManager;
    public DeckManager deckManager;
    public GameObject cardPrefab; //Assign card prefab in inspector
    public Transform handTransform; //Root of the hand position
    public float fanSpread = 5f;
    public float cardSpacing = 100f;
    public float verticalSpacing = 100f;
    private GameObject[] cardsInHand = new GameObject[5];
    public RectTransform[] cardSlots = new RectTransform[5];
    //Hold a list of the card objects in our hand
    

    public void AddCardToHand(CardInformation cardData)
    {
        for (int i = 0; i < cardsInHand.Length; i++)
        {
            if (cardsInHand[i] == null)
            {
                GameObject newCard = Instantiate(cardPrefab);

// Parent it correctly without messing up world position
                newCard.transform.SetParent(cardSlots[i], false);

// Now stretch to fill the slot
                RectTransform rt = newCard.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.localScale = Vector3.one; // reset any scale issues

                cardsInHand[i] = newCard;


                // Set the card data for CardDisplay
                CardDisplay display = newCard.GetComponent<CardDisplay>();
                display.cardData = cardData;
                display.UpdatecardDisplay();

                // Set the card data for CardAbilityUI
                CardAbilityUI ui = newCard.GetComponent<CardAbilityUI>();
                if (ui != null)
                    ui.SetCard(cardData);

                pokerHandManager?.EvaluateHandAndApplyBuffs();
                return;
            }
        }

        Debug.LogWarning("Hand is full! Cannot add more than 5 cards.");
    }
    
    private void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            deckManager.DrawCard(this);
        }
    }



    
    public void DiscardCard(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= cardsInHand.Length)
        {
            Debug.LogWarning("Invalid slot index.");
            return;
        }

        if (cardsInHand[cardIndex] == null)
        {
            Debug.Log("Slot " + cardIndex + " is already empty.");
            return;
        }

        // Safely clear the reference before destroying
        GameObject cardToRemove = cardsInHand[cardIndex];
        cardsInHand[cardIndex] = null; // Clear it first!

        Destroy(cardToRemove);

        pokerHandManager?.EvaluateHandAndApplyBuffs();
    }




    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) DiscardCard(0);
        if (Input.GetKeyDown(KeyCode.W)) DiscardCard(1);
        if (Input.GetKeyDown(KeyCode.E)) DiscardCard(2);
        if (Input.GetKeyDown(KeyCode.R)) DiscardCard(3);
        if (Input.GetKeyDown(KeyCode.T)) DiscardCard(4);
    }

    

    public List<CardInformation> GetCurrentHand()
    {
        List<CardInformation> handData = new List<CardInformation>();

        foreach (GameObject card in cardsInHand)
        {
            if (card != null)
            {
                CardDisplay display = card.GetComponent<CardDisplay>();
                if (display != null && display.cardData != null)
                {
                    handData.Add(display.cardData);
                }
            }
        }

        return handData;
    }

}