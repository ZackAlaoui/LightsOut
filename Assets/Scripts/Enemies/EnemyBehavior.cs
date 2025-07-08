using UnityEngine;

namespace Game.Enemy
{
    public class EnemyBehavior : MonoBehaviour
    {
        protected EnemyController controller;

        protected virtual void Awake()
        {
            controller = GetComponentInParent<EnemyController>();
        }
    }
}
