using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class InteractionEvent : UnityEvent<bool> {}

public class Task : MonoBehaviour
{
    public enum OutputMode {
        VideoSub = 3,
        Sub = 2,
        Keyword = 1,
        None = 0
    }

    public float[] OutputCost = {0.0f, 0.25f, 0.5f, 0.75f};

    [SerializeField] public Material texture;

    [SerializeField] public Material phone_texture;
    [SerializeField] public string taskName;
    [SerializeField] public TextAsset Subtitle;
    [SerializeField] public TextAsset Key_Subtitle;

    // if video, must have audio 
    [HideInInspector] public bool isVideo = false;
    [HideInInspector] public bool isAudio = false;
    [HideInInspector] public bool isText = false;

    [HideInInspector] public bool isSecondaryTask = false;
    [HideInInspector] public bool isTimer = false;

    // Set target Device
    [HideInInspector] public int videoNum = -1;
    [HideInInspector] public int audioNum = -1;
    [HideInInspector] public int subNum = -1;

    // TODO: change to list
    [HideInInspector] public List<SolutionModel> deviceList;
    [HideInInspector] public Dictionary<int, List<SolutionModel>> deviceDict;


    // whether if task could follow user or not

    // [SerializeField] public UnityEngine.Video.VideoClip videoClip;

    /// <summary>
    /// Device index for tha task attached at first
    /// </summary>
    [SerializeField] public int targetDeviceNum = -1; 

    [SerializeField] public Timer timer;

    [HideInInspector] public UnityEngine.Video.VideoPlayer m_VideoPlayer;
    [HideInInspector] public AudioSource m_audioPlayer;

    private int onDeviceNum = -1;

    [HideInInspector] public bool isTriggered = false;

    /// <summary>
    /// Only task triggered then could be interacted
    /// </summary>
    [HideInInspector] public bool onTaskTriggered = false;

    // this var is for object task over
    [HideInInspector] public bool isTaskOver = false;
    // user task over`
    [HideInInspector] public bool isTaskCompleted = false;

    // need to reset when interaction need repeat // TODO: if you want to use, make it FALSE first
    // usually make it false before sensortrigger
    [HideInInspector] public bool isInteracted = true;

    public InteractionEvent interactionEvent;

    public float videoTime = -1.0f;
    [HideInInspector] public int subPointer = 0;


    /// <summary>
    /// judge playing video or not 
    /// </summary>
    [HideInInspector] public bool isPlaying = false;

    [HideInInspector] public bool isNewTask = false;

    [HideInInspector] public bool needNotice = false;
    [HideInInspector] public bool isSoundAdjusted = false;

    /// <summary>
    /// last display device that the task untriggered
    /// </summary>
    int lastDeviceNum;
    [HideInInspector] public bool deviceChanged = false;
    [HideInInspector] public int LastDeviceNum {
        set {
            lastDeviceNum = value;
            deviceChanged = true;
        }
        get {
            return lastDeviceNum;
        }
    }

    /// <summary>
    /// For adatation policy
    /// </summary>
    

    [Range(1, 5)] public int taskPriority;
    [Range(1, 5)] public int contentImportance;   // * output mode 

    [HideInInspector] public OutputMode mode;

    [HideInInspector] public Vector3 audioPos;
    [HideInInspector] public ushort audiocCounts = 0;

   [SerializeField] public DeviceDetector deviceDetector;

   // video end event
   public UnityEvent videoEndEvent;


    // Start is called before the first frame update
    void Awake()
    {
        audioPos = gameObject.transform.position;
        if (interactionEvent == null) {
            interactionEvent = new InteractionEvent();
        }

        lastDeviceNum = targetDeviceNum;

        // check video
        isVideo = gameObject.TryGetComponent<UnityEngine.Video.VideoPlayer>(out m_VideoPlayer);

        // check audio
        isAudio = gameObject.TryGetComponent<AudioSource>(out m_audioPlayer);

        // check sub
        if (Subtitle == null) {
            Debug.Log("No subtitle! Pls check..");
        }



        mode = OutputMode.VideoSub;
        if (m_VideoPlayer) {
            m_VideoPlayer.loopPointReached += EndReached;
        }


        // init device-attach list
        deviceList = new List<SolutionModel>();
        if (targetDeviceNum != -1) {
            SetTargetDevices();
        }

        // video end event
        if (videoEndEvent == null) {
            videoEndEvent = new UnityEvent();
        }

    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(m_VideoPlayer.isPlaying);

        // With timer should be checked whether total time ran out or not
        if(isTimer) { 
            if (timer.CheckTimeOver()) {
                this.isTaskOver = true;
            }
        }

        // videoframe
        if (isVideo) {
            GetVideoTimeline();
            // Debug.Log("task :" + videoTime);
        }


        // update task move
        if (!isSecondaryTask && deviceChanged) {
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, audioPos, 0.1f);
            // Debug.Log(audioPos);
        }


