using UnityEngine;

public class enemyanim : MonoBehaviour
{

    Rigidbody rb;
    Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        animator.SetFloat("xVelocity", rb.linearVelocity.x);
        animator.SetFloat("zVelocity", rb.linearVelocity.z);
    }
}
