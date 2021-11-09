using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


public class TaskManager : MonoBehaviour
{



    [System.Serializable]
    public class AdaptationEvent : UnityEvent<bool[]> {}

    public DeviceDetector deviceDetector;
    public ArrayList taskList;
    public ArrayList waitList;
    public Queue<Task> newTaskQueue;

    // pre-task finding
    public GameObject[] taskObjects;

    private int TriggerNumber = -1;

    private bool taskTriggered = false;

    // sub
    public SubtitleDisplayer subtitleDisplayer;
    public Coroutine sub;

    int lastCount;

    [SerializeField] public AdaptationEvent adaptationEvent;



    // Start is called before the first frame update
    void Start()
    {
        taskList = new ArrayList();
        waitList = new ArrayList();
        newTaskQueue = new Queue<Task>();
        lastCount = taskList.Count;

        taskObjects = GameObject.FindGameObjectsWithTag("Task Objects").OrderBy(i => i.transform.GetSiblingIndex()).ToArray();

        if (adaptationEvent == null) {
            adaptationEvent = new AdaptationEvent();
        }

    }

    // Update is called once per frame
    void Update()
    {
        /*
            TODO: Do in context manager?
                1. Check TaskQueue
                2. if Task
                    2.1 Check device availability
                    2.2 if available
                        put task
        */
 
    }

    public bool CheckNewTask() {
        // if (taskList.Count != lastCount) {
        //     lastCount = taskList.Count;
        //     return true;
        // }
        if (newTaskQueue.Count > 0) {
            return true;
        }
        return false;   
    }

    public bool CheckWaitTask() {
        if (waitList.Count > 0) {
            return true;
        }
        return false;
    }

    // new task ready to start
    public void CallingTask(Task task) {
        Debug.Log("task called");
        Debug.Log(task.taskName + " started");
        // taskList.Add(task);
        task.isTaskOver = false;
        newTaskQueue.Enqueue(task);
    }

    // task completed
    public void RemoveTask(Task task) {
        Debug.Log("task removed");
        Debug.Log(task.taskName + " ended");
        if (taskList.Contains(task)) {
            // remove task in last displayed device
            deviceDetector.setTexture(ResourcesManager.emptyScreenTexture, task.LastDeviceNum);
            deviceDetector.devices[task.LastDeviceNum].task = null;

            // reset last displayed device num
            task.LastDeviceNum = task.targetDeviceNum;

            task.isTaskOver = true;
            taskList.Remove(task);

            // remove all outputs from each devices
            RemoveAllTasksOnDevices(task);

            // if (task.isVideo) {
            //     task.PauseVideo();
            //     deviceDetector.devices[task.targetDeviceNum].subtitleModule.SaveSubpointer();
            //     task.subPointer = deviceDetector.devices[task.targetDeviceNum].subtitleModule.subPointer;
            //     deviceDetector.devices[task.targetDeviceNum].StopSubtitle();
            //     if (task.isTaskCompleted) {
            //         deviceDetector.devices[task.targetDeviceNum].subtitleModule.subPointer = -1;
            //     }
            //     // StopCoroutine(sub);
            // }

            // if (task.isAudio) {
            //     task.PauseAudio();
            // }
        } else {
            Debug.Log("No task here");
        }
    }

    public void CallingAllTasks() {
        for (int i = 0; i < waitList.Count; i++) {
            CallingTask((Task) waitList[i]);
        }
        waitList.Clear();
    }

    public void RemoveAllTasks() {
        for (int i = 0; i < taskList.Count; i++) {
            waitList.Add((Task) taskList[i]);
            RemoveTask((Task) taskList[i]);
        }
    }


    public void ScreenInteraction(Task task, bool yes) {
        if (!task.isInteracted && task.onTaskTriggered) {
            task.isInteracted = true;
            task.interactionEvent.Invoke(yes);
        }
    }

