using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float panDamp = 1f;
    public float panSmoothTime = 0.1f;
    public float spinSpeed = 20f;
    
    public float zoomDamp = 1f;
    public float zoomSmoothTime = 0.5f;
    public float maxZoom = 12;
    public float minZoom = -30;

    public float rotationDamp = 10f;
    public float rotationSmoothTime = 0.1f;
    public float rotationRestoreSpeed = 1f;
    
    private Vector3 pan = Vector3.zero;
    private Vector3 targetPan = Vector3.zero;
    private Vector3 panSmoothV;

    private float zoom = 0;
    private float targetZoom = 0;
    private float zoomSmoothV;
    

    private bool rotating = false;
    private float pitch = 0;
    private float yaw = 0;
    private float targetPitch = 0;
    private float targetYaw = 0;
    private float spin;
    private Vector3 rotation;
    private Quaternion originalRotation;
    private Vector3 rotSmoothV;

    private Camera camera;
    private Unit selected = null;
    

    
    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
        originalRotation = transform.rotation;
    }

    void Update()
    {
        pan = Vector3.SmoothDamp(pan, targetPan, ref panSmoothV, panSmoothTime, Mathf.Infinity, Time.deltaTime);
        
        transform.Translate(pan, Space.World);

        // always apply spin
        // Vector3 rotation = Time.deltaTime * spinSpeed * spin * Vector3.forward;
        
        if (rotating)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            // transform.Rotate(rot, Space.Self);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (rotating)
        {
            Vector3 targetEulers = new Vector3(targetPitch, targetYaw, 0);
            if (Vector3.Distance(rotation, targetEulers) > 0.001f)
            {
                rotation = Vector3.SmoothDamp(rotation, targetEulers, ref rotSmoothV, rotationSmoothTime);
                transform.Rotate(rotation, Space.Self);
                float clampedX = Mathf.Clamp(transform.rotation.eulerAngles.x, 0, 90);
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
            }
        }
        else
        {
            // restore to original rotation
            transform.rotation = Quaternion.Lerp(transform.rotation, originalRotation, Time.deltaTime * rotationRestoreSpeed);
        }
        


        if (!Mathf.Approximately(zoom, targetZoom))
        {
            var dir = transform.forward;
            var prevZoom = zoom;
            zoom = Mathf.SmoothDamp(zoom, targetZoom, ref zoomSmoothV, zoomSmoothTime, Mathf.Infinity, Time.deltaTime);
            transform.position += (zoom - prevZoom) * dir;
        }
    }

    // Update is called once per frame

    void OnLook(InputValue value)
    {
        if (rotating)
        {
            Vector2 lookInput = value.Get<Vector2>();
            targetPitch = -lookInput.y / rotationDamp;
            targetYaw = lookInput.x / rotationDamp;
        }
        // targetRot = Quaternion.AngleAxis(targetPitch, Vector3.up) * Quaternion.AngleAxis(targetYaw, Vector3.right);
        // rot.x = -lookInput.y;
        // rot.y = lookInput.x;
    }

    void OnMove(InputValue value)
    {
        Vector2 panInput = value.Get<Vector2>() / panDamp;
        targetPan = new Vector3(panInput.x, 0, panInput.y);

    }

    void OnZoom(InputValue value)
    {
        float zoomInput = value.Get<float>();
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
        Debug.Log("OnRotate");
        
        if (value.isPressed)
        {
            rotating = true;
        }
        else
        {
            rotating = false;
            
            // Attempt at reseting camera to previous rotation (doesn't work)
            targetPitch = 0;
            targetYaw = 0;
            
            Debug.Log(targetPitch);
            Debug.Log(targetYaw);
            
            Debug.Log("reset");
        }
    }

    void OnSpin(InputValue value)
    {
        spin = value.Get<float>();
    }


}
