using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.XR;




/*
    Detect all devices in the environment. All display devices are tagged with "Displays"
*/
public class DeviceDetector : MonoBehaviour{

    // test texture


    public GameObject[] _devices;
    public Device[] devices;
    public GameObject[] _cameras;
    

    TMP_Dropdown _deviceSelector;

    // Material[] _materials;

    public int deviceCount = 0;
    int deviceSelected;

    
    // TEST
    public GameObject tmp;
    TMP_Text text;
    int TESTcount = 0;
    


    void Start() {
        Init();
        // tempoarary
        Material black = Resources.Load<Material>("renderers/screen_black");
        setTextureAll(material: black);
        // DeviceDetector.setTextureAll(ResourcesManager.textRenderTexture);
        // DeviceDetector.FindDeviceSelector(tMP_Dropdown);
        // setDeviceOptions();
    }


    public void Init() {

        // set all devices // TODO: need to sort
        _devices = FindDisplays(); 

        _cameras = FindCameras();
        deviceCount = _devices.Length;
        deviceSelected = deviceCount;   // means all devices, otherelse, the value represents each device

        devices = new Device[deviceCount];
        for (int i = 0; i < deviceCount; i++) {
            devices[i] = _devices[i].GetComponent<Device>();
        }

        //TODO: have to check available or not : DONE
        

        tmp = GameObject.FindGameObjectWithTag("TEST");
        // tmp.TryGetComponent<TMP_Text>(out text);

    }

    private void Update() {
        DetectNotice();
        // Debug.Log(XRDevice.fovZoomFactor);

    }

    public GameObject[] FindDisplays() {
        return GameObject.FindGameObjectsWithTag("Displays").OrderBy(i => i.transform.GetSiblingIndex()).ToArray();
    }

    public GameObject[] FindCameras() {
        return GameObject.FindGameObjectsWithTag("Cameras").OrderBy(i => i.transform.GetSiblingIndex()).ToArray();

    }

    public void FindDeviceSelector(TMP_Dropdown tMP_Dropdown) {
        _deviceSelector = tMP_Dropdown;
    }


/*
    Change texture in single device
*/
    public void setTexture(Material material, int index) {
        if (index == -1) return ;
        devices[index].setTexture(material);
    }

/*
    Change every texture in each device
*/
    public void setTextureAll(Material material) {
        // Debug.Log("asdfasdfadsf");
        // test changing texture
        for (int i = 0; i < deviceCount; i++) {
            devices[i].setTexture(material);
        }
    }

    public void ChangeTexture(int src, int dst) {
        if (src == dst) {
            Debug.Log("Same index in ChangeTexture");
            return ;
        }
        
        Task src_task = devices[src].task;
        Task dst_task = devices[dst].task;


        if (src_task == null && dst_task == null) {
            Debug.Log("src and dst is null");
            return ;
        } else {
            if (src_task == null) {
                ChangeTexture(dst, src);
                return ;
            }
        }


        if (dst_task == null) {
            // just put src into dst device
            if (src_task.isVideo) {
                devices[src].StopSubtitle();

                devices[src].subtitleModule.SaveSubpointer();

                // move audio position
                src_task.audioPos = devices[dst].transform.position;

                // last displayed device
                src_task.LastDeviceNum = dst;

                // change subpointer
                devices[dst].subtitleModule.subPointer = devices[src].subtitleModule.subPointer;
                devices[src].subtitleModule.subPointer = 0;

                // set task
                devices[src].setTask(null);
                devices[dst].setTask(src_task);

                // set sub
                TextAsset asset = src_task.Subtitle;
                devices[dst].subtitleModule.SetSubtitle(asset);
                devices[dst].StartSubtitle(src_task.videoTime);


            }
            if (src_task.isAudio) {
                // move audio position
                src_task.audioPos = devices[dst].transform.position;

                // last displayed device
                src_task.LastDeviceNum = dst;

                // set task
                devices[src].setTask(null);
                devices[dst].setTask(src_task);
            }
            return ;
        }
        // if (src_task.isVideo)
        ChangeTexture_copy(src, dst);
    } 

