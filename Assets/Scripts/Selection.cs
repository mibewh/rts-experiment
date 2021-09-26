using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        Debug.Log(inputValue.isPressed);
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
                if (bounds.Contains(unit.transform.position))
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

    public List<Unit> GetSelected()
    {
        return selected;
    }
}
