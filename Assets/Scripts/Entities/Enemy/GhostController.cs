using UnityEngine;
using Game.Enemy.Behavior;
using Game.Enemy.Trigger;
using Game.Player;

namespace Game.Enemy
{
	public class GhostController : EnemyController
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

		void Start()
		{
			if (_chaseTrigger == null) _chaseTrigger = transform.Find("ChaseRadius").GetComponent<ChaseRadiusTrigger>();

			GameObject player = GameObject.Find("Player");

			BehaviorTree = new("Ghost");
			BehaviorTreeRepeater repeater = new("Repeat");
			BehaviorTreeSelector attackOrPursueOrWander = new("Attack OR Pursue OR Wander");
			BehaviorTreeLeaf attack = new("Attack", new AttackBehavior(this, player.GetComponent<PlayerController>(), _attackRangeTrigger, _attackReachTrigger, _attackDamage, _attackDuration, _attackCooldown));
			BehaviorTreeLeaf pursue = new("Pursue", new PursueBehavior(Agent, player, _chaseTrigger));
			BehaviorTreeLeaf wander = new("Wander", new WanderBehavior(Agent, _wanderRadius, _minIdleTime, _maxIdleTime));
			attackOrPursueOrWander.AddChild(attack);
			attackOrPursueOrWander.AddChild(pursue);
			attackOrPursueOrWander.AddChild(wander);
			repeater.AddChild(attackOrPursueOrWander);
			BehaviorTree.AddChild(repeater);
		}

		protected override void Update()
		{
			if (Health <= 0)
			{
				--EnemyManager.Get().GhostCount;
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