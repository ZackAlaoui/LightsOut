using System.Collections;
using UnityEngine;
using Game.Player;
using Game.Enemy;

public class AbilityManager : MonoBehaviour
{
    private PlayerController _player;
    private FlashlightManager _flashlight;
    
    
    private IEnumerator Start()
    {
        yield return null; // wait one frame

        _player = PlayerController.Instance;
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
            // 🔴 Blood
            case "Blood Frenzy": StartCoroutine(BloodFrenzy()); break;
            case "Blood Pact": StartCoroutine(BloodPact()); break;
            case "Blood Bank": StartCoroutine(BloodBank()); break;
            case "Crimson Howl": StartCoroutine(CrimsonHowl()); break;

            // 🧠 Brains
            case "Battery Freeze": StartCoroutine(BatteryFreeze()); break;
            case "Synaptic Reflex": StartCoroutine(SynapticReflex()); break;
            case "Cognitive Loop": StartCoroutine(CognitiveLoop()); break;
            case "Eidetic Recall": StartCoroutine(EideticRecall()); break;

            // 🦴 Bones
            case "Bone Plating": StartCoroutine(BonePlating()); break;
            case "Ivory Guard": StartCoroutine(IvoryGuard()); break;
            case "Calcium Surge": StartCoroutine(CalciumSurge()); break;
            case "Fractured Payback": StartCoroutine(FracturedPayback()); break;

            // 💀 Rotten Flesh
            case "Putrid Regrowth": StartCoroutine(PutridRegrowth()); break;
            case "Fungal Carapace": StartCoroutine(FungalCarapace()); break;
            case "Corpse Bloom": StartCoroutine(CorpseBloom()); break;
            case "Rotten Surge": StartCoroutine(RottenSurge()); break;

            // 🌑 Void
            case "Voidstep": StartCoroutine(Voidstep()); break;
            case "Nullskin": StartCoroutine(Nullskin()); break;
            case "Entropy Loop": StartCoroutine(EntropyLoop()); break;

            // 🟣 Passives (can also be triggered manually, but handled elsewhere)
            case "Spinal Shield": StartCoroutine(SpinalShield()); break;
            case "Decay Bloom": StartCoroutine(DecayBloom()); break;
            case "Slipstream Echo": StartCoroutine(SlipstreamEcho()); break;

            default:
                Debug.LogWarning("Unknown ability: " + cardName);
                break;
        }
    }

    
    private int _cardsUsedThisRun = 0;

    private void ResetAllCooldowns()
    {
        var cards = FindObjectsOfType<CardAbilityUI>();
        foreach (var card in cards)
        {
            card.ResetCooldown();
        }
    }

    private void TryDiscardPassive(string passiveName)
    {
        HandManager hand = FindObjectOfType<HandManager>();
        if (hand == null) return;

        var handCards = hand.GetCurrentHand();
        for (int i = 0; i < handCards.Count; i++)
        {
            if (handCards[i].cardName == passiveName)
            {
                hand.DiscardCard(i);
                break;
            }
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
    
    private IEnumerator BloodBank()
    {
        _player.Health += 2f; // Instant heal
        _player.MovementSpeedMultiplier *= 1.25f;

        yield return new WaitForSeconds(5f);

        _player.MovementSpeedMultiplier /= 1.25f;
    }
    
    private IEnumerator CrimsonHowl()
    {
        EnemyController[] allEnemies = FindObjectsOfType<EnemyController>();
        foreach (var enemy in allEnemies)
        {
            float dist = Vector3.Distance(enemy.transform.position, _player.transform.position);
            if (dist <= 3f)
            {
                Vector3 dir = (enemy.transform.position - _player.transform.position).normalized;
            
                // Optional knockback handling: you must implement this in your EnemyController/ZombieController/etc.
                if (enemy.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    rb.AddForce(dir * 5f, ForceMode.Impulse); // Knockback force
                }
            }
        }

        _player.DamageMultiplier *= 1.5f;
        yield return new WaitForSeconds(5f);
        _player.DamageMultiplier /= 1.5f;
    }




    private IEnumerator BatteryFreeze()
    {
        _flashlight.BatteryFrozen = true;
        yield return new WaitForSeconds(8f);
        _flashlight.BatteryFrozen = false;
    }

    private IEnumerator SynapticReflex()
    {
        Time.timeScale = 0.5f;
        _player.MovementSpeedMultiplier *= 2f;
        yield return new WaitForSecondsRealtime(4f); // Use unscaled time due to timeScale change
        _player.MovementSpeedMultiplier /= 2f;
        Time.timeScale = 1f;
    }

    private IEnumerator CognitiveLoop()
    {
        // Dim flashlight
        _flashlight.IntensityMultiplier *= 0.75f;

        // Find all card UI components
        CardAbilityUI[] allCards = FindObjectsOfType<CardAbilityUI>();
        foreach (var card in allCards)
        {
            card.gameObject.AddComponent<CooldownScaler>().ApplyMultiplier(0.33f); // 3x faster cooldown
        }

        yield return new WaitForSeconds(6f);

        foreach (var card in allCards)
        {
            CooldownScaler scaler = card.GetComponent<CooldownScaler>();
            if (scaler != null) Destroy(scaler); // Remove effect after duration
        }

        _flashlight.IntensityMultiplier /= 0.75f;
    }


    private string _lastCardUsed = null;

    public void RegisterCardUse(string cardName)
    {
        _lastCardUsed = cardName;
        _cardsUsedThisRun++;

        if (_cardsUsedThisRun >= 3)
        {
            ResetAllCooldowns();
            TryDiscardPassive("Eidetic Insight");
            _cardsUsedThisRun = 0;
        }
    }

    private IEnumerator EideticRecall()
    {
        if (!string.IsNullOrEmpty(_lastCardUsed) && _lastCardUsed != "Eidetic Recall")
        {
            Debug.Log("Replaying: " + _lastCardUsed);
            ActivateAbility(_lastCardUsed);
        }
        else
        {
            Debug.Log("No valid card to recall.");
        }
        yield return null;
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

    private IEnumerator IvoryGuard()
    {
        _player.DamageResistanceMultiplier *= 0.6f; // 40% damage reduction
        yield return new WaitForSeconds(8f);
        _player.DamageResistanceMultiplier /= 0.6f;
    }

    private IEnumerator CalciumSurge()
    {
        _player.Health += 2f;              // Instant heal
        _player.IncreaseBaseMaxHealth(1f); // Permanent max HP increase
        yield return null;
    }


    private bool _spinalShieldTriggered = false;
    private IEnumerator SpinalShield()
    {
        while (!_spinalShieldTriggered)
        {
            if (_player.Health / _player.MaxHealth <= 0.3f)
            {
                _spinalShieldTriggered = true;
                _player.IsInvincible = true;
                yield return new WaitForSeconds(3f);
                _player.IsInvincible = false;

                HandManager hand = FindObjectOfType<HandManager>();
                if (hand != null)
                {
                    var handCards = hand.GetCurrentHand();
                    for (int i = 0; i < handCards.Count; i++)
                    {
                        if (handCards[i].cardName == "Spinal Shield")
                        {
                            hand.DiscardCard(i);
                            break;
                        }
                    }
                }
            }
            yield return null;
        }
        _spinalShieldTriggered = false;
    }

    private IEnumerator FracturedPayback()
    {
        float duration = 6f;
        float aoeRadius = 3f;
        float retaliateDamage = 2f;

        PlayerController.PlayerHit onHit = () =>
        {
            Collider[] hitColliders = Physics.OverlapSphere(_player.transform.position, aoeRadius);
            foreach (var col in hitColliders)
            {
                if (col.TryGetComponent<EnemyController>(out var enemy))
                {
                    enemy.Damage(_player, retaliateDamage);
                }
            }
        };

        _player.OnHit += onHit;

        yield return new WaitForSeconds(duration);

        _player.OnHit -= onHit;
    }


    private IEnumerator FungalCarapace()
    {
        _player.DamageResistanceMultiplier *= 0.3f;  // Take 70% less damage
        _player.MovementSpeedMultiplier *= 0.5f;     // Move slower
        yield return new WaitForSeconds(6f);
        _player.DamageResistanceMultiplier /= 0.3f;
        _player.MovementSpeedMultiplier /= 0.5f;
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

    private IEnumerator CorpseBloom()
    {
        float duration = 8f;
        float radius = 3f;
        float healAmount = 0.5f;

        void OnEnemyDeath(Vector3 deathPosition)
        {
            float dist = Vector3.Distance(_player.transform.position, deathPosition);
            if (dist <= radius)
            {
                _player.Health += healAmount;
            }
        }

        EnemyManager.OnEnemyDied += OnEnemyDeath;
        yield return new WaitForSeconds(duration);
        EnemyManager.OnEnemyDied -= OnEnemyDeath;
    }

    private IEnumerator RottenSurge()
    {
        _player.Health += _player.MaxHealth * 0.5f;  // Heal 50% HP

        float damageOverTime = 2f;
        float duration = 10f;
        float interval = 1f;
        float damagePerTick = damageOverTime / (duration / interval);

        for (float t = 0; t < duration; t += interval)
        {
            _player.Health -= damagePerTick;
            yield return new WaitForSeconds(interval);
        }
    }
    
    private bool _decayBloomTriggered = false;
    private IEnumerator DecayBloom()
    {
        while (!_decayBloomTriggered)
        {
            if (_player.Health / _player.MaxHealth <= 0.25f)
            {
                _decayBloomTriggered = true;

                float radius = 4f;
                float damage = 2f;

                Collider[] hitColliders = Physics.OverlapSphere(_player.transform.position, radius);
                foreach (var col in hitColliders)
                {
                    if (col.TryGetComponent<EnemyController>(out var enemy))
                    {
                        enemy.Damage(_player, damage);
                    }
                }

                TryDiscardPassive("Decay Bloom");
            }

            yield return null;
        }

        _decayBloomTriggered = false;
    }


    private IEnumerator Voidstep()
    {
        _player.IsInvincible = true;
        yield return new WaitForSeconds(1.5f);
        _player.IsInvincible = false;
    }

    private IEnumerator Nullskin()
    {
        bool absorbed = false;

        PlayerController.PlayerHit handler = () =>
        {
            if (!absorbed)
            {
                absorbed = true;
                _player.IsInvincible = true;
            }
        };

        _player.OnHit += handler;

        float timer = 0f;
        while (timer < 8f && !absorbed)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        _player.OnHit -= handler;
        _player.IsInvincible = false;
    }

    private IEnumerator EntropyLoop()
    {
        _player.Health += 2f;

        HandManager hand = FindObjectOfType<HandManager>();
        if (hand != null)
        {
            var cards = hand.GetCurrentHand();
            if (cards.Count > 1) // Don’t discard the card you're using
            {
                int discardIndex = Random.Range(0, cards.Count);
                hand.DiscardCard(discardIndex);
            }
        }

        yield return null;
    }

    private bool _waitingForSprint = false;

    private IEnumerator SlipstreamEcho()
    {
        _waitingForSprint = true;

        while (_waitingForSprint)
        {
            if (!_player.IsSprinting)
            {
                _waitingForSprint = false;

                // Apply cooldown reduction to next available card
                CardAbilityUI[] cards = FindObjectsOfType<CardAbilityUI>();
                foreach (var card in cards)
                {
                    // Make sure you have these methods implemented in CardAbilityUI
                    if (!card.IsOnCooldown())
                    {
                        card.ApplyTemporaryCooldownMultiplier(0.5f);
                        break;
                    }
                }

                TryDiscardPassive("Slipstream Echo");
            }

            yield return null;
        }
    }
    
    public void TriggerPassive(string cardName)
    {
        Debug.Log("Triggering passive: " + cardName);
        StartCoroutine(ActivatePassiveAbility(cardName));
    }

    private IEnumerator ActivatePassiveAbility(string cardName)
    {
        switch (cardName)
        {
            case "Last Light":
                _player.Flashlight.RemainingBatteryLife = 30f;
                break;

            case "Decay Bloom":
                float radius = 4f;
                float damage = 2f;
                Collider[] enemies = Physics.OverlapSphere(_player.transform.position, radius);
                foreach (var col in enemies)
                {
                    if (col.TryGetComponent<EnemyController>(out var enemy))
                        enemy.Damage(_player, damage);
                }
                break;

            case "Slipstream Echo":
                yield return new WaitForEndOfFrame(); // Let sprinting stop be detected
                CardAbilityUI[] cards = FindObjectsOfType<CardAbilityUI>();
                foreach (var card in cards)
                {
                    if (!card.IsOnCooldown())
                    {
                        card.ApplyTemporaryCooldownMultiplier(0.5f);
                        break;
                    }
                }
                break;
        }

        TryDiscardPassive(cardName); // Will remove the card
    }

    
}
