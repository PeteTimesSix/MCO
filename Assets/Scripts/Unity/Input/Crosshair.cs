﻿using UnityEngine;
using System.Collections;

public class Crosshair : MonoBehaviour
{
    public Camera CameraFacing;
    private Vector3 originalScale;

    // Use this for initialization
    void Start()
    {
        originalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hitCull;
        int cursorboxMask = 1 << 8;
        if (!Physics.Raycast(new Ray(CameraFacing.transform.position,
                                     CameraFacing.transform.rotation * Vector3.forward),
                             out hitCull, Mathf.Infinity, cursorboxMask))
        {
            //MonoBehaviour.print("ray miss");
            this.GetComponent<Renderer>().enabled = false;
        }
        else
        {
            //MonoBehaviour.print("ray hit");
            this.GetComponent<Renderer>().enabled = true; 
            RaycastHit hit;
            float distance;
            if (Physics.Raycast(new Ray(CameraFacing.transform.position,
                                         CameraFacing.transform.rotation * Vector3.forward),
                                 out hit))
            {
                distance = hit.distance;
            }
            else
            {
                distance = CameraFacing.farClipPlane * 0.95f;
            }
            transform.position = CameraFacing.transform.position +
                CameraFacing.transform.rotation * Vector3.forward * distance;
            transform.LookAt(CameraFacing.transform.position);
            transform.Rotate(0.0f, 180.0f, 0.0f);
            if (distance < 10.0f)
            {
                distance *= 1 + 5 * Mathf.Exp(-distance);
            }
            transform.localScale = originalScale * distance;
        }

        
    }
}