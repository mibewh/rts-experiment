using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Selection : MonoBehaviour
{
    public Camera camera;
    
    bool isSelecting = false;
    Vector3 mousePosition1;

    private List<Selectable> selected;

    private void Awake()
    {
        selected = new List<Selectable>();
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

            foreach (Selectable selectable in selected)
            {
                if (selectable != null && !selectable.IsDestroyed())
                {
                    selectable.Deselect();
                }
            }

            selected.Clear();

            // Pls optimize me i am just a code
            Unit[] allUnits = GameObject.FindObjectsOfType<Unit>();
            foreach (Unit unit in allUnits)
            {
                if (bounds.Contains(camera.WorldToViewportPoint(unit.transform.position)))
                {
                    selected.Add(unit);
                }
            }

            // Next see if there were buildings
            if (selected.Count == 0)
            {
                Building[] allBuildings = GameObject.FindObjectsOfType<Building>();
                
            }

            // Fallback to raycast
            if (selected.Count == 0)
            {
                Selectable selectable = GetSelectableClick();
                if (selectable)
                {
                    selected.Add(selectable);
                }
            }

            foreach (Selectable selectable in selected)
            {
                selectable.Select();
            }
        }
    }
    
    void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            List<Unit> selectedUnits = GetSelectedUnits();
            if (selectedUnits.Count > 0)
            {
                // check if attack
                Selectable targetSelect = GetSelectableClick();
                if (targetSelect is Unit)
                {
                    selectedUnits.ForEach(delegate(Unit unit)
                    {
                        unit.AttackTarget((Unit) targetSelect);
                    });
                }
                else
                {
                    // Move units to point
                    Vector3? tryWorldClick = GetTerrainClick();
                    if (tryWorldClick is Vector3 worldClick)
                    {
                        Debug.Log("World Click: " + worldClick);

                        FormUnitsAround(selectedUnits, worldClick);
                    }
                }
            }
        }
    }

    void OnSpawn(InputValue value)
    {
        if (value.isPressed)
        {
            List<Building> buildings = GetSelectedBuildings();
            if (buildings.Count > 0)
            {
                buildings.ForEach(building => {
                    building.SpawnUnit();
                });
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
    
    Selectable GetSelectableClick()
    {
        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Selectable selectable = null;
        if(Physics.Raycast(ray, out RaycastHit hit, 1000f, LayerMask.GetMask("Unit", "Building")))
        {
            selectable = hit.transform.GetComponentInParent<Selectable>();
        }

        return selectable;
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

    public List<Selectable> GetSelected()
    {
        return selected;
    }

    public List<Unit> GetSelectedUnits()
    {
        return GetSelected().OfType<Unit>().ToList();
    }
    
    public List<Building> GetSelectedBuildings()
    {
        return GetSelected().OfType<Building>().ToList();
    }

    private void FormUnitsAround(List<Unit> units, Vector3 target)
    {
        if (units.Count > 0)
        {
            // Get the right normal of the direction to the destination
            var center = GetSelectedCenter();
            var dir = (target - center).normalized;
            // var right = Quaternion.Euler(0, 90, 0) * dir;
            List<Vector3> pointsAround = GetPointsAround(target, dir, units.Count, (units.Count - 1) * Mathf.Sqrt(units.Count));

            units.Sort(delegate(Unit unit1, Unit unit2)
            {
                return Vector3.Distance(unit2.transform.position, target)
                    .CompareTo(Vector3.Distance(unit1.transform.position, target));
            });
                    
                    
            for (int i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                // Vector3 target = worldClick + (right * i * 5);
                Vector3 radialTarget = pointsAround[i];
                radialTarget = GetNavMeshPoint(radialTarget);
                        
                // convert target to navmsesh point
                Debug.Log("Agent Target: " + radialTarget);
                unit.MoveToward(radialTarget);
            }
                    
            // units.ForEach(unit => unit.SetTarget(worldClick));
        }
    }


    // Generates num points in a circle of radius rad around a center
    private List<Vector3> GetPointsAround(Vector3 center, Vector3 forward, int num, float rad)
    {
        List<Vector3> results = new List<Vector3>();
        int offset = 90;
        for (int i = 0; i < num; i++)
        {
            Vector3 delta = Quaternion.AngleAxis((360 / num * i) + offset, Vector3.up) * forward.normalized * rad;
            results.Add(center + delta);
        }

        Vector3 back = results[0];
        
        results.Sort((Vector3 vector1, Vector3 vector2) =>
        {
            return Vector3.Distance(vector1, back).CompareTo(Vector3.Distance(vector2, back));
        });

        return results;
    }
}
