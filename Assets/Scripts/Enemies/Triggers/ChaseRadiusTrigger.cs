using UnityEngine;

namespace Game.Enemy.Trigger {
    public class ChaseRadiusTrigger : MonoBehaviour
    {
        public GameObject Target { get; set; }
        private IPursuer _enemy;

        void Awake()
        {
            Target = GameObject.Find("Player");
            _enemy = GetComponentInParent<IPursuer>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == Target)
            {
                _enemy.PursueState.IsTargetInRange = true;
            }
        }

		private void OnTriggerExit(Collider other)
		{
            if (other.gameObject == Target)
            {
                _enemy.PursueState.IsTargetInRange = false;
            }
		}
	}
}
