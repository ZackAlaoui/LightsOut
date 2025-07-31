using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardData;
using TMPro;
using UnityEngine.UI; // For Image
using Game.Player;

public class PauseMenu : MonoBehaviour
{
    public static bool IsPausingEnabled = true;
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    public GameObject cardInfoPanel;
    public TMP_Text cardInfoText;
    public GameObject resumeButton;
    public GameObject currentCardsButton;
    public GameObject quitButton;

    private void Start()
    {
        Debug.Log("PauseMenu initialized.");

        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false); // Force it visible for testing
            Debug.Log("pauseMenuUI forced visible in Start().");
        }
        else
        {
            Debug.LogWarning("pauseMenuUI is not assigned in Inspector.");
        }

        if (cardInfoPanel != null)
            cardInfoPanel.SetActive(false);

        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log($"Escape pressed. IsPausingEnabled: {IsPausingEnabled}, GameIsPaused: {GameIsPaused}");
        }

        if (IsPausingEnabled && Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        Debug.Log("Resuming game...");
        PlayerController.Instance.IsGunEnabled = true;

        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (cardInfoPanel != null) cardInfoPanel.SetActive(false);

        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    private void Pause()
    {
        Debug.Log("Pausing game...");
        PlayerController.Instance.IsGunEnabled = false;

        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        else Debug.LogWarning("pauseMenuUI is null!");

        if (cardInfoPanel != null) cardInfoPanel.SetActive(false);

        if (resumeButton != null) resumeButton.SetActive(true);
        else Debug.LogWarning("resumeButton is null!");
        if (currentCardsButton != null) currentCardsButton.SetActive(true);
        else Debug.LogWarning("currentCardsButton is null!");
        if (quitButton != null) quitButton.SetActive(true);
        else Debug.LogWarning("quitButton is null!");

        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void CurrentCardInformation()
    {
        Debug.Log("Current Card Information loading...");

        List<CardInformation> cards = HandManager.GetCurrentHand();

        if (cards == null || cards.Count == 0)
        {
            cardInfoText.text = "You are not holding any cards.";
        }
        else
        {
            cardInfoText.text = "";
            foreach (CardInformation card in cards)
            {
                cardInfoText.text += $"<b>{card.cardName}</b>\n{card.abilityDesc}\n\n";
            }

            PokerHandManager poker = FindObjectOfType<PokerHandManager>();
            if (poker != null)
            {
                cardInfoText.text += $"<b>Current Poker Hand:</b>\n{poker.CurrentPokerHandDescription}";
            }
        }

        resumeButton.SetActive(false);
        currentCardsButton.SetActive(false);
        quitButton.SetActive(false);
        cardInfoPanel.SetActive(true);
    }

    public void Quit()
    {
        Debug.Log("Quitting Game");
        Application.Quit();
    }

    public void BackToPauseMenu()
    {
        cardInfoPanel.SetActive(false);
        resumeButton.SetActive(true);
        currentCardsButton.SetActive(true);
        quitButton.SetActive(true);
    }
}
