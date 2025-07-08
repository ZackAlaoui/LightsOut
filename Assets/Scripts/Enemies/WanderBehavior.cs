using UnityEngine;
using UnityEngine.AI;

namespace Game.Enemy
{
    public class WanderBehavior : EnemyBehavior
    {
        [SerializeField] private float _wanderRadius = 5f;

        enum EState
        {
            Wandering,
            Waiting,
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            SetRandomDestination();
        }

        // Update is called once per frame
        void Update()
        {
            if (HasArrived())
            {
                SetRandomDestination();
            }
        }

        bool HasArrived()
        {
            return controller.Agent.remainingDistance <= controller.Agent.stoppingDistance;
        }

        // TODO Modify to use set base point (set when aggro lost) rather than current position
        // TODO Modify to restrict travel distance on NavMesh to wander distance
        void SetRandomDestination()
        {
            Vector3 offset = Random.insideUnitSphere * _wanderRadius;
            offset.y = 0;

            Vector3 target = transform.position + offset;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(target, out hit, 2f, 1))
            {
                target = hit.position;
            }

            controller.Agent.SetDestination(target);
        }
    }
}
