using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutputTriggerItem : OutputItem
{
    // Start is called before the first frame update
    public GameObject triggerItem;

    private void Update() {
        Debug.Log(task.isTaskCompleted);
        if (task.isTaskCompleted) {
            // triggerOutputItem -> OutputItem
            Debug.Log("h3");

            // TEST manual trigger for SOCKET (because used calling task, sensor is not triggered)

            SensorUntrigger(task);
            triggerItem.GetComponent<OutputItem>().CheckValue(true);
            this.gameObject.SetActive(false);
            Debug.Log("h4");

        }

    }
}