    public Task TriggerTask(int index) {
        Task task = taskObjects[index].GetComponent<Task>();
        var item = GetTaskItem(index);
        task.isTriggered = !task.isTriggered;
        if (task.isTriggered) {
            if (task.taskName == "beforeCall") {
                task.gameObject.GetComponent<AudioSource>().Play();
                // delete laptop
                for (int i = 0; i < deviceDetector.getDeviceCount(); i++) {
                    if (i == deviceDetector.getDeviceCount()-1) 
                        break;
                    deviceDetector.devices[i] = deviceDetector.devices[i+1];
                
                }
                deviceDetector.deviceCount -= 1;
            }
            if (task.taskName == "TvVideo") {
                for (int i = 0; i < taskList.Count; i++) {
                    Task tk = (Task) taskList[i];
                    Debug.Log(tk.taskName);
                }
                ((Task) taskList[1]).GetComponent<AudioSource>().Stop();
                // task.GetComponent<AudioSource>().Stop();
            }
            item.SensorTrigger(task);
        } else {
            item.SensorUntrigger(task);
        }
        return task;
    }

    public Task TriggerTaskOnce(int index) {
        Task task = taskObjects[index].GetComponent<Task>();
        var item = GetTaskItem(index);
        item.SensorTrigger(task);
        return task;
    }

    public SensorItem GetTaskItem(int index) {
        // TODO: should user tryGetComponent
        
        SensorItem item = null;
        // if (index == 0 || index == 1 || index == 2 || index == 3 || index == 4) {
        //     item = taskObjects[index].GetComponent<OutputItem>();
        // }
        if (index == 0 || index == 2) {
            item = taskObjects[index].GetComponent<OutputItem>();
        } else if (index == 1) {
            item = taskObjects[index].GetComponent<BeforeCallItem>();
        // } else if (index == 3) {
        //     item = taskObjects[index].GetComponent<MathItem>();
        // } else if (index == 4) {
        //     item = taskObjects[index].GetComponent<PlateSensorItem>();
        // } else if (index == 5) {
        //     item = taskObjects[index].GetComponent<OutputItem>();
        }
        return item;
    }

