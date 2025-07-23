using UnityEngine;

public class Raycast : MonoBehaviour
{
    public float raycast_length;

    //Update is called once per frame
    void Update()
    { 
        Debug.DrawRay(transform.position, transform.forward * raycast_length, Color.red);
    }
}