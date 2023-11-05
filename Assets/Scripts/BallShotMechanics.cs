using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Shot mechanics for a ball
public class BallShotMechanics : MonoBehaviour
{
    [SerializeField] Transform rim;

    Dictionary<ReleaseTiming, float> timingInaccuracy = new Dictionary<ReleaseTiming, float>()
    {
        [ReleaseTiming.Perfect] = 0,
        [ReleaseTiming.Ok] = 0.25f,
        [ReleaseTiming.Bad] = 0.35f
    };

    [SerializeField] int longShotArc, shortShotArc = 60;

    private Ball ball;

    private void Start()
    {
        ball = GetComponent<Ball>();
    }

    Vector3 GetPerfectVelocity(ShotDistance shotDistance)
    {
        Vector3 p = rim.position;
        float gravity = Physics.gravity.magnitude;

        int shotArc = shotDistance < ShotDistance.Midrange ? shortShotArc : longShotArc;

        float angle = shotArc * Mathf.Deg2Rad;

        Vector3 planarTarget = new Vector3(p.x, 0, p.z);
        Vector3 planarPostion = new Vector3(transform.position.x, 0, transform.position.z);

        float distance = Vector3.Distance(planarTarget, planarPostion);
        float yOffset = transform.position.y - p.y;

        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));

        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

        // Rotate our velocity to match the direction between the two objects
        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion);

        if (transform.position.x > rim.position.x)
        {
            angleBetweenObjects *= -1;
        }

        Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

        return finalVelocity;
    }

    public void Shoot(ReleaseTiming timing, ShotDistance distance)
    {
        var rigid = GetComponent<Rigidbody>();

        

        Vector3 velocity = GetPerfectVelocity(distance);
        float maxInaccuracy = timingInaccuracy[timing];

        velocity.x += Random.Range(-maxInaccuracy, maxInaccuracy);
        velocity.y += Random.Range(-maxInaccuracy, maxInaccuracy);
        velocity.z += Random.Range(-maxInaccuracy, maxInaccuracy);


        StartCoroutine(PreventCollisionWithPlayer());

        if (velocity == Vector3.positiveInfinity || velocity == Vector3.negativeInfinity || velocity.magnitude == float.NaN)
        {
            Debug.Log("Invalid velocity values, not going ahead with applying");
        } else
        {
            rigid.velocity = velocity;
        }
        

        ball.Status = BallStatus.Free;

    }

    IEnumerator PreventCollisionWithPlayer()
    {
        var collider = GetComponent<Collider>();

        collider.enabled = false;

        yield return new WaitForSeconds(0.1f);

        collider.enabled = true;
    }

}
