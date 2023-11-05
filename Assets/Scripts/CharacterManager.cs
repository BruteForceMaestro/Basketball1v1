
using Assets;
using UnityEngine;

// handles collision and other character wide logic
public class CharacterManager : MonoBehaviour
{
    [SerializeField] Team team;
    [SerializeField] Transform blockRadius;

    public bool CollidedWithPlayer { get; private set; }

    public Team Team
    {
        get { return team; }
    }

    public Transform BlockRadius
    {
        get { return blockRadius; }
    }

    public bool IsBeingBumped { get; private set; }

    Animator animator;
    CharacterSwitcher characterSwitcher;
    CharacterMovement movement;
    Rigidbody rb;


    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<CharacterMovement>();
        characterSwitcher = GetComponentInParent<CharacterSwitcher>();
    }

    private void Update()
    {
        if (Ball.Instance.Handler != null && Ball.Instance.Handler != gameObject && Ball.Instance.Status != BallStatus.Shot)
        {
            movement.InDefensiveStance = true;
        }
        if (Ball.Instance.Status == BallStatus.Free || Ball.Instance.Status == BallStatus.Shot)
        {
            movement.InDefensiveStance = false;
        }
    }

    public void PlayBallBounceSound() // this is here due to animation events
    {
        Ball.Instance.PlayBounceSound(5f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Ball ball) && ball.Status == BallStatus.Free && GameController.Instance.JustScoredTeam != team)
        {
            characterSwitcher.SwitchToHandler();
            return;
        }

        if (collision.gameObject.TryGetComponent<CharacterMovement>(out _)) // collided with player
        { 
            CollidedWithPlayer = true;
        }
    }

    private void OnCollisionStay(Collision collision)
    {

        if (collision.gameObject.TryGetComponent<CharacterMovement>(out _)) // collided with player
        {
            if (Ball.Instance.Handler == null)
            {
                return;
            }
            if (Ball.Instance.Handler == gameObject)
            {
                
                rb.AddForce(collision.transform.forward * Mathf.Pow(collision.rigidbody.mass, 2f) + -rb.velocity);

            }
            else if (Ball.Instance.HandlerTeam != team)
            {
                IsBeingBumped = true;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<CharacterMovement>(out _)) // collided with player
        {
            IsBeingBumped = false;
            CollidedWithPlayer = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("2pt") && Ball.Instance.Handler == gameObject && GameController.Instance.ScoreTurnTeam != team)
        {
            GameController.Instance.ScoreTurnReset(team);
        }
    }

}
