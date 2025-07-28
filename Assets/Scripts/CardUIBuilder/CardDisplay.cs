using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardData;

public class CardDisplay : MonoBehaviour
{
    public CardInformation cardData;
    public Image cardImage;
    public TMP_Text nameText;
    public Image[] typeImage;   // Suits (Brains, Bones, etc.)
    public Image[] usageImage;  // Usage (Active, Passive)
    public Image displayImage;

    private Dictionary<CardInformation.CardType, Color> typeColors = new Dictionary<CardInformation.CardType, Color>
    {
        { CardInformation.CardType.Brains, new Color(0.25f, 0.13f, 0.38f) },   // Dark violet
        { CardInformation.CardType.Bones, new Color(0.45f, 0.38f, 0.29f) },    // Muted tan
        { CardInformation.CardType.Blood, new Color(0.5f, 0.05f, 0.05f) },     // Dark crimson
        { CardInformation.CardType.RottenFlesh, new Color(0.45f, 0.39f, 0.1f) }, // Mustard-brown
        { CardInformation.CardType.Void, new Color(0.12f, 0.12f, 0.3f) }       // Deep indigo
    };

    public void UpdatecardDisplay()
    {
        if (cardData == null) return;

        // 🔹 Set background color based on primary suit
        if (cardData.cardType.Count > 0 && typeColors.ContainsKey(cardData.cardType[0]))
        {
            cardImage.color = typeColors[cardData.cardType[0]];
        }

        // 🔹 Set name and image
        nameText.text = cardData.cardName;
        displayImage.sprite = cardData.cardSprite;

        // 🔹 Update Suit Icons (hide if not present)
        for (int i = 0; i < typeImage.Length; i++)
        {
            if (i < System.Enum.GetValues(typeof(CardInformation.CardType)).Length)
            {
                CardInformation.CardType suitType = (CardInformation.CardType)i;
                typeImage[i].gameObject.SetActive(cardData.cardType.Contains(suitType));
            }
            else
            {
                typeImage[i].gameObject.SetActive(false);
            }
        }

        // 🔹 Update Usage Icons
        for (int i = 0; i < usageImage.Length; i++)
        {
            if (i < System.Enum.GetValues(typeof(CardInformation.CardUsageType)).Length)
            {
                CardInformation.CardUsageType usageType = (CardInformation.CardUsageType)i;
                usageImage[i].gameObject.SetActive(cardData.usageType.Contains(usageType));
            }
            else
            {
                usageImage[i].gameObject.SetActive(false);
            }
        }
    }
}
