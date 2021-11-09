using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
public class UserState : MonoBehaviour, IMixedRealityPointerHandler, IMixedRealitySpeechHandler 
{
    
    public DeviceDetector deviceDetector;   
    public TaskManager taskManager;

    // state for user is moving or not
    [HideInInspector] public bool isMoving;
    [HideInInspector] public bool isMoved;

    [HideInInspector] public bool isStanding; // TODO: need to calibrate threshold of standing height

    [HideInInspector] public bool isSpeaking = false;
    [HideInInspector] public bool isGazing = false;

    bool isGazed = false;

    public bool[] ChannelLimitStatus;
    

    [HideInInspector] public bool isHandUsing = false;

    const float userHeightThres = 0.5f;

    // first gaze needed
    bool isFirstGaze = true;

    Vector3 lastUserPos;
    Vector3 curUserPos;
    GameObject lastDeviceGazed;
    GameObject curDeviceGazed;

    Vector3 lastDirection;
    Vector2 lastUserPosition;

    // device num for gazing
    [HideInInspector] public int lastDeviceGazedNum = -1;
    [HideInInspector] public int curDeviceGazedNum = -1;

    float adaptationTime = 0.0f;

    float speakTime = 0.0f;
    float movingTime = 0.0f; // detect moving interval
    float gazeTime = 0.0f;
    float userMovingTime = 0.0f; // actual user moving time
    float stopMovingTime = 0.0f;
    float averMoving = 0.0f;
    float averVolume = 0.0f;
    float directionTime = 0.0f;
    float moveTime = 0.0f;

    float walkingChannelIncreasement = 0.0f;


    //TEST 
    public TMPro.TMP_Text text;
    public TMPro.TMP_Text text2;
    public TMPro.TMP_Text text3;



    int speakingCount = 0;
    int movingCount = 0;
    int movingFrameCount = 0;

    // channel capacitiy 

    float visualChannelCapacity = 0.0f;

    ///
    /// const variables
    /// 
    ///

    const int SPEAK_DURATION_COUNT = 2;
    const int MOVING_DURATION_COUNT = 3;

    const float MAX_CHANNEL_CAPACITY = 1.0f;
    const float DETECT_TIME = 1.0f;

    const float INTERVAL_SPEAK = 1.0f;
    const float INTERVAL_MOVING = 0.3f;
    const float INTERVAL_MOVEMENT = 2.0f;
    const float INTERVAL_DIRECTION = 2.0f;

    const float USER_SPEAKING_VALUE = 2.0f;
    const float MOVING_THRES = 0.010f;

    const float MAX_MOVING_CHANNEL_CAPACITY = 0.2f;


    const float CAPACITY_ALPHA = 0.0f; // may different for disability people



    bool needAdaptation;
    const float INTERVAL_ADAPTATION = 3.0f;



    public GameObject grabbedDevice {
        set {
            // Debug.Log(value.name);
            _grabbedDevice = value;
        }
    }

    GameObject _grabbedDevice;

    AudioSource audioSource;
    string micName;
    int frameCount = 0;
    float maxVolume = 0.0f;
    public bool adapatationEnabled = false;

    void Start() {
        isMoving = false;
        isMoved = false;
        lastUserPos = this.getUserPosition();
        _grabbedDevice = null;

        audioSource = gameObject.GetComponent<AudioSource>();


        // detect speaking
        micName = Microphone.devices[0];
        audioSource.clip = Microphone.Start(micName, true, 3000, 44100);
        audioSource.loop = true;
        while (! (Microphone.GetPosition(null) > 0)) {}
        audioSource.Play();


        // 
        ChannelLimitStatus = new bool[4];
        for (int i = 0; i < 4; i++) {
            ChannelLimitStatus[i] = false;

        }

        // set channel limit
        // ChannelLimitStatus[0] = true;
        // ChannelLimitStatus[3] = true;


        lastDirection = getUserDirection();

        needAdaptation = false;
    }

    void Update() {

        // TODO: detect user is consitently moving or not, if true, it means isMovet = true, should be adapted.

        // smoothing

        // DetectUserMoving();
        DetectUserStanding();
        DetectUserSpeaking();
        // DetectUserGazing();
        DetectUserDirection();
        DetectPositionChanged();

        // Debug.Log("isMoving :" + isMoving);
        // Debug.Log("isStanding :" + isStanding + " " + getUserPosition());


        // walking increase channel capacity
        // if (isMoving && userMovingTime >= 2.0f) {
        //     if (walkingChannelIncreasement < MAX_MOVING_CHANNEL_CAPACITY) {
        //         walkingChannelIncreasement += 0.001f;
        //         visualChannelCapacity += 0.001f;
        //     }
        // }

        // text3.text = visualChannelCapacity.ToString();


        // channel capacities
        if (isMoved) {
            // partly visual limited
            ChannelLimitStatus[0] = true;
            ChannelLimitStatus[3] = true;
            taskManager.adaptationEvent.Invoke(ChannelLimitStatus);
            isMoved = false;
        }
        if (isGazed) {
            visualChannelCapacity += 0.1f;
            isGazed = false;
        } 


        // adaptation
        // adaptationTime += Time.deltaTime;
        // if (adaptationTime >= 2.0f) {
        //     taskManager.adaptationEvent.Invoke(ChannelLimitStatus);
        //     adaptationTime = 0.0f;
        // }

        adaptationTime += Time.deltaTime;
        if (adapatationEnabled && needAdaptation && adaptationTime > INTERVAL_ADAPTATION) {
            taskManager.adaptationEvent.Invoke(ChannelLimitStatus);
            needAdaptation = false;
            adaptationTime = 0.0f;
        }
        
        // show user height
        text2.text = "isStanding :" + isStanding + " curPosY :" + curUserPos.y + "\n";
    }

