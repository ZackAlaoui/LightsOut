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
            RottenFlesh
        }

        public enum CardUsageType
        {
            Cooldown,
            SingleUse
        }

    }
}