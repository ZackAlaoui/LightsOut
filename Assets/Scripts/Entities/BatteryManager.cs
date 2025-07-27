using System;
using System.Collections.Generic;
using System.Xml.Schema;
using Game.Enemy;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Game
{
	public class BatteryManager : MonoBehaviour
	{
		[SerializeField] private GameObject _batteryPrefab;

		private static List<Battery> BatteryList { get; set; } = new();
		private static int BatteryCount { get => BatteryList.Count; }

		private static BatteryManager s_instance;

		private BatteryManager() { }

		private void Awake()
		{
			if (s_instance != null) throw new InvalidOperationException("BatteryManager has already been instantiated.");
			s_instance = this;

			if (_batteryPrefab == null) throw new NullReferenceException("Battery Prefab is null.");
		}

		public static void SpawnBatteries(int count)
		{
			Bounds bounds = EnemyManager.DefaultNavMeshSurface.navMeshData.sourceBounds;
			for (int i = 0; i < count; ++i)
			{
				Vector3 spawnPoint = new(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), 1f, UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
				int numTries;
				bool isTooClose = false;
				bool isNearNavMesh = true;
				for (numTries = 0; numTries < 25; ++numTries)
				{
					isTooClose = Physics.CheckSphere(spawnPoint, 20f, LayerMask.GetMask("Player", "Battery"));
					isNearNavMesh = NavMesh.SamplePosition(spawnPoint, out NavMeshHit navMeshHit, 4f, 1 << NavMesh.GetAreaFromName("Walkable"));
					if (isNearNavMesh) spawnPoint = navMeshHit.position;
					if (!isTooClose && isNearNavMesh) break;
					spawnPoint = new(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), 1f, UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
				}
				if (isTooClose) Debug.LogWarning("Spawning batteries in close proximity.");
				if (!isNearNavMesh) Debug.LogWarning("Spawning battery in unreachable location.");

				Battery battery = Instantiate(s_instance._batteryPrefab, spawnPoint, Quaternion.identity).GetComponent<Battery>();
				BatteryList.Add(battery);
			}
		}

		public static void Delete(Battery battery)
		{
			BatteryList.Remove(battery);
			Destroy(battery.gameObject);
			SpawnBatteries(1);
		}

		public static void DeleteAll()
		{
			foreach (Battery battery in BatteryList)
			{
				if (battery != null)
				{
					Destroy(battery.gameObject);
				}
			}
			BatteryList = new();
		}
	}
}
