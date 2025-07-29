using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("Card Visual Elements")]
    public Image cardBackground;
    public Image cardFrame;
    public Image suitIcon;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI abilityDescText;
    public TextMeshProUGUI usageTypeText;
    public Button cardButton;

    [Header("Suit Colors")]
    public Color brainsColor = new Color(0.7f, 0.3f, 1f);
    public Color bonesColor = new Color(0.9f, 0.9f, 0.8f);
    public Color bloodColor = new Color(1f, 0.2f, 0.2f);
    public Color rottenFleshColor = new Color(0.5f, 0.8f, 0.2f);

    [Header("Card States")]
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(1.1f, 1.1f, 1.1f);
    public Color selectedColor = new Color(0.8f, 1f, 0.8f);
    public Color usedColor = new Color(0.5f, 0.5f, 0.5f);

    private Card associatedCard;
    private bool isUsed = false;
    private int cardIndex;

    public void SetupCard(Card card, int index)
    {
        associatedCard = card;
        cardIndex = index;

        // Set card name
        if (cardNameText != null)
            cardNameText.text = card.cardName;

        // Set ability description
        if (abilityDescText != null)
            abilityDescText.text = card.abilityDesc;

        // Set usage type
        if (usageTypeText != null)
            usageTypeText.text = card.usageType.ToString();

        // Set suit-specific colors
        Color suitColor = GetSuitColor(card.suit);

        if (cardFrame != null)
            cardFrame.color = suitColor;

        if (suitIcon != null)
        {
            suitIcon.color = suitColor;
            // You can set different sprites for different suits here
            // suitIcon.sprite = GetSuitSprite(card.suit);
        }

        // Setup button
        if (cardButton != null)
        {
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(() => OnCardClicked());
        }

        if (cardBackground != null && card.CardImage != null)
        {
            cardBackground.sprite = card.CardImage;
            cardBackground.color = Color.white;
        }

        Debug.Log($"Assigned sprite to card '{card.cardName}': {card.CardImage?.name}, texture: {card.CardImage?.texture?.width}x{card.CardImage?.texture?.height}");


        UpdateVisualState();
    }

    private Color GetSuitColor(CardSuit suit)
    {
        return suit switch
        {
            CardSuit.Brains => brainsColor,
            CardSuit.Bones => bonesColor,
            CardSuit.Blood => bloodColor,
            CardSuit.RottenFlesh => rottenFleshColor,
            _ => Color.white
        };
    }

    private void OnCardClicked()
    {
        if (isUsed) return;

        // Use the card through CardManager
        if (CardManager.Instance != null)
        {
            CardManager.Instance.UseCard(cardIndex);

            // If it's a single use card, mark as used
            if (associatedCard.usageType == CardUsageType.SingleUse)
            {
                isUsed = true;
                UpdateVisualState();
            }
        }
    }

    private void UpdateVisualState()
    {
        if (cardBackground == null) return;

        if (isUsed)
        {
            cardBackground.color = usedColor;
            if (cardButton != null)
                cardButton.interactable = false;
        }
        else
        {
            cardBackground.color = normalColor;
            if (cardButton != null)
                cardButton.interactable = true;
        }
    }

    public void OnPointerEnter()
    {
        if (!isUsed && cardBackground != null)
            cardBackground.color = hoverColor;
    }

    public void OnPointerExit()
    {
        if (!isUsed && cardBackground != null)
            cardBackground.color = normalColor;
    }

    public void SetSelected(bool selected)
    {
        if (isUsed) return;

        if (cardBackground != null)
            cardBackground.color = selected ? selectedColor : normalColor;
    }

}