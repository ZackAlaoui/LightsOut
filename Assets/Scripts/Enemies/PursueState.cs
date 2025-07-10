using UnityEngine;
using UnityEngine.AI;

namespace Game.Enemy
{
    public interface IPursuer
    {
        public PursueState PursueState { get; }
    }

    public class PursueState : IState
    {
        private class ChaseState : IState
        {
            private PursueState _pursueState;

            public ChaseState(PursueState pursueState)
            {
                _pursueState = pursueState;
            }

            public void Enter() { }

            public void Update()
            {
                if (_pursueState.IsTargetInRange)
                {
                    _pursueState._enemy.Agent.SetDestination(_pursueState._target.transform.position);
                }
                else
                {
                    _pursueState.ChangeState(_pursueState._investigateState);
                }
            }

            public void Exit() { }
        }

        private class InvestigateState : IState
        {
            private PursueState _pursueState;

            public InvestigateState(PursueState pursueState)
            {
                _pursueState = pursueState;
            }

            public void Enter()
            {
                _pursueState._enemy.Agent.SetDestination(_pursueState._target.transform.position);
            }
            public void Update()
            {
                if (_pursueState.IsTargetInRange)
                {
                    _pursueState.ChangeState(_pursueState._chaseState);
                }
            }
            public void Exit() { }
        }

        public bool IsTargetInRange { get; set; }

        private EnemyController _enemy;
        private GameObject _target;

        private ChaseState _chaseState;
        private InvestigateState _investigateState;
        private IState _state;

        public PursueState(EnemyController enemy, GameObject target)
        {
            _enemy = enemy;
            _target = target;

            _chaseState = new ChaseState(this);
            _investigateState = new InvestigateState(this);
        }

        private void ChangeState(IState newState)
        {
            _state.Exit();
            _state = newState;
            _state.Enter();
        }

        public void Enter()
        {
            _state = _chaseState;
            _state.Enter();
        }

        public void Update()
        {
            _state.Update();
        }

        public void Exit()
        {
            _state.Exit();
        }
    }
}
