using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Controller : MonoBehaviour
{
    public Camera headCamera;
    float vAngle = 0;
    public float speed = 2f;

    CharacterController ctrl;

    private void Start()
    {
        ctrl = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            vAngle -= Input.GetAxis("Mouse Y");
            vAngle = Mathf.Clamp(vAngle, -90, 90);

            headCamera.transform.localEulerAngles = Vector3.right * vAngle;

            transform.Rotate(0f, Input.GetAxis("Mouse X"), 0f);
        }

        // ctrl.SimpleMove( ( transform.forward * Input.GetAxis("Vertical") + transform.right*Input.GetAxis("Horizontal" ) ) * speed * Time.deltaTime);
        transform.Translate(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")) * speed * Time.deltaTime, Space.Self);
    }
}
