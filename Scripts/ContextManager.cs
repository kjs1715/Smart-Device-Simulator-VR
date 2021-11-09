using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Linq;
using System;



public class ContextManager : MonoBehaviour
{
    // variables
    public UserState userState;
    public TaskManager taskManager;
    public DeviceDetector deviceDetector;

    public AdaptationModule adaptationModule;


    // Context variables
    public Dictionary<int, float> user2deviceDistance;
    public Dictionary<int, float> user2deviceAngle;
    Dictionary<int, float> user2deviceAngleDown;

    Dictionary<int, float> angleDot;


    // 1:35
    const float _dis = 3.0f;
    // const float camera_dis = 20.0f;
    const float camera_dis = 24.0f;

    // renderScene variables
    GameObject textCamera;
    float m_dis;

    // based on distance
    float dis_threshold {
        //TODO: not natural when user get close to the device
        set {
            if (value < _dis && value > 0) {
                float temp = -((_dis - value) * camera_dis);
                // Debug.Log(temp);
                // textCamera.transform.position = new Vector3(textCamera.transform.position.x, textCamera.transform.position.y, -99.68f + temp);
            }
        }
    }

    

    int targetDeviceNum;
    string targetDeviceName;
    int currentDeviceNum;
    string currentDeviceName;



    // User direction
    int targetDirectionNum;



    // test variables

    public int testFactor;
    public Task currentTask;

    public TMPro.TMP_Text dis_text;
    public TMPro.TMP_Text pri_text;
    public TMPro.TMP_Text ang_down_text;
    public TMPro.TMP_Text dot_text;
    public TMPro.TMP_Text gaze_text;

    public TMPro.TMP_Text text3;
    public TMPro.TMP_Text text4;

    public ExperimenPlatform platform;
 

    // for experiment
    [HideInInspector] public bool writeData = false;
    [HideInInspector] public int score = 0;
    [HideInInspector] public Dictionary<string, string> datas;
    [HideInInspector] public bool emptyDatas = false;
    [HideInInspector] public bool experimentMode = false;          // control whole logic 1. experiment mode 2. normal mode

    public int dataCount = -1;
    [HideInInspector] public int Count = 0;

    bool first = true;

    string[] ContentText = {"Video", "Audio", "Sub"};
    string[] MethodText = {"Normal videoSub", "Only sub", "Slow Down Sub", "Normal audio", "Sub + sound", "Adjust Volume", "Center Sub", "Key Sub", "Adjust Sub Size"};
    public string[] ChannelText = {"Visual easy", "Visual hard", "Audio easy", "Audio hard"};


    // variables for experiment

    bool[] is_content_method;


    Task exp_task = null;

    // order

    int order1 = 0;
    int order2 = 0;
    int order3 = 0;
    int order4 = 0;
    int order5 = 0;


    bool contentChanged = true;
    bool subAdjusted = false;
    bool subClosed = false;

    bool task1 = false;




    private void Start() {
        testFactor = -1;

        user2deviceDistance = new Dictionary<int, float>();
        user2deviceAngle = new Dictionary<int, float>();
        user2deviceAngleDown = new Dictionary<int, float>();
        angleDot = new Dictionary<int, float>();

        for (int i = 0; i < deviceDetector.getDeviceCount(); i++) {
            user2deviceAngle.Add(i, 0.0f);
            user2deviceAngleDown.Add(i, 0.0f);
            user2deviceDistance.Add(i, 0.0f);
            angleDot.Add(i, 0.0f);
        }

        // init target device and save initial devices dis
        this.getDistance();
        // targetDeviceNum = user2deviceDistance.Keys[0];

        // float dis = this.getNearestUser2deviceDistance(out targetDeviceNum, out targetDeviceName);
        // currentDeviceNum = targetDeviceNum;
        // currentDeviceName = targetDeviceName;
        // deviceDetector.setTexture(ResourcesManager.textRenderTexture, currentDeviceNum);

        // save textCamera initial position
        // textCamera = deviceDetector._cameras[0];
        datas = new Dictionary<string, string>();

        // for experiment
        is_content_method = new bool[9];
        for (int i = 0; i < 9; i++) {
            is_content_method[i] = false;
        }

    }

