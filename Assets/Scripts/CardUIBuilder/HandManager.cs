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
    public List<GameObject> cardsInHand = new List<GameObject>(); //Hold a list of the card objects in our hand
    

    public void AddCardToHand(CardInformation cardData)
    {

        if (cardsInHand.Count >= 5)
        {
            Debug.LogWarning("Hand is full! Cannot add more than 5 cards.");
            return;
        }
        //Instantiate the card
        GameObject newCard = Instantiate(cardPrefab, handTransform.position, Quaternion.identity, handTransform);
        cardsInHand.Add(newCard);

        //Set the cardData of the Instantiated card
        newCard.GetComponent<CardDisplay>().cardData = cardData;

        pokerHandManager?.EvaluateHandAndApplyBuffs();
        
        // After adding the card
        int index = cardsInHand.Count - 1; // index of the newly added card
        CardDisplay cardDisplay = newCard.GetComponent<CardDisplay>();
        newCard.GetComponent<CardDisplay>().UpdatecardDisplay();
        
        UpdateHandVisuals();
    }
    
    public void DiscardCard(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= cardsInHand.Count)
        {
            Debug.LogWarning("Invalid card index to discard.");
            return;
        }

        GameObject cardToRemove = cardsInHand[cardIndex];
        cardsInHand.RemoveAt(cardIndex);
        Destroy(cardToRemove);

        UpdateHandVisuals();

        pokerHandManager?.EvaluateHandAndApplyBuffs();

        // Draw a new card
        deckManager.DrawCard(this);
    }


    void Update()
    {
        UpdateHandVisuals();

        if (Input.GetKeyDown(KeyCode.Q)) // Example: discard card at index 0
        {
            DiscardCard(0);
        }
    }


    private void UpdateHandVisuals()
    {
        int cardCount = cardsInHand.Count;

        if (cardCount == 1)
        {
            cardsInHand[0].transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            cardsInHand[0].transform.localPosition = new Vector3(0f, 0f, 0f);
            return;
        }

        for (int i = 0; i < cardCount; i++)
        {
            float rotationAngle = (fanSpread * (i - (cardCount - 1) / 2f));
            cardsInHand[i].transform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);

            float horizontalOffset = (cardSpacing * (i - (cardCount - 1) / 2f));

            float normalizedPosition = (2f * i / (cardCount - 1) - 1f); //Normalize card position between -1 and 1
            float verticalOffset = verticalSpacing * (1 - normalizedPosition * normalizedPosition);

            //Set card position
            cardsInHand[i].transform.localPosition = new Vector3(horizontalOffset, verticalOffset, 0f);
        }
    }

    public List<CardInformation> GetCurrentHand()
    {
        List<CardInformation> handData = new List<CardInformation>();

        foreach (GameObject card in cardsInHand)
        {
            CardDisplay cardDisplay = card.GetComponent<CardDisplay>();
            if (cardDisplay != null && cardDisplay.cardData != null)
            {
                handData.Add(cardDisplay.cardData);
            }
        }
        
        return handData;
    }
}