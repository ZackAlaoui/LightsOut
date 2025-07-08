using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        public NavMeshAgent Agent { get; private set; }
        protected GameObject player;
        protected Vector3 target;

		protected virtual void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            player = GameObject.Find("Player");
        }
    }
}
