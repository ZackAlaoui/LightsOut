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
		private NavMeshSurface _defaultNavMeshSurfacePrefab;
		[SerializeField]
		private NavMeshSurface _ghostNavMeshSurfacePrefab;
		private static NavMeshSurface s_defaultNavMeshSurface;
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
		private AsyncOperation _navMeshUpdate;
		private float _timeSinceUpdate = 0f;
		private void Update()
		{
			if (_navMeshUpdate == null || _navMeshUpdate.isDone && _timeSinceUpdate >= 1f)
			{
				_navMeshUpdate = s_ghostNavMeshSurface.UpdateNavMesh(s_ghostNavMeshSurface.navMeshData);
				_timeSinceUpdate = 0f;
			}
			else
			{
				_timeSinceUpdate += Time.deltaTime;
			}
			_textComponent.text = $"Enemy Count: {EnemyCount}";
		}

		public static void SpawnEnemies(EnemyType type, int count)
		{
			static Vector3 ChooseSpawnPoint(float proximity, int proximityLayerMask)
			{
				Vector3 spawnPoint = new(UnityEngine.Random.Range(-50f, 50f), 1, UnityEngine.Random.Range(-50f, 50f));
				for (int numTries = 0; numTries < 15; ++numTries)
				{
					bool hit = Physics.CheckSphere(spawnPoint, proximity, proximityLayerMask);
					if (!hit) break;
					spawnPoint = new Vector3(UnityEngine.Random.Range(-50f, 50f), 1, UnityEngine.Random.Range(-50f, 50f));
				}
				return spawnPoint;
			}

			Transform parent;
			switch (type)
			{
				case EnemyType.Zombie:
					parent = new GameObject("Zombies").transform;
					for (int i = 0; i < count; ++i)
					{
						Vector3 spawnPoint = ChooseSpawnPoint(15f, LayerMask.GetMask("Player"));
						ZombieController zombie = Instantiate(s_instance._zombiePrefab, spawnPoint, Quaternion.identity).GetComponent<ZombieController>();
						zombie.transform.parent = parent;
						ZombieList.Add(zombie);
						EnemyList.Add(zombie);
					}
					break;
				case EnemyType.Ghost:
					parent = new GameObject("Ghosts").transform;
					for (int i = 0; i < count; ++i)
					{
						Vector3 spawnPoint = ChooseSpawnPoint(15f, LayerMask.GetMask("Player") | LayerMask.GetMask("Ghost"));
						GhostController ghost = Instantiate(s_instance._ghostPrefab, spawnPoint, Quaternion.identity).GetComponent<GhostController>();
						ghost.transform.parent = parent;
						GhostList.Add(ghost);
						EnemyList.Add(ghost);
					}
					break;
				default:
					throw new InvalidOperationException($"Unable to spawn enemy type {type}.");
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
			foreach (EnemyController enemy in EnemyList) Destroy(enemy.gameObject); // consider replacing with Destroy(GameObject.Find("Zombies")), etc.
			EnemyList = new();
			ZombieList = new();
			GhostList = new();
		}

		public static void Unload()
		{
			KillAll();
			Destroy(s_ghostNavMeshSurface);
		}

		public static void BuildNavMesh()
		{
			if (s_defaultNavMeshSurface != null) Destroy(s_defaultNavMeshSurface);
			if (s_ghostNavMeshSurface != null) Destroy(s_ghostNavMeshSurface);

			s_defaultNavMeshSurface = Instantiate(s_instance._defaultNavMeshSurfacePrefab);
			s_ghostNavMeshSurface = Instantiate(s_instance._ghostNavMeshSurfacePrefab);

			s_defaultNavMeshSurface.BuildNavMesh();
			s_ghostNavMeshSurface.BuildNavMesh();
		}

		public static void LoadEnemyShowcase()
		{
			BuildNavMesh();
			SpawnEnemies(EnemyType.Zombie, 20);
			SpawnEnemies(EnemyType.Ghost, 5);
		}
	}
}
