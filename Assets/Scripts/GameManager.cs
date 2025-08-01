using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Enemy;
using static Game.Enemy.EnemyManager;
using System.Collections;
using Game.Player;

using UnityEngine.InputSystem;
using UnityEditorInternal;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager s_instance;                                  //Singleton instance for the GameManager
        [SerializeField] private GameObject _enemyManagerPrefab;                //EnemyManager prefab 
        public static EnemyManager EnemyManager { get; private set; }           //Getter and setter for the EnemyManager
        [SerializeField] private GameObject _batteryManagerPrefab;              //Gameobject for the battery manager
        public static BatteryManager BatteryManager { get; private set; }       //Getter and setter for the BatteryManager
        [SerializeField] private GameObject _audioManagerPrefab;
        public static AudioManager AudioManager { get; private set; }
        [SerializeField] private GameObject _deckManagerPrefab;
        public static DeckManager DeckManager { get; private set; }
        [SerializeField] private GameObject _handUIPrefab;
        [SerializeField] private GameObject _portalPrefab;
        public static GameObject HandUI { get; private set; } 

        public static int CurrentRound { get; private set; } = 0;

        // Tracks the currently loaded scene name as well as the previously active scene. This
        // allows other systems (e.g. UI overlays such as the PlayingCardDrawer) to return the
        // player back to the correct level after temporarily loading a different scene.
        public static string CurrentSceneName { get; private set; }
        public static string PreviousSceneName { get; private set; }

        static bool allowActivation = false;

        private GameManager() { }   //Constructor for the GameManager

        private void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Debug.LogWarning("GameManager has already been instantiated. Deleting duplicate GameManager.");
                Destroy(this.gameObject);
                return;
            }
            else
            {
                s_instance = this;
            }

            if (_enemyManagerPrefab == null) throw new NullReferenceException("EnemyManager prefab is null.");
            if (_batteryManagerPrefab == null) throw new NullReferenceException("BatteryManager prefab is null.");
            if (_audioManagerPrefab == null) throw new NullReferenceException("AudioManager prefab is null.");
            if (_deckManagerPrefab == null) throw new NullReferenceException("DeckManager prefab is null.");
            if (_handUIPrefab == null) throw new NullReferenceException("HandManager prefab is null.");

            EnemyManager = Instantiate(_enemyManagerPrefab, transform).GetComponent<EnemyManager>();
            BatteryManager = Instantiate(_batteryManagerPrefab, transform).GetComponent<BatteryManager>();
            AudioManager = Instantiate(_audioManagerPrefab, transform).GetComponent<AudioManager>();
            DeckManager = Instantiate(_deckManagerPrefab, transform).GetComponent<DeckManager>();

            // Register for scene change callbacks so we can keep track of which scene the game
            // is currently on and which one was active previously.
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            // Initialize the scene tracking variables.
            CurrentSceneName = SceneManager.GetActiveScene().name;
            PreviousSceneName = CurrentSceneName;
        }

        
        private void OnEnable()
        {

            //If we disable the portal then the portal will not be active
            //If we enable the portal then the portal will be active
            if (!allowActivation)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }

        private void OnDisable()
        {
            //Once we leave this will make sure the portal is deactivated
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "FirstMap")
            {
                //Deactivate portal
                s_instance._portalPrefab = GameObject.Find("Portal");
                if (s_instance._portalPrefab != null)
                {
                    allowActivation = false;
                    s_instance._portalPrefab.SetActive(false);
                }
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            DontDestroyOnLoad(gameObject);
            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("FirstMap"))
            {
                StartGame();
            }
            else if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("BossFight"))
            {
                EnemyManager.BuildNavMeshes();
                EnemyManager.SpawnEnemies(EnemyType.Zombie, 20);
                EnemyManager.SpawnEnemies(EnemyType.Ghost, 10);
            }
#if DEBUG
            InputSystem.actions.FindAction("Interact").performed += (InputAction.CallbackContext context) => { Debug.Log("Next Round"); StartCoroutine(GameManager.NextRound()); };
