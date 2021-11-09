using UnityEngine;


/// <summary>
/// Provides a sensor trigger functionality 
/// based on the distance to the connected Gameobject
/// </summary>
public class LocationSensorItem : SensorItem
{
    private Transform _transform;
    [SerializeField]
    [Tooltip("Connect a Gameobject transform to calculate distance to it")] Transform _connectedItemTransform;
    [SerializeField]
    [Tooltip("The available distance to the connected item. If the distance is higher than defined, the sensor triggers.")]
    private float _availableDistanceToConnectedItem = 1.0f;
    private float _calculatedDistanceToConnectedItem;

    GenericItem _distanceToConnectedItem;

    // Start is called before the first frame update
    void Start()
    {
        _transform = GetComponent<Transform>();
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
        if (_calculatedDistanceToConnectedItem < _availableDistanceToConnectedItem)
        {
            // SensorTrigger(); // Sensor triggers if the distance is too big
        }
        else
        {
            // Debug.Log(_calculatedDistanceToConnectedItem + " " + _availableDistanceToConnectedItem);
            // SensorUnrigger(); // Otherwise untriggers
        }
    }

    public void SerializeValue(string name = "")
    {
        base.SerializeValue(name + "_availableDistanceToConnectedItemTrigger");
        _distanceToConnectedItem.name = name + "_distanceToConnectedItem";
        _distanceToConnectedItem.state = _calculatedDistanceToConnectedItem.ToString();
        _distanceToConnectedItem.type = "Float";
    }
}

