using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardData;
using Game.Player;
using Game.Enemy;

public class HandManager : MonoBehaviour
{
    private static HandManager s_instance;

    public PokerHandManager pokerHandManager;
    public DeckManager deckManager;
    public GameObject cardPrefab;
    public Transform handTransform;
    public float fanSpread = 5f;
    public float cardSpacing = 100f;
    public float verticalSpacing = 100f;

    public static GameObject[] CardsInHand = new GameObject[5];
    public RectTransform[] cardSlots = new RectTransform[5];

    // Track passive cards separately
    private List<int> passiveCardIndexes = new List<int>();
    
    private PlayerController _player;                                   //PlayerController reference
    private bool _slipstreamTriggered = false;

    private void Awake()
    {
        if (s_instance != null && s_instance != this)
        {
            Debug.LogWarning("HandManager has already been instantiated. Deleting duplicate HandManager.");
            Destroy(gameObject);
            return;
        }
        else
        {
            s_instance = this;
        }

        _player = PlayerController.Instance;
    }


    public static void AddCardToHand(CardInformation cardData)
    {
        for (int i = 0; i < CardsInHand.Length; i++)
        {
            if (CardsInHand[i] == null)
            {
                GameObject newCard = Instantiate(s_instance.cardPrefab);
                newCard.transform.SetParent(s_instance.cardSlots[i], false);

                RectTransform rt = newCard.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.localScale = Vector3.one;

                CardsInHand[i] = newCard;

                CardDisplay display = newCard.GetComponent<CardDisplay>();
                display.cardData = cardData;
                display.UpdatecardDisplay();

                CardAbilityUI ui = newCard.GetComponent<CardAbilityUI>();
                if (ui != null) ui.SetCard(cardData);

                // Track passive cards
                if (cardData.usageType.Contains(CardInformation.CardUsageType.Passive))
                {
                    s_instance.passiveCardIndexes.Add(i);
                }

                s_instance.pokerHandManager?.EvaluateHandAndApplyBuffs();
                return;
            }
        }

        Debug.LogWarning("Hand is full! Cannot add more than 5 cards.");
    }

    public static void SwapCardInHand(CardInformation cardData, int index)
    {
        GameObject newCard = Instantiate(s_instance.cardPrefab);
        newCard.transform.SetParent(s_instance.cardSlots[index], false);

        RectTransform rt = newCard.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;

        CardsInHand[index] = newCard;

        CardDisplay display = newCard.GetComponent<CardDisplay>();
        display.cardData = cardData;
        display.UpdatecardDisplay();

        CardAbilityUI ui = newCard.GetComponent<CardAbilityUI>();
        if (ui != null) ui.SetCard(cardData);

        // Track passive cards
        if (cardData.usageType.Contains(CardInformation.CardUsageType.Passive))
        {
            s_instance.passiveCardIndexes.Add(index);
        }

        s_instance.pokerHandManager?.EvaluateHandAndApplyBuffs();
    }

    private void Start()
    {
        // AddCardToHand(DeckManager.Instance.allCards.Find((cardInfo) => cardInfo.cardName == "Blood Pact"));
        for (int i = 0; i < 3; i++)
        {
            DeckManager.DrawCard();
        }
    }

    public static void DiscardCard(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= CardsInHand.Length)
        {
            Debug.LogWarning("Invalid slot index.");
            return;
        }

        if (CardsInHand[cardIndex] == null)
        {
            Debug.Log("Slot " + cardIndex + " is already empty.");
            return;
        }

        GameObject cardToRemove = CardsInHand[cardIndex];
        CardsInHand[cardIndex] = null;
        s_instance.passiveCardIndexes.Remove(cardIndex); // Remove from passive list if it was there

        Destroy(cardToRemove);
        s_instance.pokerHandManager?.EvaluateHandAndApplyBuffs();
    }

    void Update()
    {
        // 🔹 Manual activation only if NOT passive
        for (int i = 0; i < CardsInHand.Length; i++)
        {
            if (CardsInHand[i] == null) continue;

            CardDisplay display = CardsInHand[i].GetComponent<CardDisplay>();
            if (display != null && !display.cardData.usageType.Contains(CardInformation.CardUsageType.Passive))
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    Debug.Log($"Key {(KeyCode.Alpha1 + i)} pressed - activating card index {i}");
                    ActivateCard(i);
                }
            }
        }
        
        // 🔹 Passive card condition monitoring (generalized for all passive cards)
        foreach (int index in passiveCardIndexes.ToArray())
        {
            GameObject card = CardsInHand[index];
            if (card == null) continue;

            CardDisplay display = card.GetComponent<CardDisplay>();
            if (display == null) continue;

            string name = display.cardData.cardName;
            bool triggered = false;

            switch (name)
            {
                case "Last Light":
                    if (_player.Flashlight.RemainingBatteryLife <= 2f)
                        triggered = true;
                    break;

                case "Decay Bloom":
                    if (_player.Health / _player.MaxHealth <= 0.25f)
                        triggered = true;
                    break;

                case "Slipstream Echo":
                    if (!_player.IsSprinting && !_slipstreamTriggered)
                    {
                        _slipstreamTriggered = true;
                        triggered = true;
                    }
                    break;
                case "Adrenaline Spike":
                    if (_player.Health / _player.MaxHealth < 0.5f)
                        triggered = true;
                    break;

                case "Crimson Howl":
                    EnemyController[] nearbyEnemies = FindObjectsOfType<EnemyController>();
                    int count = 0;
                    foreach (var enemy in nearbyEnemies)
                    {
                        if (Vector3.Distance(enemy.transform.position, _player.transform.position) <= 3f)
                            count++;
                    }
                    if (count >= 3)
                        triggered = true;
                    break;
                
            }

            if (triggered)
            {
                Debug.Log($"Passive Trigger: {name}");
                FindObjectOfType<AbilityManager>()?.TriggerPassive(name);
                break;
            }
        }

    }

    public static List<CardInformation> GetCurrentHand()
    {
        List<CardInformation> handData = new List<CardInformation>();
        foreach (GameObject card in CardsInHand)
        {
            if (card != null)
            {
                CardDisplay display = card.GetComponent<CardDisplay>();
                if (display != null && display.cardData != null)
                    handData.Add(display.cardData);
            }
        }
        return handData;
    }

    public static void ActivateCard(int index)
    {
        if (index < 0 || index >= CardsInHand.Length) return;

        GameObject card = CardsInHand[index];
        if (card == null) return;

        CardDisplay display = card.GetComponent<CardDisplay>();
        if (display == null) return;

        if (display.cardData.usageType.Contains(CardInformation.CardUsageType.Passive))
        {
            Debug.Log("Cannot activate passive card manually.");
            return;
        }

        CardAbilityUI abilityUI = card.GetComponent<CardAbilityUI>();
        if (abilityUI != null)
            abilityUI.StartCooldown();

        AbilityManager abilityManager = FindObjectOfType<AbilityManager>();
        if (abilityManager != null)
            abilityManager.ActivateAbility(display.cardData.cardName);

        Debug.Log($"Activated card in slot {index + 1}: {display.cardData.cardName}");
    }
    
    public static bool TryAddCardToHand(CardInformation cardData)
    {
        for (int i = 0; i < CardsInHand.Length; i++)
        {
            if (CardsInHand[i] == null)
            {
                AddCardToHand(cardData); // Reuse existing method
                return true;
            }
        }

        return false;
    }

}
