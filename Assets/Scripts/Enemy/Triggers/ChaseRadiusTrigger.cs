using System;
using UnityEngine;

namespace Game.Enemy.Trigger {
    public class ChaseRadiusTrigger : MonoBehaviour
    {
        public GameObject Target { get; set; }

        public event Action<bool> OnChaseRadiusUpdate;

        void Awake()
        {
            Target = GameObject.Find("Player");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == Target)
            {
                OnChaseRadiusUpdate(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == Target)
            {
                OnChaseRadiusUpdate(false);
            }
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, GetComponent<SphereCollider>().radius);
		}
	}
}