    // set devices
    public void AttachTaskOnDevices(Task task, bool exp1 = false) {
        if (exp1) {
            if (task.videoNum != -1 && task.audioNum != -1 && task.subNum != -1 && task.videoNum == task.audioNum && task.audioNum == task.subNum) {
                Debug.Log("Attaching on same device :" + deviceDetector.devices[task.audioNum]);
                // return ;
            }

            // set video
            if (task.videoNum != -1 && deviceDetector.devices[task.videoNum].isAvailable) {
                Material texture = deviceDetector.devices[task.videoNum].deviceName == "Phone" ? task.phone_texture : task.texture;
                deviceDetector.devices[task.videoNum].setTexture(texture);
                deviceDetector.devices[task.videoNum].backupTexture = deviceDetector.devices[task.videoNum].texture;
                Debug.Log(texture);
                task.PlayVideo();

                deviceDetector.devices[task.videoNum].setTask(task);
                Debug.Log("Attached video on " + deviceDetector.devices[task.videoNum].deviceName);

            }

            // set audio 
            if (task.audioNum != -1 && deviceDetector.devices[task.audioNum].isAvailable) {
                task.audioPos = deviceDetector.devices[task.audioNum].m_screen.transform.position; 
                task.deviceChanged = true;
                task.PlayAudio();
                Debug.Log("is here?");
            
                deviceDetector.devices[task.audioNum].setTask(task);
                Debug.Log("Attached audio on " + deviceDetector.devices[task.audioNum].deviceName);
                

            }

            // set sub
            if (task.subNum != -1 && deviceDetector.devices[task.subNum].isAvailable) {
                // deviceDetector.devices[task.subNum].StopSubtitle();
                // deviceDetector.devices[task.subNum].subtitleModule.SaveSubpointer();
                deviceDetector.devices[task.subNum].subtitleModule.subPointer = task.subPointer;
                deviceDetector.devices[task.subNum].subtitleModule.SetSubtitle(task.Subtitle);
                deviceDetector.devices[task.subNum].StartSubtitle(task.videoTime);

                deviceDetector.devices[task.subNum].setTask(task);
                Debug.Log("Attached sub on " + deviceDetector.devices[task.subNum].deviceName);

            }
            return ;
        }
        // Debug.Log("Before attach :" + task.subPointer);
        // count devices
        Dictionary<int, List<SolutionModel>> deviceDict = new Dictionary<int, List<SolutionModel>>();
        foreach (SolutionModel model in task.deviceList) {
            if (!deviceDict.ContainsKey(model._device)) {
                List<SolutionModel> sl = new List<SolutionModel>();
                sl.Add(model);
                deviceDict.Add(model._device, sl);
            } else {
                deviceDict[model._device].Add(model);
            }
        }

        // set methods for each device
        float time = task.videoTime;
        bool isVideoPlayspeed = true;
        foreach (KeyValuePair<int, List<SolutionModel>> pair in deviceDict) {
            List<SolutionModel> model_list = pair.Value;

            bool NeedAdjustSub = false;
            bool NeedChangeKeysub = false;
            bool NeedAdjustDis = false;
            foreach (SolutionModel model in model_list) {
                int content_index = model._content;
                int method_index = model._method;

                switch (content_index) {
                    case 0:

                        deviceDetector.devices[model._device].subtitleModule.subPointer = task.subPointer;
                        deviceDetector.devices[model._device].subtitleModule.SetSubtitle(task.Subtitle);
                        // deviceDetector.devices[model._device].StartSubtitle(task.videoTime);
                        deviceDetector.devices[model._device].subPlaying = true;
                        
                        Material texture = deviceDetector.devices[model._device].deviceName == "Phone" ? task.phone_texture : task.texture;
                        deviceDetector.devices[model._device].backupTexture = deviceDetector.devices[model._device].texture;

                        if (method_index == 0 || method_index == 2) {
                            deviceDetector.devices[model._device].setTexture(texture);
                            if (method_index == 2) {
                                // task.VideoPlaySpeed(true);  // need to adjust audio after //TODO: .....fuck sth problem
                                isVideoPlayspeed = true;
                            }
                        }
                        break;
                    
                    case 1:
                        if (content_index == 1) {
                            deviceDetector.devices[model._device].subtitleModule.subPointer = task.subPointer;
                            deviceDetector.devices[model._device].subtitleModule.SetSubtitle(task.Subtitle);
                            // deviceDetector.devices[model._device].StartSubtitle(task.videoTime);
                            deviceDetector.devices[model._device].subPlaying = true;
                        }
                        deviceDetector.devices[model._device].setAudioCilp(time, task.m_audioPlayer.clip);
                        // task.m_VideoPlayer.SetTargetAudioSource(task.audiocCounts, deviceDetector.devices[model._device].m_audio_player);
                        task.audiocCounts += 1;
                        if (method_index == 2) {
                            deviceDetector.devices[model._device].AdjustSound(true);
                        }
                        break;

                    case 2:
                        deviceDetector.devices[model._device].subtitleModule.subPointer = task.subPointer;
                        deviceDetector.devices[model._device].subtitleModule.SetSubtitle(task.Subtitle);
                        // deviceDetector.devices[model._device].StartSubtitle(task.videoTime);
                        deviceDetector.devices[model._device].subPlaying = true;
                        
                        if (method_index == 0) {
                            NeedAdjustSub = true;
                        }
                        if (method_index == 1) {
                            NeedChangeKeysub = true;
                        }
                        if (method_index == 2) {
                            // AdjustText(model._device, true);
                            NeedAdjustDis = true;
                        }
                        break;
                }
            }
            deviceDetector.devices[pair.Key].setTask(task);
            // sub
            if (NeedAdjustSub) {
                deviceDetector.devices[pair.Key].AdjustPostion(true);
                Debug.Log("Here to adjust!!");
            }
            if (NeedChangeKeysub) {
                deviceDetector.devices[pair.Key].changeSub(true);
                Debug.Log("Here to keysub!!");
                deviceDetector.devices[pair.Key].StartSubtitle(task.videoTime);
            }
            if (NeedAdjustDis) {
                AdjustText(pair.Key, true);
            }


        }
        bool isAudioPlaying = false;
        foreach (KeyValuePair<int, List<SolutionModel>> pair in deviceDict) {
            Device d = deviceDetector.devices[pair.Key];
            if (isVideoPlayspeed) {
                // d.m_audio_player.pitch = 0.7f;
            }
            if (d.m_audio_player.clip != null && !isAudioPlaying) {
                d.m_audio_player.mute = true;
                d.m_audio_player.Play();
                isAudioPlaying = true;
            }
            if (d.subPlaying) {
                d.StartSubtitle(task.videoTime);
            }
        }
        task.PlayVideo();

        // fix audio rac
        foreach (KeyValuePair<int, List<SolutionModel>> pair in deviceDict) {
            Device d = deviceDetector.devices[pair.Key];
            if (d.m_audio_player.clip != null) {
                d.m_audio_player.mute = false;
                if (d.deviceName == "Desk") {
                    d.m_audio_player.mute = true;
                } else {
                    d.m_audio_player.volume = 0.2f;
                }
                
            }
        }

        // set attached device dictionary
        task.deviceDict = deviceDict;

    }

