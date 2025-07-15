using System;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera Camera { get; private set; }
    [SerializeField] private Matrix4x4 projectionMatrix;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Camera = GetComponent<Camera>();
        Matrix4x4 p = Camera.projectionMatrix;
        p.m11 = 0.0883883476483f; // Scale vertical axis of the viewport by a factor of sqrt(2) to account for camera angle
        Camera.projectionMatrix = p;
    }
}
