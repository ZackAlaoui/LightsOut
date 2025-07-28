using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class enemyanim : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Animator _animator;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_agent == null) _agent = GetComponentInChildren<NavMeshAgent>();
        if (_animator == null) _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        _animator.SetFloat("xVelocity", _agent.velocity.x);
        _animator.SetFloat("zVelocity", _agent.velocity.z);
	}
}