    public void RemoveAllTasksOnDevices(Task task) {
        task.PauseVideo();

        bool subSaved = false;
        foreach (KeyValuePair<int, List<SolutionModel>> pair in task.deviceDict) {
            // Debug.Log("Revmoing task on " + pair.Key);
            // cancel methods
            task.deviceDetector.devices[pair.Key].closeSub(false);
            if (task.deviceDetector.devices[pair.Key].isClosedVideo)
                task.deviceDetector.devices[pair.Key].CloseVideo(false);
            task.deviceDetector.devices[pair.Key].m_audio_player.volume = 0.6f;
            if (task.deviceDetector.devices[pair.Key].isSlowed) {
                task.VideoPlaySpeed(false);
                task.deviceDetector.devices[pair.Key].m_audio_player.pitch = 1.0f;
            }
            task.deviceDetector.devices[pair.Key].AdjustPostion(false);
            task.AdjustSound(false);
            if (task.deviceDetector.devices[pair.Key].isChangedKeysub)
                task.deviceDetector.devices[pair.Key].changeSub(false);


            // remove task
            deviceDetector.devices[pair.Key].setTexture(ResourcesManager.emptyScreenTexture);
            deviceDetector.devices[pair.Key].setTask(null);
            deviceDetector.devices[pair.Key].isAvailable = true;
            // }

            // if (model._content == 1) {
            deviceDetector.devices[pair.Key].m_audio_player.Pause();
            deviceDetector.devices[pair.Key].m_audio_player.clip = null;          
            deviceDetector.devices[pair.Key].setTask(null);
            deviceDetector.devices[pair.Key].isAvailable = true;

            // }

            // if (model._content == 2) {
            // if (!subSaved) {
                // Debug.Log("More Before :" + task.subPointer + " " + deviceDetector.devices[pair.Key].subtitleModule.subPointer + " " + deviceDetector.devices[pair.Key].subtitleModule.subtitleDisplayer.parser.subPointer);
            deviceDetector.devices[pair.Key].subtitleModule.SaveSubpointer();
            // Debug.Log("Before :" + task.subPointer + " " + deviceDetector.devices[pair.Key].subtitleModule.subPointer + " " + deviceDetector.devices[pair.Key].subtitleModule.subtitleDisplayer.parser.subPointer);
            task.subPointer = deviceDetector.devices[pair.Key].subtitleModule.subPointer;
            // Debug.Log("After :" + task.subPointer + " " + deviceDetector.devices[pair.Key].subtitleModule.subPointer + " " + deviceDetector.devices[pair.Key].subtitleModule.subtitleDisplayer.parser.subPointer);

            deviceDetector.devices[pair.Key].StopSubtitle();
            deviceDetector.devices[pair.Key].setTask(null);
            deviceDetector.devices[pair.Key].isAvailable = true;

                subSaved = true;
            // }
        }





        // save context
        // SaveTaskContexts(task);
        // foreach (SolutionModel model in task.deviceList) {

            // switch(model._method) {
            //     case 0:
            //         task.Unmute();
            //         break;
            //     case 1:
            //         deviceDetector.devices[model._device].CloseVideo(false);
            //         task.Unmute();
            //         break;
            //     case 2:
            //         task.VideoPlaySpeed(false);
            //         Debug.Log("Back to normal speed");
            //         task.Unmute();
            //         break;
            //     case 3:
            //         task.deviceDetector.devices[model._device].closeSub(false);
            //         task.deviceDetector.devices[model._device].CloseVideo(false);
            //         break;
            //     case 4:
            //         task.deviceDetector.devices[model._device].CloseVideo(false);
            //         task.deviceDetector.devices[model._device].AdjustPostion(false);
            //         break;
            //     case 5:
            //         task.AdjustSound(false);
            //         task.deviceDetector.devices[model._device].closeSub(false);
            //         task.deviceDetector.devices[model._device].CloseVideo(false);
            //         break;
            //     case 6:
            //         task.deviceDetector.devices[model._device].AdjustPostion(false);
            //         task.deviceDetector.devices[model._device].CloseVideo(false);
            //         task.Unmute();
            //         break;

            //     case 7:
            //         task.deviceDetector.devices[model._device].changeSub(false);
            //         task.deviceDetector.devices[model._device].CloseVideo(false);
            //         task.Unmute();
            //         break;

            //     case 8:
            //         AdjustText(model._device, false);
            //         task.deviceDetector.devices[model._device].CloseVideo(false);
            //         task.Unmute();
            //         break;

            // }



            // if (model._content == 0) {
            // deviceDetector.devices[model._device].setTexture(ResourcesManager.emptyScreenTexture);
            // task.PauseVideo();
            // deviceDetector.devices[model._device].setTask(null);
            // deviceDetector.devices[model._device].isAvailable = true;
            // }

            // if (model._content == 1) {
            // task.PauseAudio();
            // deviceDetector.devices[model._device].setTask(null);
            // deviceDetector.devices[model._device].isAvailable = true;

            // }

            // if (model._content == 2) {
            // deviceDetector.devices[model._device].subtitleModule.SaveSubpointer();
            // task.subPointer = deviceDetector.devices[model._device].subtitleModule.subPointer;
            // deviceDetector.devices[model._device].StopSubtitle();
            // deviceDetector.devices[model._device].setTask(null);
            // deviceDetector.devices[model._device].isAvailable = true;

                // Debug.Log("When remove all task :" + task.subPointer + " " + deviceDetector.devices[model._device].subtitleModule.subPointer);
            // }
            // taskManager.RemoveTask(task);
        // }


    }

    public void SaveTaskContexts(Task task) {
        if (task.isTaskCompleted)
            return ;
        // save subpointer
        if (task.subNum != -1) {
            // Debug.Log("sub pointer bef: " + task.subPointer + " " + deviceDetector.devices[task.subNum].subtitleModule.subPointer);
            task.subPointer = deviceDetector.devices[task.subNum].subtitleModule.subPointer;
            // Debug.Log("sub pointer aft: " + task.subPointer + " " + deviceDetector.devices[task.subNum].subtitleModule.subPointer);
        }
    }

    public void AdjustText(int index, bool value) {
        if (value) {
            deviceDetector.devices[index].subtitleModule.isTextSizeAdjusted = true;
        } else {
            deviceDetector.devices[index].subtitleModule.isTextSizeAdjusted = false;
        } 
    }
}

