using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCameraControl : MonoBehaviour
{
    public Transform target;

    public Vector3 offset = new Vector3(0, 10, 0);
    public GameObject minimapPoint;
    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

     void Start()
    {
        if (minimapPoint != null)
        {
            minimapPoint.SetActive(true);
        }
    }
}