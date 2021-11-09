using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class GrabEventHandler : MonoBehaviour
{
    [SerializeField]
    public GameObject userState_obj;

    UserState userState;


    void Start() {
        userState = userState_obj.GetComponent<UserState>();
    }


    public void Grabbed(ManipulationEventData eventData) {
        // Debug.Log(eventData.ManipulationSource.name);
        userState.grabbedDevice = eventData.ManipulationSource;
        userState.isHandUsing = true;
    }

    public void NotGrabbed() {
        StartCoroutine(userState.HandWetCor());
    }
}
