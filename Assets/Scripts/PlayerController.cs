using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _baseSpeed = 7f;

    private CharacterController _controller;
    private InputAction _moveAction;

    private float _movementSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _moveAction = InputSystem.actions.FindAction("Move");

        _movementSpeed = _baseSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 direction = _moveAction.ReadValue<Vector2>();
        Vector3 velocity = _movementSpeed * new Vector3(direction.x, 0f, direction.y * (float) Math.Sqrt(2));

        _controller.Move(velocity * Time.deltaTime);
    }
}
