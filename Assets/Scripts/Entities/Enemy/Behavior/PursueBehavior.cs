using UnityEngine;
using UnityEngine.AI;
using Game.Enemy.Trigger;
using static Game.Enemy.Behavior.BehaviorTreeNode;

namespace Game.Enemy.Behavior
{
	public class PursueBehavior : IBehavior
	{
		private class ChaseState : IState
		{
			private PursueMachine _fsm;

			public ChaseState(PursueMachine fsm)
			{
				_fsm = fsm;
			}

			public void Enter() { }

			public void Update()
			{
				if (_fsm.IsTargetInRange)
				{
					_fsm.Agent.SetDestination(_fsm.Target.transform.position);
				}
				else
				{
					_fsm.ChangeState(_fsm.InvestigateState);
				}
			}

			public void Exit() { }
		}

		private class InvestigateState : IState
		{
			private PursueBehavior _behavior;
			private PursueMachine _fsm;

			public InvestigateState(PursueBehavior behavior, PursueMachine fsm)
			{
				_behavior = behavior;
				_fsm = fsm;
			}

			public void Enter()
			{
				_fsm.Agent.SetDestination(_fsm.Target.transform.position);
			}

			public void Update()
			{
				if (_fsm.IsTargetInRange)
				{
					_fsm.ChangeState(_fsm.ChaseState);
				}
				else if (_fsm.Agent.remainingDistance <= _fsm.Agent.stoppingDistance)
				{
					_behavior.Status = Status.Success;
				}
			}

			public void Exit() { }
		}

		private class PursueMachine : FiniteStateMachine
		{
			public bool IsTargetInRange { get; set; }

			public NavMeshAgent Agent { get; }
			public GameObject Target { get; }
			public ChaseRadiusTrigger ChaseTrigger { get; }

			public ChaseState ChaseState { get; }
			public InvestigateState InvestigateState { get; }

			public PursueMachine(PursueBehavior behavior, NavMeshAgent agent, GameObject target, ChaseRadiusTrigger chaseTrigger) : base("Pursue")
			{
				IsTargetInRange = false;

				Agent = agent;
				Target = target;
				ChaseTrigger = chaseTrigger;

				ChaseState = new ChaseState(this);
				InvestigateState = new InvestigateState(behavior, this);
				State = ChaseState;
				State.Enter();

				if (chaseTrigger != null) chaseTrigger.OnChaseRadiusUpdate += (bool isTriggered) => IsTargetInRange = isTriggered;
				else IsTargetInRange = true;
			}
		}

		public Status Status { get; set; }

		private readonly EnemyController _enemy;
		private readonly PursueMachine _fsm;

		public PursueBehavior(EnemyController enemy, NavMeshAgent agent, GameObject target, ChaseRadiusTrigger chaseTrigger)
		{
			Status = Status.Failure;

			_enemy = enemy;
			_fsm = new PursueMachine(this, agent, target, chaseTrigger);
		}

		public Status Process()
		{
			if (_fsm.IsTargetInRange) Status = Status.Running;
			if (Status == Status.Running) {
				_enemy.CanBeDamaged = true;
				_fsm.Update();
			}
			return Status;
		}

		public void Reset()
		{
			Status = Status.Failure;
		}
	}
}