    private void Update() {
        DetectContext();
        // UpdateContext();

        text3.text = "";
        for (int i = 0; i < deviceDetector.getDeviceCount(); i++) {
            text3.text += deviceDetector.devices[i].deviceName + " :" + deviceDetector.devices[i].Visibility + "\tsize: " + deviceDetector.devices[i].device_actual_size + "\n";
        }

        // for record data
        if (writeData) {
            WriteDataLog();
            writeData = false;
            score = 0;
        }
        if (emptyDatas) {
            datas.Clear();
            emptyDatas = false;
        }



        // control logic for experiment
        if (contentChanged && experimentMode) {
            Debug.Log("Why not here");
            WriteDataLog();
            ControlMethods(order1, order2, order3, order4, order5);
            contentChanged = false;
        }
        if (!experimentMode) {
            DetectTask();
        }
        
        text4.text = "Info: " + order1 + "\nOutput: " + ContentText[order2] + "\nMethod: " + MethodText[order2*3 + order3] + "\nChannel limit: " + ChannelText[order4]+ "\nDevice :" + deviceDetector.devices[order5].deviceName + "\ndataCount: " + dataCount;

        // Debug.Log(currentTask);
        
    }

    public void WriteDataLog() {
        Debug.Log("remain " + datas.Count);
        Debug.Log("???? :" + dataCount);
        string temp = "";
        // info
        for (int i = 0; i < 2; i++) {
            int t = order1 * 288 + order2 * 96 + order3 * 32 + order4 * 8 + order5;
            // call task here
            if (i == 0 && !task1) {
                Task task = taskManager.TriggerTask(i);
                if (!taskManager.taskList.Contains(task)) {
                    taskManager.taskList.Add(task);
                    taskManager.AttachTaskOnDevices(task);

                    exp_task = task;
                }

            }
            if (i == 1) {
                if (t == 288 && !task1) {
                    taskManager.TriggerTask(i-1);
                    task1 = true;
                }
                Task task = taskManager.TriggerTask(i);
                if (!taskManager.taskList.Contains(task)) {
                    taskManager.taskList.Add(task);
                    taskManager.AttachTaskOnDevices(task);

                    exp_task = task;
                }
            }
            // initailize
            if (dataCount == -1) {
                // TODO: here can modify datacount
                dataCount += 1;
                // Debug.Log("wtf");
                return ;
            }

            // output 
            for (int j = 0; j < 3; j++) {
                // method
                for (int k = 0; k < 3; k++) {
                    // channel
                    for (int n = 0; n < 4; n++) {
                        // devices
                        for (int m = 0; m < 8; m++) {
                            int index = i * 288 + j * 96 + k * 32 + n * 8 + m;
                            if (index == dataCount + 1) {
                                order1 = i;
                                order2 = j;
                                order3 = k;
                                order4 = n;
                                order5 = m;
                                ControlMethods(order1, order2, order3, order4, order5);
                                dataCount += 1;                          

                                return;
                            }
                            if (index == dataCount) {
                                string trial = score + "," + i + "," + j  + "," + k + "," + n + ","+ m;
                                Debug.Log(trial);
                                Debug.Log("dataCount :" + dataCount);


                                float size = deviceDetector.devices[m].device_actual_size;
                                int avail = deviceDetector.devices[m].isAvailable == true ? 1 : 0;
                                float vis = deviceDetector.devices[m].Visibility;
                                float dis = user2deviceDistance[m];
                                float angle = user2deviceAngle[m];

                                temp += size + "," + avail + "," + vis + "," + dis + "," + angle;
                                datas[dataCount.ToString() + Count.ToString()] = trial + " " + temp;

                                
                                continue;
                            }

                

                        }
                    }
                }
            }
        }

    }

