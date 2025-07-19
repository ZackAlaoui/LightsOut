using System;
using TMPro;
using UnityEngine;

namespace Game.Enemy
{
	public class EnemyManager : MonoBehaviour
	{
		public enum EnemyType
		{
			Zombie,
			Ghost,
		}

		public int EnemyCount { get; private set; }
		private int _zombieCount;
		public int ZombieCount
		{
			get => _zombieCount;
			set
			{
				EnemyCount += value - _zombieCount;
				_zombieCount = value;
			}
		}
		private int _ghostCount;
		public int GhostCount
		{
			get => _ghostCount;
			set
			{
				EnemyCount += value - _ghostCount;
				_ghostCount = value;
			}
		}

		[SerializeField] private GameObject _zombiePrefab;
		[SerializeField] private GameObject _ghostPrefab;

		private static EnemyManager s_instance;

		private EnemyManager() { }

		public static EnemyManager Get()
		{
			if (s_instance == null) s_instance = new();

			return s_instance;
		}

		public void SpawnEnemies(EnemyType type, int count)
		{
			for (int i = 0; i < count; ++i)
			{
				Vector3 spawnPoint;
				do
				{
					spawnPoint = new Vector3(UnityEngine.Random.Range(-50f, 50f), 1, UnityEngine.Random.Range(-50f, 50f));
				} while ((spawnPoint - GameObject.FindWithTag("Player").transform.position).magnitude < 15f);

				GameObject prefab;
				switch (type)
				{
					case EnemyType.Zombie:
						++ZombieCount;
						prefab = _zombiePrefab;
						break;
					case EnemyType.Ghost:
						++GhostCount;
						prefab = _ghostPrefab;
						break;
					default:
						throw new InvalidOperationException($"Unable to spawn enemy type {type}.");
				}
				Instantiate(prefab, spawnPoint, Quaternion.identity);
			}
		}

		private void Awake()
		{
			if (s_instance != null) throw new InvalidOperationException("EnemyManager has already been instantiated.");
			s_instance = this;

			if (_zombiePrefab == null) throw new NullReferenceException("Zombie Prefab is null.");
			if (_ghostPrefab == null) throw new NullReferenceException("Ghost Prefab is null.");
		}

		private void Start()
		{
			DontDestroyOnLoad(gameObject);
			SpawnEnemies(EnemyType.Zombie, 20);
			SpawnEnemies(EnemyType.Ghost, 5);
		}

		[SerializeField] private TMP_Text _textComponent; // TEMPORARY
		private void Update()
		{
			_textComponent.text = $"Enemy Count: {EnemyCount}";
		}
	}
}
