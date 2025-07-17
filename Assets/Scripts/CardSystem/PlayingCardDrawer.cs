using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

[System.Serializable]
public class CardSlot
{
    [Header("Core Components")]
    public Image cardImage;
    public Button cardButton;
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;
    
    [Header("Visual Effects")]
    public Image glowEffect;
    public Image shadowEffect;
    public Image borderEffect;
    public Image highlightEffect;
    
    [Header("Animation State")]
    public bool isHovered = false;
    public bool isSelected = false;
    public Vector3 originalScale;
    public Vector3 originalPosition;
    public float originalShadowDistance;
}

public class PlayingCardDrawer : MonoBehaviour
{
    [Header("Card Display")]
    public CardSlot[] cardSlots = new CardSlot[3];
    public Button drawButton;
    public Sprite cardBackSprite;
    
    [Header("Background")]
    public Image backgroundImage;
    [Tooltip("Drag your background sprite here")]
    public Sprite backgroundSprite;
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
    public Color backgroundOverlayColor = new Color(0f, 0f, 0f, 0.3f);

    [Header("Card Animation")]
    public float hoverScale = 1.05f;
    public float selectionScale = 1.1f;
    public float hoverLiftHeight = 20f;
    public float animationSpeed = 0.15f;
    public Color selectionGlowColor = new Color(1f, 0.8f, 0.2f, 0.7f);
    public Color hoverGlowColor = new Color(0.3f, 0.7f, 1f, 0.4f);
    public Color selectionBorderColor = new Color(1f, 0.8f, 0.2f, 1f);
    public float glowIntensity = 1.0f;
    public float borderThickness = 2f;
    
    [Header("Modern Button Styling")]
    [Header("Primary Button (Draw New Hand)")]
    public Color primaryButtonColor = new Color(0.2f, 0.6f, 0.9f, 1f);
    public Color primaryButtonHoverColor = new Color(0.3f, 0.7f, 1f, 1f);
    public Color primaryButtonPressedColor = new Color(0.15f, 0.5f, 0.8f, 1f);
    public string primaryButtonText = "Draw New Hand";
    public float primaryButtonCornerRadius = 15f;
    public float buttonWidth = 200f;
    public float buttonHeight = 50f;
    
    [Header("Secondary Button (Draw Card)")]
    public Color secondaryButtonColor = new Color(0.9f, 0.4f, 0.2f, 1f);
    public Color secondaryButtonHoverColor = new Color(1f, 0.5f, 0.3f, 1f);
    public Color secondaryButtonPressedColor = new Color(0.8f, 0.3f, 0.1f, 1f);
    public string secondaryButtonText = "Draw Selected Card";
    public float secondaryButtonCornerRadius = 15f;
    
    [Header("UI Layout")]
    public float cardSpacing = 120f;
    public float buttonSpacing = 20f;
    public float cardShadowDistance = 8f;
    public float buttonShadowDistance = 4f;
    public float cardScale = 1.25f;
    
    [Header("Draw Card Button")]
    public Button drawCardButton;
    public int maxCardsInHand = 5;
    
    [Header("Back Button")]
    public Button backButton;
    public Color backButtonColor = new Color(0.6f, 0.2f, 0.2f, 1f);
    public Color backButtonHoverColor = new Color(0.8f, 0.3f, 0.3f, 1f);
    public Color backButtonPressedColor = new Color(0.5f, 0.15f, 0.15f, 1f);
    public string backButtonText = "Back to Menu";
    public float backButtonCornerRadius = 15f;
    
