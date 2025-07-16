using System;
using UnityEngine;

namespace Game.Enemy.Trigger
{
    public class AttackReachTrigger : MonoBehaviour
    {
        public GameObject Target { get; set; }

        public event Action<bool> OnAttackReachUpdate;

        void Awake()
        {
            Target = GameObject.FindWithTag("Player");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == Target)
            {
                OnAttackReachUpdate(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == Target)
            {
                OnAttackReachUpdate(false);
            }
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.Lerp(Color.red, Color.yellow, 0.5f);
            Gizmos.DrawWireSphere(transform.position, GetComponent<SphereCollider>().radius);
		}
	}
}
