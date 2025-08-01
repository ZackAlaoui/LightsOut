using UnityEngine;
using UnityEngine.AI;
using Game.Enemy;

public class GhostAnim : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Animator _animator;
    [SerializeField] private GhostController _ghost;

    private Vector3 _lastGhostPosition;

    void Start()
    {
        Debug.Log("starting ghost");
        if (_agent == null)
        {
            Debug.Log("agent"); _agent = GetComponentInChildren<NavMeshAgent>();

        }

        if (_animator == null)
        {
            Debug.Log("animator");_animator = GetComponent<Animator>();
        }


        if (_ghost != null) {
            Debug.Log("ghost");_lastGhostPosition = _ghost.transform.position;
        }
            
        else
            Debug.LogWarning("Ghost reference is not assigned!");
    }

    void Update()
    {
        if (_ghost == null || _animator == null)
            return;

        Vector3 currentPosition = _ghost.transform.position;

        float xDirection = GetAxisDirection(currentPosition.x, _lastGhostPosition.x);
        

        _animator.SetFloat("xVelocity", xDirection);
        

        _lastGhostPosition = currentPosition;
    }

    private float GetAxisDirection(float current, float previous)
    {
        if (current > previous)
        {
            Debug.Log("right");
            return 1.0f;
        }

        else if (current < previous)
        {
            Debug.Log("left");
            return -1.0f;
        }

        else
        {
            Debug.Log("no change");
            return 0.0f;
        }
            
    }
}
