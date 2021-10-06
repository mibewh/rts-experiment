using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 1f;
    public float panSmoothTime = 0.1f;
    public float rotationSpeed = 10f;
    public float spinSpeed = 20f;
    
    public float zoomDamp = 1f;
    public float zoomSmoothTime = 0.5f;
    public float maxZoom = 12;
    public float minZoom = -30;
    
    private Vector3 pan = Vector3.zero;
    private Vector3 targetPan = Vector3.zero;
    private Vector3 panSmoothV;

    private float zoom = 0;
    private float targetZoom = 0;
    private float zoomSmoothV;
    

    private bool rotating = false;
    private float pitch;
    private float yaw;
    private float spin;

    private Camera camera;
    private Unit selected = null;
    

    
    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        pan = panSpeed * Vector3.SmoothDamp(pan, targetPan, ref panSmoothV, panSmoothTime);
        
        transform.Translate(pan, Space.World);

        // always apply spin
        Vector3 rotation = Time.deltaTime * spinSpeed * spin * Vector3.forward;
        
        if (rotating)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            rotation += Time.deltaTime * rotationSpeed * pitch * Vector3.right +
                        Time.deltaTime * rotationSpeed * yaw * Vector3.up;
            // transform.Rotate(rot, Space.Self);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        transform.Rotate(rotation);


        if (!Mathf.Approximately(zoom, targetZoom))
        {
            var dir = transform.forward;
            var prevZoom = zoom;
            zoom = Mathf.SmoothDamp(zoom, targetZoom, ref zoomSmoothV, zoomSmoothTime);
            transform.position += (zoom - prevZoom) * dir;
            Debug.Log(zoom);
        }
    }

    void OnMove(InputValue value)
    {
        Vector2 panInput = value.Get<Vector2>();
        targetPan = new Vector3(panInput.x, 0, panInput.y);

    }

    void OnLook(InputValue value)
    {
        Vector2 lookInput = value.Get<Vector2>();
        pitch = -lookInput.y;
        yaw = lookInput.x;
        // targetRot = Quaternion.AngleAxis(targetPitch, Vector3.up) * Quaternion.AngleAxis(targetYaw, Vector3.right);
        // rot.x = -lookInput.y;
        // rot.y = lookInput.x;
    }

    void OnZoom(InputValue value)
    {
        float zoomInput = value.Get<float>();
        Debug.Log(zoomInput);
        targetZoom += zoomInput / zoomDamp;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

    }
    

    // void OnSelect(InputValue value)
    // {
    //     if (value.isPressed)
    //     {
    //         if (selected)
    //         {
    //             // selected.Deselect();
    //             selected = null;
    //         }
    //         
    //         Unit unit = GetUnitClick();
    //         if (unit)
    //         {
    //             // unit.Select();
    //             selected = unit;
    //         }
    //     }
    // }

    void OnRotate(InputValue value)
    {
        if (value.isPressed)
        {
            rotating = true;
        }
        else
        {
            rotating = false;
        }
    }

    void OnSpin(InputValue value)
    {
        spin = value.Get<float>();
    }


}
