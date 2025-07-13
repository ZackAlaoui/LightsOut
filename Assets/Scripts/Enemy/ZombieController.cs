using UnityEngine;
using Game.Enemy.Behavior;
using Game.Enemy.Trigger;

namespace Game.Enemy
{
    public class ZombieController : EnemyController
    {
        [SerializeField] private float _wanderRadius = 5;
        [SerializeField] private float _minWaitTime = 3;
        [SerializeField] private float _maxWaitTime = 7;
        [SerializeField] private ChaseRadiusTrigger _chaseTrigger;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (_chaseTrigger == null) _chaseTrigger = transform.Find("ChaseRadius").GetComponent<ChaseRadiusTrigger>();

            GameObject player = GameObject.Find("Player");

            BehaviorTree = new BehaviorTree("Zombie");
            BehaviorTree.AddChild(new BehaviorTreeLeaf("Pursue", new PursueBehavior(Agent, player, _chaseTrigger)));
        }

        // Update is called once per frame
        void Update()
        {
            BehaviorTree.Process();
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _wanderRadius);
		}
	}
}
