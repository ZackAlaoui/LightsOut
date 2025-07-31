using UnityEngine;
using Game.Entity;

namespace Game.Enemy
{
    public class BulletController : MonoBehaviour
    {
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _lifespan = 5f;
        [SerializeField] private float _damage = 15f;

        private Vector3 _direction;
        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                _rigidbody = gameObject.AddComponent<Rigidbody>();
                _rigidbody.useGravity = false;
            }
            Debug.Log("Bullet spawned at " + transform.position);
        }

        public void Initialize(Vector3 direction)
        {
            _direction = direction;
            if (direction != Vector3.zero)
            {
                float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                transform.eulerAngles = new Vector3(0, 0, -angle);
            }
            Destroy(gameObject, _lifespan);
        }

        private void Update()
        {
            _rigidbody.linearVelocity = _direction * _speed;
            Vector3 currentPosition = transform.position;
            currentPosition.y = 1f;
            transform.position = currentPosition;
        }

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log("Bullet collided with " + collision.gameObject.name);
            if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.Damage(this, _damage);
            }

            if (collision.gameObject.layer != LayerMask.NameToLayer("Enemy"))
            {
                Destroy(gameObject);
            }
        }
    }
} 