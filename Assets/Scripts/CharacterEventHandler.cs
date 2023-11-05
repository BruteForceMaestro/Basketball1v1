using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this component handles events, subscribes and unsubscribes.
public class CharacterEventHandler : MonoBehaviour
{
    BotInputController controller;
    CharacterMovement movement;
    CharacterSwitcher switcher;

    void Start()
    {
        if (TryGetComponent<BotInputController>(out var botInputController))
        {
            controller = botInputController;
        }
        movement = GetComponent<CharacterMovement>();
        switcher = GetComponentInParent<CharacterSwitcher>();
        Ball.Instance.ScoredEvent += OnScored;
        Ball.Instance.PickedUpEvent += OnBallPickedUp;
        Ball.Instance.ReleasedEvent += OnBallReleased;
        Ball.Instance.MissedEvent += OnMissed;
    }
    // i do geniunely despise this event fuckery
    void OnBallPickedUp(GameObject handler)
    {
        handler.GetComponent<CharacterDribble>().DribbleEvent += OnHandlerDribbled;
    }

    void OnBallReleased(GameObject handler)
    {
        handler.GetComponent<CharacterDribble>().DribbleEvent -= OnHandlerDribbled;
        if (gameObject == handler)
        {
            switcher.BallReleased();
        }
    }

    void OnDestroy()
    {
        UnsubscribeFromAllEvents();
    }

    void OnMissed(Team team)
    {
        if (controller != null)
        {
            controller.Missed(team);
        }
    }

    public void UnsubscribeFromAllEvents()
    {
        Ball.Instance.ScoredEvent -= OnScored;
        Ball.Instance.PickedUpEvent -= OnBallPickedUp;
        if (Ball.Instance.Handler != null)
        {
            Ball.Instance.Handler.GetComponent<CharacterDribble>().DribbleEvent -= OnHandlerDribbled;
        }
        Ball.Instance.ReleasedEvent -= OnBallReleased;
    }

    void OnScored(ShotDistance distance, Team team)
    {
        if (controller != null)
        {
            controller.OnScored(distance, team);
        }
        
    }

    void OnHandlerDribbled(DribbleMove move)
    {
        if (controller != null)
        {
            controller.OnHandlerDribbled(move);
        }

        movement.OnDribbleMove();
    }
}
