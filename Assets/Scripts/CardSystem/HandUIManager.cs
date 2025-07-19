using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject cardUIPrefab;
    public Transform cardContainer;
    public HorizontalLayoutGroup layoutGroup;
    
    [Header("Card Animation")]
    public float cardSpacing = 10f;
    public float animationDuration = 0.5f;
    
    private List<GameObject> cardUIObjects = new List<GameObject>();
    
    private void Start()
    {
        // Subscribe to card manager events if needed
        if (CardManager.Instance != null)
        {
            // Update hand display when starting
            Invoke("UpdateHandDisplay", 0.1f);
        }
    }
    
    public void UpdateHandDisplay()
    {
        // Clear existing cards
        ClearHandDisplay();
        
        if (CardManager.Instance == null) return;
        
        // Create UI for each card in hand
        for (int i = 0; i < CardManager.Instance.playerHand.Count; i++)
        {
            CreateCardUI(CardManager.Instance.playerHand[i], i);
        }
        
        // Update layout
        if (layoutGroup != null)
            layoutGroup.spacing = cardSpacing;
    }
    
    private void CreateCardUI(Card card, int index)
    {
        if (cardUIPrefab == null || cardContainer == null) return;
        
        GameObject cardObj = Instantiate(cardUIPrefab, cardContainer);
        CardUI cardUI = cardObj.GetComponent<CardUI>();
        
        if (cardUI != null)
        {
            cardUI.SetupCard(card, index);
        }
        
        cardUIObjects.Add(cardObj);
    }
    
    private void ClearHandDisplay()
    {
        foreach (GameObject cardObj in cardUIObjects)
        {
            if (cardObj != null)
                DestroyImmediate(cardObj);
        }
        cardUIObjects.Clear();
    }
    
    // Call this when a card is used or hand changes
    public void RefreshHandDisplay()
    {
        UpdateHandDisplay();
    }
    
    // Method to highlight cards of specific suits (for combo visualization)
    public void HighlightSuitCards(CardSuit suit)
    {
        foreach (GameObject cardObj in cardUIObjects)
        {
            CardUI cardUI = cardObj.GetComponent<CardUI>();
            if (cardUI != null)
            {
                // You can implement highlighting logic here
            }
        }
    }
}