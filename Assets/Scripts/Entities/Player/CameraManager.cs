using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera Camera { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Camera = GetComponent<Camera>();
        Vector4 x = new(1 / (Camera.aspect * Camera.orthographicSize), 0, 0, 0);
        Vector4 y = new(0, Mathf.Sqrt(2) / Camera.orthographicSize, 0, 0);
        Vector4 z = new(0, 0, -2 / (Camera.farClipPlane - Camera.nearClipPlane), 0);
        Vector4 w = new(0, 0, -(Camera.farClipPlane + Camera.nearClipPlane) / (Camera.farClipPlane - Camera.nearClipPlane), 1);
        Camera.projectionMatrix = new Matrix4x4(x, y, z, w);
    }
}
