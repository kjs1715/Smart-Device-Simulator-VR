using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Microsoft.MixedReality.Toolkit;

public class MathItem : SensorItem
{
    // Start is called before the first frame update

    public Task task;

    public TMPro.TMP_Text text;
    public LogTimer logTimer;
    public UserState userState;
    // public DeviceDetector DeviceDetector

    public bool isPassed = false;

    bool isStarted = false;
    bool objActive = false;

    int objCount = -1;
    int Qcount = 0;
    const int MAX_Qcount = 10;
    string[] questions = {"5 + 17", "53 - 7", "7 * 9", "12 * 4", "5 * 14", "4 * 11 + 5", "(3 + 5) * 7", "13 * 3 * 2", "36 / 3 - 4 * 2", "3 * 11 - 1 * 3", "44 - 3 * 14", "23 - (13 - 7)", "3 + 12 * 4 - 5"};

    int[] route = {0, 1, 2, 3, 4, 2, 0, 4, 3, 1};

    GameObject[] touchObjects;

    void Start()
    {
        touchObjects = GameObject.FindGameObjectsWithTag("Math").OrderBy(i => i.transform.GetSiblingIndex()).ToArray();
        foreach (GameObject obj in touchObjects) {
            // obj.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {

        // if (task.isTriggered) {
        //     // first ball
        //     if (objCount == -1) {
        //         objCount += 1;
        //         touchObjects[route[objCount]].SetActive(true);
        //         isPassed = true;

        //         return ;
        //     }

        //     // task end
        //     if (task.isTaskCompleted) {
        //         // touchObjects[route[objCount]].SetActive(false);
        //         task.isTaskCompleted = true;
        //         // Logger export time
        //         Logger.AddLog(logTimer.Exportdic());
        //     } else {

        //         // user and ball distance
        //         // Debug.Log(isPassed);
        //         // TODO: need to sort without order
        //         if ((CoreServices.InputSystem.GazeProvider.GazeOrigin - touchObjects[route[objCount]].transform.position).magnitude < 0.5f) {   
        //             if (objCount == 0) {
        //                 logTimer.StartTime();
        //             }
        //             touchObjects[route[objCount]].SetActive(false);
        //             objCount += 1;
        //             if (objCount >= route.Length) {
        //                 // first complete condition
        //                 task.isTaskCompleted = true;
        //                 return ;
        //             }
        //             touchObjects[route[objCount]].SetActive(true);
        //             logTimer.Checkpoint(0);
        //         }   
                
        //         // Change question
        //         if (isPassed) {
        //             // if (Qcount >= questions.Length) {
        //             //     // test
        //             //     Logger.AddLog(logTimer.Exportdic());

        //             //     return ;
        //             // }
        //             logTimer.Checkpoint(1, userState.deviceDetector.devices[userState.lastDeviceGazedNum].name + " t");
        //             text.text = questions[Qcount];
        //             Qcount += 1;
        //             isPassed = false;
        //         }
        //     }

        // }


    }
}
