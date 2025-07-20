using UnityEngine;
using Game.Enemy.Behavior;
using Game.Enemy.Trigger;
using Game.Player;

namespace Game.Enemy
{
    public class ZombieController : EnemyController
    {
        [SerializeField] private AttackRangeTrigger _attackRangeTrigger;
        [SerializeField] private AttackReachTrigger _attackReachTrigger;
        [SerializeField] private float _attackDamage = 5f;
        [SerializeField] private float _attackDuration = 0.5f;
        [SerializeField] private float _attackCooldown = 1f;

        [SerializeField] private ChaseRadiusTrigger _chaseTrigger;

        [SerializeField] private float _wanderRadius = 5f;
        [SerializeField] private float _minIdleTime = 3f;
        [SerializeField] private float _maxIdleTime = 7f;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (_attackRangeTrigger == null) _attackRangeTrigger = transform.Find("AttackRangeTrigger").GetComponent<AttackRangeTrigger>();
            if (_attackReachTrigger == null) _attackReachTrigger = transform.Find("AttackReachTrigger").GetComponent<AttackReachTrigger>();
            if (_chaseTrigger == null) _chaseTrigger = transform.Find("ChaseRadius").GetComponent<ChaseRadiusTrigger>();

            GameObject player = GameObject.Find("Player");

            BehaviorTree = new("Zombie");
            BehaviorTreeRepeater repeater = new("Repeat");
            BehaviorTreeSelector attackOrPursueOrWander = new("Attack OR Pursue OR Wander");
            BehaviorTreeLeaf attack = new("Attack", new AttackBehavior(this, player.GetComponent<PlayerController>(), _attackRangeTrigger, _attackReachTrigger, _attackDamage, _attackDuration, _attackCooldown));
            BehaviorTreeLeaf pursue = new("Pursue", new PursueBehavior(this, Agent, player, _chaseTrigger));
            BehaviorTreeLeaf wander = new("Wander", new WanderBehavior(this, Agent, _wanderRadius, _minIdleTime, _maxIdleTime));
            attackOrPursueOrWander.AddChild(attack);
            attackOrPursueOrWander.AddChild(pursue);
            attackOrPursueOrWander.AddChild(wander);
            repeater.AddChild(attackOrPursueOrWander);
            BehaviorTree.AddChild(repeater);
        }

        // Update is called once per frame
        protected override void Update()
        {
            if (Health <= 0)
            {
                --EnemyManager.Get().ZombieCount;
                Destroy(gameObject);
            }
            
            BehaviorTree.Process();
        }

		public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _wanderRadius);
        }
	}
}
