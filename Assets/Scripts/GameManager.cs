using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Enemy;
using static Game.Enemy.EnemyManager;
using System.Collections;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager s_instance;      //Singleton instance for the GameManager

        [SerializeField] private GameObject _enemyManagerPrefab;                //EnemyManager prefab 
        public static EnemyManager EnemyManager { get; private set; }           //Getter and setter for the EnemyManager
        [SerializeField] private GameObject _batteryManagerPrefab;              //Gameobject for the battery manager
        public static BatteryManager BatteryManager { get; private set; }       //Getter and setter for the BatteryManager           

        public static int CurrentRound { get; private set; } = 0;

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

            EnemyManager = Instantiate(_enemyManagerPrefab, transform).GetComponent<EnemyManager>();
            BatteryManager = Instantiate(_batteryManagerPrefab, transform).GetComponent<BatteryManager>();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            DontDestroyOnLoad(gameObject);
            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("FirstMap")) StartGame();
            else if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("BossFight"))
            {
                EnemyManager.BuildNavMeshes();
                EnemyManager.SpawnEnemies(EnemyType.Zombie, 20);
                EnemyManager.SpawnEnemies(EnemyType.Ghost, 10);
                BatteryManager.SpawnBatteries(5);
            }
            else if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("EnemyShowcase"))
            {
                EnemyManager.BuildNavMeshes();
                EnemyManager.SpawnEnemies(EnemyType.Zombie, 20);
                EnemyManager.SpawnEnemies(EnemyType.Ghost, 5);
                BatteryManager.SpawnBatteries(7);
            }
        }

        public static void StartGame()
        {
            CurrentRound = 0;
            s_instance.StartCoroutine(NextRound());
        }

        public static IEnumerator NextRound()
        {
            ++CurrentRound;
            EnemyManager.KillAll();
            BatteryManager.DeleteAll();
            switch (CurrentRound)
            {
                case 1:
                    if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("FirstMap"))
                    {
                        yield return TransitionManager.LoadLevel("FirstMap");
                    }
                    EnemyManager.BuildNavMeshes();
                    EnemyManager.SpawnEnemies(EnemyType.Zombie, 20);
                    BatteryManager.SpawnBatteries(10);
                    break;
                case 2:
                    EnemyManager.SpawnEnemies(EnemyType.Zombie, 25);
                    EnemyManager.SpawnEnemies(EnemyType.Ghost, 5);
                    BatteryManager.SpawnBatteries(10);
                    break;
                case 3:
                    EnemyManager.SpawnEnemies(EnemyType.Zombie, 30);
                    EnemyManager.SpawnEnemies(EnemyType.Ghost, 10);
                    BatteryManager.SpawnBatteries(7);
                    break;
                case 4:
                    // Dungeon
                    break;
                case 5:
                    // FirstMap
                    break;
                case 6:
                    // FirstMap
                    break;
                case 7:
                    // FirstMap
                    break;
                case 8:
                    // Dungeon
                    break;
                case 9:
                    // FirstMap
                    break;
                case 10:
                    // FirstMap
                    break;
                case 11:
                    // FirstMap
                    break;
                case 12:
                    // Dungeon
                    break;
                case 13:
                    // BossFight
                    break;
                default:
                    break;
            }
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
