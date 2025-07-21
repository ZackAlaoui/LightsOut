using System;
using System.Collections.Generic;
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

		public static List<EnemyController> EnemyList { get; private set; } = new();
		public static int EnemyCount { get => EnemyList.Count; }
		public static List<ZombieController> ZombieList { get; private set; } = new();
		public static int ZombieCount { get => ZombieList.Count; }
		public static List<GhostController> GhostList { get; private set; } = new();
		public static int GhostCount { get => GhostList.Count; }

		[SerializeField] private GameObject _zombiePrefab;
		[SerializeField] private GameObject _ghostPrefab;

		private static EnemyManager s_instance;

		private EnemyManager() { }

		private void Awake()
		{
			if (s_instance != null) throw new InvalidOperationException("EnemyManager has already been instantiated.");
			s_instance = this;

			if (_zombiePrefab == null) throw new NullReferenceException("Zombie Prefab is null.");
			if (_ghostPrefab == null) throw new NullReferenceException("Ghost Prefab is null.");
		}

		[SerializeField] private TMP_Text _textComponent; // TEMPORARY
		private void Update()
		{
			_textComponent.text = $"Enemy Count: {EnemyCount}";
		}

		public static void SpawnEnemies(EnemyType type, int count)
		{
			for (int i = 0; i < count; ++i)
			{
				Vector3 spawnPoint;
				do
				{
					spawnPoint = new Vector3(UnityEngine.Random.Range(-50f, 50f), 1, UnityEngine.Random.Range(-50f, 50f));
				} while ((spawnPoint - GameObject.FindWithTag("Player").transform.position).magnitude < 15f);

				switch (type)
				{
					case EnemyType.Zombie:
						ZombieController zombie = Instantiate(s_instance._zombiePrefab, spawnPoint, Quaternion.identity).GetComponent<ZombieController>();
						ZombieList.Add(zombie);
						EnemyList.Add(zombie);
						break;
					case EnemyType.Ghost:
						GhostController ghost = Instantiate(s_instance._ghostPrefab, spawnPoint, Quaternion.identity).GetComponent<GhostController>();
						GhostList.Add(ghost);
						EnemyList.Add(ghost);
						break;
					default:
						throw new InvalidOperationException($"Unable to spawn enemy type {type}.");
				}
			}
		}

		public static void Kill(EnemyController enemy)
		{
			switch (enemy.Type)
			{
				case EnemyType.Zombie:
					ZombieList.Remove((ZombieController)enemy);
					break;
				case EnemyType.Ghost:
					GhostList.Remove((GhostController)enemy);
					break;
			}
			EnemyList.Remove(enemy);
			Destroy(enemy.gameObject);
		}

		public static void KillAll()
		{
			foreach (EnemyController enemy in EnemyList) Destroy(enemy.gameObject);
			EnemyList = new();
			ZombieList = new();
			GhostList = new();
		}
	}
}
