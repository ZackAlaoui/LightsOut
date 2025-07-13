using Game.Enemy.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Enemy
{
    abstract public class EnemyController : MonoBehaviour
    {
        public NavMeshAgent Agent { get; private set; }

        protected BehaviorTree BehaviorTree { get; set; }

        protected virtual void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
        }
    }
}
