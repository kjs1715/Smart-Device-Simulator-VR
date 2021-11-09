using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    // Start is called before the first frame update


    // default total time
    [SerializeField]
    public float totalTime = 0.0F;
    public float initTime = 0.0F;

    public bool isTicking = false;

    // public bool CountDownMode = true; // TODO

    public Timer(float time) {
        initTime = totalTime = time;
    }
    
    void Start()
    {
        initTime = totalTime;

    }

    // Update is called once per frame
    void Update()
    {
        if (isTicking) {
            // TODO
            // if (CountDownMode) {
            //     totalTime -= Time.deltaTime;
            //     return ;
            // }
            totalTime -= Time.deltaTime;
            // Debug.Log(gameObject.name + " time ticking :" + totalTime);
        }
    }

    public void CountStart() {
        isTicking = true;
    }

    public void Pause() {
        isTicking = false;
    }

    public void ResetTimer() {
        totalTime = initTime;
    }

    public bool CheckTimeOver() {
        if (totalTime <= 0.0F) {
            isTicking = false;
            return true;
        }
        return false;
    }
}


// 1. countdown totaltime 
