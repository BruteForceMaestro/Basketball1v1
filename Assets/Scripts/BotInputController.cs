using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;

public class BotInputController : MonoBehaviour
{
    CharacterMovement movement;
    CharacterShooting shooting;
    CharacterDribble dribble;
    CharacterManager manager;
    Collider plyCollider;

    Rigidbody rb;

    [SerializeField] float guardingCushion;
    [SerializeField] float totalBiteChance;
    [SerializeField] float bitePower;
    const float CUSHION_LOWER_BOUND = 0.7f;
    const float CUSHION_UPPER_BOUND = 3f;

    bool shootingInput = false;
    bool inAction = false;
    DribbleMove actionDribble = DribbleMove.None;
    Vector3 actionDirection;

    ShotDifficulty lastShotDifficulty; // difficulty of the last shot to adjust;

    float chanceToBiteOnCross = 0.2f; // totalBiteChance - chanceToBiteOnCross = chanceToBiteOnInNOut

    // Start is called before the first frame update
    void Start()
    {
        movement = GetComponent<CharacterMovement>();
        shooting = GetComponent<CharacterShooting>();
        dribble = GetComponent<CharacterDribble>();
        manager = GetComponent<CharacterManager>();
        rb = GetComponent<Rigidbody>();
        plyCollider = GetComponent<Collider>();
    }

    static Dictionary<ShotDifficulty, Dictionary<ActionType, float>> behaviours = new Dictionary<ShotDifficulty, Dictionary<ActionType, float>>()
    {
        [ShotDifficulty.Hard] = new Dictionary<ActionType, float>() // no shoot cause horrible percentage
        {
            [ActionType.Crossover] = 0.2f,
            [ActionType.InNOut] = 0.2f,
            [ActionType.Pullback] = 0.2f,
            [ActionType.DriveR] = 0.2f,
            [ActionType.DriveL] = 0.2f,
            [ActionType.Stop] = 0.3f,
            [ActionType.Shoot] = 0.3f
        },
        [ShotDifficulty.Normal] = new Dictionary<ActionType, float>()
        {
            [ActionType.Shoot] = 0.5f,
            [ActionType.Crossover] = 0.2f,
            [ActionType.InNOut] = 0.2f,
            [ActionType.Pullback] = 0.2f,
            [ActionType.DriveR] = 0.2f,
            [ActionType.DriveL] = 0.2f,
            [ActionType.Stop] = 0.3f
        }
    };

    // Update is called once per frame
    void Update()
    {
        MovingHandler();
        ShootingHandler();
        DribbleHandler();

    }

    public void OnHandlerDribbled(DribbleMove move)
    {
        if (Ball.Instance.Handler == gameObject)
        {
            return;
        }
        if (move == DribbleMove.Crossover)
        {
            if (Random.Range(0f, 1f) < chanceToBiteOnCross)
            {
                int directionModifier = Ball.Instance.Handler.GetComponent<CharacterDribble>().BallInLeftHand ? 1 : -1;
                rb.velocity += transform.right * bitePower * directionModifier;
                chanceToBiteOnCross -= 0.1f;
            }
            else
            {
                chanceToBiteOnCross += 0.1f;
            }
            
        }
        else if (move == DribbleMove.InNOut)
        {
            if (Random.Range(0f, 1f) < totalBiteChance - chanceToBiteOnCross)
            {
                int directionModifier = Ball.Instance.Handler.GetComponent<CharacterDribble>().BallInLeftHand ? -1 : 1;
                rb.velocity += transform.right * bitePower * directionModifier;
                chanceToBiteOnCross += 0.1f;
            }
            else
            {
                chanceToBiteOnCross -= 0.1f;
            }
        }
    }

    void DribbleHandler()
    {
        if (inAction)
        {
            dribble.BallDribble(actionDribble);
            return;
        }
        dribble.BallDribble(DribbleMove.None);
    }

    bool LaneToBasketAvailable()
    {
        return !Physics.Raycast(plyCollider.bounds.center, SceneObjectsReferences.RimPosition.position, 50f, LayerMask.GetMask(new string[] { "Player" })) && !manager.CollidedWithPlayer;
    }

    float EvaluateShotValue()
    {
        return -(int)shooting.GetShotDifficulty() - Vector3.Distance(transform.position, SceneObjectsReferences.RimPosition.position) + Ball.Instance.OppositePlayerDistance();
    }

