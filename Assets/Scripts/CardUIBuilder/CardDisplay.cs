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
    public Image[] typeImage;
    public Image[] usageImage;
    public Image displayImage;

    private Color[] typeColors = {
        new Color(0.23f, 0.06f, 0.21f), //Brains
        new Color(0.8f, 0.52f, 0.24f), //Bones
        Color.red, //blood
        Color.yellow //RottenFlesh
    };
    

    public void UpdatecardDisplay()
{
    // Update card background color based on first type (optional)
    cardImage.color = typeColors[(int)cardData.cardType[0]];
    nameText.text = cardData.cardName;
    displayImage.sprite = cardData.cardSprite;

    // --- Update Suit (CardType) Images ---
    for (int i = 0; i < typeImage.Length; i++)
    {
        // Cast index to CardType enum
        CardInformation.CardType suitType = (CardInformation.CardType)i;

        // Show image if card has this suit, otherwise hide
        typeImage[i].gameObject.SetActive(cardData.cardType.Contains(suitType));
    }

    // --- Update UsageType Images ---
    for (int i = 0; i < usageImage.Length; i++)
    {
        // Cast index to UsageType enum
        CardInformation.CardUsageType usageType = (CardInformation.CardUsageType)i;

        // Show image if card has this usage type, otherwise hide
        usageImage[i].gameObject.SetActive(cardData.usageType.Contains(usageType));
    }
}


}