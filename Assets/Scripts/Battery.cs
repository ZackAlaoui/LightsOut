using Game.Player;
using UnityEngine;

public class Battery : MonoBehaviour
{
    [SerializeField] float _batteryLife = 20f;

    private GameObject _player;

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
            Destroy(transform.gameObject);
        }
	}
}
