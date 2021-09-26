using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Selection : MonoBehaviour
{
    public Camera camera;
    
    bool isSelecting = false;
    Vector3 mousePosition1;

    private List<Unit> selected;

    private void Awake()
    {
        selected = new List<Unit>();
    }

    void OnSelect(InputValue inputValue)
    {
        // If we press the left mouse button, save mouse location and begin selection
        if (inputValue.isPressed)
        {
            isSelecting = true;
            mousePosition1 = Mouse.current.position.ReadValue();
        }
        // If we let go of the left mouse button, end selection
        else
        {
            isSelecting = false;
            Vector3 mousePosition2 = Mouse.current.position.ReadValue();
            Bounds bounds = RectangleUtil.GetViewportBounds(camera, mousePosition1, mousePosition2);

            foreach (Unit unit in selected)
            {
                unit.Deselect();
            }
            
            // Pls optimize me i am just a code
            Unit[] allUnits = GameObject.FindObjectsOfType<Unit>();
            selected.Clear();
            foreach (Unit unit in allUnits)
            {
                if (bounds.Contains(camera.WorldToViewportPoint(unit.transform.position)))
                {
                    selected.Add(unit);
                }
            }

            // Fallback to raycast
            if (selected.Count == 0)
            {
                Unit unit = GetUnitClick();
                if (unit)
                {
                    selected.Add(unit);
                }
            }

            foreach (Unit unit in selected)
            {
                unit.Select();
            }
        }
    }
    
    void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            Vector3? tryWorldClick = GetTerrainClick();
            if (tryWorldClick is Vector3 worldClick)
            {
                Debug.Log("World Click: " + worldClick);

                if (selected.Count > 0)
                {
                    // Get the right normal of the direction to the destination
                    var center = GetSelectedCenter();
                    var dir = (worldClick - center).normalized;
                    var right = Quaternion.Euler(0, 90, 0) * dir;

                    for (int i = 0; i < selected.Count; i++)
                    {
                        var unit = selected[i];
                        Vector3 target = worldClick + (right * i * 5);
                        target = GetNavMeshPoint(target);
                        
                        // convert target to navmsesh point
                        Debug.Log("Target: " + target);
                        unit.SetTarget(target);
                    }
                    
                    // selected.ForEach(unit => unit.SetTarget(worldClick));
                }
            }
        }
    }
    
    Vector3? GetTerrainClick()
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
    
    Unit GetUnitClick()
    {
        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Unit unit = null;
        if(Physics.Raycast(ray, out RaycastHit hit, 1000f, LayerMask.GetMask("Unit")))
        {
            unit = hit.transform.GetComponentInParent<Unit>();
        }

        return unit;
    }
 
    void OnGUI()
    {
        if( isSelecting )
        {
            // Create a rect from both mouse positions
            var rect = RectangleUtil.GetScreenRect( mousePosition1, Mouse.current.position.ReadValue());
            RectangleUtil.DrawScreenRect( rect, new Color( 0.8f, 0.8f, 0.95f, 0.25f ) );
            RectangleUtil.DrawScreenRectBorder( rect, 2, new Color( 0.8f, 0.8f, 0.95f ) );
        }
    }

    private Vector3 GetSelectedCenter()
    {
        var total = Vector3.zero;
        foreach (var sel in selected)
        {
            total += sel.transform.position;
        }

        return total / selected.Count;
    }

    private Vector3 GetNavMeshPoint(Vector3 point)
    {
        Vector3 target = point;
        if (NavMesh.SamplePosition(point, out NavMeshHit hit, 4.0f, NavMesh.AllAreas))
        {
            target = hit.position;
        }

        return target;
    }

    public List<Unit> GetSelected()
    {
        return selected;
    }
}