    public float GetVolume() {
        if (Microphone.IsRecording(null)) {
            int sampleSize = 128;
            float[] samples = new float[sampleSize];
            int startPosition = Microphone.GetPosition(micName) - (sampleSize + 1);
            audioSource.clip.GetData(samples, startPosition);
            float levelMax = 0;
            for (int i = 0; i < sampleSize; ++i) {
                float wavePeak = samples[i];

                if (levelMax < wavePeak) {
                    levelMax = wavePeak;
                }
            }
            return levelMax * 200;

        }
        return 0;
    }



    public Vector3 getUserDirection() {
        return CoreServices.InputSystem.GazeProvider.GazeDirection;
    }

    public Vector3 getUserPosition() {
        return CoreServices.InputSystem.GazeProvider.GazeOrigin;
    }

    
    // only returns display devices
    // P.S. Need to gaze at least one device when the system is intitialized
    public GameObject getGazingObject(out int deviceNum, out string deviceName) {

        // Test: Solving problem for no device gazed at first time
        // if (isFirstGaze) {
        //     deviceNum = 0;
        //     deviceName = deviceDetector._devices[0].name;
        //     lastDeviceGazed = deviceDetector._devices[0];
        //     isFirstGaze = false;
        //     return lastDeviceGazed;
        // }

        deviceNum = -1;
        deviceName = "";
        if (CoreServices.InputSystem.GazeProvider.GazeTarget) {

            for (int i = 0; i < deviceDetector.getDeviceCount(); i++) {
                GameObject temp = CoreServices.InputSystem.GazeProvider.GazeTarget;
                if (temp == deviceDetector._devices[i]) {

                    deviceNum = i;
                    deviceName = temp.name;
                    // add gaze changed outside this script
                    lastDeviceGazedNum = curDeviceGazedNum;
                    curDeviceGazedNum = i;          // TODO: for u2, 

                    lastDeviceGazed = curDeviceGazed;
                    curDeviceGazed = temp;

                    if (isFirstGaze) {
                        lastDeviceGazedNum = curDeviceGazedNum;
                        isFirstGaze = false;
                    }
                    return temp;
                }
            }
        }
        return curDeviceGazed;
    }

    public GameObject getLastDeviceGazed() {
        return lastDeviceGazed;
    }

//     public void GetFOV() {
// 　　　   = gameObject.GetComponent<Camera>();
// 　　　　float cameraHeight = 2.0f * distance * Mathf.Tan(myCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
// 　　　　var cameraWidth = cameraHeight * myCamera.aspect;
// 　　　　var horizontalfov = 2 * Mathf.Atan(cameraWidth * 0.5f / distance) * Mathf.Rad2Deg;
// 　　　　Debug.LogError("horizontalfov: " + horizontalfov);

//     }


    public void DetectUserMoving() { // TODO: add standing
        // detect user movement
        curUserPos = getUserPosition();
        var displacement = curUserPos - lastUserPos;
        lastUserPos = curUserPos;

        if (isMoving) {
            userMovingTime += Time.deltaTime;
        }
        if (!isMoving && stopMovingTime > 0.0f) {
            stopMovingTime -= Time.deltaTime;
        }

        if (movingTime >= INTERVAL_MOVING) {
            averMoving /= movingFrameCount;
            if (averMoving > MOVING_THRES && isStanding) {
                stopMovingTime = 0.0f;
                isMoving = true;
            } else {
                if (isMoving) {
                    movingCount += 1;
                    if (movingCount >= MOVING_DURATION_COUNT) {
                        isMoved = true;
                        isMoving = false;
                        movingCount = 0;
                        userMovingTime = 0.0f;
                    }
                }
            }

            text2.text = "isMoving :" + isMoving + "\n";
            // text2.text += "displacement :" + displacement + "\n";
            text2.text += "avermag :" + averMoving + "\n";
            text2.text += "isStanding :" + isStanding + " curPosY :" + curUserPos.y + "\n";
            text2.text += "actual moving time :" + userMovingTime;




            averMoving = 0.0f;
            movingFrameCount = 0;
            movingTime = 0.0f;
        } else {
            averMoving += displacement.magnitude;
        }
        movingFrameCount += 1;
        movingTime += Time.deltaTime;
    }

