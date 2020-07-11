using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyPosition : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        if (target == null) return;

        transform.position = target.position + offset;
    }
}
