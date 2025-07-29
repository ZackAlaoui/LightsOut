using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardData;
using Game.Player;
using Game.Enemy;

public class HandManager : MonoBehaviour
{
    public PokerHandManager pokerHandManager;
    public DeckManager deckManager;
    public GameObject cardPrefab;
    public Transform handTransform;
    public float fanSpread = 5f;
    public float cardSpacing = 100f;
    public float verticalSpacing = 100f;

    private GameObject[] cardsInHand = new GameObject[5];               // Number of cards in hand
    public RectTransform[] cardSlots = new RectTransform[5];            // Number of slots in hand

    // Track passive cards separately
    private List<int> passiveCardIndexes = new List<int>();
    
    private PlayerController _player;                                   //PlayerController reference
    private bool _slipstreamTriggered = false;

    private void Awake()
    {
        _player = PlayerController.Instance;
        deckManager = DeckManager.Instance;

        if (deckManager == null)
        {
            Debug.LogError("DeckManager.Instance was not initialized. Check scene hierarchy and script execution order.");
        }
    }


    public void AddCardToHand(CardInformation cardData)
    {
        for (int i = 0; i < cardsInHand.Length; i++)
        {
            if (cardsInHand[i] == null)
            {
                GameObject newCard = Instantiate(cardPrefab);
                newCard.transform.SetParent(cardSlots[i], false);

                RectTransform rt = newCard.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.localScale = Vector3.one;

                cardsInHand[i] = newCard;

                CardDisplay display = newCard.GetComponent<CardDisplay>();
                display.cardData = cardData;
                display.UpdatecardDisplay();

                CardAbilityUI ui = newCard.GetComponent<CardAbilityUI>();
                if (ui != null) ui.SetCard(cardData);

                // Track passive cards
                if (cardData.usageType.Contains(CardInformation.CardUsageType.Passive))
                {
                    passiveCardIndexes.Add(i);
                }

                pokerHandManager?.EvaluateHandAndApplyBuffs();
                return;
            }
        }

        Debug.LogWarning("Hand is full! Cannot add more than 5 cards.");
    }

    private void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            deckManager.DrawCard(this);
        }
    }

    public void DiscardCard(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= cardsInHand.Length)
        {
            Debug.LogWarning("Invalid slot index.");
            return;
        }

        if (cardsInHand[cardIndex] == null)
        {
            Debug.Log("Slot " + cardIndex + " is already empty.");
            return;
        }

        GameObject cardToRemove = cardsInHand[cardIndex];
        cardsInHand[cardIndex] = null;
        passiveCardIndexes.Remove(cardIndex); // Remove from passive list if it was there

        Destroy(cardToRemove);
        pokerHandManager?.EvaluateHandAndApplyBuffs();
    }

    void Update()
    {
        // 🔹 Manual activation only if NOT passive
        for (int i = 0; i < cardsInHand.Length; i++)
        {
            if (cardsInHand[i] == null) continue;

            CardDisplay display = cardsInHand[i].GetComponent<CardDisplay>();
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
            GameObject card = cardsInHand[index];
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

    public List<CardInformation> GetCurrentHand()
    {
        List<CardInformation> handData = new List<CardInformation>();
        foreach (GameObject card in cardsInHand)
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

    public void ActivateCard(int index)
    {
        if (index < 0 || index >= cardsInHand.Length) return;

        GameObject card = cardsInHand[index];
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
}