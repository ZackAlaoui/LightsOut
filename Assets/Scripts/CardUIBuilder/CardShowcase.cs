using UnityEngine;
using CardData;

public class CardShowcase : MonoBehaviour
{
    public GameObject cardPrefab;         // Universal card prefab
    public Transform[] cardSlots;         // 5 UI slots
    public DeckManager deckManager;       // Assign in Inspector or fetch in Start()

    private void Start()
    {
        if (deckManager == null)
            deckManager = DeckManager.Instance;

        ShowRandomCards();
    }

    public void ShowRandomCards()
    {
        if (deckManager == null || deckManager.allCards.Count == 0)
        {
            Debug.LogWarning("No deck data available.");
            return;
        }

        for (int i = 0; i < cardSlots.Length; i++)
        {
            // Clean old card
            foreach (Transform child in cardSlots[i])
                Destroy(child.gameObject);

            // Pull random card info
            CardInformation info = deckManager.allCards[Random.Range(0, deckManager.allCards.Count)];

            // Instantiate and parent
            GameObject cardGO = Instantiate(cardPrefab, cardSlots[i]);
            RectTransform rt = cardGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;

            // Set visual + ability logic
            CardDisplay display = cardGO.GetComponent<CardDisplay>();
            if (display != null)
            {
                display.cardData = info;
                display.UpdatecardDisplay();
            }

            CardAbilityUI ui = cardGO.GetComponent<CardAbilityUI>();
            if (ui != null)
                ui.SetCard(info);
        }
    }
}