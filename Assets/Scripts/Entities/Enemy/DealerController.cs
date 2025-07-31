using UnityEngine;
using static Game.Enemy.EnemyManager;
using Game.Enemy.Behavior;
using Game.Enemy.Trigger;
using Game.Player;
using UnityEngine.AI;

namespace Game.Enemy
{
    public class DealerController : EnemyController
    {
        public override EnemyType Type { get; } = EnemyType.Dealer;

        [SerializeField] private AttackRangeTrigger _attackRangeTrigger;
        [SerializeField] private AttackReachTrigger _attackReachTrigger;
        [SerializeField] private float _attackDamage = 5f;
        [SerializeField] private float _attackDuration = 0.5f;
        [SerializeField] private float _attackCooldown = 1f;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (_attackRangeTrigger == null) _attackRangeTrigger = transform.Find("AttackRangeTrigger").GetComponent<AttackRangeTrigger>();
            if (_attackReachTrigger == null) _attackReachTrigger = transform.Find("AttackReachTrigger").GetComponent<AttackReachTrigger>();

            GameObject player = GameObject.Find("Player");

            BehaviorTree = new("Dealer");
            BehaviorTreeRepeater repeater = new("Repeat");
            BehaviorTreeSelector attackOrPursue = new("Attack OR Pursue");
            BehaviorTreeLeaf attack = new("Attack", new AttackBehavior(this, player.GetComponent<PlayerController>(), _attackRangeTrigger, _attackReachTrigger, _attackDamage, _attackDuration, _attackCooldown));
            BehaviorTreeLeaf pursue = new("Pursue", new PursueBehavior(this, Agent, player, null));
            attackOrPursue.AddChild(attack);
            attackOrPursue.AddChild(pursue);
            repeater.AddChild(attackOrPursue);
            BehaviorTree.AddChild(repeater);
        }

		public void OnDrawGizmosSelected()
        {
        }
	}
} 