    // TODO: separate for normal task, audio task and video task, src always be src?
    public void ChangeTexture_copy(int src, int dst) {
        if (src == dst) {
            Debug.Log("Same index in ChangeTexture");
            return ;
        }
        // Task properties should be changed either
        // last device num changed
        // if subtitle module exists, save video process
        if (devices[src].task != null) {
            if (devices[src].task.isVideo) {
                if (devices[src].sub != null) {
                    devices[src].StopSubtitle();
                }
                devices[src].subtitleModule.SaveSubpointer();
            }
            devices[src].task.LastDeviceNum = dst;
        }
        if (devices[dst].task != null) {
            if (devices[dst].task.isVideo) {
                if (devices[dst].sub != null) {
                    devices[dst].StopSubtitle();
                }
            devices[dst].subtitleModule.SaveSubpointer();
            }
            devices[dst].task.LastDeviceNum = src;
        }

        // change subPointer
        if (devices[src].subtitleModule != null && devices[dst].subtitleModule != null) {
            int Tsubpointer = devices[src].subtitleModule.subPointer;
            devices[src].subtitleModule.subPointer = devices[dst].subtitleModule.subPointer;
            devices[dst].subtitleModule.subPointer = Tsubpointer;
        }

        // change task
        Task tempTask = devices[src].task;
        devices[src].setTask(devices[dst].task);
        devices[dst].setTask(tempTask);

        // change subtitle object
        // Device.SubtitleModule subTemp = devices[src].subtitleModule;
        // devices[src].subtitleModule = devices[dst].subtitleModule;
        // devices[dst].subtitleModule = subTemp;

        // change subtitle (need to restart subtitle module)
        if (devices[src].task != null) {
            if (devices[src].task.isVideo) {
                TextAsset asset = devices[src].task.Subtitle;
                devices[src].subtitleModule.SetSubtitle(asset);
                devices[src].StartSubtitle(devices[src].task.videoTime);
            }
        }
        if (devices[dst].task != null) {
            if (devices[dst].task.isVideo) {
                TextAsset asset = devices[dst].task.Subtitle;
                devices[dst].subtitleModule.SetSubtitle(asset);
                devices[dst].StartSubtitle(devices[dst].task.videoTime);
            }
        }

        Debug.Log("Changed...");
    }

    public void DetectNotice() {
        for (int i = 0; i < deviceCount; i++) {
            if (devices[i].noticeOn && devices[i].isVisible) {
                int targetNoticeNum = devices[i].noticingDeviceNum;
                if (devices[targetNoticeNum].task != null) {
                    devices[targetNoticeNum].task.VolumeUp();
                    devices[targetNoticeNum].StopGlint();

                    // turn off notice
                    devices[i].noticeOn = false;
                    devices[i].noticingDeviceNum = -1;
                }
            }
        }
    }
    
    public IEnumerator ChangeTextureForSeconds(int src, int dst) {
        yield return new WaitForSeconds(1.0f);
        ChangeTexture(src, dst);
        yield break;
    }



    // initialize Device selector menu
    public void setDeviceOptions() {        
        // Example();
        for (int i = 0; i < deviceCount; i++) {
            // Debug.Log(_devices[i].name);
            _deviceSelector.options.Add(new TMPro.TMP_Dropdown.OptionData() {text = devices[i].name});
        }
        _deviceSelector.options.Add(new TMPro.TMP_Dropdown.OptionData() {text = "All"});

    }

    public int getDeviceSelected() {
        deviceSelected = _deviceSelector.value;
        Debug.Log(deviceSelected);
        return deviceSelected;
    }

    public int getDeviceCount() {
        return deviceCount;
    }

 



}

