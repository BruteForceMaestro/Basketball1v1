using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] Team beginsTeam;
    [SerializeField]
    private GameObject[] players;
    [SerializeField] Transform positionHandler, positionDefender;
    [SerializeField] ScoreUIController scoreUIController;
    [SerializeField] Text shotClockUI;
    [SerializeField] int shotClock, endConditionPoints;
    [SerializeField] GameObject endParent;

    Team scoreTurn; // the team who gets awarded for buckets.
    Team justScored; // the team who just scored. not allowed to pick up the ball.

    int shotClockTimeInSeconds;

    Dictionary<Team, int> score = new Dictionary<Team, int>()
    {
        [Team.Ballerz] = 0,
        [Team.Swishers] = 0,
    };

    public static GameController Instance { get; private set; }

    public GameObject[] Players
    {
        get { return players; }
    }

    public Team JustScoredTeam
    {
        get { return justScored; }
    }

    public Team ScoreTurnTeam
    {
        get { return scoreTurn; }
    }
    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
    }

    Team GetOpponents(Team team)
    {
        return team == Team.Ballerz ? Team.Swishers : Team.Ballerz;
    }

    void Start()
    {
        StartCoroutine(ShotClock(shotClock));
        PlaceInStartingPositions(beginsTeam);
        Ball.Instance.ScoredEvent += OnBallScored;
    }

    void PlaceInStartingPositions(Team handler)
    {
        resetShotClock = true;
        foreach (var player in players)
        {
            if (player.GetComponentInChildren<CharacterManager>().Team == handler)
            {
                player.transform.GetChild(0).position = positionHandler.position;
                continue;
            }
            player.transform.GetChild(0).position = positionDefender.position;
        }
        scoreTurn = handler;
        if (Ball.Instance.Handler != null)
        {
            Ball.Instance.SetLoose();
        }
        Ball.Instance.transform.position = positionHandler.position;
    }

    public void ScoreTurnReset(Team team)
    {
        scoreTurn = team;
        justScored = Team.None;
        resetShotClock = true;
    }

    void OnBallScored(ShotDistance distance, Team team)
    {
        justScored = scoreTurn;
        if (distance == ShotDistance.ThreePT)
        {
            score[scoreTurn] += 2;
        } else
        {
            score[scoreTurn] += 1;
        }

        scoreUIController.UpdateScore(scoreTurn, score[scoreTurn]);
        stopShotClock = false;
        resetShotClock = true;

        if (score[scoreTurn] > endConditionPoints)
        {
            EndGame(scoreTurn);
        }
    }

    Color GetTeamColor(Team team)
    {
        if (team == Team.Ballerz)
        {
            return Color.blue;
        }
        else
        {
            return Color.red;
        }
    }

    void EndGame(Team winners)
    {
        Time.timeScale = 0;
        endParent.SetActive(true);
        Text endText = endParent.transform.GetChild(0).GetChild(0).GetComponent<Text>();
        endText.text = winners.ToString() + " WIN!";
        endText.color = GetTeamColor(winners);
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OutOfBounds()
    {
        PlaceInStartingPositions(GetOpponents(scoreTurn));
    }

    public void StopShotClock()
    {
        stopShotClock = true;
    }

    bool resetShotClock;
    bool stopShotClock;

    IEnumerator ShotClock(int initialSeconds)
    {
        while (true)
        {
            shotClockTimeInSeconds = initialSeconds;
            shotClockUI.text = initialSeconds.ToString();
            while (shotClockTimeInSeconds > 0)
            {
                if (resetShotClock)
                {
                    shotClockTimeInSeconds = initialSeconds + 1;
                    resetShotClock = false;
                }
                if (stopShotClock)
                {
                    if (Ball.Instance.Status == BallStatus.Free)
                    {
                        stopShotClock = false;
                    }
                    yield return new WaitForEndOfFrame();
                }
                yield return new WaitForSeconds(1f);
                shotClockTimeInSeconds--;
                shotClockUI.text = shotClockTimeInSeconds.ToString();
            }

            PlaceInStartingPositions(GetOpponents(scoreTurn));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
