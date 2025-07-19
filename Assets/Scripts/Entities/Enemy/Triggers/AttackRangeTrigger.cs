using System;
using UnityEngine;

namespace Game.Enemy.Trigger
{
    public class AttackRangeTrigger : MonoBehaviour
    {
        public GameObject Target { get; set; }

        public event Action<bool> OnAttackRangeUpdate;

        void Awake()
        {
            Target = GameObject.FindWithTag("Player");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == Target)
            {
                OnAttackRangeUpdate(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == Target)
            {
                OnAttackRangeUpdate(false);
            }
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, GetComponent<SphereCollider>().radius);
		}
	}
}
