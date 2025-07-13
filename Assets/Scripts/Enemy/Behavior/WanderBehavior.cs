using UnityEngine;
using UnityEngine.AI;

namespace Game.Enemy.Behavior
{
	public class WanderBehavior : IBehavior
	{
		private class IdleState : IState
		{
			private WanderMachine _fsm;

			private readonly float _minIdleTime;
			private readonly float _maxIdleTime;
			private float _selectedIdleTime;
			private float _currentIdleTime;

			public IdleState(WanderMachine fsm, float minIdleTime, float maxIdleTime)
			{
				_fsm = fsm;
				_minIdleTime = minIdleTime;
				_maxIdleTime = maxIdleTime;
			}

			public void Enter()
			{
				_selectedIdleTime = Random.Range(_minIdleTime, _maxIdleTime);
				Debug.Log("Waiting " + _selectedIdleTime + " seconds");
				_currentIdleTime = 0;
			}

			public void Update()
			{
				_currentIdleTime += Time.deltaTime;
				if (_currentIdleTime >= _selectedIdleTime) _fsm.ChangeState(_fsm.MoveState);
			}

			public void Exit() { }
		}

		private class MoveState : IState
		{
			private WanderMachine _fsm;

			private Vector3 _origin;
			private readonly float _radius;

			public MoveState(WanderMachine fsm, float radius)
			{
				_fsm = fsm;
				_radius = radius;
			}

			public void Enter()
			{
				_origin = _fsm.Agent.transform.position;

				Vector2 offset = Random.insideUnitCircle * _radius;
				Vector3 target = _origin + new Vector3(offset.x, 0, offset.y);
				if (NavMesh.Raycast(_origin, target, out NavMeshHit hit, NavMesh.AllAreas))
				{
					target = hit.position;
				}

				_fsm.Agent.SetDestination(target);
			}

			public void Update()
			{
				if (_fsm.Agent.remainingDistance <= _fsm.Agent.stoppingDistance)
				{
					_fsm.ChangeState(_fsm.IdleState);
				}
			}

			public void Exit() { }
		}

		private class WanderMachine : FiniteStateMachine
		{
			public NavMeshAgent Agent { get; }
			
			public IdleState IdleState { get; }
			public MoveState MoveState { get; }

			public WanderMachine(NavMeshAgent agent, float radius, float minIdleTime, float maxIdleTime) : base("Wander")
			{
				Agent = agent;

				IdleState = new IdleState(this, minIdleTime, maxIdleTime);
				MoveState = new MoveState(this, radius);

				State = IdleState;
				State.Enter();
			}
		}

		public BehaviorTreeNode.Status Status { get; set; }

		private readonly WanderMachine _fsm;

		public WanderBehavior(NavMeshAgent agent, float radius, float minIdleTime, float maxIdleTime)
		{
			Status = BehaviorTreeNode.Status.Running;

			_fsm = new WanderMachine(agent, radius, minIdleTime, maxIdleTime);
		}

		public BehaviorTreeNode.Status Process()
		{
			_fsm.Update();
			return Status;
		}

		public void Reset() { }

        // // Start is called once before the first execution of Update after the MonoBehaviour is created
		// void Start()
		// {
		//     SetRandomDestination();
		// }

		// // Update is called once per frame
		// void Update()
		// {
		//     if (HasArrived())
		//     {
		//         SetRandomDestination();
		//     }
		// }

		// bool HasArrived()
		// {
		//     return controller.Agent.remainingDistance <= controller.Agent.stoppingDistance;
		// }

		// // TODO Modify to use set base point (set when aggro lost) rather than current position
		// // TODO Modify to restrict travel distance on NavMesh to wander distance
		// void SetRandomDestination()
		// {
		//     Vector3 offset = Random.insideUnitSphere * _wanderRadius;
		//     offset.y = 0;

		//     Vector3 target = transform.position + offset;
		//     NavMeshHit hit;
		//     if (NavMesh.SamplePosition(target, out hit, 2f, 1))
		//     {
		//         target = hit.position;
		//     }

		//     controller.Agent.SetDestination(target);
		// }
	}
}
