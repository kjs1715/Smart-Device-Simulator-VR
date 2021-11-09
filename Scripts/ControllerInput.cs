using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Microsoft.MixedReality;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;



public class ControllerInput : InputSystemGlobalListener
                            , IMixedRealityInputHandler<Vector2>
                            , IMixedRealityFocusHandler
{

    [SerializeField]
    public TMP_Text tmp_text;
    public MixedRealityInputAction moveAction;
    public MixedRealityInputAction triggerAction;

    public bool isHandUsing = false;

    public float speed = 2.0f;

    public DeviceDetector deviceDetector;

    private Vector3 delta = Vector3.zero;

    private Vector3 direction = Vector3.zero;

    private GameObject PointFocusingObj = null;
    private GameObject LastPointFocusingObj = null;

    // TEST

    public TaskEvent taskEvent;

    public ExperimenPlatform platform;

    string t = "";

    // Start is called before the first frame update
    public void OnInputChanged(InputEventData<Vector2> eventData) {
        float horizontal = eventData.InputData.x;
        float vertical = eventData.InputData.y;

        // direction = headYaw * new Vector3(horizontal, 0, vertical);

        string hand = eventData.InputSource.SourceName.Split(' ')[2];

        // for character move
        if (eventData.MixedRealityInputAction == moveAction && hand == "Left") {
            Quaternion headYaw = Quaternion.Euler(0, MixedRealityPlayspace.Transform.rotation.y, 0);
            direction = CameraCache.Main.transform.TransformDirection(new Vector3(horizontal, 0, vertical));
            direction = headYaw * direction;

        }
        // modify 
        if (eventData.MixedRealityInputAction == triggerAction) {

            
        }
    

    }

    // TEST
    // void ReplayVideo(Task task) {
    //     deviceDetector.devices[task.LastDeviceNum].StopSubtitle();
    //     deviceDetector.devices[task.LastDeviceNum].subtitleModule.SetSubPointer(0);
    //     deviceDetector.devices[task.LastDeviceNum].StartSubtitle(task.videoTime);
    // }

    void IMixedRealityFocusHandler.OnFocusEnter(FocusEventData eventData) {
        if (eventData.NewFocusedObject.tag == "Displays" || eventData.NewFocusedObject.tag == "Foods") {
            PointFocusingObj = eventData.NewFocusedObject;
        }
    }

    void IMixedRealityFocusHandler.OnFocusExit(FocusEventData eventData) {
        PointFocusingObj = null;
    }


    void Awake() {
        if (taskEvent == null) {
            taskEvent = new TaskEvent();
        }

    }

    void Update() {
        // OVR requirement
        OVRInput.Update();
        
        Vector3 temp = Vector3.ProjectOnPlane(Time.deltaTime * direction * speed, Vector3.up);
        MixedRealityPlayspace.Position += temp;

        // text.text = "";
        // text.text += t + "\n";
        if (PointFocusingObj != null) {
            // text.text += PointFocusingDevice.name;
        }

        // oculus Input
        ButtonInputHandler();
        
        tmp_text.text = isHandUsing.ToString();
        // MixedRealityPlayspace.Transform.Translate(delta * speed * Time.deltaTime);
    }

    void ButtonInputHandler() {
        // need to point on device
        if (PointFocusingObj != null) {
            // judge hand using
            // if (PointFocusingObj.tag == "Foods") {
            //     if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger)) {
            //         LastPointFocusingObj = PointFocusingObj;
            //         isHandUsing = true;
            //         Debug.Log("Triggered???");
            //     }
            // }
                
            // if (LastPointFocusingObj != null) {
            //     if (LastPointFocusingObj.tag == "Foods") {
            //         if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger)) {
            //             isHandUsing = false;
            //             LastPointFocusingObj = null;
            //             Debug.Log("Triggered!!!");
            //             return ;
            //         }
            //     }
                
            // }



            int deviceNum = -1;
            for (int i = 0; i < deviceDetector.getDeviceCount(); i++) {
                if (PointFocusingObj.name == deviceDetector.devices[i].name) {
                    deviceNum = i;
                    break;
                }
            }
            if (deviceDetector.devices[deviceNum].task == null) return;
            // event for input triggering functions
            if (OVRInput.Get(OVRInput.Button.One)) {
                taskEvent.Invoke(deviceDetector.devices[deviceNum].task, true);
            }
            if (OVRInput.Get(OVRInput.Button.Two)) {
                taskEvent.Invoke(deviceDetector.devices[deviceNum].task, false);
            }
        }
    }
}
