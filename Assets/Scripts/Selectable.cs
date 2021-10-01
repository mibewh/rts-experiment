using UnityEngine;

public class Selectable : MonoBehaviour
{
    public Material selectedCircle;
    public Material attackCircle;

    protected bool selected;
    protected Projector projector;


    protected virtual void Start()
    {
        projector = GetComponentInChildren<Projector>();
    }
    
    public void Select()
    {
        selected = true;
        if (projector != null)
        {
            projector.material = selectedCircle;
            projector.enabled = true;
        }
    }
    
    public void Deselect()
    {
        selected = false;
        Projector projector = GetComponentInChildren<Projector>();
        if (projector != null)
        {
            projector.enabled = false;
        }
    }
}