        if (isSoundAdjusted) {
            if (deviceDetector.devices[subNum].disToUser <= 4.5f && deviceDetector.devices[subNum].disToUser >= 1.2f) {
                float new_volume = 1 - 0.2424f * (4.5f - deviceDetector.devices[subNum].disToUser);
                m_audioPlayer.volume = new_volume;
            }
        }


        // Debug.Log("task subpointer :" + subPointer);


    }

    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        // when video is over
        if (videoNum != -1) {
            // restart
            Debug.Log("Video ended...");
            // Debug.Log("Sub pointer bef : " + deviceDetector.devices[subNum].subtitleModule.subPointer + " " + deviceDetector.devices[subNum].task.subPointer);
            deviceDetector.devices[subNum].subtitleModule.SaveSubpointer();
            // Debug.Log("Sub pointer mid : " + deviceDetector.devices[subNum].subtitleModule.subPointer + " " + deviceDetector.devices[subNum].task.subPointer);
            deviceDetector.devices[subNum].StopSubtitle();
            deviceDetector.devices[subNum].subtitleModule.subPointer = 0;
            // Debug.Log("Sub pointer aft : " + deviceDetector.devices[subNum].subtitleModule.subPointer + " " + deviceDetector.devices[subNum].task.subPointer);
            deviceDetector.devices[subNum].subtitleModule.SetSubtitle(Subtitle);
            deviceDetector.devices[subNum].StartSubtitle(videoTime);
        }

        videoEndEvent.Invoke();
        
    }

    public void SetTargetDevices() {
        SolutionModel videoModel = new SolutionModel(0, 0, targetDeviceNum, 0.0f);
        SolutionModel audioModel = new SolutionModel(1, 0, targetDeviceNum, 0.0f);
        SolutionModel subModel = new SolutionModel(2, 2, targetDeviceNum, 0.0f);

        deviceList.Add(videoModel);
        deviceList.Add(audioModel);
        deviceList.Add(subModel);

        deviceDict = new Dictionary<int, List<SolutionModel>>();
        List<SolutionModel> sl = new List<SolutionModel>();
        sl.Add(videoModel);
        deviceDict.Add(targetDeviceNum, sl);

    }

    public void SetDevice(int deviceNum) {
        onDeviceNum = deviceNum;
    }

    public int GetDevice() {
        return onDeviceNum;
    }


    public void SetTaskComplete() {
        isTaskCompleted = true;
    }

    public void PlayVideo() {
        Debug.Log(videoTime);
        m_VideoPlayer.time = videoTime;
        Debug.Log("videotime = " + m_VideoPlayer.time);
        m_VideoPlayer.Play();
        isPlaying = true;
    }


    public void PauseVideo() {
        m_VideoPlayer.Pause();
        isPlaying = false;
    }

    public void ReplayVideo(float time = 0.0f) {
        PlayVideo();
        m_VideoPlayer.time = time;
        GetVideoTimeline();
    }

    public void PlayAudio() {
        m_audioPlayer.Play();
        isPlaying = true;
    }

    public void PauseAudio() {
        m_audioPlayer.Pause();
        isPlaying = false;
    }

    public void VideoPlaySpeed(bool value) {
        if (value) {
            m_VideoPlayer.playbackSpeed = 0.7f;
        } else {
            m_VideoPlayer.playbackSpeed = 1.0f;
        }
    }

    public bool AudioOver() {
        // Debug.Log(m_audioPlayer.time);
        return false;
    }

    public void GetVideoTimeline() {
        // Debug.Log(m_VideoPlayer.frame +  " " + m_VideoPlayer.frameCount);
        if (m_VideoPlayer.isPlaying) {
            videoTime = (float) m_VideoPlayer.time;
        }
    }

    public void VolumeDown() {
        if (isAudio || isVideo) {
            m_audioPlayer.volume = 0.2f;
        }
    }
    public void VolumeUp() {
        if (isAudio || isVideo) {
            m_audioPlayer.volume = 1.0f;
        }
    }

    public void Mute() {
        if (isAudio || isVideo) {
            m_audioPlayer.volume = 0.0f;
        }
    }

    public void Unmute() {
        if (isAudio || isVideo) {
            m_audioPlayer.volume = 0.6f;
        }
    }

    public void MoveAudioPosition(Vector3 pos) {
        audioPos = pos;
        // gameObject.transform.position = pos;
    }

    public void AdjustSound(bool value) {
        if (value) {
            isSoundAdjusted = true;
        } else {
            isSoundAdjusted = false;
            ResetVolume();
        }
    }

    public void ResetVolume() {
        m_audioPlayer.volume = 0.6f;
    }

    public void fff() {
        m_VideoPlayer.audioOutputMode = UnityEngine.Video.VideoAudioOutputMode.AudioSource;
    }


    public override string ToString()
    {
        string s = "";
        s += "Task Name: " + taskName + "\n";
        s += "Target: " + targetDeviceNum + "\n";
        s += "Last Target:" + lastDeviceNum + "\n";
        s += "Texture: " + texture.name + "\n";
        s += "Task over " + isTaskOver + "\n";
        s += "Task completed " + isTaskCompleted + "\n";  

        return s;
    }
}
