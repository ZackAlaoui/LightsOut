
using System;
using UnityEngine;

namespace Game
{
	public class BatteryManager : MonoBehaviour
	{
		[SerializeField] private GameObject _batteryPrefab;

        private int _batteryCount = 0;

		private static BatteryManager s_instance;

		private BatteryManager() { }

		public static BatteryManager Get()
		{
			if (s_instance == null) s_instance = new();

			return s_instance;
		}

		public void SpawnBatteries(int count)
		{
			for (int i = 0; i < count; ++i)
			{
				Vector3 spawnPoint;
				do
				{
					spawnPoint = new Vector3(UnityEngine.Random.Range(-50f, 50f), 1, UnityEngine.Random.Range(-50f, 50f));
				} while ((spawnPoint - GameObject.FindWithTag("Player").transform.position).magnitude < 15f);

				Instantiate(_batteryPrefab, spawnPoint, Quaternion.identity);
                ++_batteryCount;
			}
		}

		private void Awake()
		{
			if (s_instance != null) throw new InvalidOperationException("BatteryManager has already been instantiated.");
			s_instance = this;

			if (_batteryPrefab == null) throw new NullReferenceException("Battery Prefab is null.");
		}

		private void Start()
		{
			DontDestroyOnLoad(gameObject);
			SpawnBatteries(7);
		}

		private void Update()
		{
			SpawnBatteries(7 - _batteryCount);
		}
	}
}

