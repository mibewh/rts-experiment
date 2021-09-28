using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Vector3 = UnityEngine.Vector3;

public class Unit : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;
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

    }

    // Update is called once per frame
    void Update()
    {
        CheckAnimationState(agent.velocity);
    }

    public void SetTarget(Vector3 pos)
    {
        agent.SetDestination(pos);
    }

    public void AttackTarget(Unit target)
    {
        SetTarget(target.transform.position);
        StartCoroutine(AttackCoroutine(target));
    }

    private IEnumerator AttackCoroutine(Unit target)
    {
        yield return new WaitUntil(() => Vector3.Distance(transform.position, target.transform.position) < 4);
        SetTarget(transform.position); // stay still
        animator.SetBool("attacking", true);
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
