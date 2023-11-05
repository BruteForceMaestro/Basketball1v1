using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUIController : MonoBehaviour
{
    [SerializeField] Text ballerz, swishers; // scores

    public void UpdateScore(Team team, int score)
    {
        if (team == Team.Ballerz)
        {
            ballerz.text = score.ToString();
            return;
        }
        swishers.text = score.ToString();
    }
}
