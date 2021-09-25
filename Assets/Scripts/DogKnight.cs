using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class DogKnight : MonoBehaviour
{
    private Animator animator;
    private CharacterController characterController;
    private bool walking = false;
    private bool running = false;
    private Vector3 targetPos;

    public float speed = 1;
    
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        
        animator.SetBool("run", false);
        running = false;
        targetPos = transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.W))
        // {
        //     Debug.Log("w down");
        //     running = true;
        //     animator.SetBool("run", true);
        // }
        // else if (Input.GetKeyUp(KeyCode.W))
        // {
        //     Debug.Log("w up");
        //     running = false;
        //     animator.SetBool("run", false);
        //
        // }

        // Move toward target
        var move = Vector3.zero;
        if (transform.position != targetPos)
        {
            move = MoveToward(targetPos);
        }
        
        // Gravity
        move.y -= 9.81f;
        
        CollisionFlags collisionFlags = characterController.Move(move * Time.deltaTime);
        
        CheckAnimationState(move, collisionFlags);
    }

    public void SetTarget(Vector3 pos)
    {
        targetPos = pos;
    }

    private void CheckAnimationState(Vector3 velocity, CollisionFlags collisionFlags)
    {
        if ((collisionFlags & CollisionFlags.CollidedBelow) == 0)
        {
            Debug.Log("Falling!");
        }
    }

    private Vector3 MoveToward(Vector3 target)
    {
        var offset = target - transform.position;
        // normalize and adjust for speed
        offset = offset.normalized * speed;
        return offset;
    }
}
