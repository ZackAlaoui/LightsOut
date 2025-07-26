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
		private static GameObject ZombiesObject { get; set; }
		public static List<GhostController> GhostList { get; private set; } = new();
		public static int GhostCount { get => GhostList.Count; }
		private static GameObject GhostsObject { get; set; }

		[SerializeField] private GameObject _zombiePrefab;
		[SerializeField] private GameObject _ghostPrefab;

		[SerializeField]
		private NavMeshSurface _defaultNavMeshSurfacePrefab;
		[SerializeField]
		private NavMeshSurface _ghostNavMeshSurfacePrefab;
		public static NavMeshSurface DefaultNavMeshSurface { get; private set; }
		private static NavMeshSurface s_ghostNavMeshSurface;

		private static EnemyManager s_instance;

		private EnemyManager() { }

		private void Awake()
		{
			if (s_instance != null) throw new InvalidOperationException("EnemyManager has already been instantiated.");
			s_instance = this;

			if (_zombiePrefab == null) throw new NullReferenceException("Zombie prefab is null.");
			if (_ghostPrefab == null) throw new NullReferenceException("Ghost prefab is null.");

			if (_defaultNavMeshSurfacePrefab == null) throw new NullReferenceException("Default NavMesh Surface prefab is null.");
			if (_ghostNavMeshSurfacePrefab == null) throw new NullReferenceException("Ghost NavMesh Surface prefab is null.");

			DefaultNavMeshSurface = Instantiate(s_instance._defaultNavMeshSurfacePrefab, transform);
			s_ghostNavMeshSurface = Instantiate(s_instance._ghostNavMeshSurfacePrefab, transform);
			Debug.Log(s_ghostNavMeshSurface);
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
		}

		public static void SpawnEnemies(EnemyType type, int count)
		{
			static Vector3 ChooseSpawnPoint(Bounds bounds, float minDistance, int proximityLayerMask, int navMeshLayerMask)
			{
				Vector3 spawnPoint = new(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), 1f, UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
				int numTries;
				bool isTooClose = false;
				bool isNearNavMesh = true;
				for (numTries = 0; numTries < 15; ++numTries)
				{
					isTooClose = Physics.CheckSphere(spawnPoint, minDistance, proximityLayerMask);
					isNearNavMesh = NavMesh.SamplePosition(spawnPoint, out NavMeshHit navMeshHit, 4f, navMeshLayerMask) && navMeshHit.position.y < 2f;
					if (isNearNavMesh) spawnPoint = navMeshHit.position;
					if (!isTooClose && isNearNavMesh) break;
					spawnPoint = new(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), 1f, UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
				}
				if (isTooClose) Debug.LogWarning("Spawning enemies in close proximity.");
				if (!isNearNavMesh) Debug.LogWarning("Spawning enemy in unreachable location.");

				return spawnPoint;
			}

			int proximityLayerMask;
			int navMeshLayerMask;
			switch (type)
			{
				case EnemyType.Zombie:
					if (ZombiesObject == null) ZombiesObject = new GameObject("Zombies");
					proximityLayerMask = LayerMask.GetMask("Player") | LayerMask.GetMask("Ghost");
					navMeshLayerMask = 1 << NavMesh.GetAreaFromName("Walkable");
					for (int i = 0; i < count; ++i)
					{
						Vector3 spawnPoint = ChooseSpawnPoint(DefaultNavMeshSurface.navMeshData.sourceBounds, 15f, proximityLayerMask, navMeshLayerMask);
						ZombieController zombie = Instantiate(s_instance._zombiePrefab, spawnPoint, Quaternion.identity).GetComponent<ZombieController>();
						zombie.transform.parent = ZombiesObject.transform;
						ZombieList.Add(zombie);
						EnemyList.Add(zombie);
					}
					break;
				case EnemyType.Ghost:
					if (GhostsObject == null) GhostsObject = new GameObject("Ghosts");
					proximityLayerMask = LayerMask.GetMask("Player") | LayerMask.GetMask("Zombie") | LayerMask.GetMask("Ghost");
					navMeshLayerMask = 1 << NavMesh.GetAreaFromName("Ethereal");
					for (int i = 0; i < count; ++i)
					{
						Vector3 spawnPoint = ChooseSpawnPoint(s_ghostNavMeshSurface.navMeshData.sourceBounds, 15f, proximityLayerMask, navMeshLayerMask);
						GhostController ghost = Instantiate(s_instance._ghostPrefab, spawnPoint, Quaternion.identity).GetComponent<GhostController>();
						ghost.transform.parent = GhostsObject.transform;
						GhostList.Add(ghost);
						EnemyList.Add(ghost);
					}
					break;
				default:
					throw new InvalidOperationException($"Unable to spawn enemy type {type}.");
			}

			s_instance._textComponent.text = $"Enemy Count: {EnemyCount}";
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

			s_instance._textComponent.text = $"Enemy Count: {EnemyCount}";

			if (EnemyCount <= 0) s_instance.StartCoroutine(GameManager.NextRound());
		}

		public static void KillAll()
		{
			if (ZombiesObject != null) Destroy(ZombiesObject);
			if (GhostsObject != null) Destroy(GhostsObject);
			ZombiesObject = null;
			GhostsObject = null;
			EnemyList = new();
			ZombieList = new();
			GhostList = new();
		}

		public static void Unload()
		{
			KillAll();
			DefaultNavMeshSurface.RemoveData();
			s_ghostNavMeshSurface.RemoveData();
		}

		public static void BuildNavMeshes()
		{
			DefaultNavMeshSurface.BuildNavMesh();
			s_ghostNavMeshSurface.BuildNavMesh();
		}
	}
}
