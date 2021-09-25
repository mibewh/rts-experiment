using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float panSpeed;
    public float panSmoothTime = 0.1f;
    
    private Vector3 pan = Vector3.zero;
    private Vector3 targetPan = Vector3.zero;
    private Vector3 panSmoothV;

    private Vector3 rot = Vector3.zero;

    private Camera camera;

    
    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        pan = Vector3.SmoothDamp(pan, targetPan, ref panSmoothV, panSmoothTime);

        transform.Translate(pan);
        // transform.Rotate(rot);
    }

    void OnMove(InputValue value)
    {
        Vector2 panInput = value.Get<Vector2>();
        targetPan = new Vector3(panInput.x, 0, panInput.y);

    }

    void OnLook(InputValue value)
    {
        Vector2 lookInput = value.Get<Vector2>();
        rot.x = -lookInput.y;
        rot.y = lookInput.x;
    }

    void OnFire(InputValue value)
    {
        if (value.isPressed)
        {
            Vector3? tryWorldClick = GetWorldClick();
            if (tryWorldClick is Vector3 worldClick)
            {
                Debug.Log("World Click: " + worldClick);
                var dogKnight = GameObject.Find("DogPBR").GetComponent<DogKnight>();
                dogKnight.SetTarget(worldClick);
            }
        }
    }


    Vector3? GetWorldClick()
    {
        // This will current only work for mouse. Replace with more generic "cursor" data for virtual cursor
        // set an event system here in the future, could activate an attack sequence

        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if(Physics.Raycast(ray, out RaycastHit hit, 1000f, LayerMask.GetMask("Terrain")))
        {
            return hit.point;
        }
        
        return null;
    }
    
    
}
