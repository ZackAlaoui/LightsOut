using UnityEngine;
using static Game.Enemy.Behavior.BehaviorTreeNode;

namespace Game.Enemy.Behavior
{
    public class ShootBehavior : IBehavior
    {
        public Status Status { get; set; }

        private readonly Transform _enemyTransform;
        private readonly GameObject _player;
        private readonly GameObject _bulletPrefab;
        private readonly float _shootCooldown;
        private float _timeSinceLastShot;

        public ShootBehavior(Transform enemyTransform, GameObject player, GameObject bulletPrefab, float shootCooldown)
        {
            _enemyTransform = enemyTransform;
            _player = player;
            _bulletPrefab = bulletPrefab;
            _shootCooldown = shootCooldown;
            _timeSinceLastShot = _shootCooldown; // Start with a full cooldown to allow an immediate shot
        }

        public Status Process()
        {
            _timeSinceLastShot += Time.deltaTime;
            if (_timeSinceLastShot >= _shootCooldown)
            {
                _timeSinceLastShot = 0f;
                Debug.Log("Dealer is attempting to shoot.");

                if (_bulletPrefab == null)
                {
                    Debug.LogError("Bullet prefab is not assigned in ShootBehavior!");
                    return Status.Failure;
                }

                Vector3 playerPosition = new Vector3(_player.transform.position.x, 0, _player.transform.position.z);
                Vector3 enemyPosition = new Vector3(_enemyTransform.position.x, 0, _enemyTransform.position.z);
                Vector3 direction = (playerPosition - enemyPosition).normalized;

                Vector3 spawnPosition = new Vector3(_enemyTransform.position.x, 1f, _enemyTransform.position.z);

                GameObject bullet = Object.Instantiate(_bulletPrefab, spawnPosition + direction, Quaternion.identity);
                BulletController bulletController = bullet.GetComponent<BulletController>();
                if (bulletController != null)
                {
                    bulletController.Initialize(direction);
                }
                else
                {
                    Debug.LogError("Bullet prefab is missing BulletController script!");
                }


                Status = Status.Success;
            }
            else
            {
                Status = Status.Failure;
            }

            return Status;
        }

        public void Reset()
        {
            Status = Status.Failure;
            _timeSinceLastShot = _shootCooldown;
        }
    }
} 