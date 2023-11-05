using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public static Ball Instance { get; private set; }
    
    private const float speedLimit = 10;

    private AudioSource bounceSound;

    private Rigidbody rb;

    private BallStatus status;

    private BallShotMechanics mechanics;

    private bool aboveRim;

    private GameObject handler;

    private Team team;

    private ShotDistance distance;

    private new Collider collider;

    bool twoPtAttempt; // at the beginning of a shot.

    bool in2ptZone; // controlled by the collider

    bool scored;

    public BallStatus Status
    {
        get { return status; }
        set { 
            if (value == BallStatus.Dribbled)
            {
                aboveRim = false;
            }
            status = value; 
        }
    }

    public GameObject Handler
    {
        get
        {
            return handler;
        }
    }

    public Team HandlerTeam { 
        get
        {
            return team;
        }
    }

    public bool In2PTZone
    {
        get { return in2ptZone; }
    }


    private void Start()
    {
        status = BallStatus.Free;
        rb = GetComponent<Rigidbody>();
        bounceSound = GetComponent<AudioSource>();
        mechanics = GetComponent<BallShotMechanics>();
        collider = GetComponent<Collider>();
    }

    private void Awake()
    {
        Instance = this;
    }

    public void PickUp(GameObject handler)
    {

        this.handler = handler;
        PickedUpEvent(handler);
        team = handler.GetComponent<CharacterManager>().Team;
        status = BallStatus.Dribbled;  
        rb.isKinematic = true;

        transform.SetParent(handler.GetComponent<CharacterDribble>().BallBone);
        
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Release()
    {
        status = BallStatus.Shot;
        rb.isKinematic = false;
        transform.parent = null;
        GameController.Instance.StopShotClock();
    }

    public void SetLoose()
    {
        status = BallStatus.Free;
        rb.isKinematic = false;
        transform.parent = null;
        ReleasedEvent(handler);
        handler = null;
    }

    public void Shoot(ReleaseTiming timing, ShotDistance shotDistance)
    {
        distance = shotDistance;
        mechanics.Shoot(timing, shotDistance);
    }

    private void Update()
    {
        if (handler == null)
        {
            status = BallStatus.Free;
        }
    }

    public float OppositePlayerDistance()
    {
        if (handler == null)
        {
            return 0;
        }
        float minDistance = float.MaxValue;
        foreach (GameObject player in GameController.Instance.Players)
        {
            var charMan = player.GetComponentInChildren<CharacterManager>();
            if (charMan.Team == handler.GetComponent<CharacterManager>().Team)
            {
                continue;
            }
            if (charMan.BlockRadius == null)
            {
                return 0;
            }
            
            float distance = Vector3.Distance(transform.position, charMan.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }

        return minDistance;
    }

    
    IEnumerator MissedCheck()
    {
        Team prevTeam = team;
        yield return new WaitUntil(() => status != BallStatus.Free);
        if (!scored)
        {
            // missed
            MissedEvent(prevTeam);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (status == BallStatus.Shot && collision.gameObject != handler && transform.parent == null && handler != null)
        {
            status = BallStatus.Free;
            ReleasedEvent(handler);
            StartCoroutine(MissedCheck());  
            handler = null;
        }

        PlayBounceSound(rb.velocity.magnitude);
    }

    public void PlayBounceSound(float strength)
    {
        float normalizedVolume = strength / speedLimit;

        if (normalizedVolume > 1)
        {
            normalizedVolume = 1;
        }

        bounceSound.volume = normalizedVolume; // normalized volume
        bounceSound.time = 0.380f; // immediate bounce sound without dribble
        bounceSound.Play();
    }

    public void AssignDunkDistance()
    {
        distance = ShotDistance.UnderBasket;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("2pt"))
        {
            in2ptZone = true;
        }

        if (other.CompareTag("OutOfBounds"))
        {
            GameController.Instance.OutOfBounds();
        }

        if (handler != null && other.gameObject.CompareTag("Block") && other.gameObject.GetComponentInParent<CharacterManager>().Team != team && status != BallStatus.Dribbled)
        {
            rb.velocity = -rb.velocity;

            handler.GetComponent<CharacterShooting>().Release();
        }

        if (other.gameObject.CompareTag("Above The Rim"))
        {
            aboveRim = true;
            
        }

        if (other.gameObject.CompareTag("make") && aboveRim && team != GameController.Instance.JustScoredTeam) // if the ball is going down, no double scores.
        {
            Debug.Log("make");

            ScoredEvent(distance, team);
            scored = true;
            if (handler != null)
            {
                ReleasedEvent(handler);
            }

            var swish = other.gameObject.GetComponent<AudioSource>();
            swish.time = 0.4f;
            swish.Play();

            status = BallStatus.Free;
            handler = null;

            aboveRim = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("2pt"))
        {
            in2ptZone = false;
        }
    }

    public delegate void EventHandler( ShotDistance distance, Team team );
    public event EventHandler ScoredEvent = delegate { };

    public delegate void PickedUpHandler(GameObject handler);
    public event PickedUpHandler PickedUpEvent = delegate { };

    public delegate void ReleasedHandler(GameObject handler);
    public event ReleasedHandler ReleasedEvent = delegate { };

    public delegate void MissedHandler(Team team);
    public event MissedHandler MissedEvent = delegate { };
}

public enum BallStatus {
    Free,
    Dribbled,
    Shot
}

public enum Team
{
    None,
    Ballerz,
    Swishers
}
