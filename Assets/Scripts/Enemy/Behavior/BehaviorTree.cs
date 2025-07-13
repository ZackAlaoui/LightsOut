using System.Collections.Generic;

namespace Game.Enemy.Behavior
{
	public interface IBehavior
	{
		public BehaviorTreeNode.Status Process();
		public void Reset();
	}

	public class BehaviorTree : BehaviorTreeNode
	{
		public BehaviorTree(string name) : base(name) { }

		public override Status Process()
		{
			foreach (BehaviorTreeNode child in children)
			{
				Status status = child.Process();
				if (status != Status.Success) return status;
			}
			return Status.Success;
		}
	}

	public class BehaviorTreeLeaf : BehaviorTreeNode
	{
		readonly IBehavior behavior;

		public BehaviorTreeLeaf(string name, IBehavior behavior) : base(name)
		{
			this.behavior = behavior;
		}

		public override Status Process()
		{
			return behavior.Process();
		}

		public override void Reset()
		{
			behavior.Reset();
		}
	}

	public class BehaviorTreeNode
	{
		public enum Status { Success, Failure, Running }

		public string Name { get; }

		public readonly List<BehaviorTreeNode> children = new();
		protected int currentChild;

		public BehaviorTreeNode(string name = "BehaviorTreeNode")
		{
			Name = name;
		}

		public void AddChild(BehaviorTreeNode child)
		{
			children.Add(child);
		}

		public virtual Status Process()
		{
			return children[currentChild].Process();
		}

		public virtual void Reset()
		{
			currentChild = 0;
			foreach (BehaviorTreeNode child in children)
			{
				child.Reset();
			}
		}
	}
}