    public void DetectPositionChanged() {
        if (moveTime > INTERVAL_MOVEMENT) {
            // curUserPos = getUserPosition();
            Vector2 currentUserPos = getUserPosition();
            Vector2 displacement = lastUserPosition - currentUserPos;
            
            if (Vector2.Distance(lastUserPosition, currentUserPos) > 0.8f) {
                needAdaptation = true;
            }

            lastUserPosition = currentUserPos;
            moveTime = 0.0f;
        }
        moveTime += Time.deltaTime;

    }

    public void DetectUserStanding() {
        // detect user standing
        curUserPos = getUserPosition();
        if (curUserPos.y >= 0.5) {
            isStanding = true;
            return ;
        }
        isStanding = false;
    }

    public void DetectUserGazing() {
        // gaze area
        if (CoreServices.InputSystem.GazeProvider.GazeTarget.tag == "Gaze Area" || CoreServices.InputSystem.GazeProvider.GazeTarget.tag == "Displays") {
            // gaze device
            if (CoreServices.InputSystem.GazeProvider.GazeTarget.tag == "Displays") {
                GameObject obj = CoreServices.InputSystem.GazeProvider.GazeTarget;
                Device device = obj.GetComponent<Device>();
                if (device.task == null) {
                    return ;
                }
            }

            // Debug.Log("Gazing");
            gazeTime += Time.deltaTime;
            if (gazeTime >= 7.0f && !isGazing) {
                isGazing = true;  
                isGazed = true;   
            }
        } else {
            gazeTime = 0.0f;
            if (isGazing) {
                isGazing = false;
                visualChannelCapacity -= 0.1f;
            }
        }
    }
    public void DetectUserSpeaking() {
        float volume = GetVolume();

        // if user is speaking, isSpeaking turns to false when user stopped speaking for N interval time
        if (speakTime >= INTERVAL_SPEAK) {
            averVolume /= frameCount;

            if (averVolume >= USER_SPEAKING_VALUE) {
                isSpeaking = true;
            } else {
                if (isSpeaking) {
                    speakingCount += 1;
                    if (speakingCount >= SPEAK_DURATION_COUNT) {
                        isSpeaking = false;
                        speakingCount = 0;
                    }
                }
            }


            // text.text = isSpeaking.ToString() + " " + averVolume.ToString();

            // reset value
            averVolume = 0.0f;
            speakTime = 0.0f;
            frameCount = 0;
        } else {
            averVolume += volume;
        }
        speakTime += Time.deltaTime;
        frameCount += 1;
    }

    public void DetectUserDirection() {
        if (directionTime > INTERVAL_DIRECTION) {
            Vector3 currentDirection = getUserDirection();
            Vector3 displacement = lastDirection - currentDirection;
            
            // Debug.Log(displacement.magnitude);
            text3.text = displacement.magnitude.ToString();
            // Debug.Log(displacement.magnitude);
            if (displacement.magnitude > 0.8f) {
                needAdaptation = true;
            } 
            
            lastDirection = currentDirection;
            directionTime = 0.0f;
        }

        directionTime += Time.deltaTime;
    }

    public void DetectEventObjects() {
        // ray ground detection
        // Ray ray = new Ray(getUserPosition(), Vector3.down);
        RaycastHit hit;
        Physics.Raycast(getUserPosition(), Vector3.down, out hit);
        if (hit.collider.tag == "Event Objects") {
            Debug.Log("Event Object detected");
        }
    }




    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
        if (eventData.Pointer is SpherePointer)
        {
            isHandUsing = true;
        }

    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData) {}
    public void OnPointerDragged(MixedRealityPointerEventData eventData) {}
    public void OnPointerUp(MixedRealityPointerEventData eventData) {
        if (eventData.Pointer is SpherePointer)
        {
            StartCoroutine(HandWetCor());      // Debug.Log(eventData.selectedObject.name);
        }
    }

    public IEnumerator HandWetCor() {
        yield return new WaitForSeconds(2);
        isHandUsing = false;
    }

    public IEnumerator ReduceMovingIncrement() {
        yield return new WaitForSeconds(2.0f);
        // for (float i = 0.0f; i > 0.0f; i += )
    }

    // public IEnumerator GazeOver() {
    //     yield return new WaitForSeconds(1.0f);
    //     isGazing = false;
    //     visualChannelCapacity -= 0.1f;
        

    // }
    
    // speech part 
    void IMixedRealitySpeechHandler.OnSpeechKeywordRecognized(SpeechEventData eventData) {
        Debug.Log(eventData.Confidence);
        
    }
}