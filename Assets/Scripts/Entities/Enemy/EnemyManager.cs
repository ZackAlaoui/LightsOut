//EnemyManager class

using System;
using System.Collections.Generic;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Game.Enemy
{
	public class EnemyManager : MonoBehaviour
	{

		public static event Action<Vector3> OnEnemyDied;
		public enum EnemyType
		{
			Zombie,
			Ghost,
			Dealer,
		}

		public static List<EnemyController> EnemyList { get; private set; } = new();	//List of Enemies
		public static int EnemyCount { get => EnemyList.Count; }						//Enemy Count
		public static List<ZombieController> ZombieList { get; private set; } = new();	//List of Zombies
		public static int ZombieCount { get => ZombieList.Count; }						//Number of Zombie enemies
		private static GameObject ZombiesObject { get; set; }							//Gameobject of the Zombie
		public static List<GhostController> GhostList { get; private set; } = new();	//List Of GhostController
		public static int GhostCount { get => GhostList.Count; }						//Number of Ghost enemies
		private static GameObject GhostsObject { get; set; }							//Gameobject for the Ghost enemy
		public static List<DealerController> DealerList { get; private set; } = new();	//Dealer list
		public static int DealerCount { get => DealerList.Count; }						//Dealer count
		private static GameObject DealersObject { get; set; }							//Dealer Gameobject

		[SerializeField] private GameObject _zombiePrefab;
		[SerializeField] private GameObject _ghostPrefab;
		[SerializeField] private GameObject _dealerPrefab;

		[SerializeField]
		private NavMeshSurface _defaultNavMeshSurfacePrefab;
		[SerializeField]
		private NavMeshSurface _ghostNavMeshSurfacePrefab;
		[SerializeField]
		private NavMeshSurface _dealerNavMeshSurfacePrefab;
		public static NavMeshSurface DefaultNavMeshSurface { get; private set; }
		private static NavMeshSurface s_ghostNavMeshSurface;
		private static NavMeshSurface s_dealerNavMeshSurface;

		private static EnemyManager s_instance;

		private EnemyManager() { }

		private void Awake()
		{
			if (s_instance != null) throw new InvalidOperationException("EnemyManager has already been instantiated.");
			s_instance = this;

			if (_zombiePrefab == null) throw new NullReferenceException("Zombie prefab is null.");
			if (_ghostPrefab == null) throw new NullReferenceException("Ghost prefab is null.");
			if (_dealerPrefab == null) throw new NullReferenceException("Dealer prefab is null.");

			if (_defaultNavMeshSurfacePrefab == null) throw new NullReferenceException("Default NavMesh Surface prefab is null.");
			if (_ghostNavMeshSurfacePrefab == null) throw new NullReferenceException("Ghost NavMesh Surface prefab is null.");
			if (_dealerNavMeshSurfacePrefab == null) throw new NullReferenceException("Dealer NavMesh Surface prefab is null.");

			DefaultNavMeshSurface = Instantiate(s_instance._defaultNavMeshSurfacePrefab, transform);
			s_ghostNavMeshSurface = Instantiate(s_instance._ghostNavMeshSurfacePrefab, transform);
			s_dealerNavMeshSurface = Instantiate(s_instance._dealerNavMeshSurfacePrefab, transform);
		}

		[SerializeField] private TMP_Text _textComponent; // TEMPORARY
		private AsyncOperation _navMeshUpdate;
		private float _timeSinceUpdate = 0f;
		private void Update()
		{
			if (_navMeshUpdate == null || _navMeshUpdate.isDone && _timeSinceUpdate >= 1f)
			{
				if (s_ghostNavMeshSurface != null && s_ghostNavMeshSurface.navMeshData != null) _navMeshUpdate = s_ghostNavMeshSurface.UpdateNavMesh(s_ghostNavMeshSurface.navMeshData);
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
				case EnemyType.Dealer:
					if (DealersObject == null) DealersObject = new GameObject("Dealers");
					proximityLayerMask = LayerMask.GetMask("Player") | LayerMask.GetMask("Ghost");
					navMeshLayerMask = 1 << NavMesh.GetAreaFromName("Walkable");
					for (int i = 0; i < count; ++i)
					{
						Vector3 spawnPoint = new Vector3(0, 1f, 25);
						DealerController dealer = Instantiate(s_instance._dealerPrefab, spawnPoint, Quaternion.identity).GetComponent<DealerController>();
						dealer.transform.parent = DealersObject.transform;
						DealerList.Add(dealer);
						EnemyList.Add(dealer);
					}
					break;
				default:
					throw new InvalidOperationException($"Unable to spawn enemy type {type}.");
			}
			if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("BossFight"))
			{
				s_instance._textComponent.text = $"Enemy Count: {EnemyCount}";
			}
			else{
				s_instance._textComponent.text = $"";
			}
		}

		public static void Kill(EnemyController enemy)
		{
			if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("BossFight"))
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
				switch (enemy.Type)
				{
					case EnemyType.Zombie:
						ZombieList.Remove((ZombieController)enemy);
						Vector3 zombieSpawnPoint = ChooseSpawnPoint(DefaultNavMeshSurface.navMeshData.sourceBounds, 15f, 
							LayerMask.GetMask("Player") | LayerMask.GetMask("Ghost"),
							1 << NavMesh.GetAreaFromName("Walkable"));
						ZombieController newZombie = Instantiate(s_instance._zombiePrefab, zombieSpawnPoint, Quaternion.identity).GetComponent<ZombieController>();
						newZombie.transform.parent = ZombiesObject.transform;
						ZombieList.Add(newZombie);
						EnemyList.Add(newZombie);
						break;
					case EnemyType.Ghost:
						GhostList.Remove((GhostController)enemy);
						Vector3 ghostSpawnPoint = ChooseSpawnPoint(s_ghostNavMeshSurface.navMeshData.sourceBounds, 15f,
							LayerMask.GetMask("Player") | LayerMask.GetMask("Zombie") | LayerMask.GetMask("Ghost"),
							1 << NavMesh.GetAreaFromName("Ethereal"));
						GhostController newGhost = Instantiate(s_instance._ghostPrefab, ghostSpawnPoint, Quaternion.identity).GetComponent<GhostController>();
						newGhost.transform.parent = GhostsObject.transform;
						GhostList.Add(newGhost);
						EnemyList.Add(newGhost);
						break;
					case EnemyType.Dealer:
						DealerList.Remove((DealerController)enemy);
						SceneManager.LoadScene("Victory");
						Debug.Log("Victory");
						break;
				}
				Destroy(enemy.gameObject);
				return;

			} else {
			switch (enemy.Type)
			{
				case EnemyType.Zombie:
					ZombieList.Remove((ZombieController)enemy);
					break;
				case EnemyType.Ghost:
					GhostList.Remove((GhostController)enemy);
					break;
				case EnemyType.Dealer:
					DealerList.Remove((DealerController)enemy);
					break;
			}
			EnemyList.Remove(enemy);
			
			OnEnemyDied?.Invoke(enemy.transform.position);

			Destroy(enemy.gameObject);

			if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName("BossFight"))
			{
				s_instance._textComponent.text = $"Enemy Count: {EnemyCount}";
			}
			else{
				s_instance._textComponent.text = $"";
			}
			}
			if (EnemyCount <= 0)
			{
				Debug.Log("Killed enemy");
				s_instance.StartCoroutine(GameManager.NextRound());
			}
		}

		public static void KillAll()
		{
			if (ZombiesObject != null) Destroy(ZombiesObject);
			if (GhostsObject != null) Destroy(GhostsObject);
			if (DealersObject != null) Destroy(DealersObject);
			ZombiesObject = null;
			GhostsObject = null;
			DealersObject = null;
			EnemyList = new();
			ZombieList = new();
			GhostList = new();
			DealerList = new();
		}

		public static void Unload()
		{
			KillAll();
			DefaultNavMeshSurface.RemoveData();
			s_ghostNavMeshSurface.RemoveData();
			s_dealerNavMeshSurface.RemoveData();
		}

		public static void BuildNavMeshes()
		{
			DefaultNavMeshSurface.BuildNavMesh();
			s_ghostNavMeshSurface.BuildNavMesh();
			s_dealerNavMeshSurface.BuildNavMesh();
		}
	}
}
