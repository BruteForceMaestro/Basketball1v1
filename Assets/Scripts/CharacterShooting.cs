using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterShooting : MonoBehaviour
{
    [SerializeField] float jumperDurationSeconds = 2;
    
    [SerializeField] float dunkDurationSeconds = 1;

    [SerializeField] float dunkForce = 2;

    [SerializeField] float contestRange;

    [SerializeField] Transform jumperPos;

    Animator animator;
    CharacterMovement movement;
    CharacterManager manager;
    Collider coll;

    float jumperPower = 0f;
    ShotState shotState = ShotState.NotShooting;

    public ShotState State { 
        get { return shotState; }
    }

    public Transform JumperSetPoint { 
        get { return jumperPos; }
    }

    public float JumperSpeed
    {
        get {
            return jumperDurationSeconds;
        }
    }

    public float DunkSpeed
    {
        get
        {
            return dunkDurationSeconds;
        }
    }

    Ball ball;
    

    // Start is called before the first frame update
    void Start()
    {
        SceneObjectsReferences.ShotBar.maxValue = 1;
        ball = Ball.Instance;
        animator = GetComponent<Animator>();
        movement = GetComponent<CharacterMovement>();
        manager = GetComponent<CharacterManager>();
        coll = GetComponent<Collider>();
    }

    public void Dunk(bool input)
    {
        jumperPower += Time.deltaTime / dunkDurationSeconds;
        transform.LookAt(new Vector3(SceneObjectsReferences.RimPosition.position.x, transform.position.y, SceneObjectsReferences.RimPosition.position.z));


        SceneObjectsReferences.ShotBar.value = jumperPower;

        if (jumperPower > 1 || !input)
        {
            DunkRelease();
        }

    }

    void DunkRelease()
    {
        var dunkSound = SceneObjectsReferences.RimPosition.GetComponent<AudioSource>();
        dunkSound.Play();

        ReleaseTiming timing = SceneObjectsReferences.ReleaseWindow.GetReleaseTiming(jumperPower);

        ShotInfo.DisplayShotInfo(3, timing, SceneObjectsReferences.ReleaseWindow.Difficulty);

        if (timing == ReleaseTiming.Perfect)
        {
            SlamBall();
        }
        else if (timing == ReleaseTiming.Ok)
        {
            int randomValue = UnityEngine.Random.Range(0, 1);

            if (randomValue == 0)
            {
                SlamBall();
            } 
            else
            {
                GetRejected();
            }
        }
        else
        {
            GetRejected();
        }

        Release();

        coll.enabled = true;

        animator.SetBool("Runner", false);
        animator.SetBool("HasBall", false);
    }
    void GetRejected()
    {
        ball.transform.position = ball.transform.position + Vector3.up * 0.5f;
        ball.GetComponent<Rigidbody>().AddForce(Vector3.up * 5f);
    }
    void SlamBall()
    {
        ball.transform.position = SceneObjectsReferences.RimPosition.position + Vector3.up * 0.3f;
        ball.GetComponent<Rigidbody>().AddForce(Vector3.down * dunkForce);
    }

    ShotDistance GetShotDistance()
    {
        float distance = Vector3.Distance(transform.position, SceneObjectsReferences.RimPosition.position);
        if (distance < 3.5f)
        {
            return ShotDistance.UnderBasket;
        }
        else if (distance < 4.6f)
        {
            return ShotDistance.ShortMidrange;
        } else if (distance < 6f)
        {
            return ShotDistance.Midrange;
        }
        return ShotDistance.ThreePT;
    }

    public ShotDifficulty GetShotDifficulty()
    {
        int diff = 0;
        if (animator.GetBool("Running"))
        {
            diff += 2;
        }
        else if (animator.GetBool("Move"))
        {
            diff += 1;
        }
        diff += ContestMagnitude();
        if (IsDunkAvailable())
        {
            diff -= 1;
        }

        int maxShotDifficulty = Enum.GetNames(typeof(ShotDifficulty)).Length - 1;

        diff = diff > maxShotDifficulty ? maxShotDifficulty : diff; // clamping difficulty to prevent overflows
        diff = diff < 0 ? 0 : diff;

        return (ShotDifficulty)diff;   
    }

    void AdjustReleaseWindow()
    {
        SceneObjectsReferences.ReleaseWindow.AdjustToShot(GetShotDifficulty(), GetShotDistance());
    }

    int ContestMagnitude()
    {
        return (int)(contestRange / Ball.Instance.OppositePlayerDistance());
    }

    bool IsStandingDunkAvailable()
    {
        return GetShotDistance() == ShotDistance.UnderBasket;
    }

    bool IsRunningDunkAvailable()
    {
        return GetShotDistance() == ShotDistance.ShortMidrange && movement.MovingInDirectionOfRim() == true && animator.GetBool("Running") && !IsEnemyInTheWay();
    }

    public bool IsDunkAvailable()
    {
        return IsStandingDunkAvailable() || IsRunningDunkAvailable();
    }

    public void Shoot(bool input)
    {
        ball = Ball.Instance;

        if (ball.Handler != gameObject)
        {
            return;
        }
        if (GameController.Instance.ScoreTurnTeam != manager.Team)
        {
            // to do interface
        }

        if (shotState == ShotState.NotShooting && input)
        {
            animator.SetBool("HasBall", false);

            if (IsStandingDunkAvailable())
            {
                
                DunkBegin();
                return;
            }

            if (IsRunningDunkAvailable())
            {
                animator.SetBool("Runner", true);
                DunkBegin();
                return;
            }

            ShotBegin();
        }

        if ((jumperPower > 0f && !input) || jumperPower >= 1 && shotState == ShotState.Aim)
        {
            ShotRelease();
            return;
        }

        if (input && shotState == ShotState.Aim)
        {
            ShotAim();
        }
    }

    bool IsEnemyInTheWay()
    {
        int playerLayer = 6;
        int layerMask = 1 << playerLayer;

        return Physics.Raycast(transform.position, SceneObjectsReferences.RimPosition.position, out RaycastHit hit, 10f, layerMask) && hit.collider.gameObject != gameObject;


    }

    void DunkBegin()
    {
        shotState = ShotState.Dunking;
        Ball.Instance.AssignDunkDistance();
        movement.DunkJump();
        animator.SetTrigger("Dunk");
        SceneObjectsReferences.ShotBar.gameObject.SetActive(true);
        AdjustReleaseWindow();
    }

    void ShotRelease()
    {
        
        ReleaseTiming timing = SceneObjectsReferences.ReleaseWindow.GetReleaseTiming(jumperPower);

        ShotInfo.DisplayShotInfo(3, timing, SceneObjectsReferences.ReleaseWindow.Difficulty);
        ball.Shoot(timing, GetShotDistance());

        Release();
        
    }

    public void Release()
    {
        shotState = ShotState.Release;

        ball.Release();

        jumperPower = 0f;

        StartCoroutine(DisableShotBarAfterWait());
    }

    

    void ShotBegin()
    {
        // distance * vertical * vector3.up
        animator.SetBool("HasBall", false);
        movement.Jump();

        shotState = ShotState.Aim;

        animator.SetTrigger("Jumper");
        SceneObjectsReferences.ShotBar.gameObject.SetActive(true);

        AdjustReleaseWindow();
    }

    void ShotAim()
    {
        transform.LookAt(new Vector3(SceneObjectsReferences.RimPosition.position.x, transform.position.y, SceneObjectsReferences.RimPosition.position.z));

        jumperPower += Time.deltaTime / jumperDurationSeconds;

        SceneObjectsReferences.ShotBar.value = jumperPower;
    }

    IEnumerator DisableShotBarAfterWait()
    {
        yield return new WaitForSeconds(2);
        SceneObjectsReferences.ShotBar.gameObject.SetActive(false);

        shotState = ShotState.NotShooting;
    }

}

public enum ShotDistance
{
    UnderBasket,
    ShortMidrange,
    Midrange,
    ThreePT
}

public enum ShotDifficulty
{
    Easy,
    Normal,
    Hard
}

[Flags]
public enum ShotState
{
    NotShooting = 1, 
    Aim = 2,
    Release = 4,
    Dunking = 8
}
