using CardData;
using UnityEngine;
using UnityEngine.UI;

public class CardAbilityUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Color unavailableColor;
    public GameObject durationBar; // The fill image for cooldown display

    private bool abilityActive = false;
    private float currentDuration = 0f;
    private CardInformation cardInfo;

    private Image backgroundImage;

    private void Awake()
    {
        backgroundImage = GetComponent<Image>();
        durationBar.SetActive(false);
    }

    // Assigns the card data to this UI
    public void SetCard(CardInformation info)
    {
        cardInfo = info;
        currentDuration = cardInfo.cooldownDuration;
    }

    // Call this when ability starts
    public void StartCooldown()
    {
        if (cardInfo == null) return;

        abilityActive = true;
        currentDuration = cardInfo.cooldownDuration;
        durationBar.SetActive(true);
        UpdateImage(false); // Show ability is unavailable
    }

    // Enable/disable card visuals based on availability
    public void UpdateImage(bool abilityAvailable)
    {
        backgroundImage.color = abilityAvailable ? Color.white : unavailableColor;
    }

    private void Update()
    {
        if (!abilityActive || cardInfo == null) return;

        currentDuration -= Time.deltaTime;

        float fill = Mathf.Clamp01(currentDuration / cardInfo.cooldownDuration);
        durationBar.GetComponent<Image>().fillAmount = fill;

        if (currentDuration <= 0)
        {
            abilityActive = false;
            durationBar.SetActive(false);
            UpdateImage(true); // Ability ready again
        }
    }
}