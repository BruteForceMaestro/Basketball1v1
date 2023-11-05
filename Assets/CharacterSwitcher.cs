using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSwitcher : MonoBehaviour
{
    Transform current_child;
    [SerializeField] GameObject handler, regular;
    bool isHandler;
    bool isBallInHands;
    // Update is called once per frame

    void Start()
    {
        current_child = transform.GetChild(0);
        isHandler = false;
        isBallInHands = false;
    }
    
    
    void Update()
    {

        if (Ball.Instance.Handler != current_child.gameObject && isHandler && Ball.Instance.transform.parent != current_child.GetComponent<CharacterDribble>().BallBone && !isBallInHands)
        {
            var new_child = Instantiate(regular, current_child.position, current_child.rotation, transform);
            Destroy(current_child.gameObject);
            current_child = new_child.transform;
            isHandler = false;
        }
    }

    public void SwitchToHandler()
    {
        var newHandler = Instantiate(handler, current_child.position, current_child.rotation, transform);
        Ball.Instance.PickUp(newHandler);
        Destroy(current_child.gameObject);
        current_child = newHandler.transform;
        isHandler = true;
        isBallInHands = true;
    }

    public void BallReleased()
    {
        isBallInHands=false;
    }
}
