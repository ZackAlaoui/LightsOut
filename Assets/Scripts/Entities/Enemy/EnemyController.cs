using UnityEngine;
using UnityEngine.AI;
using Game.Enemy.Behavior;
using Game.Entity;
using System;
using UnityEngine.UI;

namespace Game.Enemy
{
    abstract public class EnemyController : MonoBehaviour, IDamageable
    {
        public abstract EnemyManager.EnemyType Type { get; }

        [SerializeField] private NavMeshAgent _agent;
        public NavMeshAgent Agent
        {
            get => _agent;
            private set => _agent = value;
        }

        [SerializeField] protected float maxHealth = 10;
        protected float health;
        public float Health
        {
            get => health;
            set
            {
                health = Math.Clamp(value, 0, maxHealth);
            }
        }
        public bool CanBeDamaged { get; set; }

        [SerializeField] private Slider _healthSlider;

        protected BehaviorTree BehaviorTree { get; set; }

        public virtual void Damage(MonoBehaviour source, float damage)
        {
            if (CanBeDamaged)
            {
                Health -= damage;
                _healthSlider.value = health;
            }
        }

        protected virtual void Awake()
        {
            if (Agent == null) Agent = GetComponent<NavMeshAgent>();
            if (_healthSlider == null) _healthSlider = transform.Find("HealthBar").GetComponent<Slider>();

            health = maxHealth;
            CanBeDamaged = false;

            _healthSlider.value = _healthSlider.maxValue = Health;
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
