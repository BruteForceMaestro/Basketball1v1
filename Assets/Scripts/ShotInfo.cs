using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShotInfo : MonoBehaviour
{
    [SerializeField] Text timingText, difficultyText;

    static ShotInfo Instance;

    Dictionary<ReleaseTiming, Color> timingColors = new Dictionary<ReleaseTiming, Color>()
    {
        [ReleaseTiming.Perfect] = Color.green,
        [ReleaseTiming.Ok] = Color.yellow,
        [ReleaseTiming.Bad] = Color.red,
    };

    Dictionary<ShotDifficulty, Color> difficultyColors = new Dictionary<ShotDifficulty, Color>()
    {
        [ShotDifficulty.Easy] = Color.green,
        [ShotDifficulty.Normal] = Color.yellow,
        [ShotDifficulty.Hard] = Color.red,
    };

    private void Start()
    {
        // only one is meant to be had in a game
        Instance = this;
        gameObject.SetActive(false);
    }

    public static void DisplayShotInfo(int seconds, ReleaseTiming timing, ShotDifficulty difficulty)
    {
        Instance.gameObject.SetActive(true);

        Instance.Display(seconds, timing, difficulty);
    }

    void Display(int seconds, ReleaseTiming timing, ShotDifficulty difficulty)
    {
        timingText.color = timingColors[timing];
        timingText.text = timing.ToString();

        difficultyText.color = difficultyColors[difficulty];
        difficultyText.text = difficulty.ToString();

        StartCoroutine(DisableAfterWait(seconds));
    }

    IEnumerator DisableAfterWait(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        gameObject.SetActive(false);
    }

}
