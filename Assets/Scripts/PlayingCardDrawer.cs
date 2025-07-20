using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum PlayingCardSuit { Clubs, Hearts, Diamonds, Spades }

[System.Serializable]
public class PlayingCard
{
    public int value; // 1-13 (1=Ace, 11=Jack, 12=Queen, 13=King)
    public PlayingCardSuit suit;
    public Sprite cardSprite;
    
    public string GetCardName()
    {
        string valueName = value switch
        {
            1 => "Ace",
            11 => "Jack",
            12 => "Queen", 
            13 => "King",
            _ => value.ToString()
        };
        return $"{valueName} of {suit}";
    }
}

public class PlayingCardDrawer : MonoBehaviour
{
    [Header("Card Display")]
    public Image[] cardSlots = new Image[3]; // 3 card slots for drawing
    public Button drawButton;
    public Sprite cardBackSprite;
    
    [Header("Auto-Load Settings")]
    [Tooltip("Path to the Standard Cards folder")]
    public string cardsBasePath = "Assets/2D Cards Game Art Pack/Sprites/Standard 52 Cards/Standard Cards";
    
    [Header("Debug Info")]
    [SerializeField] private int loadedCardsCount = 0;
    
    private List<PlayingCard> allCards = new List<PlayingCard>();
    private List<PlayingCard> currentDrawnCards = new List<PlayingCard>();
    
    private void Start()
    {
        LoadAllCards();
        InitializeCardSlots();
        
        if (drawButton != null)
            drawButton.onClick.AddListener(DrawRandomCards);
            
        DrawRandomCards(); // Draw initial cards
    }
    
    private void LoadAllCards()
    {
        allCards.Clear();
        
#if UNITY_EDITOR
        LoadCardsFromAssetDatabase();
#else
        Debug.LogWarning("Auto-loading only works in editor. Please assign sprites manually for builds.");
#endif
        
        loadedCardsCount = allCards.Count;
        Debug.Log($"Loaded {allCards.Count} cards successfully!");
    }
    
#if UNITY_EDITOR
    private void LoadCardsFromAssetDatabase()
    {
        string[] suitFolders = { "Clubs", "Hearts", "Diamonds", "Spades" };
        string[] suitSuffixes = { "club", "heart", "diamond", "spade" };
        PlayingCardSuit[] cardSuits = { PlayingCardSuit.Clubs, PlayingCardSuit.Hearts, PlayingCardSuit.Diamonds, PlayingCardSuit.Spades };
        
        for (int suitIndex = 0; suitIndex < suitFolders.Length; suitIndex++)
        {
            string suitFolderPath = $"{cardsBasePath}/{suitFolders[suitIndex]}";
            
            for (int value = 1; value <= 13; value++)
            {
                string cardFileName = $"{value}{suitSuffixes[suitIndex]}.png";
                string fullPath = $"{suitFolderPath}/{cardFileName}";
                
                Sprite cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
                
                if (cardSprite != null)
                {
                    PlayingCard card = new PlayingCard
                    {
                        value = value,
                        suit = cardSuits[suitIndex],
                        cardSprite = cardSprite
                    };
                    allCards.Add(card);
                    Debug.Log($"Loaded: {card.GetCardName()} from {fullPath}");
                }
                else
                {
                    Debug.LogWarning($"Could not load card sprite at path: {fullPath}");
                }
            }
        }
        
        // Load card back sprite automatically
        if (cardBackSprite == null)
        {
            string cardBackPath = $"{cardsBasePath}/Card Back/card_back_rect_1.png";
            cardBackSprite = AssetDatabase.LoadAssetAtPath<Sprite>(cardBackPath);
            
            if (cardBackSprite != null)
            {
                Debug.Log($"Loaded card back from: {cardBackPath}");
            }
            else
            {
                Debug.LogWarning($"Could not load card back from: {cardBackPath}");
            }
        }
    }
#endif
    
    private void InitializeCardSlots()
    {
        // If card slots aren't assigned, try to find them automatically
        if (cardSlots[0] == null)
        {
            Image[] foundImages = FindObjectsOfType<Image>();
            int slotIndex = 0;
            
            foreach (Image img in foundImages)
            {
                if (img.name.ToLower().Contains("card") && slotIndex < cardSlots.Length)
                {
                    cardSlots[slotIndex] = img;
                    slotIndex++;
                }
            }
        }
        
        // Initialize all slots with card backs
        foreach (Image slot in cardSlots)
        {
            if (slot != null && cardBackSprite != null)
            {
                slot.sprite = cardBackSprite;
            }
        }
    }
    
    public void DrawRandomCards()
    {
        if (allCards.Count == 0)
        {
            Debug.LogWarning("No cards loaded! Check the cards base path and make sure sprites exist.");
            return;
        }
        
        currentDrawnCards.Clear();
        
        // Draw 3 random cards
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i] != null)
            {
                PlayingCard randomCard = GetRandomCard();
                currentDrawnCards.Add(randomCard);
                
                // Display the card
                cardSlots[i].sprite = randomCard.cardSprite;
                
                Debug.Log($"Drew: {randomCard.GetCardName()}");
            }
        }
    }
    
    private PlayingCard GetRandomCard()
    {
        int randomIndex = Random.Range(0, allCards.Count);
        return allCards[randomIndex];
    }
    
    public void SelectCard(int cardIndex)
    {
        if (cardIndex >= 0 && cardIndex < currentDrawnCards.Count)
        {
            PlayingCard selectedCard = currentDrawnCards[cardIndex];
            Debug.Log($"Selected card: {selectedCard.GetCardName()}");
            
            // Add your card selection logic here
            OnCardSelected(selectedCard);
        }
    }
    
    private void OnCardSelected(PlayingCard card)
    {
        // This is where you can add what happens when a player selects a card
        Debug.Log($"Player selected: {card.GetCardName()}");
        
        // Example: Hide other cards, apply card effect, etc.
        // You can customize this based on your game's needs
    }
    
    public void ResetCards()
    {
        // Reset all card slots to show card backs
        foreach (Image slot in cardSlots)
        {
            if (slot != null && cardBackSprite != null)
            {
                slot.sprite = cardBackSprite;
            }
        }
        currentDrawnCards.Clear();
    }
    
    // Utility method to get a card by value and suit
    public PlayingCard GetSpecificCard(int value, PlayingCardSuit suit)
    {
        return allCards.Find(card => card.value == value && card.suit == suit);
    }
    
    // Utility method to get all cards of a specific suit
    public List<PlayingCard> GetCardsBySuit(PlayingCardSuit suit)
    {
        return allCards.FindAll(card => card.suit == suit);
    }
    
    // Method to add click listeners to card slots
    public void SetupCardClickListeners()
    {
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i] != null)
            {
                Button cardButton = cardSlots[i].GetComponent<Button>();
                if (cardButton == null)
                    cardButton = cardSlots[i].gameObject.AddComponent<Button>();
                
                int cardIndex = i; // Capture the index for the closure
                cardButton.onClick.RemoveAllListeners();
                cardButton.onClick.AddListener(() => SelectCard(cardIndex));
            }
        }
    }
    
    // Editor-only method to refresh card loading
#if UNITY_EDITOR
    [ContextMenu("Reload Cards")]
    public void ReloadCards()
    {
        LoadAllCards();
        Debug.Log($"Reloaded {allCards.Count} cards from {cardsBasePath}");
    }
#endif
} 