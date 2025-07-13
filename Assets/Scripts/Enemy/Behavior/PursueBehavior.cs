using UnityEngine;
using UnityEngine.AI;
using Game.Enemy.Trigger;

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
					_behavior.Status = BehaviorTreeNode.Status.Success;
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
				Agent = agent;
				Target = target;
				ChaseTrigger = chaseTrigger;

				ChaseState = new ChaseState(this);
				InvestigateState = new InvestigateState(behavior, this);
				State = ChaseState;

				chaseTrigger.OnChaseRadiusUpdate += (bool isTriggered) => {
					Debug.Log(isTriggered);
					IsTargetInRange = isTriggered;
				};
			}
		}

		public BehaviorTreeNode.Status Status { get; set; }

		private readonly PursueMachine _fsm;

		public PursueBehavior(NavMeshAgent agent, GameObject target, ChaseRadiusTrigger chaseTrigger)
		{
			Status = BehaviorTreeNode.Status.Running;

			_fsm = new PursueMachine(this, agent, target, chaseTrigger);
		}

		public BehaviorTreeNode.Status Process()
		{
			_fsm.Update();
			return Status;
		}

		public void Reset() { }
	}
}