    [Header("Hotbar System")]
    public GameObject hotbarContainer;
    public float hotbarHeight = 120f;
    public float hotbarCardScale = 0.8f;
    public float hotbarCardSpacing = 15f;
    public Color hotbarBackgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.9f);
    public Color hotbarBorderColor = new Color(0.3f, 0.3f, 0.4f, 0.8f);
    public List<PlayingCard> playerHand = new List<PlayingCard>();
    public int selectedHotbarCardIndex = -1;
    public bool isSwapMode = false;
    
    [Header("Auto-Load Settings")]
    [Tooltip("Path to the Standard Cards folder")]
    public string cardsBasePath = "Assets/2D Cards Game Art Pack/Sprites/Standard 52 Cards/Rect Cards";
    
    [Header("Manual Card Assignment (for builds)")]
    [Tooltip("Assign card sprites manually for builds")]
    public Sprite[] manualCardSprites = new Sprite[52];
    public Sprite manualCardBackSprite;
    
    [Header("Debug Info")]
    [SerializeField] private int loadedCardsCount = 0;
    
    private List<PlayingCard> allCards = new List<PlayingCard>();
    private List<PlayingCard> currentDrawnCards = new List<PlayingCard>();
    private int selectedCardIndex = -1;
    
    // Modern UI components
    private Canvas mainCanvas;
    private GameObject uiContainer;
    private GameObject backgroundContainer;
    
    private void Start()
    {
        LoadAllCards();
        CreateModernUI();
        SetupBackground();
        InitializeCardSlots();
        
        if (drawButton != null)
        {
            drawButton.onClick.AddListener(() => StartCoroutine(DrawCardsWithAnimation()));
            SetupModernButton(drawButton, primaryButtonColor, primaryButtonHoverColor, primaryButtonPressedColor);
        }
        
        if (drawCardButton != null)
        {
            drawCardButton.onClick.AddListener(() => StartCoroutine(DrawSingleCard()));
            SetupModernButton(drawCardButton, secondaryButtonColor, secondaryButtonHoverColor, secondaryButtonPressedColor);
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoToSampleScene);
            SetupModernButton(backButton, backButtonColor, backButtonHoverColor, backButtonPressedColor);
        }
            
        StartCoroutine(DrawCardsWithAnimation()); // Draw initial cards
    }
    
    private void CreateModernUI()
    {
        // Find or create Canvas with modern settings
        mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            GameObject canvasGO = new GameObject("Modern Card Canvas");
            mainCanvas = canvasGO.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 0;
            
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // Create EventSystem if needed
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<StandaloneInputModule>();
            }
        }
        
        // Create background container
        CreateBackgroundContainer();
        
        // Create main UI container
        CreateUIContainer();
        
        // Create card container with modern layout
        CreateCardContainer();
        
        // Create modern buttons
        CreateModernButtons();
        
        // Create modern hotbar
        CreateModernHotbar();
    }
    
    private void CreateBackgroundContainer()
    {
        backgroundContainer = new GameObject("Background Container");
        backgroundContainer.transform.SetParent(mainCanvas.transform);
        backgroundContainer.transform.SetAsFirstSibling();
        
        RectTransform bgRect = backgroundContainer.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Background image
        GameObject bgImageGO = new GameObject("Background Image");
        bgImageGO.transform.SetParent(backgroundContainer.transform);
        
        RectTransform bgImageRect = bgImageGO.AddComponent<RectTransform>();
        bgImageRect.anchorMin = Vector2.zero;
        bgImageRect.anchorMax = Vector2.one;
        bgImageRect.offsetMin = Vector2.zero;
        bgImageRect.offsetMax = Vector2.zero;
        
        backgroundImage = bgImageGO.AddComponent<Image>();
        backgroundImage.color = backgroundColor;
        
        // Background overlay for better contrast
        GameObject overlayGO = new GameObject("Background Overlay");
        overlayGO.transform.SetParent(backgroundContainer.transform);
        
        RectTransform overlayRect = overlayGO.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        
        Image overlayImage = overlayGO.AddComponent<Image>();
        overlayImage.color = backgroundOverlayColor;
    }
    
    private void CreateUIContainer()
    {
        uiContainer = new GameObject("UI Container");
        uiContainer.transform.SetParent(mainCanvas.transform);
        
        RectTransform containerRect = uiContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;
    }
    
    private void CreateCardContainer()
    {
        GameObject cardContainer = new GameObject("Card Container");
        cardContainer.transform.SetParent(uiContainer.transform);
        
        RectTransform cardContainerRect = cardContainer.AddComponent<RectTransform>();
        cardContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
        
        // Calculate container size based on card spacing and scale
        float cardWidth = 214 * cardScale;
        float containerWidth = (cardSlots.Length - 1) * (cardWidth + cardSpacing) + cardWidth;
        cardContainerRect.sizeDelta = new Vector2(containerWidth, 300);
        cardContainerRect.anchoredPosition = new Vector2(0, 50);
        
        Debug.Log($"Card container width: {containerWidth}, Card width: {cardWidth}, Spacing: {cardSpacing}, Total cards: {cardSlots.Length}");
        
        // Create card slots with modern styling
        for (int i = 0; i < cardSlots.Length; i++)
        {
            CreateModernCardSlot(i, cardContainer.transform);
        }
        
        // Setup all card slots with click and hover handlers
        for (int i = 0; i < cardSlots.Length; i++)
        {
            SetupCardSlot(i);
        }
    }
    
    private void CreateModernCardSlot(int index, Transform parent)
    {
        // Main card container
        GameObject cardGO = new GameObject($"Card{index + 1}");
        cardGO.transform.SetParent(parent);
        
        RectTransform rectTransform = cardGO.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(214 * cardScale, 227 * cardScale);
        float cardWidth = 214 * cardScale;
        // Calculate position to center the cards with extra spacing for edge cards
        float totalWidth = (cardSlots.Length - 1) * (cardWidth + cardSpacing);
        float startX = -totalWidth / 2f;
        float xPosition = startX + index * (cardWidth + cardSpacing);
        
        // Add extra spacing for leftmost and rightmost cards
        if (index == 0) // Leftmost card
        {
            xPosition -= cardSpacing * 0.5f; // Push further left
        }
        else if (index == cardSlots.Length - 1) // Rightmost card
        {
            xPosition += cardSpacing * 0.5f; // Push further right
        }
        
        rectTransform.anchoredPosition = new Vector2(xPosition, 0);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        Debug.Log($"Card {index} positioned at X: {xPosition}, CardWidth: {cardWidth}, Spacing: {cardSpacing}");
        
        // Shadow effect
        GameObject shadowGO = new GameObject("Shadow");
        shadowGO.transform.SetParent(cardGO.transform);
        shadowGO.transform.SetAsFirstSibling();
        
        RectTransform shadowRect = shadowGO.AddComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.offsetMin = new Vector2(cardShadowDistance, -cardShadowDistance);
        shadowRect.offsetMax = new Vector2(cardShadowDistance, -cardShadowDistance);
        
        Image shadowImage = shadowGO.AddComponent<Image>();
        shadowImage.sprite = cardBackSprite;
        shadowImage.color = new Color(0f, 0f, 0f, 0.3f);
        
        // Glow effect
        GameObject glowGO = new GameObject("Glow");
        glowGO.transform.SetParent(cardGO.transform);
        
        RectTransform glowRect = glowGO.AddComponent<RectTransform>();
        glowRect.anchorMin = Vector2.zero;
        glowRect.anchorMax = Vector2.one;
        glowRect.offsetMin = new Vector2(-8f, -8f);
        glowRect.offsetMax = new Vector2(8f, 8f);
        
        Image glowImage = glowGO.AddComponent<Image>();
        glowImage.sprite = cardBackSprite;
        glowImage.color = Color.clear;
        
        // Border effect for selection
        GameObject borderGO = new GameObject("Border");
        borderGO.transform.SetParent(cardGO.transform);
        
        RectTransform borderRect = borderGO.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-borderThickness, -borderThickness);
        borderRect.offsetMax = new Vector2(borderThickness, borderThickness);
        
        Image borderImage = borderGO.AddComponent<Image>();
        borderImage.sprite = cardBackSprite;
        borderImage.color = Color.clear;
        
        // Highlight effect for hover
        GameObject highlightGO = new GameObject("Highlight");
        highlightGO.transform.SetParent(cardGO.transform);
        
        RectTransform highlightRect = highlightGO.AddComponent<RectTransform>();
        highlightRect.anchorMin = Vector2.zero;
        highlightRect.anchorMax = Vector2.one;
        highlightRect.offsetMin = new Vector2(-2f, -2f);
        highlightRect.offsetMax = new Vector2(2f, 2f);
        
        Image highlightImage = highlightGO.AddComponent<Image>();
        highlightImage.sprite = cardBackSprite;
        highlightImage.color = Color.clear;
        
        // Card image (main card)
        Image cardImage = cardGO.AddComponent<Image>();
        cardImage.sprite = cardBackSprite;
        
        // Button (invisible, for interaction)
        Button button = cardGO.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        
        // Canvas Group for fading and effects
        CanvasGroup canvasGroup = cardGO.AddComponent<CanvasGroup>();
        
        // Setup card slot with modern effects
        cardSlots[index] = new CardSlot
        {
            cardImage = cardImage,
            cardButton = button,
            canvasGroup = canvasGroup,
            rectTransform = rectTransform,
            glowEffect = glowImage,
            shadowEffect = shadowImage,
            borderEffect = borderImage,
            highlightEffect = highlightImage,
            originalScale = Vector3.one,
            originalPosition = rectTransform.anchoredPosition,
            originalShadowDistance = cardShadowDistance
        };
        
        Debug.Log($"Created modern card slot {index}");
    }
    
    private void CreateModernButtons()
    {
        // Create button container positioned above hotbar
        GameObject buttonContainer = new GameObject("Button Container");
        buttonContainer.transform.SetParent(uiContainer.transform);
        
        RectTransform buttonContainerRect = buttonContainer.AddComponent<RectTransform>();
        buttonContainerRect.anchorMin = new Vector2(0.5f, 0f);
        buttonContainerRect.anchorMax = new Vector2(0.5f, 0f);
        buttonContainerRect.sizeDelta = new Vector2(buttonWidth + 20, 150);
        buttonContainerRect.anchoredPosition = new Vector2(0, hotbarHeight + 180);
        
        // Create primary button (Draw New Hand)
        if (drawButton == null)
        {
            drawButton = CreateModernButton(buttonContainer.transform, primaryButtonText, primaryButtonColor, 
                primaryButtonHoverColor, primaryButtonPressedColor, primaryButtonCornerRadius, 
                new Vector2(0, 30), new Vector2(buttonWidth, buttonHeight));
        }
        
        // Create secondary button (Draw Card)
        if (drawCardButton == null)
        {
            drawCardButton = CreateModernButton(buttonContainer.transform, secondaryButtonText, secondaryButtonColor,
                secondaryButtonHoverColor, secondaryButtonPressedColor, secondaryButtonCornerRadius,
                new Vector2(0, -30), new Vector2(buttonWidth, buttonHeight));
        }
        
        // Create back button
        if (backButton == null)
        {
            backButton = CreateModernButton(buttonContainer.transform, backButtonText, backButtonColor,
                backButtonHoverColor, backButtonPressedColor, backButtonCornerRadius,
                new Vector2(0, -90), new Vector2(buttonWidth, buttonHeight));
        }
    }
    
    private Button CreateModernButton(Transform parent, string text, Color normalColor, Color hoverColor, 
        Color pressedColor, float cornerRadius, Vector2 position, Vector2 size)
    {
        // Main button container
        GameObject buttonGO = new GameObject($"Modern{text.Replace(" ", "")}Button");
        buttonGO.transform.SetParent(parent);
        
        RectTransform rectTransform = buttonGO.AddComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = position;
        
        // Enhanced shadow effect
        GameObject shadowGO = new GameObject("Shadow");
        shadowGO.transform.SetParent(buttonGO.transform);
        shadowGO.transform.SetAsFirstSibling();
        
        RectTransform shadowRect = shadowGO.AddComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.offsetMin = new Vector2(6f, -6f);
        shadowRect.offsetMax = new Vector2(6f, -6f);
        
        Image shadowImage = shadowGO.AddComponent<Image>();
        shadowImage.color = new Color(0f, 0f, 0f, 0.4f);
        
        // Main button background with rounded corners
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = normalColor;
        
        // Add rounded corners using a more sophisticated approach
        var mask = buttonGO.AddComponent<Mask>();
        
        Button button = buttonGO.AddComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        
        // Setup button colors
        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = hoverColor;
        colors.pressedColor = pressedColor;
        colors.selectedColor = hoverColor;
        colors.fadeDuration = 0.15f;
        button.colors = colors;
        
        // Button text with consistent modern styling
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform);
        
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        Text buttonText = textGO.AddComponent<Text>();
        buttonText.text = text;
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 20; // Consistent font size
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.fontStyle = FontStyle.Bold;
        
        // Enhanced shadow for better readability
        var shadow = textGO.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
        shadow.effectDistance = new Vector2(3, -3);
        
        Debug.Log($"Created modern button: {text} with enhanced styling");
        return button;
    }
    
    private void CreateModernHotbar()
    {
        // Create hotbar container
        hotbarContainer = new GameObject("Hotbar Container");
        hotbarContainer.transform.SetParent(uiContainer.transform);
        
        RectTransform hotbarRect = hotbarContainer.AddComponent<RectTransform>();
        hotbarRect.anchorMin = new Vector2(0f, 0f);
        hotbarRect.anchorMax = new Vector2(1f, 0f);
        hotbarRect.sizeDelta = new Vector2(0, hotbarHeight);
        hotbarRect.anchoredPosition = new Vector2(0, hotbarHeight / 2f);
        
        // Hotbar background
        GameObject bgGO = new GameObject("Hotbar Background");
        bgGO.transform.SetParent(hotbarContainer.transform);
        
        RectTransform bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        Image bgImage = bgGO.AddComponent<Image>();
        bgImage.color = hotbarBackgroundColor;
        
        // Hotbar border
        GameObject borderGO = new GameObject("Hotbar Border");
        borderGO.transform.SetParent(hotbarContainer.transform);
        
        RectTransform borderRect = borderGO.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-2f, -2f);
        borderRect.offsetMax = new Vector2(2f, 2f);
        
        Image borderImage = borderGO.AddComponent<Image>();
        borderImage.color = hotbarBorderColor;
        
        // Cards container
        GameObject cardsContainerGO = new GameObject("Hotbar Cards Container");
        cardsContainerGO.transform.SetParent(hotbarContainer.transform);
        
        RectTransform cardsContainerRect = cardsContainerGO.AddComponent<RectTransform>();
        cardsContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardsContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardsContainerRect.sizeDelta = new Vector2(800, hotbarHeight - 20f);
        cardsContainerRect.anchoredPosition = Vector2.zero;
        
        Debug.Log("Created modern hotbar");
    }
    
    private void SetupModernButton(Button button, Color normalColor, Color hoverColor, Color pressedColor)
    {
        if (button == null) return;
        
        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = hoverColor;
        colors.pressedColor = pressedColor;
        colors.selectedColor = hoverColor;
        colors.fadeDuration = 0.1f;
        button.colors = colors;
    }
    
    private void SetupBackground()
    {
        if (backgroundImage != null && backgroundSprite != null)
        {
            backgroundImage.sprite = backgroundSprite;
            backgroundImage.color = Color.white;
        }
        else if (backgroundImage != null)
        {
            backgroundImage.sprite = null;
            backgroundImage.color = backgroundColor;
        }
    }
    
    public void UpdateBackground()
    {
        if (backgroundImage != null && backgroundSprite != null)
        {
            backgroundImage.sprite = backgroundSprite;
            backgroundImage.color = Color.white;
        }
        else if (backgroundImage != null)
        {
            backgroundImage.sprite = null;
            backgroundImage.color = backgroundColor;
        }
    }
    
    private void SetupCardSlot(int index)
    {
        if (cardSlots[index].cardImage == null) 
        {
            Debug.LogWarning($"Card slot {index} has no cardImage!");
            return;
        }
        
        // Store original values
        cardSlots[index].originalScale = cardSlots[index].rectTransform.localScale;
        cardSlots[index].originalPosition = cardSlots[index].rectTransform.anchoredPosition;
        
        // Setup button click for selection
        int cardIndex = index;
        cardSlots[index].cardButton.onClick.RemoveAllListeners();
        cardSlots[index].cardButton.onClick.AddListener(() => {
            Debug.Log($"Card {cardIndex} clicked!");
            SelectCard(cardIndex);
        });
        
        // Add hover handlers to the main card GameObject (not just the image)
        CardHoverHandler hoverHandler = cardSlots[index].rectTransform.gameObject.AddComponent<CardHoverHandler>();
        hoverHandler.Initialize(this, cardIndex);
        
        Debug.Log($"Setup modern card slot {index} with hover and selection - Button: {cardSlots[index].cardButton != null}, HoverHandler: {hoverHandler != null}");
    }
    
    // Public method for hover handler to call
    public void OnCardHover(int cardIndex, bool isHovering)
    {
        Debug.Log($"OnCardHover called: Card {cardIndex}, Hovering: {isHovering}");
        
        // Handle hotbar cards (cardIndex = -1)
        if (cardIndex == -1)
        {
            // Simple hover effect for hotbar cards
            return;
        }
        
        if (cardIndex < 0 || cardIndex >= cardSlots.Length) 
        {
            Debug.LogWarning($"Invalid card index: {cardIndex}");
            return;
        }
        
        cardSlots[cardIndex].isHovered = isHovering;
        StartCoroutine(AnimateCardHover(cardIndex, isHovering));
    }
    
    private IEnumerator AnimateCardHover(int cardIndex, bool isHovering)
    {
        CardSlot slot = cardSlots[cardIndex];
        if (slot.cardImage == null) yield break;
        
        Vector3 targetScale = isHovering ? slot.originalScale * hoverScale : slot.originalScale;
        Vector3 targetPosition = isHovering ? 
            slot.originalPosition + Vector3.up * hoverLiftHeight : 
            slot.originalPosition;
        
        // If card is selected, use selection scale instead
        if (slot.isSelected)
        {
            targetScale = slot.originalScale * selectionScale;
        }
        
        // Animate glow effect
        Color targetGlowColor = Color.clear;
        Color targetBorderColor = Color.clear;
        Color targetHighlightColor = Color.clear;
        
        if (slot.isSelected)
        {
            targetGlowColor = selectionGlowColor * glowIntensity;
            targetBorderColor = selectionBorderColor;
        }
        else if (isHovering)
        {
            targetGlowColor = Color.clear; // No glow on hover
            targetHighlightColor = new Color(1f, 1f, 1f, 0.1f);
        }
        
        // Animate shadow distance
        float targetShadowDistance = isHovering ? slot.originalShadowDistance * 1.3f : slot.originalShadowDistance;
        
        float elapsed = 0;
        Vector3 startScale = slot.rectTransform.localScale;
        Vector3 startPosition = slot.rectTransform.anchoredPosition;
        Color startGlowColor = slot.glowEffect.color;
        Color startBorderColor = slot.borderEffect.color;
        Color startHighlightColor = slot.highlightEffect.color;
        float startShadowDistance = slot.shadowEffect.rectTransform.offsetMin.x;
        
        while (elapsed < animationSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationSpeed;
            
            // Smooth animation curve
            float smoothT = Mathf.SmoothStep(0, 1, t);
            
            slot.rectTransform.localScale = Vector3.Lerp(startScale, targetScale, smoothT);
            slot.rectTransform.anchoredPosition = Vector3.Lerp(startPosition, targetPosition, smoothT);
            
            if (slot.glowEffect != null)
            {
                slot.glowEffect.color = Color.Lerp(startGlowColor, targetGlowColor, smoothT);
            }
            
            if (slot.borderEffect != null)
            {
                slot.borderEffect.color = Color.Lerp(startBorderColor, targetBorderColor, smoothT);
            }
            
            if (slot.highlightEffect != null)
            {
                slot.highlightEffect.color = Color.Lerp(startHighlightColor, targetHighlightColor, smoothT);
            }
            
            if (slot.shadowEffect != null)
            {
                float currentShadowDistance = Mathf.Lerp(startShadowDistance, targetShadowDistance, smoothT);
                slot.shadowEffect.rectTransform.offsetMin = new Vector2(currentShadowDistance, -currentShadowDistance);
                slot.shadowEffect.rectTransform.offsetMax = new Vector2(currentShadowDistance, -currentShadowDistance);
            }
            
            yield return null;
        }
        
        slot.rectTransform.localScale = targetScale;
        slot.rectTransform.anchoredPosition = targetPosition;
        if (slot.glowEffect != null) slot.glowEffect.color = targetGlowColor;
        if (slot.borderEffect != null) slot.borderEffect.color = targetBorderColor;
        if (slot.highlightEffect != null) slot.highlightEffect.color = targetHighlightColor;
        if (slot.shadowEffect != null)
        {
            slot.shadowEffect.rectTransform.offsetMin = new Vector2(targetShadowDistance, -targetShadowDistance);
            slot.shadowEffect.rectTransform.offsetMax = new Vector2(targetShadowDistance, -targetShadowDistance);
        }
    }
    
    private void LoadAllCards()
    {
        allCards.Clear();
        
        // Try manual assignments first
        if (LoadFromManualAssignments())
        {
            loadedCardsCount = allCards.Count;
            Debug.Log($"Loaded {allCards.Count} cards from manual assignments!");
            return;
        }
        
        // Try loading from the same path structure
        if (LoadCardsFromSamePath())
        {
            loadedCardsCount = allCards.Count;
            Debug.Log($"Loaded {allCards.Count} cards from path structure!");
            return;
        }
        
        // Fallback to Resources
        LoadCardsFromResources();
        
        loadedCardsCount = allCards.Count;
        Debug.Log($"Loaded {allCards.Count} cards successfully!");
    }
    
    private void LoadCardsFromResources()
    {
        // First try to load from manual assignments
        if (LoadFromManualAssignments())
        {
            return;
        }
        
        // Try to load cards from Resources folder
        string[] suitFolders = { "Clubs", "Hearts", "Diamonds", "Spades" };
        string[] suitSuffixes = { "club", "heart", "diamond", "spade" };
        PlayingCardSuit[] cardSuits = { PlayingCardSuit.Clubs, PlayingCardSuit.Hearts, PlayingCardSuit.Diamonds, PlayingCardSuit.Spades };
        
        for (int suitIndex = 0; suitIndex < suitFolders.Length; suitIndex++)
        {
            for (int value = 1; value <= 13; value++)
            {
                // Convert value to proper card name
                string valueName = value switch
                {
                    1 => "A",
                    11 => "J",
                    12 => "Q",
                    13 => "K",
                    _ => value.ToString()
                };
                
                string cardFileName = $"{valueName}{suitSuffixes[suitIndex]}";
                string resourcePath = $"Cards/{cardFileName}";
                
                Sprite cardSprite = Resources.Load<Sprite>(resourcePath);
                
                if (cardSprite != null)
                {
                    PlayingCard card = new PlayingCard
                    {
                        value = value,
                        suit = cardSuits[suitIndex],
                        cardSprite = cardSprite
                    };
                    allCards.Add(card);
                    Debug.Log($"Loaded: {card.GetCardName()} from Resources/{resourcePath}");
                }
                else
                {
                    Debug.LogWarning($"Could not load card sprite from Resources: {resourcePath}");
                }
            }
        }
        
        // Load card back sprite from Resources
        if (cardBackSprite == null)
        {
            cardBackSprite = Resources.Load<Sprite>("Cards/card_back");
            if (cardBackSprite != null)
            {
                Debug.Log("Loaded card back from Resources/Cards/card_back");
            }
            else
            {
                Debug.LogWarning("Could not load card back from Resources/Cards/card_back");
            }
        }
        
        // If no cards loaded, create fallback cards
        if (allCards.Count == 0)
        {
            CreateFallbackCards();
        }
    }
    
    private bool LoadFromManualAssignments()
    {
        // Check if manual sprites are assigned
        bool hasManualSprites = false;
        for (int i = 0; i < manualCardSprites.Length; i++)
        {
            if (manualCardSprites[i] != null)
            {
                hasManualSprites = true;
                break;
            }
        }
        
        if (!hasManualSprites)
        {
            return false;
        }
        
        Debug.Log("Loading cards from manual assignments...");
        
        // Load from manual assignments
        string[] suitNames = { "Clubs", "Hearts", "Diamonds", "Spades" };
        PlayingCardSuit[] cardSuits = { PlayingCardSuit.Clubs, PlayingCardSuit.Hearts, PlayingCardSuit.Diamonds, PlayingCardSuit.Spades };
        
        int spriteIndex = 0;
        for (int suitIndex = 0; suitIndex < suitNames.Length; suitIndex++)
        {
            for (int value = 1; value <= 13; value++)
            {
                if (spriteIndex < manualCardSprites.Length && manualCardSprites[spriteIndex] != null)
                {
                    PlayingCard card = new PlayingCard
                    {
                        value = value,
                        suit = cardSuits[suitIndex],
                        cardSprite = manualCardSprites[spriteIndex]
                    };
                    allCards.Add(card);
                    Debug.Log($"Loaded: {card.GetCardName()} from manual assignment");
                }
                spriteIndex++;
            }
        }
        
        // Set manual card back
        if (manualCardBackSprite != null)
        {
            cardBackSprite = manualCardBackSprite;
            Debug.Log("Loaded card back from manual assignment");
        }
        
        return allCards.Count > 0;
    }
    
    private void CreateFallbackCards()
    {
        Debug.Log("Creating fallback cards for build...");
        
        // Create a simple colored rectangle as fallback
        Texture2D fallbackTexture = new Texture2D(214, 227);
        Color[] pixels = new Color[214 * 227];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        fallbackTexture.SetPixels(pixels);
        fallbackTexture.Apply();
        
        Sprite fallbackSprite = Sprite.Create(fallbackTexture, new Rect(0, 0, 214, 227), new Vector2(0.5f, 0.5f));
        
        // Create fallback cards for each suit and value
        string[] suitNames = { "Clubs", "Hearts", "Diamonds", "Spades" };
        PlayingCardSuit[] cardSuits = { PlayingCardSuit.Clubs, PlayingCardSuit.Hearts, PlayingCardSuit.Diamonds, PlayingCardSuit.Spades };
        
        for (int suitIndex = 0; suitIndex < suitNames.Length; suitIndex++)
        {
            for (int value = 1; value <= 13; value++)
            {
                PlayingCard card = new PlayingCard
                {
                    value = value,
                    suit = cardSuits[suitIndex],
                    cardSprite = fallbackSprite
                };
                allCards.Add(card);
            }
        }
        
        // Set fallback card back
        if (cardBackSprite == null)
        {
            cardBackSprite = fallbackSprite;
        }
        
        Debug.Log($"Created {allCards.Count} fallback cards");
    }
    
    private bool LoadCardsFromSamePath()
    {
        string[] suitFolders = { "Clubs", "Hearts", "Diamonds", "Spades" };
        string[] suitSuffixes = { "club", "heart", "diamond", "spade" };
        PlayingCardSuit[] cardSuits = { PlayingCardSuit.Clubs, PlayingCardSuit.Hearts, PlayingCardSuit.Diamonds, PlayingCardSuit.Spades };
        
        bool loadedAnyCards = false;
        
        for (int suitIndex = 0; suitIndex < suitFolders.Length; suitIndex++)
        {
            string suitFolderPath = $"{cardsBasePath}/{suitFolders[suitIndex]}";
            
            for (int value = 1; value <= 13; value++)
            {
                // Convert value to proper card name
                string valueName = value switch
                {
                    1 => "A",
                    11 => "J",
                    12 => "Q",
                    13 => "K",
                    _ => value.ToString()
                };
                
                string cardFileName = $"{valueName}{suitSuffixes[suitIndex]}.png";
                string fullPath = $"{suitFolderPath}/{cardFileName}";
                
                Sprite cardSprite = null;
                
#if UNITY_EDITOR
                cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
#else
                // In builds, try to load from Resources with the same structure
                string resourcePath = fullPath.Replace("Assets/", "").Replace(".png", "");
                cardSprite = Resources.Load<Sprite>(resourcePath);
#endif
                
                if (cardSprite != null)
                {
                    PlayingCard card = new PlayingCard
                    {
                        value = value,
                        suit = cardSuits[suitIndex],
                        cardSprite = cardSprite
                    };
                    allCards.Add(card);
                    loadedAnyCards = true;
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
            
#if UNITY_EDITOR
            cardBackSprite = AssetDatabase.LoadAssetAtPath<Sprite>(cardBackPath);
#else
            string resourcePath = cardBackPath.Replace("Assets/", "").Replace(".png", "");
            cardBackSprite = Resources.Load<Sprite>(resourcePath);
#endif
            
            if (cardBackSprite != null)
            {
                Debug.Log($"Loaded card back from: {cardBackPath}");
            }
            else
            {
                Debug.LogWarning($"Could not load card back from: {cardBackPath}");
            }
        }
        
        return loadedAnyCards;
    }
    
    private void InitializeCardSlots()
    {
        // Initialize all slots with card backs
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i].cardImage != null && cardBackSprite != null)
            {
                cardSlots[i].cardImage.sprite = cardBackSprite;
                cardSlots[i].isSelected = false;
                cardSlots[i].isHovered = false;
                
                // Initialize shadow and glow effects
                if (cardSlots[i].shadowEffect != null)
                {
                    cardSlots[i].shadowEffect.sprite = cardBackSprite;
                    cardSlots[i].shadowEffect.color = new Color(0f, 0f, 0f, 0.3f);
                }
                
                if (cardSlots[i].glowEffect != null)
                {
                    cardSlots[i].glowEffect.sprite = cardBackSprite;
                    cardSlots[i].glowEffect.color = Color.clear;
                }
                
                if (cardSlots[i].borderEffect != null)
                {
                    cardSlots[i].borderEffect.sprite = cardBackSprite;
                    cardSlots[i].borderEffect.color = Color.clear;
                }
                
                if (cardSlots[i].highlightEffect != null)
                {
                    cardSlots[i].highlightEffect.sprite = cardBackSprite;
                    cardSlots[i].highlightEffect.color = Color.clear;
                }
            }
        }
    }
    
    private IEnumerator DrawCardsWithAnimation()
    {
        if (allCards.Count == 0)
        {
            Debug.LogWarning("No cards loaded! Check the cards base path and make sure sprites exist.");
            yield break;
        }
        
        // Deselect any currently selected card
        DeselectAllCards();
        
        // Hide all cards first with fade out
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i].cardImage != null)
            {
                StartCoroutine(FadeCard(i, 0f, 0.2f));
                cardSlots[i].cardButton.interactable = true;
            }
        }
        
        yield return new WaitForSeconds(0.3f);
        
        // Draw new cards
        currentDrawnCards.Clear();
        
        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (cardSlots[i].cardImage != null)
            {
                PlayingCard randomCard = GetRandomCard();
                currentDrawnCards.Add(randomCard);
                
                // Show card back first
                cardSlots[i].cardImage.sprite = cardBackSprite;
                if (cardSlots[i].shadowEffect != null) cardSlots[i].shadowEffect.sprite = cardBackSprite;
                if (cardSlots[i].glowEffect != null) cardSlots[i].glowEffect.sprite = cardBackSprite;
                if (cardSlots[i].borderEffect != null) cardSlots[i].borderEffect.sprite = cardBackSprite;
                if (cardSlots[i].highlightEffect != null) cardSlots[i].highlightEffect.sprite = cardBackSprite;
                
                StartCoroutine(FadeCard(i, 1f, 0.2f));
                
                yield return new WaitForSeconds(0.1f);
                
                // Flip to reveal card with enhanced animation
                yield return StartCoroutine(FlipCardWithEffects(i, randomCard.cardSprite));
                
                Debug.Log($"Drew: {randomCard.GetCardName()}");
            }
        }
    }
    
    private IEnumerator FadeCard(int cardIndex, float targetAlpha, float duration)
    {
        if (cardSlots[cardIndex].canvasGroup == null) yield break;
        
        float startAlpha = cardSlots[cardIndex].canvasGroup.alpha;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            cardSlots[cardIndex].canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }
        
        cardSlots[cardIndex].canvasGroup.alpha = targetAlpha;
    }
    
    private IEnumerator FlipCardWithEffects(int cardIndex, Sprite newSprite)
    {
        CardSlot slot = cardSlots[cardIndex];
        if (slot.cardImage == null) yield break;
        
        // Enhanced flip animation with shadow and glow effects
        float elapsed = 0;
        Vector3 originalScale = slot.rectTransform.localScale;
        
        // First half of flip - scale down
        while (elapsed < 0.15f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.15f;
            float scaleX = Mathf.Lerp(1f, 0f, t);
            slot.rectTransform.localScale = new Vector3(scaleX, 1f, 1f);
            
            // Animate shadow and glow during flip
            if (slot.shadowEffect != null)
            {
                float shadowAlpha = Mathf.Lerp(0.3f, 0f, t);
                slot.shadowEffect.color = new Color(0f, 0f, 0f, shadowAlpha);
            }
            
            if (slot.glowEffect != null)
            {
                float glowAlpha = Mathf.Lerp(0f, 0.5f, t);
                slot.glowEffect.color = new Color(1f, 1f, 1f, glowAlpha);
            }
            
            yield return null;
        }
        
        // Change sprite at the middle of flip
        slot.cardImage.sprite = newSprite;
        if (slot.shadowEffect != null) slot.shadowEffect.sprite = newSprite;
        if (slot.glowEffect != null) slot.glowEffect.sprite = newSprite;
        if (slot.borderEffect != null) slot.borderEffect.sprite = newSprite;
        if (slot.highlightEffect != null) slot.highlightEffect.sprite = newSprite;
        
        // Second half of flip - scale back up
        elapsed = 0;
        while (elapsed < 0.15f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.15f;
            float scaleX = Mathf.Lerp(0f, 1f, t);
            slot.rectTransform.localScale = new Vector3(scaleX, 1f, 1f);
            
            // Animate shadow and glow back
            if (slot.shadowEffect != null)
            {
                float shadowAlpha = Mathf.Lerp(0f, 0.3f, t);
                slot.shadowEffect.color = new Color(0f, 0f, 0f, shadowAlpha);
            }
            
            if (slot.glowEffect != null)
            {
                float glowAlpha = Mathf.Lerp(0.5f, 0f, t);
                slot.glowEffect.color = new Color(1f, 1f, 1f, glowAlpha);
            }
            
            yield return null;
        }
        
        slot.rectTransform.localScale = originalScale;
        if (slot.shadowEffect != null) slot.shadowEffect.color = new Color(0f, 0f, 0f, 0.3f);
        if (slot.glowEffect != null) slot.glowEffect.color = Color.clear;
    }
    
    public void SelectCard(int cardIndex)
    {
        if (cardIndex >= 0 && cardIndex < currentDrawnCards.Count)
        {
            // Deselect previous card
            if (selectedCardIndex >= 0 && selectedCardIndex < cardSlots.Length)
            {
                cardSlots[selectedCardIndex].isSelected = false;
                StartCoroutine(AnimateCardHover(selectedCardIndex, cardSlots[selectedCardIndex].isHovered));
            }
            
            // Select new card
            selectedCardIndex = cardIndex;
            cardSlots[cardIndex].isSelected = true;
            
            // Animate selection
            StartCoroutine(AnimateCardHover(cardIndex, cardSlots[cardIndex].isHovered));
            
            PlayingCard selectedCard = currentDrawnCards[cardIndex];
            Debug.Log($"Selected card: {selectedCard.GetCardName()}");
            
            OnCardSelected(selectedCard);
        }
    }
    
    private void OnCardSelected(PlayingCard card)
    {
        Debug.Log($"Player selected: {card.GetCardName()}");
        // Add your game logic here
    }
    
    private PlayingCard GetRandomCard()
    {
        // Get all cards that haven't been drawn yet
        List<PlayingCard> availableCards = allCards.Where(card => 
            !playerHand.Any(handCard => handCard.value == card.value && handCard.suit == card.suit)).ToList();
        
        if (availableCards.Count == 0)
        {
            Debug.Log("No more unique cards available!");
            return null;
        }
        
        int randomIndex = Random.Range(0, availableCards.Count);
        return availableCards[randomIndex];
    }
    
    private void AddCardToHotbar(PlayingCard card)
    {
        if (playerHand.Count >= maxCardsInHand)
        {
            // Enter swap mode
            isSwapMode = true;
            Debug.Log($"Hand is full! Entering swap mode. Click a hotbar card to replace it with {card.GetCardName()}");
            return;
        }
        
        playerHand.Add(card);
        CreateHotbarCard(card, playerHand.Count - 1);
        Debug.Log($"Added {card.GetCardName()} to hotbar. Total cards: {playerHand.Count}");
    }
    
    private void SwapHotbarCard(PlayingCard newCard, int hotbarIndex)
    {
        if (hotbarIndex < 0 || hotbarIndex >= playerHand.Count)
        {
            Debug.LogWarning($"Invalid hotbar index: {hotbarIndex}");
            return;
        }
        
        // Get the old card
        PlayingCard oldCard = playerHand[hotbarIndex];
        
        // Replace the card in the list
        playerHand[hotbarIndex] = newCard;
        
        // Update the visual representation
        UpdateHotbarCardVisual(hotbarIndex, newCard);
        
        Debug.Log($"Swapped {oldCard.GetCardName()} with {newCard.GetCardName()} at position {hotbarIndex}");
        
        // Exit swap mode
        isSwapMode = false;
        selectedHotbarCardIndex = -1;
    }
    
    private void UpdateHotbarCardVisual(int index, PlayingCard card)
    {
        Transform cardsContainer = hotbarContainer.transform.Find("Hotbar Cards Container");
        if (cardsContainer == null) return;
        
        Transform cardTransform = cardsContainer.Find($"HotbarCard_{index}");
        if (cardTransform == null) return;
        
        // Update the card image
        Image cardImage = cardTransform.GetComponent<Image>();
        if (cardImage != null)
        {
            cardImage.sprite = card.cardSprite;
        }
        
        // Update shadow sprite
        Transform shadowTransform = cardTransform.Find("Shadow");
        if (shadowTransform != null)
        {
            Image shadowImage = shadowTransform.GetComponent<Image>();
            if (shadowImage != null)
            {
                shadowImage.sprite = card.cardSprite;
            }
        }
        
        Debug.Log($"Updated hotbar card visual at index {index} to {card.GetCardName()}");
    }
    
    private void CreateHotbarCard(PlayingCard card, int index)
    {
        if (hotbarContainer == null) return;
        
        Transform cardsContainer = hotbarContainer.transform.Find("Hotbar Cards Container");
        if (cardsContainer == null) return;
        
        // Create hotbar card
        GameObject cardGO = new GameObject($"HotbarCard_{index}");
        cardGO.transform.SetParent(cardsContainer);
        
        RectTransform cardRect = cardGO.AddComponent<RectTransform>();
        float cardWidth = 214 * hotbarCardScale;
        float cardHeight = 227 * hotbarCardScale;
        cardRect.sizeDelta = new Vector2(cardWidth, cardHeight);
        
        // Position cards in hotbar
        float totalWidth = (maxCardsInHand - 1) * (cardWidth + hotbarCardSpacing);
        float startX = -totalWidth / 2f;
        float xPosition = startX + index * (cardWidth + hotbarCardSpacing);
        cardRect.anchoredPosition = new Vector2(xPosition, 0);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        
        // Card image
        Image cardImage = cardGO.AddComponent<Image>();
        cardImage.sprite = card.cardSprite;
        
        // Add hover effect
        CardHoverHandler hoverHandler = cardGO.AddComponent<CardHoverHandler>();
        hoverHandler.Initialize(this, -1); // -1 indicates hotbar card
        
        // Add click handler
        Button cardButton = cardGO.AddComponent<Button>();
        cardButton.transition = Selectable.Transition.ColorTint;
        int cardIndex = index;
        cardButton.onClick.AddListener(() => {
            Debug.Log($"Hotbar card clicked: {card.GetCardName()}");
            OnHotbarCardClicked(card, cardIndex);
        });
        
        // Setup button colors for swap mode feedback
        ColorBlock colors = cardButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9f, 0.9f, 1f, 1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 1f, 1f);
        colors.selectedColor = new Color(0.9f, 0.9f, 1f, 1f);
        colors.fadeDuration = 0.1f;
        cardButton.colors = colors;
        
        // Add subtle shadow
        GameObject shadowGO = new GameObject("Shadow");
        shadowGO.transform.SetParent(cardGO.transform);
        shadowGO.transform.SetAsFirstSibling();
        
        RectTransform shadowRect = shadowGO.AddComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.offsetMin = new Vector2(3f, -3f);
        shadowRect.offsetMax = new Vector2(3f, -3f);
        
        Image shadowImage = shadowGO.AddComponent<Image>();
        shadowImage.sprite = card.cardSprite;
        shadowImage.color = new Color(0f, 0f, 0f, 0.2f);
        
        Debug.Log($"Created hotbar card: {card.GetCardName()} at position {xPosition}");
    }
    
    private void OnHotbarCardClicked(PlayingCard card, int index)
    {
        if (isSwapMode)
        {
            // In swap mode, replace this card with the pending card
            PlayingCard pendingCard = GetPendingSwapCard();
            if (pendingCard != null)
            {
                SwapHotbarCard(pendingCard, index);
            }
        }
        else
        {
            // Normal mode - just log the selection
            Debug.Log($"Hotbar card selected: {card.GetCardName()}");
            // Add your game logic here for when a hotbar card is clicked
        }
    }
    
    private PlayingCard pendingSwapCard = null;
    
    private void SetPendingSwapCard(PlayingCard card)
    {
        pendingSwapCard = card;
    }
    
    private PlayingCard GetPendingSwapCard()
    {
        return pendingSwapCard;
    }
    
    public void ResetCards()
    {
        // Clear selection
        selectedCardIndex = -1;
        
        // Reset all card states
        for (int i = 0; i < cardSlots.Length; i++)
        {
            cardSlots[i].isSelected = false;
            cardSlots[i].isHovered = false;
        }
        
        StartCoroutine(DrawCardsWithAnimation());
    }
    
    private void DeselectAllCards()
    {
        // Deselect any currently selected card
        if (selectedCardIndex >= 0 && selectedCardIndex < cardSlots.Length)
        {
            cardSlots[selectedCardIndex].isSelected = false;
            StartCoroutine(AnimateCardHover(selectedCardIndex, cardSlots[selectedCardIndex].isHovered));
        }
        
        selectedCardIndex = -1;
        
        // Reset all card selection states
        for (int i = 0; i < cardSlots.Length; i++)
        {
            cardSlots[i].isSelected = false;
        }
    }
    
    private IEnumerator DrawSingleCard()
    {
        if (allCards.Count == 0)
        {
            Debug.LogWarning("No cards loaded! Check the cards base path and make sure sprites exist.");
            yield break;
        }
        
        // Check if a card is selected
        if (selectedCardIndex < 0 || selectedCardIndex >= currentDrawnCards.Count)
        {
            Debug.Log("No card selected! Please select a card first.");
            yield break;
        }
        
        // Check if we have room for more cards in hotbar
        if (playerHand.Count >= maxCardsInHand)
        {
            Debug.Log($"Hotbar is full! Maximum {maxCardsInHand} cards allowed.");
            yield break;
        }
        
        // Get the selected card
        PlayingCard selectedCard = currentDrawnCards[selectedCardIndex];
        
        // Add selected card to hotbar (or enter swap mode if full)
        AddCardToHotbar(selectedCard);
        
        // If entering swap mode, store the card to be swapped
        if (isSwapMode)
        {
            SetPendingSwapCard(selectedCard);
        }
        
        // Only turn card back and deselect if not in swap mode
        if (!isSwapMode)
        {
            // Turn the selected card back to card back and make it undrawable
            yield return StartCoroutine(TurnCardToBack(selectedCardIndex));
            
            // Remove the card from current drawn cards
            currentDrawnCards[selectedCardIndex] = null;
            
            // Deselect the card
            DeselectAllCards();
        }
        else
        {
            // In swap mode, just deselect the card but keep it available
            DeselectAllCards();
        }
        
        Debug.Log($"Drew selected card: {selectedCard.GetCardName()}. Total cards in hotbar: {playerHand.Count}");
        
        // Placeholder behavior - you can customize this
        OnCardDrawn(selectedCard);
    }
    
    private void GoToSampleScene()
    {
        Debug.Log("Loading SampleScene...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }
    
    private IEnumerator TurnCardToBack(int cardIndex)
    {
        CardSlot slot = cardSlots[cardIndex];
        if (slot.cardImage == null) yield break;
        
        // Animate the card turning back
        float elapsed = 0;
        Vector3 originalScale = slot.rectTransform.localScale;
        
        // First half of flip - scale down
        while (elapsed < 0.15f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.15f;
            float scaleX = Mathf.Lerp(1f, 0f, t);
            slot.rectTransform.localScale = new Vector3(scaleX, 1f, 1f);
            
            // Animate shadow and glow during flip
            if (slot.shadowEffect != null)
            {
                float shadowAlpha = Mathf.Lerp(0.3f, 0f, t);
                slot.shadowEffect.color = new Color(0f, 0f, 0f, shadowAlpha);
            }
            
            if (slot.glowEffect != null)
            {
                float glowAlpha = Mathf.Lerp(0f, 0.3f, t);
                slot.glowEffect.color = new Color(1f, 1f, 1f, glowAlpha);
            }
            
            yield return null;
        }
        
        // Change sprite to card back
        slot.cardImage.sprite = cardBackSprite;
        if (slot.shadowEffect != null) slot.shadowEffect.sprite = cardBackSprite;
        if (slot.glowEffect != null) slot.glowEffect.sprite = cardBackSprite;
        if (slot.borderEffect != null) slot.borderEffect.sprite = cardBackSprite;
        if (slot.highlightEffect != null) slot.highlightEffect.sprite = cardBackSprite;
        
        // Second half of flip - scale back up
        elapsed = 0;
        while (elapsed < 0.15f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.15f;
            float scaleX = Mathf.Lerp(0f, 1f, t);
            slot.rectTransform.localScale = new Vector3(scaleX, 1f, 1f);
            
            // Animate shadow and glow back
            if (slot.shadowEffect != null)
            {
                float shadowAlpha = Mathf.Lerp(0f, 0.3f, t);
                slot.shadowEffect.color = new Color(0f, 0f, 0f, shadowAlpha);
            }
            
            if (slot.glowEffect != null)
            {
                float glowAlpha = Mathf.Lerp(0.3f, 0f, t);
                slot.glowEffect.color = new Color(1f, 1f, 1f, glowAlpha);
            }
            
            yield return null;
        }
        
        slot.rectTransform.localScale = originalScale;
        if (slot.shadowEffect != null) slot.shadowEffect.color = new Color(0f, 0f, 0f, 0.3f);
        if (slot.glowEffect != null) slot.glowEffect.color = Color.clear;
        
        // Disable the button to make it undrawable
        if (slot.cardButton != null)
        {
            slot.cardButton.interactable = false;
        }
        
        Debug.Log($"Card {cardIndex} turned back to card back and made undrawable");
    }
    
    private void OnCardDrawn(PlayingCard card)
    {
        Debug.Log($"Card drawn: {card.GetCardName()}. Add your custom logic here!");
        // Placeholder behavior - add your game logic here
        // For example:
        // - Check for specific card combinations
        // - Update score or game state
        // - Trigger special effects
        // - Check win/lose conditions
    }
    
    // Utility methods
    public PlayingCard GetSpecificCard(int value, PlayingCardSuit suit)
    {
        return allCards.Find(card => card.value == value && card.suit == suit);
    }
    
    public List<PlayingCard> GetCardsBySuit(PlayingCardSuit suit)
    {
        return allCards.FindAll(card => card.suit == suit);
    }
    
    // Background management methods
    public void SetBackgroundSprite(Sprite newBackground)
    {
        backgroundSprite = newBackground;
        UpdateBackground();
    }
    
#if UNITY_EDITOR
    [ContextMenu("Reload Cards")]
    public void ReloadCards()
    {
        LoadAllCards();
        Debug.Log($"Reloaded {allCards.Count} cards from {cardsBasePath}");
    }
#endif
}

// Custom hover handler for cards
public class CardHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private PlayingCardDrawer cardDrawer;
    private int cardIndex;
    
    public void Initialize(PlayingCardDrawer drawer, int index)
    {
        cardDrawer = drawer;
        cardIndex = index;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"Hover ENTER detected on card {cardIndex}");
        if (cardDrawer != null)
        {
            cardDrawer.OnCardHover(cardIndex, true);
        }
        else
        {
            Debug.LogWarning($"CardDrawer is null for card {cardIndex}");
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"Hover EXIT detected on card {cardIndex}");
        if (cardDrawer != null)
        {
            cardDrawer.OnCardHover(cardIndex, false);
        }
        else
        {
            Debug.LogWarning($"CardDrawer is null for card {cardIndex}");
        }
    }
} 
