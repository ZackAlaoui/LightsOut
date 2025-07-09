using System;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Enemy
{
    public class ChaseState : IState
    {
        private NavMeshAgent _agent;
        private Transform _targetTransform;
        private float _detectionRadius;

        public ChaseState(NavMeshAgent agent, GameObject target)
        {
            _agent = agent;
            _targetTransform = target.transform;
        }

        public void Enter() { }

        public void Update()
        {
            _agent.SetDestination(_targetTransform.position);
        }

        public void Exit() { }
    }
}
