using System;
using System.Collections.Generic;
using UnityEngine;

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
			for (int i = 0; i < count; ++i)
			{
				Vector3 spawnPoint = new(UnityEngine.Random.Range(-50f, 50f), 1, UnityEngine.Random.Range(-50f, 50f));
				for (int numTries = 0; numTries < 15; ++numTries)
				{
					bool hit = Physics.CheckSphere(spawnPoint, 15f, LayerMask.GetMask("Player", "Battery"));
					if (!hit) break;
					spawnPoint = new Vector3(UnityEngine.Random.Range(-50f, 50f), 1, UnityEngine.Random.Range(-50f, 50f));
				}

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
			foreach (Battery battery in BatteryList) Destroy(battery.gameObject);
			BatteryList = new();
		}
	}
}
