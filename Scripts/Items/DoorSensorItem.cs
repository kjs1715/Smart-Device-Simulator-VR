using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using TMPro;

public class DoorSensorItem : SensorItem
{
    [SerializeField]
    public GameObject movingObject;

    [SerializeField]
    public GameObject doorObject;

    // [SerializeField]
    // public GameObject taskTag;


    [SerializeField]
    public UserState userState;

    [SerializeField]
    public Camera doorCamera;

    [SerializeField]
    public TMPro.TMP_Text text;

    [SerializeField]
    public TMPro.TMP_Text nameTag;

    [SerializeField]
    public Timer timer;

    Vector3 initPos;



    bool isStopped = false;
    bool isStoppedReverse = true;


    string[] tagStr = {"I am Helen", "I am Bob", "I am John"};
    int peopleNum = 0;
    int rightPeopleNum = 2;

    
    private float _availableDistanceToConnectedItem = 1.8f;
    private float _leaveDistanceToConnectedItem = 1.5f;
    private float _calculatedDistanceToConnectedItem;


    Task task;

    float angle;

    void Start()
    {
        task = gameObject.GetComponent<Task>();

        // start position of Knocking cube
        initPos = gameObject.transform.position;
        Debug.Log(initPos.x);

        // nameTag = taskTag.GetComponent<ToolTip>();
        nameTag.text = tagStr[peopleNum];
        

    }

    // Update is called once per frame
    void Update()
    {
        DetectObjectMoving();
        // text.text = "isStopped : " + isStopped + ", isTaskOver : " + task.isTaskOver; 

        // TEST INPUT
        if (Input.GetKeyDown(KeyCode.Space)) {
            CheckValue(true);
        }

        if (timer.CheckTimeOver()) {
            SensorUntrigger(task);
        }
  
    }

    void DetectObjectMoving() {
        if (!isStopped && peopleNum <= rightPeopleNum) {
            MoveObject(-movingObject.transform.right * 3);
            if (movingObject.transform.position.x < 11) {
                nameTag.text = tagStr[peopleNum];
                isStopped = true;
                task.isInteracted = false;
                SensorTrigger(task);
            }
            // CheckValue(false);
            return ;
        } 

        // check object left or not
        Vector3 viewPos = doorCamera.WorldToViewportPoint(movingObject.transform.position);
        // Debug.Log(viewPos);
        if  ((viewPos.x >= 0.3F) && (viewPos.y >= 1.0F)) {
            // Debug.Log("here???");
            SensorUntrigger(task);
        } 
        
        if (!isStoppedReverse) {
            // back to init place, start to move again
            MoveObject(movingObject.transform.right * 3);
            if (movingObject.transform.position.x >= initPos.x) {
                isStoppedReverse = true;
                isStopped = false;
            }
            return ;
        }

  

    }

    void MoveObject(Vector3 direction) {
        movingObject.transform.Translate(direction * Time.deltaTime);
    }


    public void GetConnectedItemDistance()
    {
        _calculatedDistanceToConnectedItem = (doorObject.transform.position - userState.getUserPosition()).magnitude;
        // Debug.Log(_calculatedDistanceToConnectedItem);
    }


    public void CalculateAngle() {
        // Transform[] transforms = DeviceDetector._devices[task.targetDeviceNum].GetComponentsInChildren<Transform>(); // FOR TEST
        // foreach (Transform tf in transforms) {
        //     if (tf.name == "Screen") { 
        //         angle = Vector3.Angle(userState.getUserDirection(), tf.up);
        //         break;
        //     }
        // }
        angle = Vector3.Angle(movingObject.transform.right, userState.getUserDirection());
        // Debug.Log(angle);
    }

    public void CheckValue(bool yes) {
        // if (peopleNum > rightPeopleNum) {
        //     task.SetTaskComplete();  
        //     return ;
        // }
        if (yes && peopleNum == rightPeopleNum) {
            gameObject.GetComponent<Renderer>().material.color = Color.blue;
            task.SetTaskComplete();
            timer.CountStart();
        } else {
            peopleNum += 1;
            isStoppedReverse = false;
            nameTag.text = "No, I am not!";
        }
    }

}
