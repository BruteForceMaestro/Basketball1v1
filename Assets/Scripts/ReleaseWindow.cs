using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReleaseWindow : MonoBehaviour
{
    RectTransform rectTransform;
    ShotDifficulty difficulty = ShotDifficulty.Easy;
    Slider slider;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        slider = GetComponentInParent<Slider>();

        slider.gameObject.SetActive(false);
    }

    public ShotDifficulty Difficulty
    {
        get { return difficulty; }
    }

    Dictionary<ShotDifficulty, float> widthScale = new Dictionary<ShotDifficulty, float>()
    {
        [ShotDifficulty.Easy] = 1f,
        [ShotDifficulty.Normal] = 0.7f,
        [ShotDifficulty.Hard] = 0.2f,
    };

    Dictionary<ShotDistance, float> distanceScale = new Dictionary<ShotDistance, float>()
    {
        [ShotDistance.UnderBasket] = 2f,
        [ShotDistance.ShortMidrange] = 2f,
        [ShotDistance.Midrange] = 1.5f,
        [ShotDistance.ThreePT] = 1f
    };

    float GetNormalizedWidth()
    {
        var width = rectTransform.sizeDelta.x * rectTransform.localScale.x;
        var slider_width = slider.GetComponent<RectTransform>().sizeDelta.x;
        var normalized_width = width / slider_width / 2;

        return normalized_width;
    }

    public ReleaseTiming GetReleaseTiming(float power)
    {
        var target = slider.maxValue * 0.75f;


        float inaccuracy = Mathf.Abs(power - target);

        float normalized_width = GetNormalizedWidth();

        // Debug.Log($"target: {target}, inaccuracy: {inaccuracy}, power: {power}, width: {normalized_width}");

        if (inaccuracy < normalized_width / 5)
        {
            
            // perfect release
            return ReleaseTiming.Perfect;
        } 
        else if (inaccuracy < normalized_width)
        {
            return ReleaseTiming.Ok;
        }
        return ReleaseTiming.Bad;
    }

    public void AdjustToShot(ShotDifficulty difficulty, ShotDistance distance)
    {
        this.difficulty = difficulty;
        rectTransform.localScale = new Vector3(widthScale[difficulty] * distanceScale[distance], 1, 1);
    }
}

public enum ReleaseTiming
{
    Bad,
    Ok,
    Perfect
}
