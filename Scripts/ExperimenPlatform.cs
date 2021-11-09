using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimenPlatform : MonoBehaviour
{
    public enum Command{
        Stop,
        Change,
        Operation,
        Task,
        UserFactor,
        ResetOper,
        Secondary,
        None
    }

    [SerializeField]
    public TaskManager taskManager;

    [SerializeField]
    public DisplayManager displayManager;

    [SerializeField]
    public DeviceDetector deviceDetector;


    // secondary tasks

    [SerializeField] public VIsualSecondaryTaskEasy v_easy;
    [SerializeField] public VisualSecondaryTaskHard v_hard;
    [SerializeField] public AudioSecondaryTask a_easy;



    public bool[] SecondaryTaskStarted;
    int exp_secondary_index = 3;

    Command socketCmd = Command.None;
    bool isSocketFlag = false;
    int src;
    int dst;

    public Task currentTask;
    public Task secondTask;



    // start/stop
    public bool pauseApp = false;


    // about rules
    // visual
    public bool oper_deviceGlint = false;
    public bool oper_closeSub = false;
    public bool oper_closeVideo = false;
    public bool oper_subDistanceChanged = false;

    public bool oper_changeKeywordSub = false;


    // audio
    public bool oper_volumedown = false;
    public bool oper_audioVolumeDown = false;

    // 
    public LogTimer logTimer;

    void Start()
    {
        src = -1;
        dst = -1;
        SecondaryTaskStarted = new bool[4];
        for (int i = 0; i < 4; i++) {
            SecondaryTaskStarted[i] = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: not useful when experiment
        // // check queue is empty
        // if (taskManager.CheckNewTask()) {
        //     Task task = taskManager.newTaskQueue.Dequeue();
        //     // we do not load task if completed
        //     if (task.isTaskCompleted) {
        //         return ;
        //     }

        //     //set Devices
        //     Debug.Log(task.videoNum.ToString() + task.audioNum.ToString() + task.subNum.ToString());
        //     // task.SetTargetDevices(1, 4, 6);
        //     AttachTaskOnDevices(task);

        //     // set texture (except phone) {}
        //     // Material texture = deviceDetector.devices[task.LastDeviceNum].deviceName == "Phone" ? task.phone_texture : task.texture;
        //     // deviceDetector.setTexture(texture, task.targetDeviceNum);
        //     // deviceDetector.devices[task.targetDeviceNum].setTask(task);


        //     task.SetDevice(task.targetDeviceNum);     

        //     // if (task.isVideo) {
        //     //     // set subtitle
        //     //     deviceDetector.devices[task.targetDeviceNum].subtitleModule.SetSubtitle(task.Subtitle);
        //     //     task.PlayVideo();
        //     //     deviceDetector.devices[task.targetDeviceNum].StartSubtitle(task.videoTime);
        //     // }

        //     // if (task.isAudio) {
        //     //     task.PlayAudio();
        //     // }
        //     if (taskManager.taskList.Count == 1) {
        //         currentTask = task; 
        //     } else {
        //         secondTask = task;
        //     }

        //     // set new task
        //     task.isNewTask = true;
            
        //     taskManager.taskList.Add(task);
        //     // taskManager.AdaptationEvent.Invoke();
        // }    

        if (isSocketFlag) {
            // deviceDetector.ChangeTexture(src, dst);
            rulesHandler();

            src = dst = -1;
            isSocketFlag = false;
        }

        keyHandler();
    }

    public void SocketRequestHandler(string str) {
        if (str.Split(' ').Length > 1) {
            string[] temp = str.Split(' ');
            for (int i = 0; i < temp.Length; i++) {
                rulesDecoder(temp[i]);
            }
        } else {
            rulesDecoder(str);
        }
        isSocketFlag = true;

    }

    void rulesHandler() {
        // visual rule
        switch (socketCmd) {
            case Command.Stop:
                pauseApp = !pauseApp;
                s_rule();
                break;
            case Command.Change:
                deviceDetector.ChangeTexture(src, dst);
                break;
            case Command.Operation:
                OperationHandler();
                break;
            case Command.Task:
                Task task = taskManager.TriggerTask(dst);
                // test
                // taskManager.AttachTaskOnDevices(task);
                break;
            case Command.UserFactor:
                displayManager.contextManager.testFactor = dst;
                displayManager.contextManager.currentTask = currentTask;
                break;
            case Command.ResetOper:
                ResetOperations(dst);
                break;
            case Command.Secondary:
                StartSecondaryTask(dst);
                break;
        }
        
    }

    void StartSecondaryTask(int index) {
        SecondaryTaskStarted[index] = !SecondaryTaskStarted[index];
        if (SecondaryTaskStarted[index]) {
            if (index == 0) {
                v_easy.StartTask();
            }
            if (index == 1) {
                v_hard.StartTask();
            }
            if (index == 2) {
                a_easy.StartTask();
            }
            if (index == 3) {
                a_easy.StartTask();
            }
        } else {
            if (index == 0) {
                v_easy.StopTask();
            }
            if (index == 1) {
                v_hard.StopTask();
            }
            if (index == 2) {
                a_easy.StopTask();
            }
            if (index == 3) {
                a_easy.StopTask();
            } 
        }
    }
    public void StartSecondaryTaskForExp(int index) {
        if (index != exp_secondary_index) {
            SecondaryTaskStarted[exp_secondary_index] = false;
            SecondaryTaskStarted[index] = true;

        } else {
            return ;
        }

        if (!SecondaryTaskStarted[exp_secondary_index]) {
            if (exp_secondary_index == 0) {
                v_easy.StopTask();
            }
            if (exp_secondary_index == 1) {
                v_hard.StopTask();
            }
            if (exp_secondary_index == 2) {
                a_easy.StopTask();
            }
            if (exp_secondary_index == 3) {
                a_easy.StopTask();
            } 
        }
        if (SecondaryTaskStarted[index]) {
            if (index == 0) {
                v_easy.StartTask();
            }
            if (index == 1) {
                v_hard.StartTask();
            }
            if (index == 2) {
                a_easy.StartTask();
            }
            if (index == 3) {
                a_easy.StartTask();
            }
        }

        exp_secondary_index = index;
    }

    void OperationHandler() {
        switch(src) {
            case 0:
                v_rule_glint(dst);
                break;
            case 1: 
                v_rule_sub_presence(dst);
                break;
            case 2: 
                v_rule_video_presence(dst);
                break;
            case 3:
                v_rule_dis_sub(dst);
                break;
            case 4: 
                a_rule_volume_down(dst);
                break;
            case 5:
                a_rule_audio_volume_down(dst);
                break;
            case 6:
                v_rule_change_keyword_sub(dst);
                break;
            case 7:
                v_rule_video_presence(dst);
                v_rule_change_keyword_sub(dst);
                break;
            
        }
    }

    void ResetOperations(int index) {
        if (oper_deviceGlint) {
            v_rule_glint(dst);
        }
        if (oper_closeSub) {
            v_rule_sub_presence(dst);
        } 
        if (oper_closeVideo) {
            v_rule_video_presence(dst);
        } 
        if (oper_subDistanceChanged) {
            v_rule_dis_sub(dst);
        } 
        if (oper_volumedown) {
            a_rule_audio_volume_down(dst);    
        } 
        if (oper_audioVolumeDown) {
            a_rule_volume_down(dst);
        }  
        if (oper_changeKeywordSub) {
            v_rule_change_keyword_sub(dst);
        }

    }

    void keyHandler() {

    }


    void v_rule_glint(int index) {
        oper_deviceGlint = !oper_deviceGlint;
        if (oper_deviceGlint) {
            deviceDetector.devices[index].Glint();
        } else {
            deviceDetector.devices[index].StopGlint();
        }
    }

    void v_rule_sub_presence(int index) {
        oper_closeSub = !oper_closeSub;
        deviceDetector.devices[index].subtitleModule.CloseSub(oper_closeSub);
 
    } 

    void v_rule_video_presence(int index) {
        oper_closeVideo = !oper_closeVideo;
        deviceDetector.devices[index].CloseVideo(oper_closeVideo);
    }

    void v_rule_dis_sub(int index) {
        oper_subDistanceChanged = !oper_subDistanceChanged;
        displayManager.contextManager.AdjustText(index, oper_subDistanceChanged);
    }

    void v_rule_change_keyword_sub(int index) {
        deviceDetector.devices[index].changeSub(true);
    }

     

    void a_rule_volume_down(int index) {
        oper_volumedown = !oper_volumedown;
        if (oper_volumedown) {
            deviceDetector.devices[index].task.VolumeDown();
        } else {
            deviceDetector.devices[index].task.VolumeUp();

        }
    }

    public void a_rule_audio_volume_down(int index) {
        oper_audioVolumeDown = !oper_audioVolumeDown;
        if (oper_audioVolumeDown) {
            deviceDetector.devices[index].task.VolumeDown();
        } else {
            deviceDetector.devices[index].task.VolumeUp();

        }
    }


    void s_rule() {
        if (pauseApp) {
            Pause();
            taskManager.RemoveAllTasks();
        } else {
            Restart();
            taskManager.CallingAllTasks();
        }
    }

    void Restart() {
        Time.timeScale = 1;
    }

    void Pause() {
        Time.timeScale = 0;
    }


    void rulesDecoder(string str) {
        switch (str[0]) {
            case 's':
                socketCmd = Command.Stop;
                break ;
            case 'c':
                socketCmd = Command.Change;
                if (int.TryParse(str[1].ToString(), out int result) && int.TryParse(str[2].ToString(), out int result1)) {
                    src = int.Parse(str[1].ToString());
                    dst = int.Parse(str[2].ToString());
                }

                break ;
            case 'o':
                socketCmd = Command.Operation;
                if (int.TryParse(str[1].ToString(), out int result3) && int.TryParse(str[2].ToString(), out int result4)) {
                    src = int.Parse(str[1].ToString());
                    dst = int.Parse(str[2].ToString());
                }
                break ;
            case 't':
                socketCmd = Command.Task;
                if (int.TryParse(str[1].ToString(), out int result5)) {
                    dst = int.Parse(str[1].ToString());
                }

                break;
            case 'u':
                socketCmd = Command.UserFactor;
                if (int.TryParse(str[1].ToString(), out int result6)) {
                    dst = int.Parse(str[1].ToString());
                }
                break;
            case 'r':
                socketCmd = Command.ResetOper;
                if (int.TryParse(str[1].ToString(), out int result7)) {
                    dst = int.Parse(str[1].ToString());
                }
                break;
            // case 'k':
            //     socketCmd = Command.Secondary;
            //     if (str[1])
            //     dst = int.Parse(str[1].ToString());

            //     break;
            default:
                break;
                
        }
    }
}
