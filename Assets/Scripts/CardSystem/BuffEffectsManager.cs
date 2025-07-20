using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffEffectsManager : MonoBehaviour
{
    public static BuffEffectsManager Instance;
    
    [Header("Visual Effects")]
    public GameObject playerBuffAura; // Parent object for all visual effects
    public ParticleSystem[] suitParticles = new ParticleSystem[4]; // 0=Brains, 1=Bones, 2=Blood, 3=RottenFlesh
    public Light buffLight; // Dynamic light for buffs
    public Image screenOverlay; // For screen tinting effects
    public GameObject buffIconPrefab; // UI icon for active buffs
    public Transform buffIconContainer; // UI container for buff icons
    
    [Header("Audio")]
    public AudioSource buffAudioSource;
    public AudioClip[] suitActivationSounds = new AudioClip[4]; // Activation sounds for each suit
    public AudioClip[] comboSounds = new AudioClip[3]; // 0=Pair, 1=TwoPair/FullHouse, 2=Flush/FourOfKind
    public AudioClip buffExpiredSound;
    
    [Header("Screen Shake")]
    public Camera playerCamera;
    public float shakeIntensity = 0.5f;
    public float shakeDuration = 0.3f;
    
    private Dictionary<string, BuffVisualEffect> activeBuffEffects = new();
    private Dictionary<string, GameObject> buffIcons = new();
    private Coroutine shakeCoroutine;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    public void PlayPokerHandEffect(string comboType, CardSuit primarySuit, CardSuit secondarySuit = CardSuit.Brains)
    {
        // Play combo sound
        int soundIndex = comboType switch
        {
            "pair" => 0,
            "twopair" or "fullhouse" => 1,
            "flush" or "four" => 2,
            _ => 0
        };
        
        if (comboSounds[soundIndex] != null)
            buffAudioSource.PlayOneShot(comboSounds[soundIndex]);
        
        // Screen shake for bigger combos
        if (comboType == "flush" || comboType == "four")
            StartScreenShake(shakeIntensity * 2f, shakeDuration * 1.5f);
        else if (comboType == "fullhouse" || comboType == "twopair")
            StartScreenShake(shakeIntensity * 1.5f, shakeDuration);
        else
            StartScreenShake(shakeIntensity, shakeDuration * 0.5f);
        
        // Play suit-specific particle effect
        PlaySuitParticleEffect(primarySuit, comboType);
        if (secondarySuit != primarySuit && (comboType == "twopair" || comboType == "fullhouse"))
            PlaySuitParticleEffect(secondarySuit, comboType);
    }
    
    private void PlaySuitParticleEffect(CardSuit suit, string comboType)
    {
        int suitIndex = (int)suit;
        if (suitIndex < suitParticles.Length && suitParticles[suitIndex] != null)
        {
            var emission = suitParticles[suitIndex].emission;
            var main = suitParticles[suitIndex].main;
            
            // Adjust particle intensity based on combo type
            int burstCount = comboType switch
            {
                "flush" or "four" => 50,
                "fullhouse" or "twopair" => 30,
                _ => 20
            };
            
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0.0f, burstCount)
            });
            
            // Set suit-specific colors
            Color suitColor = suit switch
            {
                CardSuit.Brains => new Color(0.7f, 0.3f, 1f), // Purple
                CardSuit.Bones => new Color(0.9f, 0.9f, 0.8f), // Bone white
                CardSuit.Blood => new Color(1f, 0.2f, 0.2f), // Red
                CardSuit.RottenFlesh => new Color(0.5f, 0.8f, 0.2f), // Sickly green
                _ => Color.white
            };
            
            main.startColor = suitColor;
            suitParticles[suitIndex].Play();
        }
    }
    
    public void ActivateBuffEffect(string buffName, float duration, CardSuit primarySuit)
    {
        // Remove existing effect if present
        if (activeBuffEffects.ContainsKey(buffName))
            DeactivateBuffEffect(buffName);
        
        // Create new visual effect
        BuffVisualEffect effect = new BuffVisualEffect
        {
            buffName = buffName,
            duration = duration,
            remainingTime = duration,
            primarySuit = primarySuit
        };
        
        activeBuffEffects[buffName] = effect;
        
        // Create UI icon
        CreateBuffIcon(buffName, duration, primarySuit);
        
        // Apply visual effects based on buff type
        ApplyBuffVisuals(buffName, primarySuit);
        
        // Play activation sound
        int suitIndex = (int)primarySuit;
        if (suitIndex < suitActivationSounds.Length && suitActivationSounds[suitIndex] != null)
            buffAudioSource.PlayOneShot(suitActivationSounds[suitIndex]);
    }
    
    private void CreateBuffIcon(string buffName, float duration, CardSuit suit)
    {
        if (buffIconPrefab == null || buffIconContainer == null) return;
        
        GameObject icon = Instantiate(buffIconPrefab, buffIconContainer);
        buffIcons[buffName] = icon;
        
        // Set icon properties
        Image iconImage = icon.GetComponent<Image>();
        if (iconImage != null)
        {
            // Set suit-specific icon color
            Color suitColor = suit switch
            {
                CardSuit.Brains => new Color(0.7f, 0.3f, 1f),
                CardSuit.Bones => new Color(0.9f, 0.9f, 0.8f),
                CardSuit.Blood => new Color(1f, 0.2f, 0.2f),
                CardSuit.RottenFlesh => new Color(0.5f, 0.8f, 0.2f),
                _ => Color.white
            };
            iconImage.color = suitColor;
        }
        
        // Add buff name text
        Text buffText = icon.GetComponentInChildren<Text>();
        if (buffText != null)
            buffText.text = GetBuffDisplayName(buffName);
    }
    
    private void ApplyBuffVisuals(string buffName, CardSuit suit)
    {
        // Apply light effects
        if (buffLight != null)
        {
            Color lightColor = suit switch
            {
                CardSuit.Brains => new Color(0.7f, 0.3f, 1f),
                CardSuit.Bones => new Color(0.9f, 0.9f, 0.8f),
                CardSuit.Blood => new Color(1f, 0.2f, 0.2f),
                CardSuit.RottenFlesh => new Color(0.5f, 0.8f, 0.2f),
                _ => Color.white
            };
            
            buffLight.color = lightColor;
            buffLight.intensity = 1.5f;
        }
        
        // Apply screen overlay effects for major buffs
        if (screenOverlay != null && IsMajorBuff(buffName))
        {
            Color overlayColor = suit switch
            {
                CardSuit.Brains => new Color(0.7f, 0.3f, 1f, 0.1f),
                CardSuit.Bones => new Color(0.9f, 0.9f, 0.8f, 0.1f),
                CardSuit.Blood => new Color(1f, 0.2f, 0.2f, 0.1f),
                CardSuit.RottenFlesh => new Color(0.5f, 0.8f, 0.2f, 0.1f),
                _ => new Color(1f, 1f, 1f, 0.1f)
            };
            
            screenOverlay.color = overlayColor;
            screenOverlay.gameObject.SetActive(true);
        }
    }
    
    public void DeactivateBuffEffect(string buffName)
    {
        if (activeBuffEffects.ContainsKey(buffName))
        {
            activeBuffEffects.Remove(buffName);
            
            // Remove UI icon
            if (buffIcons.ContainsKey(buffName))
            {
                Destroy(buffIcons[buffName]);
                buffIcons.Remove(buffName);
            }
            
            // Play expiration sound
            if (buffExpiredSound != null)
                buffAudioSource.PlayOneShot(buffExpiredSound, 0.5f);
            
            // Check if we should disable screen overlay
            if (screenOverlay != null && !HasMajorBuffs())
            {
                screenOverlay.gameObject.SetActive(false);
            }
        }
    }
    
    private void Update()
    {
        // Update buff timers and effects
        List<string> expiredBuffs = new();
        
        foreach (var kvp in activeBuffEffects)
        {
            string buffName = kvp.Key;
            BuffVisualEffect effect = kvp.Value;
            
            effect.remainingTime -= Time.deltaTime;
            
            if (effect.remainingTime <= 0)
            {
                expiredBuffs.Add(buffName);
            }
            else
            {
                // Update UI icon timer
                UpdateBuffIcon(buffName, effect.remainingTime / effect.duration);
                
                // Pulse effect for buffs about to expire
                if (effect.remainingTime <= 3f)
                {
                    float pulseIntensity = Mathf.Sin(Time.time * 5f) * 0.3f + 0.7f;
                    if (buffLight != null)
                        buffLight.intensity = pulseIntensity;
                }
            }
        }
        
        // Remove expired buffs
        foreach (string buffName in expiredBuffs)
        {
            DeactivateBuffEffect(buffName);
        }
    }
    
    private void UpdateBuffIcon(string buffName, float normalizedTime)
    {
        if (buffIcons.ContainsKey(buffName))
        {
            GameObject icon = buffIcons[buffName];
            Image fillImage = icon.transform.Find("Fill")?.GetComponent<Image>();
            if (fillImage != null)
                fillImage.fillAmount = normalizedTime;
        }
    }
    
    private void StartScreenShake(float intensity, float duration)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);
        
        shakeCoroutine = StartCoroutine(ScreenShakeCoroutine(intensity, duration));
    }
    
    private System.Collections.IEnumerator ScreenShakeCoroutine(float intensity, float duration)
    {
        Vector3 originalPos = playerCamera.transform.localPosition;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            
            playerCamera.transform.localPosition = new Vector3(x, y, originalPos.z);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        playerCamera.transform.localPosition = originalPos;
    }
    
    private bool IsMajorBuff(string buffName)
    {
        return buffName.Contains("Ultimate") || buffName.Contains("Berserker") || 
               buffName.Contains("Flush") || buffName.Contains("Executioner");
    }
    
    private bool HasMajorBuffs()
    {
        foreach (string buffName in activeBuffEffects.Keys)
        {
            if (IsMajorBuff(buffName))
                return true;
        }
        return false;
    }
    
    private string GetBuffDisplayName(string buffName)
    {
        return buffName switch
        {
            "UltimateCooldownReduction" => "Ultimate CDR",
            "StrongerLightBeam" => "Bright Light",
            "ArmorUp" => "Armor",
            "KnockbackResistance" => "Stability",
            "Lifesteal" => "Lifesteal",
            "BleedOnHit" => "Bleeding",
            "ToxicAura" => "Toxic Aura",
            "InfectionSpread" => "Infection",
            "CooldownReduction" => "CDR",
            "EnergyRegen" => "Energy",
            "HealthRegen" => "Regen",
            "AttackSpeed" => "Fast Attack",
            "DamageBoost" => "Damage+",
            "MovementSpeedBoost" => "Speed+",
            _ => buffName
        };
    }
}

[System.Serializable]
public class BuffVisualEffect
{
    public string buffName;
    public float duration;
    public float remainingTime;
    public CardSuit primarySuit;
}