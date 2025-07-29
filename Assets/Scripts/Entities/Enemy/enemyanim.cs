using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Game.Enemy;

public class enemyanim : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Animator _animator;

    [SerializeField] private ZombieController _zombie;
    private Vector3 _lastZombiePosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_agent == null) _agent = GetComponentInChildren<NavMeshAgent>();
        if (_animator == null) _animator = GetComponent<Animator>();
        if (_zombie != null)
        {
            _lastZombiePosition = _zombie.transform.position;

        }


    }

     void Update()
    {
       if (_zombie == null)
    {
        Debug.LogWarning("Zombie reference is null!");
        
    }

    if (_animator == null)
    {
        Debug.LogWarning("Animator reference is null!");
        
    }

    Vector3 currentPosition = _zombie.transform.position;

    float xDirection = GetAxisDirection(currentPosition.x, _lastZombiePosition.x);
    float zDirection = GetAxisDirection(currentPosition.z, _lastZombiePosition.z);


        _animator.SetFloat("xVelocity", xDirection);
        _animator.SetFloat("zVelocity", zDirection);

        _lastZombiePosition = currentPosition;

        //Debug.Log($"X: {xDirection}, Z: {zDirection}"); // confirm it's updating
    }
    private float GetAxisDirection(float current, float previous)
    {
        if (current > previous)
            return 1.0f;
        else if (current < previous)
            return -1.0f;
        else
            return 0.0f;
    }



}
