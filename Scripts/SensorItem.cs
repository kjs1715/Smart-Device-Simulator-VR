using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// Basic item, operates with two events:
/// Sensor trigger and untrigger
/// </summary>
public class SensorItem : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Action performed after sensor is triggered")]
    private SensorEvent m_OnSensorTriggered;
    [SerializeField]
    [Tooltip("Action performed after sensor is triggered")]
    private SensorEvent m_OnSensorUntriggered;

    [SerializeField]
    protected bool isInteractable = false;


    GenericItem _sensorTriggerItem;
    bool _isSensorTriggered = false;

    // Start is called before the first frame update
    void Start()
    {
        if (m_OnSensorTriggered == null)
            m_OnSensorTriggered = new SensorEvent();
        if (m_OnSensorUntriggered == null)
            m_OnSensorUntriggered = new SensorEvent();
    }

    void SensorTriggered(Task task)
    {
        _isSensorTriggered = true;
        task.onTaskTriggered = true;
        m_OnSensorTriggered?.Invoke(task);
        // task.ToString();
    }

    void SensorUntriggered(Task task)
    {
        _isSensorTriggered = false;
        task.onTaskTriggered = false;
        m_OnSensorUntriggered?.Invoke(task);
        // Debug.Log("Untriggered");
        // task.ToString();
    }

    public void SensorTrigger(Task task)
    {
        if (!_isSensorTriggered) SensorTriggered(task);
    }

    public void SensorUntrigger(Task task)
    {
        if (_isSensorTriggered) SensorUntriggered(task);
    }


    /// <summary>
    /// save the parameters inside the generic item
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name"></param>
    public void SerializeValue(string name = "sensorItem", string type = "Trigger")
    {
        _sensorTriggerItem.state = (_isSensorTriggered ? "ON" : "OFF");
        _sensorTriggerItem.type = type;
        _sensorTriggerItem.name = name;
    }
}

