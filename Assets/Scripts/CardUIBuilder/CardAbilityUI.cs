using CardData;
using UnityEngine;
using UnityEngine.UI;

public class CardAbilityUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Color unavailableColor;
    public GameObject durationBar; // Assigned via Inspector
    public Image backgroundImage;  // Assign in Inspector

    private bool abilityActive = false;
    private float currentDuration = 0f;
    private CardInformation cardInfo;
    private Color originalColor;

    private void Awake()
    {
        if (durationBar != null)
            durationBar.SetActive(false);
        else
            Debug.LogWarning("Duration bar is not assigned!");

        if (backgroundImage == null)
            Debug.LogWarning("Background image not assigned!");
    }

    public void SetCard(CardInformation info)
    {
        cardInfo = info;

        if (cardInfo.usageType.Contains(CardInformation.CardUsageType.Passive))
        {
            if (durationBar != null)
                durationBar.SetActive(false);

            UpdateImage(true); // Passive cards are always shown as available
        }
        else
        {
            currentDuration = cardInfo.cooldownDuration;

            if (backgroundImage != null)
                originalColor = backgroundImage.color;
        }
    }

    public void StartCooldown()
    {
        if (cardInfo == null) return;

        if (cardInfo.usageType.Contains(CardInformation.CardUsageType.Passive))
        {
            Debug.Log($"Cooldown skipped for passive card: {cardInfo.cardName}");
            return;
        }

        Debug.Log("StartCooldown() called");

        abilityActive = true;
        currentDuration = cardInfo.cooldownDuration;

        if (durationBar != null)
            durationBar.SetActive(true);
        else
            Debug.LogWarning("durationBar is NULL when trying to activate!");

        UpdateImage(false);
    }

    public void UpdateImage(bool abilityAvailable)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = abilityAvailable ? originalColor : unavailableColor;
        }
        else
        {
            Debug.LogWarning("backgroundImage is null in UpdateImage");
        }
    }

    private void Update()
    {
        if (cardInfo == null || cardInfo.usageType.Contains(CardInformation.CardUsageType.Passive)) return;
        if (!abilityActive) return;

        currentDuration -= Time.deltaTime;
        float fill = Mathf.Clamp01(currentDuration / cardInfo.cooldownDuration);

        if (durationBar != null)
        {
            Image img = durationBar.GetComponent<Image>();
            if (img != null)
                img.fillAmount = fill;
        }

        if (currentDuration <= 0f)
        {
            abilityActive = false;

            if (durationBar != null)
                durationBar.SetActive(false);

            UpdateImage(true);
        }
    }
    
    public void ResetCooldown()
    {
        abilityActive = false;
        currentDuration = 0f;

        if (durationBar != null)
            durationBar.SetActive(false);

        UpdateImage(true);
    }

    public bool IsOnCooldown()
    {
        return abilityActive;
    }

    public void ApplyTemporaryCooldownMultiplier(float multiplier)
    {
        currentDuration *= multiplier;
    }

}