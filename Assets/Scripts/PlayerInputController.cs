using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    CharacterMovement movement;
    CharacterShooting shooting;
    CharacterDribble dribble;

    // Start is called before the first frame update
    void Start()
    {
        movement = GetComponent<CharacterMovement>();
        shooting = GetComponent<CharacterShooting>();
        dribble = GetComponent<CharacterDribble>();
    }

    // Update is called once per frame
    void Update()
    {
        MovingHandler();
        ShootingHandler();
        DribbleHandler();
        
    }

    void DribbleHandler()
    {
        DribbleMove dribbleMove = DribbleMove.None;
        if (Input.GetKey(KeyCode.I))
        {
            dribbleMove = DribbleMove.Crossover;
        }
        else if (Input.GetKey(KeyCode.K))
        {
            dribbleMove = DribbleMove.Pullback;
        }
        else if (Input.GetKey(KeyCode.L))
        {
            if (dribble.BallInLeftHand)
            {
                dribbleMove = DribbleMove.Crossover;
            } else
            {
                dribbleMove = DribbleMove.InNOut;
            }
        }
        else if (Input.GetKey(KeyCode.J))
        {
            if (dribble.BallInLeftHand)
            {
                dribbleMove = DribbleMove.InNOut;
            } else
            {
                dribbleMove = DribbleMove.Crossover;
            }
        }
        dribble.BallDribble(dribbleMove);
    }

    void MovingHandler()
    {
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        bool isBlocking = Input.GetKey(KeyCode.Space);
        bool isStealing = Input.GetKey(KeyCode.LeftAlt);
        var direction = new Vector3(-Input.GetAxisRaw("Vertical"), 0, Input.GetAxisRaw("Horizontal"));

        movement.Move(direction, isRunning);
        movement.Defense(isBlocking, isStealing);
    }

    void ShootingHandler()
    {
        bool input = Input.GetKey(KeyCode.Space);

        if (shooting.State == ShotState.Dunking)
        {
            shooting.Dunk(input);
            return;
        }
        shooting.Shoot(input);
    }
}
