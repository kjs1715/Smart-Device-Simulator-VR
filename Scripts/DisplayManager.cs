using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;

public class DisplayManager : MonoBehaviour
{
    // [SerializeField]
    // public Material texture1;

    // [SerializeField]
    // public Material texture2;

    // [SerializeField]
    // public Material texture3;

    // [SerializeField]
    // public Material videoTexture;

    [SerializeField]
    public TMPro.TMP_Dropdown tMP_Dropdown;

    [SerializeField]
    public GameObject userState;

    [SerializeField]
    public GameObject taskManager;

    /// <summary>
    /// Wait for host call
    /// </summary>


    [SerializeField]
    public ContextManager contextManager;
    ResourcesManager resourcesManager;



    // variables

    Material callingMaterialPhone;




    // Start is called before the first frame update

    // Display Detector
    // DeviceDetector deviceDetector;

    void Awake() {
        InitResourcesManager();
        // InitDeviceDetector();
        InitContextManager();

        // Only call in HMD test
        AdjustUserHeight(false);

        // Evaluation scene
        // AdjustScene(true);   
    }

    void Start()
    {
  
    }

    // Update is called once per frame
    void Update()
    {

        // Debug.Log(contextManager.testFactor);

    }

/*
    Initialize Device Detector, mainly several parts 
        -> 1. Find all devices with tag 
        -> 2. Initial texture state of all devices 
        -> 3. Init device menu
*/
    // void InitDeviceDetector() {
    //     DeviceDetector.Init();
    //     DeviceDetector.setTextureAll(material: ResourcesManager.emptyScreenTexture);
    //     // DeviceDetector.setTextureAll(ResourcesManager.textRenderTexture);
    //     DeviceDetector.FindDeviceSelector(tMP_Dropdown);
    //     DeviceDetector.setDeviceOptions();

    // }

    void InitContextManager() {
        // contextManager = new ContextManager(userState, taskManager);       
    }

    void InitResourcesManager() {
        resourcesManager = new ResourcesManager();
    }

    void AdjustDropdownMenu() {
        
    }

    void AdjustUserHeight(bool value) {
        if (value) {
            MixedRealityPlayspace.Position -= new Vector3(-1.5f, 0.7f, -0.4f);
            // MixedRealityPlayspace.Position -= new Vector3(-1.5f, 1.0f, -0.4f);
        } else {
            MixedRealityPlayspace.Position -= new Vector3(-1.5f, 0.61f, -0.4f);
        }
    }

    void AdjustScene(bool Value) {
        if (Value) {
            MixedRealityPlayspace.Position -= new Vector3(1000.0f, 0.0f, -0.4f);
        }
    }
}


