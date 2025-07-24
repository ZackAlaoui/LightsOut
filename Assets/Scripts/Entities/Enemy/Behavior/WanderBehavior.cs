using UnityEngine;
using UnityEngine.AI;
using static Game.Enemy.Behavior.BehaviorTreeNode;

namespace Game.Enemy.Behavior
{
	public class WanderBehavior : IBehavior
	{
		private class IdleState : IState
		{
			private readonly WanderMachine _fsm;

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
			private readonly WanderMachine _fsm;

			private Vector3 _origin;
			private readonly float _radius;
			private readonly int _avoidLayerMask;

			public MoveState(WanderMachine fsm, float radius, int avoidLayerMask)
			{
				_fsm = fsm;
				_radius = radius;
				_avoidLayerMask = avoidLayerMask;
			}

			public void Enter()
			{
				_origin = _fsm.Agent.transform.position;

				Vector2 offset = Random.insideUnitCircle * _radius;
				Vector3 target = _origin + new Vector3(offset.x, 0, offset.y);
				float maxDistance = Mathf.Sqrt(_fsm.Agent.radius * _fsm.Agent.radius + _fsm.Agent.height * _fsm.Agent.height / 4) + 0.5f; // maxDistance also applies vertically, so we calculate the distance from the center to a point on the ground (with margin of error for floating point calculations)
				for (int numTries = 0; numTries < 25; ++numTries)
				{
					if (NavMesh.Raycast(_origin, target, out NavMeshHit hit, NavMesh.AllAreas))
					{
						target = hit.position;
					}
					if (!NavMesh.SamplePosition(target, out hit, maxDistance, _avoidLayerMask))
					{
						break;
					}
					offset = Random.insideUnitCircle * _radius;
					target = _origin + new Vector3(offset.x, 0, offset.y);
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

			public WanderMachine(NavMeshAgent agent, float radius, float minIdleTime, float maxIdleTime, int avoidLayerMask) : base("Wander")
			{
				Agent = agent;

				IdleState = new(this, minIdleTime, maxIdleTime);
				MoveState = new(this, radius, avoidLayerMask);

				State = IdleState;
				State.Enter();
			}
		}

		public Status Status { get; set; }

		private readonly EnemyController _enemy;
		private readonly WanderMachine _fsm;

		public WanderBehavior(EnemyController enemy, NavMeshAgent agent, float radius, float minIdleTime, float maxIdleTime, int avoidLayerMask)
		{
			Status = Status.Running;

			_enemy = enemy;
			_fsm = new(agent, radius, minIdleTime, maxIdleTime, avoidLayerMask);
		}

		public Status Process()
		{
			_enemy.CanBeDamaged = false;
			_fsm.Update();
			return Status;
		}

		public void Reset() { }
	}
}
