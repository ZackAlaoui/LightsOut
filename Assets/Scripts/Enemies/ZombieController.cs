using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.AI;
using UnityEditor.SearchService;
using Unity.VisualScripting;
using System;

namespace Game.Enemy
{
    public class ZombieController : EnemyController
    {
        private IState _wanderState;
        private IState _chaseState;
        private IState _investigateState;

		// Start is called once before the first execution of Update after the MonoBehaviour is created
		void Start()
        {
            // _wanderState = new WanderState();
            _chaseState = new ChaseState(this.Agent, GameObject.Find("Player"));
            // _investigateState = new InvestigateState();

            State = _chaseState;
        }

        // Update is called once per frame
        void Update()
        {
            State.Update();
        }
	}
}
