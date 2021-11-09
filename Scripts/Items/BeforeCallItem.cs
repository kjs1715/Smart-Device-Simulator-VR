using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeforeCallItem : OutputTriggerItem
{
    // Start is called before the first frame update

    public LogTimer logTimer;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (task.isTriggered && !logTimer.isTicking) {
            logTimer.StartTime();
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            CheckValue(true);
        }
        if (task.isTaskCompleted) {
            // triggerOutputItem -> OutputItem
            this.GetComponent<AudioSource>().Stop();
            logTimer.Checkpoint(0, "Before");
            Logger.AddLog(logTimer.Exportdic());
            SensorUntrigger(task);
            triggerItem.GetComponent<OutputItem>().SensorTrigger(triggerItem.GetComponent<Task>());
            this.gameObject.SetActive(false);

        }
    }

    public void CheckValue(bool value) {
        task.isTaskCompleted = true; 
    }

}
