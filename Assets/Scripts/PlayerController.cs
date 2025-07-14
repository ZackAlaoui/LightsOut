using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _baseSpeed = 7f;
    [SerializeField] private GameObject _model;

    private CharacterController _controller;
    private InputAction _moveAction;
    private InputAction _lookAction;

    private float _movementSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_model == null) _model = transform.Find("Model").gameObject;

        _controller = GetComponent<CharacterController>();
        _moveAction = InputSystem.actions.FindAction("Move");
        _lookAction = InputSystem.actions.FindAction("Look");

        _movementSpeed = _baseSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveDirection = _moveAction.ReadValue<Vector2>();
        Vector3 velocity = _movementSpeed * new Vector3(moveDirection.x, 0f, moveDirection.y);
        _controller.Move(velocity * Time.deltaTime);

        Plane lookPlane = new(Vector3.up, transform.position);
        Vector2 pointerPosition = _lookAction.ReadValue<Vector2>();
        Ray cameraRay = Camera.main.ScreenPointToRay((Vector3)pointerPosition);
        bool hit = lookPlane.Raycast(cameraRay, out float distanceFromCamera);
        if (hit)
        {
            _model.transform.LookAt(cameraRay.GetPoint(distanceFromCamera));
        }
    }
}
