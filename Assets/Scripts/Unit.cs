using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Vector3 = UnityEngine.Vector3;

public class Unit : Selectable
{
    private Animator animator;
    private NavMeshAgent agent;
    private bool attacking = false;
    private bool dead = false;
    private Unit targetUnit = null;
    private int currentHp = 1;
    private bool idle;
    private bool onAttackCooldown = false;

    
    public float speed = 1;
    public int maxHp = 100;
    public int damage = 20;
    public float minAttackDistance = 3;
    public float attackTargetDistance = 2;
    public float attackCooldown = 1;
    

    // private void Awake()
    // {
    //     throw new NotImplementedException();
    // }

    // Start is called before the first frame update
    protected void Awake()
    {
        base.Start();
        
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        currentHp = maxHp; // start with max health
    }

    // Update is called once per frame
    void Update()
    {
        CheckAnimationState(agent.velocity);
    }

    // TODO Proper rotation management in case unit not facing target
    void FixedUpdate()
    {
        Vector3 dir = transform.forward;
        agent.updateRotation = false;
        // Face the target
        if (targetUnit && !targetUnit.IsDead())
        {
            dir = targetUnit.transform.position - transform.position;
            
        }
        else
        {
            dir = agent.steeringTarget - transform.position;
        }

        bool shouldRotate = !idle;
        shouldRotate &= dir.magnitude > 0.25f;
        
        if (shouldRotate)
        {
            var targetRotation = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, agent.angularSpeed * Time.fixedDeltaTime);
        }

        bool haveTarget = UpdateTargetUnit();
        
        // Check if need to start a new attack
        if (!onAttackCooldown && !attacking && haveTarget)
        {
            StartCoroutine(AttackCoroutine(targetUnit));
        }
    }

    private bool UpdateTargetUnit()
    {
        if (targetUnit)
        {
            if (targetUnit.IsDead())
            {
                targetUnit = null;
            }
        }

        return targetUnit != null;
    }

    private void SetTarget(Vector3 pos)
    {
        agent.SetDestination(pos);
    }

    public void MoveToward(Vector3 pos)
    {
        Idle();
        idle = false;
        if (!pos.Equals(transform.position))
        {
            SetTarget(pos);
        }
    }

    public void AttackTarget(Unit target)
    {
        if (CanAttack(target))
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
    }

    private IEnumerator AttackCoroutine(Unit target)
    {
        yield return new WaitUntil(() => Vector3.Distance(transform.position, target.transform.position) < minAttackDistance);
        idle = false;
        attacking = true;
        yield return new WaitUntil(() => Vector3.Distance(transform.position, target.transform.position) < attackTargetDistance);
        SetTarget(transform.position); // stay still, but don't idle because that would cancel attack routine
        
        // yield return new WaitUntil(() =>
        // {
        //     bool stopAttacking = target.IsDead();
        //     return stopAttacking;
        // });
        //
        // Idle();
    }

    public void AttackHit()
    {
        Debug.Log("Attack hit");
        if (targetUnit != null)
        {
            targetUnit.Damage(damage, this);
            if (targetUnit.IsDead())
            {
                targetUnit = null;
                Idle();
            }
        }

        attacking = false;
        onAttackCooldown = true;
        StartCoroutine(DelayUtil.WaitAndDo(attackCooldown, () =>
        {
            onAttackCooldown = false;
        }));
    }

    public bool CanAttack(Unit other)
    {
        return !Equals(other);
    }

    public void Idle()
    {
        attacking = false;
        StopCoroutine("AttackCoroutine");
        agent.ResetPath();
    }

    private void CheckAnimationState(Vector3 velocity)
    {
        animator.SetFloat("moveSpeed", velocity.magnitude);
        animator.SetBool("attacking", attacking);
        animator.SetBool("dead", dead);
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

    public void Damage(int dmg, Unit damager = null)
    {
        currentHp -= dmg;
        if (currentHp <= 0)
        {
            Idle();
            dead = true;
        }

        if (targetUnit == null && damager)
        {
            AttackTarget(damager);
        }
    }

    public bool IsDead()
    {
        return dead;
    }

    // Called on death animation complete
    public void Die(float delay)
    {
        animator.speed = 0;
        Destroy(this.gameObject, delay);
    }
}
