using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Vector3 = UnityEngine.Vector3;

public class Unit : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent;
    private Projector projector;
    private bool selected = false;
    private bool attacking = false;
    private Unit targetUnit = null;

    
    public float speed = 1;
    public Material selectedCircle;
    public Material attackCircle;

    // private void Awake()
    // {
    //     throw new NotImplementedException();
    // }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        projector = GetComponentInChildren<Projector>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckAnimationState(agent.velocity);
    }

    // TODO Proper rotation management in case unit not facing target
    void FixedUpdate()
    {
        // Face the target
        // if (targetUnit)
        // {
        //     agent.updateRotation = false;
        //     var dir = targetUnit.transform.position - transform.position;
        //     var targetRotation = Quaternion.FromToRotation(transform.forward.normalized, dir.normalized);
        //     transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, agent.angularSpeed);
        // }
        // else
        // {
        //     agent.updateRotation = true;
        // }
    }

    private void SetTarget(Vector3 pos)
    {
        agent.SetDestination(pos);
    }

    public void MoveToward(Vector3 pos)
    {
        attacking = false;
        SetTarget(pos);
    }

    public void AttackTarget(Unit target)
    {
        bool shouldTarget = true;
        
        // Meets criteria to flash indicator
        StartCoroutine(target.FlashAttackCircle(2, 8));

        // Reset target if it is a new order
        shouldTarget = targetUnit == null;
        shouldTarget = shouldTarget || target.GetInstanceID() != targetUnit.GetInstanceID();

        if (shouldTarget)
        {
            MoveToward(target.transform.position);
            StartCoroutine(AttackCoroutine(target));
            targetUnit = target;
        }
    }

    private IEnumerator AttackCoroutine(Unit target)
    {
        yield return new WaitUntil(() => Vector3.Distance(transform.position, target.transform.position) < 3);
        attacking = true;
        yield return new WaitUntil(() => Vector3.Distance(transform.position, target.transform.position) < 2);
        SetTarget(transform.position); // stay still
    }

    public void AttackHit()
    {
        Debug.Log("Attack hit");
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

    private void CheckAnimationState(Vector3 velocity)
    {
        animator.SetFloat("moveSpeed", velocity.magnitude);
        animator.SetBool("attacking", attacking);
    }

    public IEnumerator FlashAttackCircle(float times, float rate)
    {
        if (projector != null)
        {
            projector.material = attackCircle;
            for (int i = 0; i < times; i++)
            {
                projector.enabled = true;
                yield return new WaitForSeconds(1 / rate);
                projector.enabled = false;
                yield return new WaitForSeconds(1 / rate);
            }
        }
    }
}
