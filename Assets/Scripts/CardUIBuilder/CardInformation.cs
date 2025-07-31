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

        // public static CardInformation FromPlayingCard(PlayingCard card)
        // {
        //     return new CardInformation()
        //     {
        //         cardName = card.customName,
        //         cardType = new List<CardType>
        //         {
        //             card.suit switch
        //             {
        //                 PlayingCardSuit.Clubs => CardType.Brains,
        //                 PlayingCardSuit.Hearts => CardType.Bones,
        //                 PlayingCardSuit.Diamonds => CardType.Blood,
        //                 PlayingCardSuit.Spades => CardType.RottenFlesh,
        //                 PlayingCardSuit.Five => CardType.Void,
        //                 _ => CardType.Brains
        //             }
        //         },
        //         abilityDesc = "";
        //         usageType = new List<CardUsageType>(1);
        //         usageType.Add(CardUsageType.Passive);
        //         cardSprite = card.cardSprite;
        //     };
        // }

        // public static CardInformation FromCard(Card card)
        // {
        //     return new CardInformation()
        //     {
        //         cardName = card.cardName,
        //         cardType = new List<CardType>
        //         {
        //             card.suit switch {
        //                 CardSuit.Brains => CardType.Brains,
        //                 CardSuit.Bones => CardType.Bones,
        //                 CardSuit.Blood => CardType.Blood,
        //                 CardSuit.RottenFlesh => CardType.RottenFlesh,
        //                 CardSuit.
        //             }
        //         }
        //     }
        // }
    }
}