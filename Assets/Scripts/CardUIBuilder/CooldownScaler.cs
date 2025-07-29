using UnityEngine;

public class CooldownScaler : MonoBehaviour
{
    private CardAbilityUI cardUI;
    private float multiplier = 1f;

    private void Awake()
    {
        cardUI = GetComponent<CardAbilityUI>();
    }

    public void ApplyMultiplier(float factor)
    {
        multiplier = factor;
    }

    private void Update()
    {
        if (cardUI == null || multiplier == 1f) return;

        var field = typeof(CardAbilityUI).GetField("currentDuration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            float current = (float)field.GetValue(cardUI);
            current -= Time.deltaTime * (1f - multiplier); // speed up decay
            field.SetValue(cardUI, current);
        }
    }
}