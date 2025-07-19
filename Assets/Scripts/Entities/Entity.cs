using Unity.VisualScripting;
using UnityEngine;

namespace Game.Entity
{
	public interface IDamageable
	{
		public void Damage(MonoBehaviour source, float damage);
	}
}