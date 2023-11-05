using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneObjectsReferences : MonoBehaviour
{
    [SerializeField] Transform rimPos, topOfKey;
    [SerializeField] Slider shotBar;
    [SerializeField] ReleaseWindow window;

    public static Transform RimPosition;
    public static Transform TopOfKey;
    public static Slider ShotBar;
    public static ReleaseWindow ReleaseWindow;
    // Start is called before the first frame update
    void Awake()
    {
        RimPosition = rimPos;
        TopOfKey = topOfKey;
        ShotBar = shotBar;
        ReleaseWindow = window;
    }
}
