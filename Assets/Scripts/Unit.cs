using System;
using UnityEngine;
using UnityEngine.AI;
using Vector3 = UnityEngine.Vector3;

public class Unit : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;
    private bool walking = false;
    private bool running = false;
    private bool selected = false;

    
    public float speed = 1;

    // private void Awake()
    // {
    //     throw new NotImplementedException();
    // }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        
        animator.SetBool("run", false);
        running = false;

    }

    // Update is called once per frame
    void Update()
    {
        // Move toward target
        var move = Vector3.zero;
        if (agent.remainingDistance < 0.5f)
        {
            move = agent.desiredVelocity;
        }
        
        // Gravity
        // move.y -= 9.81f; 
        agent.Move(move * Time.deltaTime);
        
        CheckAnimationState(agent.velocity);
    }

    public void SetTarget(Vector3 pos)
    {
        agent.SetDestination(pos);
    }
    
    public void Select()
    {
        selected = true;
        Projector projector = GetComponentInChildren<Projector>();
        if (projector != null)
        {
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

    private void CheckAnimationState(Vector3 velocity)
    {
        animator.SetFloat("moveSpeed", velocity.magnitude);
    }
}
