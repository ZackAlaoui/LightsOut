using Unity.VisualScripting;
using UnityEngine;

namespace Game.Entity
{
	//Interface class that contains a function called Damage, and whoever 
	public interface IDamageable
	{
		public void Damage(MonoBehaviour source, float damage);
	}
}