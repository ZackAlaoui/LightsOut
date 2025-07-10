using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.AI;
using UnityEditor.SearchService;
using Unity.VisualScripting;
using System;

namespace Game.Enemy
{
    public class ZombieController : EnemyController, /*IWanderer,*/ IPursuer
    {
        // public WanderState WanderState { get; private set; }
        public PursueState PursueState { get; private set; }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            GameObject player = GameObject.Find("Player");

            // WanderState = new WanderState();
            PursueState = new PursueState(this, player);

            State = PursueState;
            State.Enter();
        }

        // Update is called once per frame
        void Update()
        {
            State.Update();
        }
	}
}
