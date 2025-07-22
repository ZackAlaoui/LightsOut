using System;
using System.Collections.Generic;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

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

		[SerializeField]
		private NavMeshSurface _ghostNavMeshSurfacePrefab;
		private static NavMeshSurface s_ghostNavMeshSurface;

		private static EnemyManager s_instance;

		private EnemyManager() { }

		private void Awake()
		{
			if (s_instance != null) throw new InvalidOperationException("EnemyManager has already been instantiated.");
			s_instance = this;

			if (_zombiePrefab == null) throw new NullReferenceException("Zombie prefab is null.");
			if (_ghostPrefab == null) throw new NullReferenceException("Ghost prefab is null.");

			if (_ghostNavMeshSurfacePrefab == null) throw new NullReferenceException("Ghost NavMeshSurface prefab is null.");
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
				Vector3 spawnPoint = new(UnityEngine.Random.Range(-50f, 50f), 1, UnityEngine.Random.Range(-50f, 50f));
				for (int numTries = 0; numTries < 15; ++numTries)
				{
					int layerMask = LayerMask.GetMask("Player");
					switch (type)
					{
						case EnemyType.Ghost:
							layerMask |= LayerMask.GetMask("Ghost");
							break;
					}
					bool hit = Physics.CheckSphere(spawnPoint, 15f, layerMask);
					if (!hit) break;
					spawnPoint = new Vector3(UnityEngine.Random.Range(-50f, 50f), 1, UnityEngine.Random.Range(-50f, 50f));
				}

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

		public static void Unload()
		{
			KillAll();
			Destroy(s_ghostNavMeshSurface);
		}

		public static void BuildGhostNavMesh()
		{
			if (s_ghostNavMeshSurface != null) Destroy(s_ghostNavMeshSurface);

			s_ghostNavMeshSurface = Instantiate(s_instance._ghostNavMeshSurfacePrefab);
			s_ghostNavMeshSurface.BuildNavMesh();
		}

		public static void LoadEnemyShowcase()
		{
			BuildGhostNavMesh();
			SpawnEnemies(EnemyType.Zombie, 20);
			SpawnEnemies(EnemyType.Ghost, 5);
		}
	}
}
