using System;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Enemy
{
    public interface IState
    {
        public void Enter();
        public void Update();
        public void Exit();
    }

    public class EnemyController : MonoBehaviour
    {
        public NavMeshAgent Agent { get; private set; }
        protected Vector3 target;

        protected IState State { get; set; }

        public void ChangeState(IState newState)
        {
            State.Exit();
            State = newState;
            State.Enter();
        }

        protected virtual void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
        }
    }
}
