using UnityEngine;
using UnityEngine.AI;
using Game.Enemy;

public class EnemyAnimationController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Animator _animator;

    [SerializeField] private EnemyController _enemy;
    private Vector3 _lastEnemyPosition;

    void Start()
    {
        if (_agent == null) _agent = GetComponentInChildren<NavMeshAgent>();
        if (_animator == null) _animator = GetComponent<Animator>();
        if (_enemy != null)
        {
            _lastEnemyPosition = _enemy.transform.position;
        }
    }

    void Update()
    {
        if (_enemy == null)
        {
            return;
        }

        Vector3 currentPosition = _enemy.transform.position;

        Vector3 velocity = (currentPosition - _lastEnemyPosition) / Time.deltaTime;
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);

        _animator.SetFloat("xVelocity", localVelocity.x);
        _animator.SetFloat("zVelocity", localVelocity.z);

        _lastEnemyPosition = currentPosition;
    }
}