#endif
        }

        public static void StartGame()
        {
            CurrentRound = 0;
            s_instance.StartCoroutine(NextRound());
        }

        public static IEnumerator NextRound()
        {
            ++CurrentRound;
            Debug.Log($"{GameManager.CurrentRound}");
            EnemyManager.KillAll();
            BatteryManager.DeleteAll();
            switch (CurrentRound)
            {
                case 1:
                    if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("FirstMap"))
                    {
                        yield return TransitionManager.LoadLevel("FirstMap");
                        TransitionManager.instance.ShowRound("1");
                    }
                    HandUI = Instantiate(s_instance._handUIPrefab, s_instance.transform); //This will hold the cards for the player
                    EnemyManager.BuildNavMeshes();
                    EnemyManager.SpawnEnemies(EnemyType.Zombie, 1);
                    //EnemyManager.SpawnEnemies(EnemyType.Ghost, 10);
                    BatteryManager.SpawnBatteries(15);
                    break;
                case 2:
                    TransitionManager.instance.ShowRound("2");
                    EnemyManager.SpawnEnemies(EnemyType.Zombie, 1);
                    //EnemyManager.SpawnEnemies(EnemyType.Ghost, 10);
                    BatteryManager.SpawnBatteries(15);
                    break;
                case 3:
                    TransitionManager.instance.ShowRound("3");
                    EnemyManager.SpawnEnemies(EnemyType.Zombie, 1);
                    //EnemyManager.SpawnEnemies(EnemyType.Ghost, 15);
                    BatteryManager.SpawnBatteries(15);
                    break;
                case 4:
                    TransitionManager.instance.ShowRound("Portal Spawning");
                    allowActivation = true;                     //Allow the portal to be active
                    s_instance._portalPrefab.SetActive(true);   //Activate portal when round 3 finishes
                    //Set the player health to unlimited in the dungeon
                    PlayerController playerController = PlayerController.Instance;
                    playerController.Health = 5f; // Set to a high value for the dungeon
                    //Make sure the current player gameobject is sent to the dungeon scene
                    break;
                case 5:
                    PlayerController.Instance.gameObject.SetActive(true);
                    yield return TransitionManager.LoadLevel("FirstMap");
                    TransitionManager.instance.ShowRound("4");
                    EnemyManager.BuildNavMeshes();
                    EnemyManager.SpawnEnemies(EnemyType.Zombie, 1);
                    //EnemyManager.SpawnEnemies(EnemyType.Ghost, 15);
                    BatteryManager.SpawnBatteries(15);
                    break;
                case 6:
                    TransitionManager.instance.ShowRound("5");
                    EnemyManager.SpawnEnemies(EnemyType.Zombie, 1);
                    //EnemyManager.SpawnEnemies(EnemyType.Ghost, 15);
                    BatteryManager.SpawnBatteries(7);
                    break;
                case 7:
                    TransitionManager.instance.ShowRound("6");
                    EnemyManager.SpawnEnemies(EnemyType.Zombie, 1);
                    //EnemyManager.SpawnEnemies(EnemyType.Ghost, 20);
                    BatteryManager.SpawnBatteries(7);
                    break;
                case 8:
                    // Dungeon
                    allowActivation = true;                     //Allow the portal to be active
                    s_instance._portalPrefab.SetActive(true);   //Activate portal when round 3 finishes
                    //Set the player health to unlimited in the dungeon
                    playerController = PlayerController.Instance;
                    playerController.Health = 5f; // Set to a high value for the dungeon
                    //Make sure the current player gameobject is sent to the dungeon scene
                    break;
                case 9:
                    // FirstMap
                    PlayerController.Instance.gameObject.SetActive(true);
                    yield return TransitionManager.LoadLevel("FirstMap");
                    TransitionManager.instance.ShowRound("7");
                    EnemyManager.BuildNavMeshes();
                    EnemyManager.SpawnEnemies(EnemyType.Zombie, 1);
                    //EnemyManager.SpawnEnemies(EnemyType.Ghost, 25);
                    BatteryManager.SpawnBatteries(7);
                    break;
                case 10:
                    // FirstMap
                    TransitionManager.instance.ShowRound("8");
                    EnemyManager.SpawnEnemies(EnemyType.Zombie, 1);
                    //EnemyManager.SpawnEnemies(EnemyType.Ghost, 25);
                    BatteryManager.SpawnBatteries(7);
                    break;
                case 11:
                    // FirstMap
                    TransitionManager.instance.ShowRound("9");
                    EnemyManager.SpawnEnemies(EnemyType.Zombie, 1);
                    //EnemyManager.SpawnEnemies(EnemyType.Ghost, 30);
                    BatteryManager.SpawnBatteries(7);
                    break;
                case 12:
                    // Dungeon
                    allowActivation = true;                     //Allow the portal to be active
                    s_instance._portalPrefab.SetActive(true);   //Activate portal when round 3 finishes
                    //Set the player health to unlimited in the dungeon
                    playerController = PlayerController.Instance;
                    playerController.Health = 5f; // Set to a high value for the dungeon
                    //Make sure the current player gameobject is sent to the dungeon scene
                    break;
                case 13:
                    // BossFight
                    yield return TransitionManager.LoadLevel("BossFight");
                    TransitionManager.instance.ShowRound("Final Round");
                    EnemyManager.BuildNavMeshes();
                    EnemyManager.SpawnEnemies(EnemyType.Zombie, 1);
                    //EnemyManager.SpawnEnemies(EnemyType.Ghost, 10);
                    BatteryManager.SpawnBatteries(10);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Provides a public wrapper so that external systems (e.g. card-dealer overlay) can
        /// trigger the next round without needing direct access to the GameManager instance or
        /// knowing about the coroutine details.
        /// </summary>
        public static void ProceedToNextRound()
        {
            if (s_instance == null)
            {
                Debug.LogError("GameManager instance not yet initialized – cannot proceed to next round.");
                return;
            }

            s_instance.StartCoroutine(NextRound());
        }

        /// <summary>
        /// Unity callback when this GameManager is destroyed. Unsubscribe from events to avoid
        /// dangling delegates if the singleton is ever torn down (e.g. when exiting play-mode in
        /// the editor).
        /// </summary>
        private void OnDestroy()
        {
            if (s_instance == this)
            {
                SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            }
        }

        /// <summary>
        /// Keeps track of the current and previous scene names whenever Unity loads a new scene.
        /// </summary>
        /// <param name="previousScene">The scene that was active before the change.</param>
        /// <param name="newScene">The scene that has just become active.</param>
        private void OnActiveSceneChanged(Scene previousScene, Scene newScene)
        {
            PreviousSceneName = previousScene.name;
            CurrentSceneName = newScene.name;
        }

        //This moves to the MainMenu scene and clears out all the enemies from the EnemyManager class
        //This class also removes all the battery objects
        //This should be called when moving to the Main Menu, or when moving from one scene to the next
        public static void Unload()
        {
            //SceneManager.LoadScene("MainMenu");
            EnemyManager.Unload();
            BatteryManager.DeleteAll();
        }
    }
}
