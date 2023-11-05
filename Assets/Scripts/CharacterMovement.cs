using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField] float walkAccel, runAccel, walkSpeed, runSpeed, vertical, dunkVert, blockVert, rotateSpeed, firstStep, ankleBrokenChance;

    Collider colliderComponent;
    CharacterShooting shooting;
    Rigidbody rb;
    Animator animator;
    CharacterManager manager;
    CharacterDribble dribble;
    IKController ik;

    float distToGround;
    bool firstStepAvailable;

    MovementState state;

    public bool InDefensiveStance
    {
        get
        {
            return animator.GetBool("Defense");
        }
        set
        {
            animator.SetBool("Defense", value);
        }
    }

    public bool IsRunning { 
        get
        {
            return animator.GetBool("Running");
        }
        set
        {
            animator.SetBool("Running", value);
        }
    }

    public Transform RimTransform
    {
        get
        {
            return SceneObjectsReferences.RimPosition;
        }
    }

    private void Start()
    {
        colliderComponent = GetComponent<Collider>();
        distToGround = colliderComponent.bounds.extents.y;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        shooting = GetComponent<CharacterShooting>();
        manager = GetComponent<CharacterManager>();
        dribble = GetComponent<CharacterDribble>();
        ik = GetComponent<IKController>();
        firstStepAvailable = true;
    }

    public void OnDribbleMove()
    {
        if (Random.Range(0f, 1f) < ankleBrokenChance)
        {
            AnkleBroken();
        }
    }

    void AnkleBroken()
    {
        animator.SetTrigger("AnkleBroken");
        animator.applyRootMotion = true;
        StartCoroutine(AnkleDelay());
    }
    IEnumerator AnkleDelay()
    {
        yield return new WaitForSeconds(2.8f);
        animator.applyRootMotion = false;
    }
    bool IsGrounded()
    {
        // Debug.DrawRay(colliderComponent.bounds.center, -Vector3.up, Color.red, distToGround + 0.1f);

        return Physics.Raycast(colliderComponent.bounds.center, -Vector3.up, distToGround + 0.1f);
    }

    public bool MovingInDirectionOfRim()
    {
        Vector3 direction = SceneObjectsReferences.RimPosition.transform.position - transform.position;
        direction.Normalize();

        return Vector3.Dot(direction, rb.velocity) > 0;
    }

    public void Move(Vector3 direction, bool isRunning, bool lookAt = true)
    {
        if (state == MovementState.DefensiveAction)
        {
            return;
        }

        float accel = isRunning ? runAccel : walkAccel;

        if (IsGrounded())
        {

            if (lookAt && direction != Vector3.zero)
            {
                var rotGoal = Quaternion.LookRotation(direction);
                
                direction = transform.forward;
                
                transform.rotation = Quaternion.Slerp(transform.rotation, rotGoal, rotateSpeed);


            }

            Debug.DrawRay(transform.position + new Vector3(0, 0.3f, 0), direction, Color.red);

            if (manager.CollidedWithPlayer && Physics.Raycast(transform.position + new Vector3(0, 0.3f, 0), direction, out RaycastHit hit, 1f, LayerMask.GetMask(new string[] {"Player"}) ) )
            {
                direction = hit.transform.forward;
            }

            if (rb.velocity.magnitude < 0.1f && !firstStepAvailable)
            {
                firstStepAvailable = true;
            }

            if (rb.velocity.magnitude < walkSpeed && firstStepAvailable && Ball.Instance.Handler == gameObject && isRunning && direction != Vector3.zero)
            {
                rb.AddForce(direction * firstStep);
                firstStepAvailable = false;
            }

            if (shooting.State == ShotState.Dunking)
            {
                return;
            }

            rb.AddForce(direction * accel * Time.deltaTime);
        }

        ClampMovement(isRunning);

        animator.SetBool("Move", rb.velocity.magnitude > 0.2f);

        IsRunning = rb.velocity.magnitude > walkSpeed;
    }

    public IEnumerator TempFastTurnRate()
    {
        var oldTurn = rotateSpeed;
        rotateSpeed *= 3;
        yield return new WaitForSeconds(0.2f);
        rotateSpeed = oldTurn;
    }

    public void Defense(bool isBlocking, bool isStealing)
    {
        GameObject handler = Ball.Instance.Handler;
        if (handler == null || handler.GetComponent<CharacterManager>().Team == manager.Team)
        {
            return;
        }

        if (isBlocking)
        {
            TryBlock();
        } else if (isStealing)
        {
            TrySteal();
        }
    }

    public void Jump()
    {
        rb.AddForce(vertical * Vector3.up);
    }

    public void DunkJump()
    {
        rb.velocity = Vector3.zero;

        Vector3 direction = (SceneObjectsReferences.RimPosition.position - transform.position).normalized;

        rb.AddForce(Vector3.Distance(transform.position, SceneObjectsReferences.RimPosition.position) * direction * vertical * dunkVert);
    }

    void ClampMovement(bool isRunning)
    {
        if (dribble.InADribbleMove)
        {
            return;
        }

        isRunning = isRunning &&
            shooting.State != ShotState.Aim;

        float maxSpeed = !isRunning ? walkSpeed - 0.1f : runSpeed; // accounting for weird floating point maths

        Vector3 clamped = Vector3.ClampMagnitude(rb.velocity, maxSpeed);

        rb.velocity = new Vector3(clamped.x, rb.velocity.y, clamped.z);
    }

    public void TryBlock()
    {
        
        if (!IsGrounded() || state == MovementState.DefensiveAction)
        {
            return;
        }

        GameObject opponent = Ball.Instance.Handler;

        if (opponent == null)
        {
            return;
        }
        

        state = MovementState.DefensiveAction;

        InDefensiveStance = false;
        animator.SetTrigger("Block");
        Jump();

        var opponentJumperSetPoint = opponent.GetComponent<CharacterShooting>().JumperSetPoint.position;
        rb.velocity *= 0.2f;
        rb.AddForce(Utility.GetDirection(transform.position, opponentJumperSetPoint) * blockVert);

        ik.SetBlockIK(true);

        StartCoroutine(DisableDefensiveActionState());
    }

    public void TrySteal()
    {
        if (state == MovementState.DefensiveAction)
        {
            return;
        }

        GameObject opponent = Ball.Instance.Handler;

        if (opponent == null)
        {
            return;
        }


        state = MovementState.DefensiveAction;
        animator.SetTrigger("Steal");

        rb.velocity *= 0.2f;

        ik.SetBlockIK(true);

        float minDistance = 3f;
        float maxDistance = 4f;

        if (Vector3.Distance(transform.position, opponent.transform.position) < Random.Range(minDistance, maxDistance) && opponent.GetComponent<CharacterDribble>().InADribbleMove)
        {
            // steal
            Ball.Instance.SetLoose();
            Ball.Instance.transform.position = transform.position;
        }

        StartCoroutine(DisableDefensiveActionState());
    }

    IEnumerator DisableDefensiveActionState()
    {
        yield return new WaitForSeconds(0.5f); // account for unity lag
        yield return new WaitUntil(IsGrounded);
        state = MovementState.Regular;
        ik.SetBlockIK(false);
    }

}

public enum MovementState
{
    Regular,
    DefensiveAction
}