    public void ControlMethods(int info, int content, int method, int channel, int device) {

        // Secondary task control
        platform.StartSecondaryTaskForExp(channel);

        int content_method = content * 3 + method;
        Debug.Log("Content Method " + content_method);


        // reset content method first
        ResetMethods();

        // then remove devices
        taskManager.RemoveAllTasksOnDevices(exp_task);

        // set content
        exp_task.videoNum = content == 0 ? device : -1;
        exp_task.audioNum = content == 1 ? device : -1;
        exp_task.subNum = content == 2 ? device : -1;

        if (exp_task.videoNum != -1) {
            exp_task.audioNum = exp_task.videoNum;
            exp_task.subNum = exp_task.videoNum;
        } else if (exp_task.audioNum != -1) {
            exp_task.videoNum = exp_task.audioNum;
            exp_task.subNum = exp_task.audioNum;
        } else if (exp_task.subNum != -1) {
            exp_task.audioNum = exp_task.subNum;
            exp_task.videoNum = exp_task.subNum;
        }


    
        // Debug.Log("current task :" + currentTask.videoNum + " " + currentTask.audioNum + " " + currentTask.subNum);

        taskManager.AttachTaskOnDevices(exp_task, true);


        is_content_method[content_method] = true;
        switch (content_method) {
            case 0:
                exp_task.Mute();
                break;
            case 1:
                deviceDetector.devices[exp_task.videoNum].CloseVideo(true);
                exp_task.Mute();
                // exp_task.deviceDetector.devices[exp_task.videoNum].closeSub(true);

                break;
            case 2:
                exp_task.VideoPlaySpeed(true);
                exp_task.Mute();
                Debug.Log("speed changed");
                break;
            case 3:
                exp_task.deviceDetector.devices[exp_task.videoNum].closeSub(true);
                exp_task.deviceDetector.devices[exp_task.videoNum].CloseVideo(true);
                break;
            case 4:
                exp_task.deviceDetector.devices[exp_task.videoNum].CloseVideo(true);
                exp_task.deviceDetector.devices[exp_task.subNum].AdjustPostion(true);
                break;
            case 5:
                exp_task.AdjustSound(true);
                exp_task.deviceDetector.devices[exp_task.videoNum].closeSub(true);
                exp_task.deviceDetector.devices[exp_task.videoNum].CloseVideo(true);
                break;
            case 6:
                exp_task.deviceDetector.devices[exp_task.subNum].AdjustPostion(true);
                exp_task.deviceDetector.devices[exp_task.videoNum].CloseVideo(true);
                exp_task.Mute();
                break;

            case 7:
                exp_task.deviceDetector.devices[exp_task.subNum].changeSub(true);
                exp_task.deviceDetector.devices[exp_task.videoNum].CloseVideo(true);
                exp_task.Mute();
                break;

            case 8:
                AdjustText(exp_task.subNum, true);
                exp_task.deviceDetector.devices[exp_task.videoNum].CloseVideo(true);
                exp_task.Mute();
                break;

        }

        
    }

