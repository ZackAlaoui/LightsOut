namespace Game.Enemy.Behavior
{
	public interface IState
	{
		public void Enter();
		public void Update();
		public void Exit();
	}

	abstract public class FiniteStateMachine
	{
		public string Name { get; }

		public IState State { get; protected set; }

		public FiniteStateMachine(string name)
		{
			Name = name;
		}

		public void ChangeState(IState newState)
		{
			if (State != null) State.Exit();
			State = newState;
			if (State != null) State.Enter();
		}

		public void Update()
		{
			State.Update();
		}
	}
}