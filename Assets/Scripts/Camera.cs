using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    [SerializeField] Transform CameraTarget;
    [SerializeField] Vector3 CameraOffset;
    [SerializeField] Vector3 CameraRotation;

    private void Awake()
    {
        // Set camera position to target position before start
        transform.position = CameraTarget.position + CameraOffset;
        transform.eulerAngles = CameraRotation;
    }

    private void LateUpdate()
    {
        transform.position = CameraTarget.position + CameraOffset;
    }
}
