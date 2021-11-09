using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class PlateSensorItem : SensorItem, IInteractable<bool>
{
    // Start is called before the first frame update

    string[] levelStr = {"Level 1\n1 Red + 1 Green", "Level 2\n2 Red + 1 Green", "Level 3\n1 Red + 2 Green + 1 White", "Level 4\n3 Red + 2 Green + 2 White", "Cooking is over!" };

    PlateItem[] plates;

    Task task;
    int level = 0;

    public TMPro.TMP_Text levelText;
    public Timer timer;

    public LogTimer logTimer;

    public UserState userState;
    public DeviceDetector deviceDetector;

    void Awake()
    {   
        // levelText.text = levelStr[0];
        task = gameObject.GetComponent<Task>();
        GameObject[] obj = GameObject.FindGameObjectsWithTag("Plates").OrderBy(v => v.transform.GetSiblingIndex()).ToArray();
        plates = new PlateItem[obj.Count<GameObject>()];
        for (int i = 0; i < plates.Count<PlateItem>(); i++) {
            plates[i] = obj[i].GetComponent<PlateItem>();
        }

    }

    // Update is called once per frame
    void Update()
    {
        ChangeLevelText();
        // if (timer.CheckTimeOver()) {
        //     SensorUntrigger(task);
        // }

        // TEST
        if (Input.GetKeyDown(KeyCode.I)) {
            SensorTrigger(task);
        }
        if (Input.GetKeyDown(KeyCode.O)) {
            CheckValue(true);
        }
        if (Input.GetKeyDown(KeyCode.P)) {
            CheckValue(false);
        }

    }

    void ChangeLevelText() {
        if (task.isTriggered) {
            logTimer.StartTime();
        }
        if (!task.isTaskCompleted && plates[level].levelComplete) {
            logTimer.Checkpoint(0, "Plate" + level + " ");
            level += 1;
            levelText.text = levelStr[level];
        }
        if (level == 4) {
            task.SetTaskComplete();
            levelText.text = levelStr[level];
            timer.CountStart();
            logTimer.Checkpoint(0, "Plate" + level + " ");
            Logger.AddLog(logTimer.Exportdic());
            Destroy(this);
        }
    }

    // private void OnCollisionEnter(Collision other) {
    //     if (other.gameObject.name == "Tablet") {
    //         SensorTrigger(task);
    //     }
    // }

    public void CheckValue(bool value) {
        if (userState.isHandUsing) return ;

        if (value) {
            if (task.isPlaying) {
                task.PauseVideo();
            } else {
                task.PlayVideo();
            }
        } else {
            task.ReplayVideo();
            ReplayVideo();
        }
    }

    // TEMPORARY
    void ReplayVideo() {
        deviceDetector.devices[task.LastDeviceNum].StopSubtitle();
        deviceDetector.devices[task.LastDeviceNum].subtitleModule.SetSubPointer(0);
        deviceDetector.devices[task.LastDeviceNum].StartSubtitle(task.videoTime);
    }

}
