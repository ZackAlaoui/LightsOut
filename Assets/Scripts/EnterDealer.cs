using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterDealer : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "DrawCard";
    
    private bool playerInRange = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Check if player is in range and presses E key
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            LoadDrawCardScene();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player entered dealer range. Press E to enter.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the exiting object is the player
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Player left dealer range.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is the player (for 3D colliders)
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player entered dealer range. Press E to enter.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the exiting object is the player (for 3D colliders)
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Player left dealer range.");
        }
    }

    private void LoadDrawCardScene()
    {
        Debug.Log("Loading DrawCard scene...");
        SceneManager.LoadScene(sceneToLoad);
    }
}
