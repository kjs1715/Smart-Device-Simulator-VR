using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodItem : MonoBehaviour
{
    // Start is called before the first frame update
    public Timer timer;

    void Start()
    {
        timer = gameObject.GetComponent<Timer>();
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(gameObject.name + " " + timer.totalTime);
        if (timer.totalTime <= 6.0f) {
            gameObject.GetComponent<Renderer>().material.color = Color.green;
        }
        if (timer.totalTime <= 0.0f) {
            gameObject.GetComponent<Renderer>().material.color = Color.red;
            
        }
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.name == "Pan") {
            timer.CountStart();
        }
    }

    private void OnCollisionExit(Collision other) {
        if (other.gameObject.name == "Pan") {
            timer.Pause();
        }
    }
}
