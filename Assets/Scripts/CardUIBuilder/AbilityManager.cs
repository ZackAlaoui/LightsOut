using System.Collections;
using UnityEngine;
using Game.Player;

public class AbilityManager : MonoBehaviour
{
    private PlayerController _player;
    private FlashlightManager _flashlight;
    
    
    private IEnumerator Start()
    {
        yield return null; // wait one frame

        _player = FindObjectOfType<PlayerController>();
        if (_player == null)
        {
            Debug.LogError("PlayerController not found.");
            yield break;
        }

        _flashlight = _player.Flashlight;
        if (_flashlight == null)
        {
            Debug.LogError("FlashlightManager is null! Make sure it's attached to a child of the Player.");
        }
    }
    
    public void ActivateAbility(string cardName)
    {
        switch (cardName)
        {
            case "Blood Frenzy": StartCoroutine(BloodFrenzy()); break;
            case "Blood Pact": StartCoroutine(BloodPact()); break;
            case "Cerebral Surge": StartCoroutine(CerebralSurge()); break;
            case "Neural Overclock": StartCoroutine(NeuralOverclock()); break;
            case "Bone Plating": StartCoroutine(BonePlating()); break;
            case "Skeletal Adaptation": StartCoroutine(SkeletalAdaptation()); break;
            case "Hemorrhage Flow": StartCoroutine(HemorrhageFlow()); break;
            case "Putrid Regrowth": StartCoroutine(PutridRegrowth()); break;
            case "Fungal Shell": StartCoroutine(FungalShell()); break;
            case "Necrotic Burst": StartCoroutine(NecroticBurst()); break;
            default: Debug.LogWarning("Unknown ability: " + cardName); break;
        }
    }

    private IEnumerator BloodFrenzy()
    {
        _player.MovementSpeedMultiplier *= 2f;
        yield return new WaitForSeconds(7f);
        _player.MovementSpeedMultiplier /= 2f;
    }

    private IEnumerator BloodPact()
    {
        float hpLoss = _player.Health * 0.1f;
        _player.Health -= hpLoss;
        _player.DamageMultiplier *= 2f;
        yield return new WaitForSeconds(10f);
        _player.DamageMultiplier /= 2f;
    }

    private IEnumerator CerebralSurge()
    {
        float originalIntensity = _flashlight.IntensityMultiplier;
        _flashlight.IntensityMultiplier *= 1.25f;
        yield return new WaitForSeconds(10f);
        _flashlight.IntensityMultiplier = originalIntensity;
    }

    private IEnumerator NeuralOverclock()
    {
        _player.MovementSpeedMultiplier *= 2f;
        yield return new WaitForSeconds(5f);
        _player.MovementSpeedMultiplier /= 2f;
    }

    private IEnumerator BonePlating()
    {
        float duration = 10f;
        float tickRate = 0.5f;
        int ticks = Mathf.FloorToInt(duration / tickRate);
        float damageAbsorption = 0.5f;

        for (int i = 0; i < ticks; i++)
        {
            _player.Flashlight.RemainingBatteryLife += damageAbsorption;
            yield return new WaitForSeconds(tickRate);
        }
    }

    private IEnumerator SkeletalAdaptation()
    {
        float duration = 8f;
        float healPerSecond = 0.3f;
        float time = 0f;

        while (time < duration)
        {
            _player.Health += healPerSecond;
            time += 1f;
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator HemorrhageFlow()
    {
        float duration = 6f;
        float baseDamage = _player.DamageMultiplier;

        while (duration > 0)
        {
            float hpPercent = _player.Health / 5f; // Assume 5 base HP
            _player.DamageMultiplier = Mathf.Lerp(baseDamage, baseDamage * 2f, 1f - hpPercent);
            duration -= Time.deltaTime;
            yield return null;
        }

        _player.DamageMultiplier = baseDamage;
    }

    private IEnumerator PutridRegrowth()
    {
        float duration = 10f;
        float healPerSecond = 0.4f;

        for (float t = 0; t < duration; t += 1f)
        {
            _player.Health += healPerSecond;
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator FungalShell()
    {
        _player.MovementSpeedMultiplier *= 0.7f;
        float originalBattery = _flashlight.RemainingBatteryLife;
        _flashlight.RemainingBatteryLife = Mathf.Clamp(_flashlight.RemainingBatteryLife + 10f, 0f, 30f);
        yield return new WaitForSeconds(6f);
        _player.MovementSpeedMultiplier /= 0.7f;
        _flashlight.RemainingBatteryLife = originalBattery;
    }

    private IEnumerator NecroticBurst()
    {
        float originalIntensity = _flashlight.IntensityMultiplier;
        float originalBattery = _flashlight.RemainingBatteryLife;

        _flashlight.RemainingBatteryLife = 30f;
        _flashlight.IntensityMultiplier = 2f;

        yield return new WaitForSeconds(8f);

        _flashlight.RemainingBatteryLife = originalBattery;
        _flashlight.IntensityMultiplier = originalIntensity;
    }
}
