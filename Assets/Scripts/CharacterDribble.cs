using Assets.Scripts.DribbleAnimations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterDribble : MonoBehaviour
{
    [SerializeField] float crossPower;
    [SerializeField] Transform ballBone;
    CharacterManager manager;
    CharacterMovement movement;
    Animator animator;
    Rigidbody rb;

    bool leftHandDribble = false;
    bool inAnimation = false;

    Dictionary<DribbleMove, DribbleAnimation> dribbleAnimations;

    public bool InADribbleMove
    {
        get
        {
            return inAnimation;
        }
        set
        {
            inAnimation = value;
        }
    }

    public Transform BallBone
    {
        get { return ballBone; }
    }
    public float CrossPower
    {
        get { return crossPower; }
    }

    public bool BallInLeftHand
    {
        get { return leftHandDribble; }
        set
        {
            leftHandDribble = value;
            // Ball.Instance.InLeftHand = value;
            StartCoroutine(SwitchHandDelay(value));
        }
    }

    CharacterShooting shooting;

    private void Start()
    {
        animator = GetComponent<Animator>();
        manager = GetComponent<CharacterManager>();
        shooting = GetComponent<CharacterShooting>();
        movement = GetComponent<CharacterMovement>();
        rb = GetComponent<Rigidbody>(); 

        dribbleAnimations = new Dictionary<DribbleMove, DribbleAnimation>();
        dribbleAnimations[DribbleMove.Pullback] = new PullbackAnimation();
        dribbleAnimations[DribbleMove.Crossover] = new CrossoverAnimation();
        dribbleAnimations[DribbleMove.InNOut] = new InNOutAnimation();
    }
    public void StartWait()
    {
        StartCoroutine(WaitForEndOfAnimation());
    }

    IEnumerator WaitForEndOfAnimation()
    {
        yield return new WaitForSeconds(0.3f);
        if (shooting.State == ShotState.NotShooting)
        {
            animator.SetBool("HasBall", true);
        }
        inAnimation = false;
        Ball.Instance.transform.localPosition = Vector3.zero;
        Ball.Instance.transform.localRotation = Quaternion.identity;

    }

    IEnumerator SwitchHandDelay(bool val)
    {
        //again some weird unity stuff
        //animator doesn't trigger animation immediately, waits but then the hand is already switch
        // this should prevent bugs related to fast switching of the hand.
        yield return new WaitForEndOfFrame();
        animator.SetBool("DribbleInLeft", val);
    }

    public void BallDribble(DribbleMove dribbleMove)
    {
        if (Ball.Instance.Handler == gameObject)
        {
            if (dribbleMove != DribbleMove.None && !inAnimation)
            {
                dribbleAnimations[dribbleMove].Play(this);
                DribbleEvent(dribbleMove);
            } else
            {
                // Ball.Instance.transform.position = ballBone.position + ballOffset.localPosition;
            }

        }
    }

    public delegate void OnDribbleMove(DribbleMove dribbleMove);
    public event OnDribbleMove DribbleEvent = delegate { };

}

public enum DribbleMove
{
    None,
    Crossover,
    Pullback,
    InNOut
}