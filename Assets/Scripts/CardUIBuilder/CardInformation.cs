using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardData
{
    [CreateAssetMenu(fileName = "New Card", menuName = "Card")]
    public class CardInformation : ScriptableObject
    {
        public string cardName;
        public List<CardType> cardType;
        public string abilityDesc;
        public List<CardUsageType> usageType;
        public Sprite cardSprite;
        public float cooldownDuration = 0f;
        public enum CardType
        {
            Brains,
            Bones,
            Blood,
            RottenFlesh,
            Void
        }

        public enum CardUsageType
        {
            Active,
            Passive
        }

        public static CardInformation FromPlayingCard(PlayingCard card)
        {
            CardInformation cardInfo = new CardInformation();
            cardInfo.cardName = card.customName;
            cardInfo.cardType = new List<CardType>(1);
            cardInfo.cardType.Add(CardType.Brains);
            cardInfo.abilityDesc = "";
            cardInfo.usageType = new List<CardUsageType>(1);
            cardInfo.usageType.Add(CardUsageType.Passive);
            cardInfo.cardSprite = card.cardSprite;
            return cardInfo;
        }
    }
}