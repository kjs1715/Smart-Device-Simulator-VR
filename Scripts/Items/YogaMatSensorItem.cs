using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YogaMatSensorItem : SensorItem, IInteractable<bool>
{
    private Transform _transform;

    [SerializeField]
    [Tooltip("Connect a Gameobject transform to calculate distance to it")] 
    Transform _connectedItemTransform;

    [SerializeField]
    [Tooltip("The available distance to the connected item. If the distance is higher than defined, the sensor triggers.")]
    private float _availableDistanceToConnectedItem = 1.5f;
    private float _leaveDistanceToConnectedItem;

    [SerializeField]
    public UserState userState;

    // for starting the yoga task
    private bool yogaReady = false;

    private float _calculatedDistanceToConnectedItem;

    GenericItem _distanceToConnectedItem;

    Task task;

    // Start is called before the first frame update
    void Start()
    {
        _transform = GetComponent<Transform>();
        _leaveDistanceToConnectedItem = _availableDistanceToConnectedItem;
        task = gameObject.GetComponent<Task>();
        // bounds = gameObject.GetComponent<MeshCollider>().bounds;
    }

    // Update is called once per frame
    void Update()
    {
        // _transform.parent = _transform; // ???? maybe wrong
        // Debug.Log("main camera transform :" + _connectedItemTransform.position.x + " " + _connectedItemTransform.position.y + " " + _connectedItemTransform.position.z);
        // Debug.Log("Connected distance :" + _calculatedDistanceToConnectedItem);
        CalculateDictanceToConnectedItem();
    }

    public Transform GetTransform()
    {
        return _transform;
    }

    public Vector3 GetLocation()
    {
        return _transform.position;
    }

    public void SetConnectedItemTransform(Transform newConnectedItemTransform)
    {
        _connectedItemTransform = newConnectedItemTransform;
    }

    public void GetConnectedItemDistance()
    {
        _calculatedDistanceToConnectedItem = (_transform.position - _connectedItemTransform.position).magnitude;
    }

    /// <summary>
    /// Calculate the distance to the connected item. If the distance is higher than defined, the sensor triggers.
    /// </summary>
    public void CalculateDictanceToConnectedItem()
    {
        GetConnectedItemDistance();
        // Debug.Log(_calculatedDistanceToConnectedItem);
        // Debug.Log(userState.isMoving);
        if (_calculatedDistanceToConnectedItem < _availableDistanceToConnectedItem)
        {
            userState.getGazingObject(out int temp, out string str);
            if (!userState.isMoving && !userState.isStanding) { // TODO: Removed yogaReady for test, should add controller trigger for starting? // && !userState.isStanding
                SensorTrigger(gameObject.GetComponent<Task>()); // Sensor triggers if the distance is too big
            }
        }
        // untrigger only the user fara away from the matt
        // if (_calculatedDistanceToConnectedItem >= _leaveDistanceToConnectedItem)
        // {
        //     // Debug.Log(_calculatedDistanceToConnectedItem + " " + _availableDistanceToConnectedItem);
        //     SensorUntrigger(gameObject.GetComponent<Task>()); // Otherwise untriggers
        //     task.isInteracted = false;
        // }
    }

    // public void TestBoundary() {
    //     isUserInBoundary = bounds.Contains(userState.getUserPosition());
    //     Debug.Log(isUserInBoundary);
    // }

    public void SerializeValue(string name = "")
    {
        base.SerializeValue(name + "_availableDistanceToConnectedItemTrigger");
        _distanceToConnectedItem.name = name + "_distanceToConnectedItem";
        _distanceToConnectedItem.state = _calculatedDistanceToConnectedItem.ToString();
        _distanceToConnectedItem.type = "Float";
    }

    // private void OnCollisionEnter(Collision other) {
    //     if (other.gameObject.tag == "Displays") {
    //         yogaReady = true;
    //     }
    // }


    // tablet need yoga task at first
    public void CheckValue(bool yes) {
        if (!isInteractable) return ;

        if (yes) {
            task.PauseVideo();
        } else {
            task.PlayVideo();
        }
        task.isInteracted = false;
    }
}
