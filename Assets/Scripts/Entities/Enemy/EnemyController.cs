using UnityEngine;
using UnityEngine.AI;
using Game.Enemy.Behavior;
using Game.Entity;
using System;
using UnityEngine.UI;
using Game.Player;

namespace Game.Enemy
{
    abstract public class EnemyController : MonoBehaviour, IDamageable
    {
        public abstract EnemyManager.EnemyType Type { get; }

        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private MeshRenderer _mesh;

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

        private int lightSources = 0;

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

            if (lightSources > 0)
            {
                SetVisible(true);
            }
            else
            {
                SetVisible(false);
            }

            if (!Physics.Raycast(transform.position, PlayerController.Instance.transform.position - transform.position, 15f, LayerMask.GetMask("Player")) && (transform.position - PlayerController.Instance.transform.position).magnitude > 0.5f)
            {
                SetVisible(false);
            }

            BehaviorTree.Process();
        }

        protected virtual void SetVisible(bool isVisible)
        {
            if (_sprite != null) _sprite.enabled = isVisible;
            if (_mesh != null) _mesh.enabled = isVisible;
            _healthSlider.gameObject.SetActive(isVisible);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Light Enter") ++lightSources;
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "Light Exit") --lightSources;
        }
    }
}
