using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float baseSpeed = 5;

    private CharacterController controller;
    private InputAction moveAction;

    private float movementSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();
        moveAction = InputSystem.actions.FindAction("Move");

        movementSpeed = baseSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 direction = moveAction.ReadValue<Vector2>();
        Vector3 velocity = movementSpeed * new Vector3(direction.x, 0, direction.y * (float) Math.Sqrt(2));

        controller.Move(velocity * Time.deltaTime);
    }
}
