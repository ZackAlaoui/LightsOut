//This class is responsible for updating the players battery life when they collect a battery object.
//It also handles the interaction with the player when they collide with a battery object.
//The battery object will increase the player's flashlight battery life by a specified amount when collected.

using Game;
using Game.Player;
using UnityEngine;

public class Battery : MonoBehaviour
{
    [SerializeField] float _batteryLife = 20f;          //Battery Life

    private GameObject _player;                         //Player GameObject

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _player = GameObject.FindWithTag("Player").gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }

	private void OnTriggerEnter(Collider other)
	{
        if (other.gameObject == _player)
        {
            _player.GetComponent<PlayerController>().Flashlight.RemainingBatteryLife += _batteryLife;
            BatteryManager.Delete(this);
        }
	}
}