    void MovingHandler()
    {
        bool isRunning = true;
        Vector3 direction = Vector3.zero;
        bool lookAt = true;

        if (Ball.Instance.Status == BallStatus.Free)
        {
            if (GameController.Instance.JustScoredTeam == manager.Team)
            {
                direction = Vector3.zero;
            } 
            else
            {
                direction = Utility.GetDirection(transform.position, Ball.Instance.transform.position);
                direction = Utility.Horizontal(direction);
                isRunning = true;
            }
            
        }

        if (Ball.Instance.Status == BallStatus.Dribbled)
        {
            if (Ball.Instance.Handler.GetComponent<CharacterManager>().Team != manager.Team)
            {
                
                if (Ball.Instance.Handler.GetComponent<CharacterShooting>().State != ShotState.NotShooting)
                {
                    movement.TryBlock();
                }
                lookAt = false;
                transform.LookAt(Utility.Horizontal(Ball.Instance.transform.position) + new Vector3(0f, transform.position.y, 0f));
                if (IsHandlerMovingTowardsThis())
                {
                    if (manager.IsBeingBumped)
                    {
                        direction = Utility.GetDirection(transform.position, Ball.Instance.Handler.transform.position);
                        direction = Utility.Horizontal(direction);
                    } else
                    {
                        direction = Vector3.zero;
                    }

                }
                else
                {
                    direction = GetGuardingDirection();
                }
            }
            else if (Ball.Instance.Handler == gameObject)
            {
                if (shooting.State == ShotState.Aim)
                {
                    return;
                }

                if (GameController.Instance.ScoreTurnTeam != manager.Team)
                {
                    if (manager.CollidedWithPlayer)
                    {
                        StartCoroutine(ExecuteCrossover());
                    }
                    direction = Utility.GetDirection(transform.position, SceneObjectsReferences.TopOfKey.position);
                    direction = Utility.Horizontal(direction);
                }

                else
                {
                    direction = HandlerAI();
                }

            }

        }

        movement.Move(direction, isRunning, lookAt);
    }

    Vector3 HandlerAI()
    {
        Vector3 direction = Vector3.zero;
        // evaluate shot.
        ShotDifficulty difficulty = shooting.GetShotDifficulty();
        float currShotValue = EvaluateShotValue();

        if (difficulty == ShotDifficulty.Easy)
        {
            lastShotDifficulty = difficulty;
            StartCoroutine(ShootInput());
            return Vector3.zero;
        }


        if (inAction)
        {
            direction = actionDirection;
        }

        else if (Ball.Instance.OppositePlayerDistance() > 5)
        {
            StartCoroutine(ExecuteStopAndShoot());
        }

        else if (LaneToBasketAvailable())
        {
            if (Vector3.Distance(transform.position, SceneObjectsReferences.RimPosition.position) < 4)
            {
                StartCoroutine(ExecuteStopAndShoot());
            }
            //go to the basket
            direction = Utility.GetDirection(transform.position, SceneObjectsReferences.RimPosition.position);
            direction = Utility.Horizontal(direction);
        }

        else
        {
            WeightedActionSelection(difficulty, currShotValue);
        }

        return direction;
    }

    void WeightedActionSelection(ShotDifficulty difficulty, float currShotValue)
    {
        
        var behaviour = behaviours[difficulty];
        float sumOfAllWeights = 0f;
        foreach (float actionWeight in behaviour.Values)
        {
            sumOfAllWeights += actionWeight;
        }

        float rand = Random.Range(0, sumOfAllWeights);

        foreach (var actionKVP in behaviour)
        {
            if (rand < actionKVP.Value)
            {
                StartCoroutine(ExecuteActionAndAdjustWeight(actionKVP.Key, currShotValue, difficulty));
                return;
            }
            rand -= actionKVP.Value;
        }
    }

