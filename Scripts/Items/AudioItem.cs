using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioItem : SensorItem, IInteractable<bool>
{
    // Start is called before the first frame update
    [SerializeField]
    public Task task;


    // test
    bool yes = true;



    void Start()
    {
        // subtitleDisplayer = gameObject.GetComponent<SubtitleDisplayer>();
    }

    // Update is called once per frame
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.A)) {
        //     CheckValue(yes);
        //     yes = !yes;
        // }

        // update elapsed time in subtitle player
    }

    public void CheckValue(bool value) {
        if (!isInteractable) return ;

        if (value) {
            // task.PauseVideo();
            SensorTrigger(task);
            // sub = StartCoroutine(subtitleDisplayer.Begin());
        } else {
            // task.PlayVideo();
            SensorUntrigger(task);
        }
        task.isInteracted = false;
    }




}