    public void ResetMethods() {
        int index = -1;
        for (int i = 0; i < 9; i++) {
            if (is_content_method[i]) {
                index = i;
                is_content_method[i] = false;
                Debug.Log("Reset " + i);
                break;
            }
        }
        switch(index) {
            case 0:
                exp_task.Unmute();
                break;
            case 1:
                deviceDetector.devices[exp_task.videoNum].CloseVideo(false);
                exp_task.Unmute();
                break;
            case 2:
                exp_task.VideoPlaySpeed(false);
                Debug.Log("Back to normal speed");
                exp_task.Unmute();
                break;
            case 3:
                exp_task.deviceDetector.devices[exp_task.videoNum].closeSub(false);
                exp_task.deviceDetector.devices[exp_task.videoNum].CloseVideo(false);
                break;
            case 4:
                exp_task.deviceDetector.devices[exp_task.videoNum].CloseVideo(false);
                exp_task.deviceDetector.devices[exp_task.subNum].AdjustPostion(false);
                break;
            case 5:
                exp_task.AdjustSound(false);
                exp_task.deviceDetector.devices[exp_task.videoNum].closeSub(false);
                exp_task.deviceDetector.devices[exp_task.videoNum].CloseVideo(false);
                break;
            case 6:
                exp_task.deviceDetector.devices[exp_task.subNum].AdjustPostion(false);
                exp_task.deviceDetector.devices[exp_task.videoNum].CloseVideo(false);
                exp_task.Unmute();
                break;

            case 7:
                exp_task.deviceDetector.devices[exp_task.subNum].changeSub(false);
                exp_task.deviceDetector.devices[exp_task.videoNum].CloseVideo(false);
                exp_task.Unmute();
                break;

            case 8:
                AdjustText(exp_task.subNum, false);
                exp_task.deviceDetector.devices[exp_task.videoNum].CloseVideo(false);
                exp_task.Unmute();
                break;

            }

    }


    // detect new task
    public void DetectTask() {
        if (taskManager.CheckNewTask()) {
            Task task = taskManager.newTaskQueue.Dequeue();
            if (task.isTaskCompleted)
                return ;
            Debug.Log("Attaching...");
            taskManager.taskList.Add(task);
            taskManager.AttachTaskOnDevices(task);

            currentTask = task;

        }
    }

    public void demo() {
        
    }


    /*
        Update all kinds of context variables
    */
    public void DetectContext() {

            
        // Dectect user context
        DetectUserContext();

        // Detect device context
        DetectDeviceContext();


        // distance and angle update (context variables)
        this.getDistance();
        this.getUser2deviceAngles(); 

        // TEST variables
        dis_text.text = "Distance\n";
        pri_text.text = "Angle\n";
        ang_down_text.text = "Availibility";
        dot_text.text = "Visiblility\n";
        gaze_text.text = "Device size\n";

        for (int i = 0; i < deviceDetector.getDeviceCount(); i++) {
            dis_text.text += deviceDetector.devices[i].deviceName + ": " + user2deviceDistance[i] + " " + " " + Mathf.Pow(user2deviceDistance[i], 2) / deviceDetector.devices[i].device_actual_size / 10+ '\n';
            ang_down_text.text += deviceDetector.devices[i].deviceName + ": " + deviceDetector.devices[i].isAvailable + '\n';
            dot_text.text += deviceDetector.devices[i].deviceName + ": " + deviceDetector.devices[i].isVisible + '\n';
            pri_text.text += deviceDetector.devices[i].deviceName + ": " + user2deviceAngle[i] + '\n';
            // gaze_text.text += deviceDetector.devices[i].deviceSize
        }
        // if (targetDeviceNum == -1) {
        //     gaze_text.text += "Current gazing :" + "None"+ '\n';
        //     gaze_text.text += "last gazing :" + "None" + '\n';
        // } else {
        //     gaze_text.text += "Current gazing :" + deviceDetector.devices[targetDeviceNum] + '\n';
        //     gaze_text.text += "last gazing :" + deviceDetector.devices[userState.lastDeviceGazedNum] + '\n';
        // }



        // TODO: detect waiting task
        // if ()

        // detect task
        if (taskManager.CheckNewTask()) {
            // AttachTask(taskManager.newTaskQueue.Dequeue());
            // check queue is empty
            foreach (Task task in taskManager.newTaskQueue) {
                // Debug.Log(task);
            }
        }

        // for whole scheduling
        // Schedule();
        
        // user direction 
        // using GazeProvider
        // Debug.Log(userState.getUserPosition());
        // Debug.Log(userState.getUserDirection());



        // all distance output (use to test)
        // 
        // float[] dis1 = this.getUser2deviceDistance(out device_dis);

        // for (int i = 0; i < deviceDetector.getDeviceCount(); i++) {
        //     Debug.Log(device_dis[i]);
        //     Debug.Log(dis1[i]);
        // }


        // text size change with distance
        // if (currentDeviceNum != -1) {
        //     dis_threshold = user2deviceDistance[currentDeviceNum];
        // }

        // detect user movement
        // Debug.Log("isMoving" + userState.isMoving);

    
        // Debug.Log(userState.getUserDirection());
        // Debug.Log("Angle2 : " + Vector3.Angle(userState.getUserDirection(), testVector2));


    }

