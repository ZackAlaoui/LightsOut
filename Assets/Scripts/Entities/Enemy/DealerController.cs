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

        [SerializeField] private GameObject _bulletPrefab;

        private bool _halfHealthReached = false;
        private bool _quarterHealthReached = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (_attackRangeTrigger == null) _attackRangeTrigger = transform.Find("AttackRangeTrigger").GetComponent<AttackRangeTrigger>();
            if (_attackReachTrigger == null) _attackReachTrigger = transform.Find("AttackReachTrigger").GetComponent<AttackReachTrigger>();

            GameObject player = GameObject.Find("Player");
            if (player == null)
            {
                return;
            }

            BehaviorTree = new("Dealer");
            BehaviorTreeRepeater repeater = new("Repeat");
            
            BehaviorTreeParallel parallel = new BehaviorTreeParallel("Shoot And Move");

            BehaviorTreeSelector attackOrPursue = new("Attack OR Pursue");
            BehaviorTreeLeaf attack = new("Attack", new AttackBehavior(this, player.GetComponent<PlayerController>(), _attackRangeTrigger, _attackReachTrigger, _attackDamage, _attackDuration, _attackCooldown));
            BehaviorTreeLeaf pursue = new("Pursue", new PursueBehavior(this, Agent, player, null));
            attackOrPursue.AddChild(attack);
            attackOrPursue.AddChild(pursue);

            BehaviorTreeLeaf shoot = new("Shoot", new ShootBehavior(transform, player, _bulletPrefab, 5f));
            
            parallel.AddChild(attackOrPursue);
            parallel.AddChild(shoot);

            repeater.AddChild(parallel);
            BehaviorTree.AddChild(repeater);
        }

        public override void Damage(MonoBehaviour source, float damage)
        {
            base.Damage(source, damage);

            if (!_halfHealthReached && Health <= maxHealth * 0.5f)
            {
                EnemyManager.SpawnEnemies(EnemyType.Zombie, 20);
                EnemyManager.SpawnEnemies(EnemyType.Ghost, 10);
                _halfHealthReached = true;
            }

            if (!_quarterHealthReached && Health <= maxHealth * 0.25f)
            {
                EnemyManager.SpawnEnemies(EnemyType.Zombie, 10);
                EnemyManager.SpawnEnemies(EnemyType.Ghost, 5);
                _quarterHealthReached = true;
            }
        }

		public void OnDrawGizmosSelected()
        {
        }
	}
} 