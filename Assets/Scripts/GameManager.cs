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
        private static GameManager s_instance;

        [SerializeField] private GameObject _enemyManagerPrefab;
        public static EnemyManager EnemyManager { get; private set; }
        [SerializeField] private GameObject _batteryManagerPrefab;
        public static BatteryManager BatteryManager { get; private set; }

        public static int CurrentRound { get; private set; } = 0;

        private GameManager() { }

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
                        AsyncOperation loadingScene = SceneManager.LoadSceneAsync("FirstMap");
                        while (!loadingScene.isDone) yield return null;
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
                    // await async method from TransitionManager?
                    break;
                case 5:
                    break;
                case 6:
                    break;
                case 7:
                    break;
                case 8:
                    break;
                case 9:
                    break;
                case 10:
                    break;
                default:
                    break;
            }
        }

        public static void Unload()
        {
            SceneManager.LoadScene("MainMenu");
            EnemyManager.Unload();
            BatteryManager.DeleteAll();
        }
    }
}