    public void DetectUserContext() {
        // TEST
        // if (testFactor == 0 || testFactor == 2) {
        //     userState.getGazingObject(out targetDeviceNum, out targetDeviceName);
        //     // Debug.Log(targetDeviceNum);

        // } 
        // if (testFactor == 1) {
        //     this.getUser2deviceAngles();
        // }


        // this.getUser2deviceAngles();



        // Eye gaze part (done)
        
        userState.getGazingObject(out targetDeviceNum, out targetDeviceName);
    


        
        // Min distance part (done)

        // first time need to set current device
        // 1. getDistances
        // 2. min(Dis)
        // 3. then select Device to schedule
        // this.getDistance();
        // float dis = this.getNearestUser2deviceDistance(out targetDeviceNum, out targetDeviceName);
        // Debug.Log("target :" + targetDeviceNum);
        // Debug.Log("current :" + currentDeviceNum);
        // if (targetDeviceNum != currentDeviceNum) {
            // need to change target first and turn current black 

            // deviceDetector.setTexture(ResourcesManager.textRenderTexture, targetDeviceNum);
            // deviceDetector.setTexture(ResourcesManager.emptyScreenTexture, currentDeviceNum);
            // currentDeviceNum = targetDeviceNum;
            // currentDeviceName = targetDeviceName;
        // }

    }

    public void DetectDeviceContext() {
        
    }

    

    public void UpdateContext() {
        var enumerator = user2deviceDistance.GetEnumerator();
        int[] target = new int[2];
        for (int i = 0; i < 2; i++) {
            enumerator.MoveNext();
            target[i] = enumerator.Current.Key;
        }
        switch (testFactor) {
            case 0:
            //gaze
                if (currentTask.LastDeviceNum != target[0]) {
                    if (target[0] == targetDeviceNum) {
                        deviceDetector.ChangeTexture(currentTask.LastDeviceNum, target[0]);
                    }
                    if (target[1] == targetDeviceNum) {
                        deviceDetector.ChangeTexture(currentTask.LastDeviceNum, target[1]);
                    }
                }
                break;
            case 1:
            //angle

                // Debug.Log(target[0] + " " + user2deviceAngle[target[0]]);
                // Debug.Log(target[1] + " " + user2deviceAngle[target[1]]);
                if (user2deviceAngle[target[0]] >= user2deviceAngle[target[1]]) {
                    deviceDetector.ChangeTexture(currentTask.LastDeviceNum, target[0]);
                } else {
                    deviceDetector.ChangeTexture(currentTask.LastDeviceNum, target[1]);
                }                
                break;
            case 2:
                if (userState.lastDeviceGazedNum != userState.curDeviceGazedNum) {
                    Debug.Log(userState.lastDeviceGazedNum);
                    Debug.Log(userState.curDeviceGazedNum);
                    StartCoroutine(deviceDetector.ChangeTextureForSeconds(userState.lastDeviceGazedNum, userState.curDeviceGazedNum));
                    // userState.lastDeviceGazedNum = userState.curDeviceGazedNum;

                }
                break;
            default:
                break;
        }

        // after task completed, cancel context detection
        if (currentTask != null && currentTask.isTaskCompleted) {
            testFactor = -1;
        }

    }

