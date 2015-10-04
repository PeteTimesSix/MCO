using UnityEngine;
using System.Collections;

public class CameraFacingBillboard : MonoBehaviour
{
    public Camera m_Camera;

    public bool usePitch = true;
    public bool useYaw = false;
    public bool useRoll = false;

    private Quaternion originalRot;

    void Awake()
    {
        originalRot = transform.rotation;
    }

    void Update()
    {
        float pitch = 0;
        float yaw = 0;
        float roll = 0;

        if (usePitch) pitch = Camera.main.transform.eulerAngles.x;
        if (useYaw) yaw = Camera.main.transform.eulerAngles.y;
        if (useRoll) roll = Camera.main.transform.eulerAngles.z;
        transform.rotation = originalRot * Quaternion.Euler(pitch, yaw, roll);
    }
}