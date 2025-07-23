using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Vector3 starPos_cylinder;
    public Transform transform_cylinder;

    private void Awake()
    {
        starPos_cylinder = transform_cylinder.position;
    }

    void Update()
    {
        MoveLeftRight();
        MoveForwardBackward();
        // MoveRotate();
    }

    void MoveLeftRight()
    {
        Vector3 vec_left = Vector3.zero;
        vec_left.x = Input.GetAxis("Horizontal");

        Vector3 v = new Vector3(vec_left.x, 0.0f, 0.0f) * Time.deltaTime * 15.0f;
        transform_cylinder.Translate(v, Space.Self);
    }

    void MoveForwardBackward()
    {
        Vector3 vec_forward = Vector3.zero;
        vec_forward.z = Input.GetAxis("Vertical");

        Vector3 v = new Vector3(0.0f, 0.0f, vec_forward.z) * Time.deltaTime * 15.0f;
        transform_cylinder.Translate(v, Space.Self);

    }

    // void MoveRotate()
    // {
    //     Vector3 vec_rotate = Vector3.zero;
    //     vec_rotate.y = Input.GetAxis("Rotate");

    //     Vector3 v = new Vector3(0.0f, vec_rotate.y, 0.0f) * Time.deltaTime * 15.0f;
    //     transform_cylinder.Rotate(v, Space.Self);
    // }

    // public float speed = 5f;
    // private Rigidbody rb;

    // void Start()
    // {
    //     rb = GetComponent<Rigidbody>();
    //     rb.freezeRotation = true;
    // }

    // void Update()
    // {
    //     float h = Input.GetAxis("Horizontal");
    //     float v = Input.GetAxis("Vertical");

    //     Vector3 move = transform.forward * v + transform.right * h;
    //     rb.MovePosition(rb.position + move * speed * Time.deltaTime);
    //     Debug.Log($"Player moved to position: {rb.position}");
    // }
}