    public void Schedule() { // TODO: need to check waitlist
        // first ver.
        // 1. calculate angle between running device and user (all?) DONE
        // 2. if task angle < 150
        //  2.1 find 2 nearest device (in main function) DONE
        //  2.2 check availability
        //      2.2.1 if not empty (two device)
        //      2.2.2 waitList
        //      2.2.3 break
        //  2.3 compare angle

  
        foreach (Task task in taskManager.taskList) {
            if (user2deviceAngle[task.GetDevice()] < 145) {
                Debug.Log("Scheduling task " + task.name + "...");
                // int type = deviceDetector.Nearest2DeviceAvailable();
                bool Changed = false;
                int count = 0;
                var enumerator = user2deviceDistance.GetEnumerator();
                while (enumerator.MoveNext() && count < 2) {
                    int deviceNum = enumerator.Current.Key;
                    if (task.GetDevice() == deviceNum) 
                        continue ;
                    if (deviceDetector.devices[deviceNum].isAvailable) {
                        Debug.Log("Device " + deviceDetector._devices[deviceNum].name + " available!");
                        if (user2deviceAngle[deviceNum] >= 150) {
                            deviceDetector.ChangeTexture(task.GetDevice(), deviceNum);
                            task.SetDevice(deviceNum);
                            Changed = true;
                            break ;
                        }
                    }
                    count++;
                }
                if (!Changed) {
                    taskManager.waitList.Add(task);
                }

            }
        }
    }

    public void Reschedule(Solution solution) {
        // sort solution
        solution.list = solution.list.OrderBy(x => x._device).ToList();
        solution.list = solution.list.OrderBy(x => x._content).ToList();

        // remove task from devices
        taskManager.RemoveAllTasksOnDevices(currentTask);
        
        // remove devices in task
        currentTask.deviceList.Clear();
        currentTask.deviceDict.Clear();
        foreach (SolutionModel model in solution.list) {
            // add devices in task
            currentTask.deviceList.Add(model);
            // Debug.Log(model.ToString());
        }

        // Debug.Log("current task :" + currentTask.videoNum + " " + currentTask.audioNum + " " + currentTask.subNum);

        // add task on devices
        taskManager.AttachTaskOnDevices(currentTask);
    }

    // for new task allocation, then schedule all devices
    public void AttachTask(Task task) {

        //TEST
        // for (int i = 0; i < deviceDetector.getDeviceCount(); i++) {
        //     Debug.Log(deviceDetector._devices[i].name + " " + deviceDetector._available[i]);
        // }
        if (task.isVideo) {
            task.PlayVideo();
            Debug.Log("Played?");
        }

        int NearestDevice = user2deviceDistance.First().Key;
        // Debug.Log("Attach : " + deviceDetector._devices[NearestDevice] + " " + deviceDetector._available[NearestDevice]);
        if (deviceDetector.devices[NearestDevice].isAvailable) {
            Debug.Log("Attached?");
            task.SetDevice(NearestDevice);
            deviceDetector.setTexture(task.texture, NearestDevice);
            taskManager.taskList.Add(task);
            return ;
        }
        // TODO: if not available? what to do next?


        //TEST
        // for (int i = 0; i < deviceDetector.getDeviceCount(); i++) {
        //     Debug.Log(deviceDetector._devices[i].name + " " + deviceDetector._available[i] + " " + user2deviceAngle[i] + " " + user2deviceDistance[i]);
        // }
        
    }
    /// <summary>
    /// Sorted distance between user and devices
    /// </summary>
    public Dictionary<int, float> getDistance() {
        // Debug.Log(deviceDetector.getDeviceCount());
        for (int i = 0; i < deviceDetector.getDeviceCount(); i++) {
            user2deviceDistance[i] = Vector3.Distance(userState.getUserPosition(), deviceDetector.devices[i].m_screen.transform.position);
            deviceDetector.devices[i].disToUser = user2deviceDistance[i];
        }
        // user2deviceDistance = user2deviceDistance.OrderBy(i => i.Value).ToDictionary(k => k.Key, v => v.Value); // TODO: do not need sort now
        // for (int i = 0; i < user2deviceDistance.Length; i++) {
        //     Debug.Log(deviceDetector._devices[i].name + " " + user2deviceDistance[i]);
        // }
        return user2deviceDistance;
    }

