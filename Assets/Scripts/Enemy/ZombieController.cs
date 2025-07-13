using UnityEngine;
using Game.Enemy.Behavior;
using Game.Enemy.Trigger;

namespace Game.Enemy
{
    public class ZombieController : EnemyController
    {
        [SerializeField] private float _wanderRadius = 5f;
        [SerializeField] private float _minIdleTime = 3f;
        [SerializeField] private float _maxIdleTime = 7f;
        [SerializeField] private ChaseRadiusTrigger _chaseTrigger;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (_chaseTrigger == null) _chaseTrigger = transform.Find("ChaseRadius").GetComponent<ChaseRadiusTrigger>();

            GameObject player = GameObject.Find("Player");

            BehaviorTree = new BehaviorTree("Zombie");
            BehaviorTreeSelector selector = new("Pursue OR Wander");
            BehaviorTreeLeaf pursue = new("Pursue", new PursueBehavior(Agent, player, _chaseTrigger));
            BehaviorTreeLeaf wander = new("Wander", new WanderBehavior(Agent, _wanderRadius, _minIdleTime, _maxIdleTime));
            selector.AddChild(pursue);
            selector.AddChild(wander);
            BehaviorTree.AddChild(selector);
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