    IEnumerator ExecuteActionAndAdjustWeight(ActionType action, float currShotValue, ShotDifficulty difficultyInitial)
    {
        Debug.Log($"Selected Action: {action}");
        inAction = true;

        float waitTime = 0f;
        switch (action)
        {
            case ActionType.Shoot:
                lastShotDifficulty = difficultyInitial;
                StartCoroutine(ShootInput());
                break;
            case ActionType.DriveR:
                actionDirection = transform.right;
                actionDribble = DribbleMove.None;
                waitTime = 0.3f;
                break;
            case ActionType.DriveL:
                actionDirection = -transform.right;
                actionDribble = DribbleMove.None;
                waitTime = 0.3f;
                break;
            case ActionType.Crossover:
                actionDribble = DribbleMove.Crossover;
                waitTime = 0.3f;
                break;
            case ActionType.InNOut:
                actionDribble = DribbleMove.InNOut;
                waitTime = 0.3f;
                break;
            case ActionType.Pullback:
                actionDribble = DribbleMove.Pullback;
                waitTime = 0.3f;
                break;
            case ActionType.Stop:
                actionDirection = Vector3.zero;
                actionDribble = DribbleMove.None;
                waitTime = 0.5f;
                break;
        }

        yield return new WaitForSeconds(waitTime);
        float newShotValue = EvaluateShotValue();

        behaviours[difficultyInitial][action] += newShotValue - currShotValue - 1f; // the MACHINE LEARNING!!!!

        if (behaviours[difficultyInitial][action] <= 0)
        {
            behaviours[difficultyInitial][action] = 0.01f;
        }

        inAction = false;
    }

    IEnumerator ExecuteCrossover()
    {
        inAction = true;
        actionDribble = DribbleMove.Crossover;
        float waitTime = 0.5f;
        yield return new WaitForSeconds(waitTime);
        inAction = false;
    }

    IEnumerator ExecuteStopAndShoot()
    {
        inAction = true;
        actionDirection = Vector3.zero;
        actionDribble = DribbleMove.None;
        float waitTime = 0.5f;
        yield return new WaitForSeconds(waitTime);
        inAction = false;
        StartCoroutine(ShootInput());
        lastShotDifficulty = shooting.GetShotDifficulty();
    }

    public void OnScored(ShotDistance distance, Team team)
    {
        if (team == manager.Team && behaviours.TryGetValue(lastShotDifficulty, out var behav))
        {
            behav[ActionType.Shoot] += 0.1f;
        }
        if ((int)distance > (int)ShotDistance.ShortMidrange)
        {
            guardingCushion -= 0.2f;
        } else
        {
            guardingCushion += 0.2f;
        }

        guardingCushion = Mathf.Clamp(guardingCushion, CUSHION_LOWER_BOUND, CUSHION_UPPER_BOUND);
    }

    public void Missed(Team team)
    {
        if (team == manager.Team && behaviours.TryGetValue(lastShotDifficulty, out var behav))
        {
            behav[ActionType.Shoot] -= 0.1f;
        }
    }

    IEnumerator ShootInput()
    {
        float inaccuracy = !shooting.IsDunkAvailable() ? Random.Range(0, 0.03f) : Random.Range(0, 0.03f) * (shooting.DunkSpeed / shooting.JumperSpeed) ;
        float time = !shooting.IsDunkAvailable() ? shooting.JumperSpeed * 0.75f + inaccuracy : shooting.DunkSpeed * 0.75f + inaccuracy;
        
        shootingInput = true;

        yield return new WaitForSeconds(time);
        shootingInput = false;

    }

    bool IsHandlerMovingTowardsThis()
    {
        var ballRb = Ball.Instance.Handler.GetComponent<Rigidbody>();
        return Vector3.Angle(ballRb.velocity, Utility.GetDirection(Ball.Instance.Handler.transform.position, transform.position)) < 20f && ballRb.velocity.magnitude > 0.5f;

    }

    Vector3 GetGuardingDirection()
    {
        GameObject handler = Ball.Instance.Handler;
        Transform handlerTransform = handler.transform;

        Vector3 horizontalRimPosition = Utility.Horizontal(movement.RimTransform.position);
        Vector3 horizontalBallPosition = Utility.Horizontal(handlerTransform.position);

        Vector3 ballToRimDirection = Utility.GetDirection(horizontalBallPosition, horizontalRimPosition);

        float distanceToRim = Vector3.Distance(handlerTransform.position, movement.RimTransform.position) / 6;
        

        Vector3 ideal_guarding_positon = ballToRimDirection * guardingCushion * distanceToRim + handlerTransform.position + handlerTransform.forward * distanceToRim * 0.1f;

        if ((transform.position - ideal_guarding_positon).magnitude < 0.01f)
        {
            return Vector3.zero;
        }

        return Utility.GetDirection(transform.position, ideal_guarding_positon);
    }

    void ShootingHandler()
    {
        if (shooting.State == ShotState.Dunking)
        {
            shooting.Dunk(shootingInput);
            return;
        }
        shooting.Shoot(shootingInput);
    }
}

// all actions that bot can take in difficult situatiosn
enum ActionType
{
    DriveR,
    DriveL,
    Pullback,
    Crossover,
    InNOut,
    Shoot,
    Stop
}