    public float getUser2deviceDistance(int index) {
        return user2deviceDistance[index];
    }

    // public float getNearestUser2deviceDistance(out int deviceNum, out string deviceName) {
    //     // Dictionary<string, float> nearest = new Dictionary<string, float>();
    //     deviceNum = -1;
    //     deviceName = "";
    //     float dis = user2deviceDistance.Min();
    //     for (int i = 0; i < deviceDetector.getDeviceCount(); i++) {
    //         if (dis == user2deviceDistance[i]) {
    //             deviceName = deviceDetector._devices[i].name;
    //             deviceNum = i;
    //             break;
    //         }
    //     }
    //     return dis;
    //     // nearest.Add(deviceName, dis);
    //     // return nearest;
    // }

    public Dictionary<int, float> getUser2deviceAngles() {
        for (int i = 0; i < deviceDetector.getDeviceCount(); i++) {
           Transform[] transforms = deviceDetector._devices[i].GetComponentsInChildren<Transform>();
            foreach (Transform tf in transforms) {
                if (tf.name == "Screen") { 
                    user2deviceAngle[i] = Vector3.Angle(userState.getUserDirection(), tf.up);
                    user2deviceAngleDown[i] = Vector3.Angle(userState.getUserDirection(), tf.forward);
                    Vector2 temp1 = new Vector4((float) Math.Cos(user2deviceAngle[i]), (float) Math.Sin(user2deviceAngle[i]));
                    Vector2 temp2 = new Vector4((float) Math.Cos(user2deviceAngleDown[i]), (float) Math.Sin(user2deviceAngleDown[i]));

                    angleDot[i] = Vector2.Dot(temp1, temp2);
                    break;
                }
            }
        }
        // sort here?
        // user2deviceAngle = user2deviceAngle.OrderByDescending(i => i.Value).ToDictionary(k => k.Key, v => v.Value);
        return user2deviceAngle;
    }


    // public void initCameraPosition(int cameraNum) {
    //     deviceDetector._cameras[cameraNum].transform.position = textCamera;
    // }

    public void AdjustText(int index, bool value) {
        if (value) {
            deviceDetector.devices[index].subtitleModule.isTextSizeAdjusted = true;
        } else {
            deviceDetector.devices[index].subtitleModule.isTextSizeAdjusted = false;
        } 
    }



    ///<summary>
    /// Use to calculate angles between user and devices: NOW JUST FOR TEST
    ///</summary>
    public void CalculateAngles() {
        // for (int i = 0; i < )
    }


    /// <summary>
    /// Adaptation part (trigger by condition)
    /// </summary>

