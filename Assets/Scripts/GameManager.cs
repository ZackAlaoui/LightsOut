using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Enemy;
using static Game.Enemy.EnemyManager;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager s_instance;

        [SerializeField] private GameObject _enemyManagerPrefab;
        public static EnemyManager EnemyManager { get; private set; }
        [SerializeField] private GameObject _batteryManagerPrefab;
        public static BatteryManager BatteryManager { get; private set; }

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

            EnemyManager = Instantiate(_enemyManagerPrefab).GetComponent<EnemyManager>();
            BatteryManager = Instantiate(_batteryManagerPrefab).GetComponent<BatteryManager>();

            EnemyManager.transform.parent = this.transform;
            BatteryManager.transform.parent = this.transform;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene == SceneManager.GetSceneByName("EnemyShowcase"))
            {
                EnemyManager.SpawnEnemies(EnemyType.Zombie, 20);
                EnemyManager.SpawnEnemies(EnemyType.Ghost, 5);
                BatteryManager.SpawnBatteries(7);
            }
        }

        public static void Unload()
        {
            SceneManager.LoadScene("MainMenu");
            EnemyManager.KillAll();
            BatteryManager.DeleteAll();
        }
    }
}
