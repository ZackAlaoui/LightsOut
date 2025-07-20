using System;
using UnityEngine;
using UnityEngine.AI;
using Game.Entity;
using Game.Enemy.Trigger;
using static Game.Enemy.Behavior.BehaviorTreeNode;

namespace Game.Enemy.Behavior
{
	public class AttackBehavior : IBehavior
	{
		private readonly EnemyController _enemy;
		private readonly IDamageable _target;
		private readonly float _attackDamage;
		private readonly float _attackDuration;
		private readonly float _attackCooldown;

		private bool _isTargetInAttackRange;
		private bool _isTargetInAttackReach;
		private float _attackTime = 0f;
		private float _cooldownTime = 0f;

		public AttackBehavior(EnemyController enemy, IDamageable target, AttackRangeTrigger rangeTrigger, AttackReachTrigger reachTrigger, float attackDamage, float attackDuration, float attackCooldown)
		{
			_enemy = enemy;
			_target = target;
			_attackDamage = attackDamage;
			_attackDuration = attackDuration;
			_attackCooldown = attackCooldown;

			_isTargetInAttackRange = false;
			_isTargetInAttackReach = false;

			rangeTrigger.OnAttackRangeUpdate += (bool isTriggered) => _isTargetInAttackRange = isTriggered;
			reachTrigger.OnAttackReachUpdate += (bool isTriggered) => _isTargetInAttackReach = isTriggered;
		}

		public Status Process()
		{
			_cooldownTime = Math.Max(_cooldownTime - Time.deltaTime, 0);

			if (_isTargetInAttackRange == false || _cooldownTime > 0) return Status.Failure;

			_enemy.CanBeDamaged = true;

			_attackTime += Time.deltaTime;
			if (_attackTime < _attackDuration) return Status.Running;
			
			if (_isTargetInAttackReach) _target.Damage(_enemy, _attackDamage);
			_cooldownTime = _attackCooldown;
			return Status.Success;
		}

		public void Reset()
		{
			_attackTime = 0f;
		}
	}
}