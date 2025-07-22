using UnityEngine;
using UnityEngine.AI;
using Game.Enemy.Behavior;
using Game.Entity;
using System;

namespace Game.Enemy
{
    abstract public class EnemyController : MonoBehaviour, IDamageable
    {
        public abstract EnemyManager.EnemyType Type { get; }

        [SerializeField]
        private NavMeshAgent _agent;
        public NavMeshAgent Agent
        {
            get => _agent;
            private set => _agent = value;
        }

        [SerializeField] protected float _maxHealth = 10;
        protected float _health;
        public float Health
        {
            get => _health;
            set
            {
                _health = Math.Clamp(value, 0, _maxHealth);
            }
        }
        public bool CanBeDamaged { get; set; }

        protected BehaviorTree BehaviorTree { get; set; }

        public virtual void Damage(MonoBehaviour source, float damage)
        {
            if (CanBeDamaged) Health -= damage;
        }

        protected virtual void Awake()
        {
            if (Agent == null) Agent = GetComponent<NavMeshAgent>();

            _health = _maxHealth;
            CanBeDamaged = false;
        }

        protected virtual void Update()
        {
            if (Health <= 0)
            {
                EnemyManager.Kill(this);
                return;
            }

            BehaviorTree.Process();
        }
    }
}