    public void StartAdaptation() {
        ang_down_text.text = "";
        
        int needNoticeTaskNum = -1;
        // if (taskManager.taskList.Count <= 1) {
        //     // 根据用户的channel最大化
        //     return ;
        // }


        Debug.Log("Adaptation started...");

        CalculatePriority(out float[] priorityList);

        if (priorityList == null) {
            Debug.Log("Adaptation end at priority check...");
            return ;
        }

        pri_text.text = "Priority\n";
        for (int i = 0; i < priorityList.Length; i++) {
            Debug.Log("task " + i + " start to iter");

            float[] score = new float[deviceDetector.getDeviceCount()];
            float maxScore = 0.0f;
            int maxScoreDeviceIndex = -1;
            Task task = (Task) taskManager.taskList[i];


            pri_text.text += task.taskName + ": " + priorityList[i] + "\n";

            for (int j = 0; j < deviceDetector.getDeviceCount(); j++) {
                float V = deviceDetector.devices[j].isVisible == true ? 1.0f : 0.0f;
                float A;
                if (deviceDetector.devices[j].isAvailable || j == task.LastDeviceNum) {
                    A = 1.0f;
                } else {
                    A = 0.0f;
                }

                int sizeIndex = deviceDetector.devices[j].deviceSize - 1;
                float S = deviceDetector.devices[j].deviceSizeRates[sizeIndex];
                
                // if 1 task
                float x;
                // if (taskManager.taskList.Count == 1) {
                //     x = task.targetDeviceNum == j ? 0.18f : 0.0f;
                // } else {
                //     x = task.targetDeviceNum == j ? 0.18f : 0.0f;
                // }
                x = task.targetDeviceNum == j ? 0.18f : 0.0f;


                float costDis = tanh(user2deviceDistance[j]);

                score[j] = A * (S + x + 0.11f * V - costDis);
                ang_down_text.text += deviceDetector.devices[j].deviceName + ": " + score[j].ToString() + "\n";
                
                Debug.Log("Device :" + deviceDetector.devices[j].deviceName + "\n" + "V :" + V + "\n" + "A :" + A + "\n" + "S :" + S + "\n" + "SizeIndex :" + deviceDetector.devices[j].device_actual_size + "\n"+ "X :" + x + "\n" + "CostDis :" + costDis + "\n" + "Score :" + score[j] + "\n");

                if (maxScore < score[j]) {
                    maxScore = score[j];
                    maxScoreDeviceIndex = j;
                }

            }
            Debug.Log("Task " + task.taskName + " Attach to device :" + deviceDetector.devices[maxScoreDeviceIndex].deviceName); 

            // set need notice or not
            if (!deviceDetector.devices[maxScoreDeviceIndex].isVisible && task.isNewTask) {
                task.needNotice = true;
                needNoticeTaskNum = i;
            }
            task.isNewTask = false;

            // change texture
            deviceDetector.ChangeTexture(task.LastDeviceNum, maxScoreDeviceIndex);

            Debug.Log("??????");


            maxScoreDeviceIndex = -1;
            maxScore = 0.0f;
        }


        // notice part
        if (needNoticeTaskNum != -1) {

            for (int i = 0; i < priorityList.Length; i++) {
                if (i == needNoticeTaskNum) {
                    continue;
                }
                // Find first task device
                Task task = (Task) taskManager.taskList[i];
                deviceDetector.devices[task.LastDeviceNum].Glint();
                task.VolumeDown();

                deviceDetector.devices[((Task) taskManager.taskList[needNoticeTaskNum]).LastDeviceNum].noticingDeviceNum = task.LastDeviceNum;
                deviceDetector.devices[((Task) taskManager.taskList[needNoticeTaskNum]).LastDeviceNum].noticeOn = true;
                break;
            }

            // set need notice device 
        }




    }

    public void CalculatePriority(out float[] pList) {
        int totalPriority = 0;
        for (int i = 0; i < taskManager.taskList.Count; i++) {
            totalPriority += ((Task) taskManager.taskList[i]).taskPriority;
        }

        // need to set priority firsts
        if (totalPriority == 0) {
            Debug.Log("No task in tasklist!");
            pList = null;
            return ;
        }

        pList = new float[taskManager.taskList.Count];
        for (int i = 0; i < taskManager.taskList.Count; i++) {
            Task task= (Task) taskManager.taskList[i];
            pList[i] = ((float) task.taskPriority / (float) totalPriority);
            // pList[i] = ((float) task.taskPriority / (float) totalPriority) * ((float) task.contentImportance / 5.0f);
        }
    }

    public float sigmoid(float value) {
        return 1.0F / (float) (1.0F + Math.Pow(Math.E, value));
    }

    public float tanh(float value) {
        // return (float) Math.Tanh(value / 6.0f) * 0.35f;
        return (float) Math.Tanh(value);
    }


    public void OnAudioReceiveTriggered() {
        
    }
}
