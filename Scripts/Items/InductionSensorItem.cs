using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InductionSensorItem : SensorItem
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.tag == "Event Objects") {
            if (other.gameObject.name == "Kettle") {
                Task t = other.gameObject.GetComponent<Task>();
                if ( !t.isTaskOver ) {
                    SensorTrigger(t);
                    t.timer.CountStart();
                }
            }
        }
    }

    private void OnCollisionExit(Collision other) {
        if (other.gameObject.tag == "Event Objects") {
            if (other.gameObject.name == "Kettle") {
                Task t = other.gameObject.GetComponent<Task>();
                if ( !t.isTaskOver ) {
                    t.timer.Pause();
                    return ;
                } 
                SensorUntrigger(t);     
            }
        }
    }
}
