using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;

public class Device : MonoBehaviour
{
    public class SubtitleModule : MonoBehaviour{


        public TMPro.TMP_Text text1;
        public TMPro.TMP_Text text2;
        public Vector3 originalPosText1;
        public Vector3 originalPosText2;

        // IMPORTANT: solved when changing tasks, subtitle enumerates from the first subtitle
        public int subPointer = 0;
        public GameObject subtitleObject;

        public SubtitleDisplayer subtitleDisplayer;
        public bool isPlaying = false;

        // position adjusting
        public bool isAdjusted = false;

        // text size adjusting
        public bool isTextSizeAdjusted = false;

        // for adjust
        public Vector3 centerPos;

        public SubtitleModule() {

        }
        public SubtitleModule(TMPro.TMP_Text[] texts) {
            text1 = texts[0];
            text2 = texts[1];
            subtitleObject = text1.transform.parent.gameObject;

            originalPosText1 = text1.gameObject.transform.position;
            originalPosText2 = text2.gameObject.transform.position;
        }

        public void SetSubtitleDisplayer(SubtitleDisplayer obj) {
            if (obj != null) {
                subtitleDisplayer = obj;
                ConnectSubtitleModule();
            }
        }

        public void SetCenterPos(Vector3 pos) {
            centerPos = pos;
        }

        public void ConnectSubtitleModule() {
            subtitleDisplayer.Text = text1;
            subtitleDisplayer.Text2 = text2;
        }

        public TMPro.TMP_Text GetSubText1() {
            return text1;
        }
        public TMPro.TMP_Text GetSubText2() {
            return text2;
        }


        public void SetSubtitle(TextAsset textAsset) {
            subtitleDisplayer.SetSubtitleAsset(textAsset);
            subtitleDisplayer.SetParser();
            if (subPointer != -1) {
                SetSubPointer(subPointer);
            }
        }

        // use to set subpointer in subtitledisplayer.parser
        public void SetSubPointer(int subPointer) {
            subtitleDisplayer.parser.subPointer = subPointer;
        }

        // save subtitleDisplayer.parser.subPointer into sub module
        public void SaveSubpointer() {
            if (subtitleDisplayer != null && subtitleDisplayer.parser != null) {
                // Debug.Log(subPointer);
                // Debug.Log(subtitleDisplayer.parser.subPointer);
                // Debug.Log("Saved?");
                subPointer = subtitleDisplayer.parser.subPointer;
            }
        }

        public void CloseSub(bool value) {
            if (value) {
                text1.gameObject.SetActive(false);
                text2.gameObject.SetActive(false);
            } else {
                text1.gameObject.SetActive(true);
                text2.gameObject.SetActive(true);  
            }
        }

        public void CleanTexts() {
            text1.text = "";
            text2.text = "";
        }





    }

    [HideInInspector] public Task task;
    [HideInInspector] public Material texture;
    [HideInInspector] public bool isAvailable;
    [HideInInspector] public bool isVisible;

    [HideInInspector] public float Visibility = 0.0f;
    [HideInInspector] public float Size = 0.0f;


    [HideInInspector] public float disToUser;

    [HideInInspector] public string deviceName;
    [HideInInspector] public string textureName;

    [HideInInspector] public MeshRenderer meshRenderer;

    [HideInInspector] public Skode_Glinting sg;
    [HideInInspector] public bool isGlinting = false;

    [HideInInspector] public GameObject m_screen;

    [HideInInspector] public SubtitleModule subtitleModule;
    [HideInInspector] public Coroutine sub;

    [HideInInspector] public Material backupTexture;

    [HideInInspector] public bool noticeOn = false;
    [HideInInspector] public int noticingDeviceNum = -1;

    [HideInInspector] public AudioSource m_audio_player;     


    // context variable
    [HideInInspector] public int deviceSize;

    [HideInInspector] public float device_actual_size;

    
    [HideInInspector] public float[] deviceSizeRates = {0.2f, 0.4f, 0.6f, 0.8f};

    [HideInInspector] public bool subPlaying = false;

    [HideInInspector] public bool isSoundAdjusted = false;
    [HideInInspector] public bool isClosedVideo = false;
    [HideInInspector] public bool isChangedKeysub = false;
    [HideInInspector] public bool isSlowed = false;


    public bool RandomSize = false;

    void Awake()
    {
        SetMeshRenderer();

        deviceName = gameObject.name;
        task = null;
        texture = ResourcesManager.emptyScreenTexture;
        isAvailable = true;
        isVisible = false;

        // setTexture(texture); // default screen

        // get glint object
        sg = m_screen.GetComponent<Skode_Glinting>();

        // set subtitle module (instead of phone)
        TMPro.TMP_Text[] subtexts = m_screen.GetComponentsInChildren<TMPro.TMP_Text>();
        subtitleModule = new SubtitleModule(subtexts);
        subtitleModule.SetSubtitleDisplayer(gameObject.GetComponentInChildren<SubtitleDisplayer>());

        subtitleModule.SetCenterPos(m_screen.transform.position);


        // screen size in float
        Vector2 originSize = m_screen.GetComponent<MeshFilter>().mesh.bounds.size;
        float size_x = originSize.x * m_screen.transform.localScale.x * gameObject.transform.localScale.x;
        float size_y = originSize.y * m_screen.transform.localScale.y * gameObject.transform.localScale.x;
        device_actual_size = size_x * size_y * Mathf.Pow(10, 16);

        // set audio outptu
        // gameObject.AddComponent<AudioSource>();
        m_audio_player = GetComponent<AudioSource>();
        m_audio_player.playOnAwake = false;

    }

    private void Start() {
        // position
        // Size = deviceSizeRates[deviceSize-1];
    }

    // Update is called once per frame
    void Update()
    {

        textureName = texture.name; 
        
        // CheckAvailability(); 
        CheckVisibility();

        // when video paused, close subtitle
        if (task != null) {
            if (task.isVideo && subtitleModule.isPlaying) {
                subtitleModule.subtitleDisplayer.startTime = task.videoTime;
                // if (video)
                // subtitleModule.SetSubPointer(0);
            }

            // Debug.Log(subtitleModule.subtitleDisplayer.startTime);
        }


        if (!subtitleModule.isPlaying) {
            subtitleModule.CleanTexts();
        }
    
        // update sub text size
        if (subtitleModule.isTextSizeAdjusted) {
            // if (disToUser)
            // Debug.Log(disToUser);
            if (disToUser <= 3.2f && disToUser >= 1.2f) {
                float scale = 1.1f - 0.222f * (3.2f - disToUser);
                float offset = 0.95f * (3.2f - disToUser);
                subtitleModule.subtitleObject.transform.localScale = new Vector3(scale, scale, scale);
                subtitleModule.subtitleObject.transform.localPosition = new Vector3(0, 0, offset);
            }

        }
        
        if (isSoundAdjusted) {
            // if (disToUser <= 4.5f && disToUser >= 1.2f) {
                // float new_volume = 1 - 0.2424f * (4.5f - disToUser);
                // m_audio_player.volume = new_volume;
            if (disToUser <= 5.5f && disToUser >= 1.2f) {
                m_audio_player.volume = 1.0f;
            }
            // m_audio_player.volume = 1.0f; // TODO: 
        }
        

    }

    void SetMeshRenderer() {
        MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in meshRenderers) {
            // set availability
            if (mr.name == "Screen") {
                meshRenderer = mr;
                // init device gameobject
                m_screen = mr.gameObject;
                // Debug.Log("set?");
                Debug.Log(meshRenderer);
            }
        }

    }

    public void CheckAvailability() {
        if (task == null) {
            isAvailable = true;
        } else {
            isAvailable = false;
        }
    }


    public void CheckVisibility() {
    
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(CameraCache.Main);
        isVisible = GeometryUtility.TestPlanesAABB(planes, meshRenderer.bounds);
        
        if (!isVisible) {
            Visibility = 0.0f;
            return ;
        }
        Vector3 screenPoint = CameraCache.Main.WorldToViewportPoint(m_screen.transform.position);
        
        Vector2 centerPoint = new Vector2(0.5f, 0.5f);
        Vector2 objPoint = new Vector2(screenPoint.x, screenPoint.y);
        // Debug.Log(deviceName + " x, y, z : " + screenPoint.x + " " + screenPoint.y + " " + screenPoint.z);
        if (deviceName == "TV" || deviceName == "Tablet") {
            // Debug.Log(deviceName + " " +Visibility);
        }

        JudgeObjInViewport(objPoint, centerPoint);

        // if ()
        // isVisible = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y < 1 && screenPoint.y > 0;


    }

    public void JudgeObjInViewport(Vector2 point, Vector2 center) {

        if (point.x > 0.4f && point.x < 0.6f && point.y < 0.6f && point.y > 0.4f) {
            Visibility = 1.00f;
            return ;
        }
        if (point.x > 0.3f && point.x < 0.7f && point.y < 0.7f && point.y > 0.3f) {
            Visibility = 0.75f;
            return ;
        }
        if (point.x > 0.2f && point.x < 0.8f && point.y < 0.8f && point.y > 0.2f) {
            Visibility = 0.5f;
            return ;
        }
        if (point.x > 0.1f && point.x < 0.9f && point.y < 0.9f && point.y > 0.1f) {
            Visibility = 0.25f;
            return ;
        }
        Visibility = 0.00f;






    //    if ((point.x > 0.0f && point.x <= 0.1) || (point.x <= 0.9f && point.x < 1.0f) && (point.y > 0.0f && point.y <= 0.1f) || (point.y <= 0.9f && point.y < 1.0f)) {
    //         Visibility = 0.25f;
    //     } else if ((point.x > 0.1f && point.x <= 0.2f) || (point.x <= 0.8f && point.x < 0.9f) && (point.y > 0.1f && point.y <= 0.2f) || (point.y <= 0.8f && point.y < 0.9f)) {
    //         Visibility = 0.5f;
    //     } else if ((point.x > 0.2f && point.x <= 0.3f) || (point.x <= 0.7f && point.x < 0.8f) && (point.y > 0.2f && point.y <= 0.3f) || (point.y <= 0.7f && point.y < 0.8f)) {
    //         Visibility = 0.75f;
    //     } else if ((point.x > 0.3f && point.x <= 0.4f) || (point.x <= 0.6f && point.x < 0.7f) && (point.y > 0.3f && point.y <= 0.4f) || (point.y <= 0.6f && point.y < 0.7f)) {
    //         Visibility = 1.00f;
    //     } else {
    //         Visibility = 0.00f;
    //     }
    }

    public void setTask(Task task) {
        this.task = task;
        if (task == null) {
            setTexture(ResourcesManager.emptyScreenTexture);
            this.textureName = ResourcesManager.emptyScreenTexture.name;
            return ;
        }
        // Material texture = deviceName == "Phone" ? task.phone_texture : task.texture;
        // setTexture(texture);
    }

    public void setAudioCilp(double time, AudioClip clip) {
        m_audio_player.clip = clip;
        // Debug.Log(clip);
        m_audio_player.time = (float) time;
        m_audio_player.volume = 1.0f;
        // m_audio_player.Play();
    }

    public void setTexture(Material material) {
        meshRenderer.material = this.texture = material;    
        this.textureName = this.texture.name;
    }

    public void Glint() {
        if (sg != null && !isGlinting) {
            sg.StartGlinting();
            Debug.Log("GM: " + sg.gameObject);
            isGlinting = true;
        }
    }

    public void StopGlint() {
        if (sg != null && isGlinting) {
            sg.StopGlinting();
            isGlinting = false;
        }
    }

    public void StartSubtitle(float startTime) {
        subtitleModule.subtitleDisplayer.startTime = startTime;
        sub = StartCoroutine(subtitleModule.subtitleDisplayer.Begin());
        subtitleModule.isPlaying = true;
    }

    public void StopSubtitle() {
        if (sub != null) {
            StopCoroutine(sub);
            subtitleModule.CleanTexts();
            // subtitleModule.SetSubtitle(null);
            subtitleModule.isPlaying = false;
        }
    }

    // public void ResetFeedback() {
    //     if (task != null) {

    //     }
    // }

    public void CloseVideo(bool value) {
        if (value) {
            backupTexture = texture;
            texture = ResourcesManager.emptyScreenTexture;
            isClosedVideo = true;
        } else {
            texture = backupTexture;
            isClosedVideo = false;
        }
        setTexture(texture);
        // subtitleModule.AdjustPostion();
    }


    public void changeSub(bool value) {
        if (task != null && value) {
            if (task.Key_Subtitle != null) {
                subtitleModule.SaveSubpointer();
                StopSubtitle();
                subtitleModule.subPointer = task.subPointer;
                subtitleModule.SetSubtitle(task.Key_Subtitle);
                isChangedKeysub = true;
                // StartSubtitle(task.videoTime);

            }
        }

        if (task != null && !value) {
            if (task.Subtitle != null) {
                subtitleModule.SaveSubpointer();
                StopSubtitle();
                subtitleModule.subPointer = task.subPointer;
                subtitleModule.SetSubtitle(task.Subtitle);
                isChangedKeysub = false;
                // StartSubtitle(task.videoTime);
            }
        }
    }

    public void AdjustPostion(bool value) {
        // Debug.Log(text1.transform.position);
        if (value) {
            Vector3 moveDir = m_screen.transform.up * 0.005f;
            subtitleModule.text1.transform.position = Vector3.Lerp(m_screen.transform.position, -m_screen.transform.forward, 0.00f);
            subtitleModule.text1.transform.position += moveDir;

            subtitleModule.text2.transform.position = Vector3.Lerp(m_screen.transform.position, -m_screen.transform.forward, 0.00f);
            subtitleModule.text2.transform.position += moveDir;
        } else {
            subtitleModule.text1.transform.position = subtitleModule.originalPosText1;
            subtitleModule.text2.transform.position = subtitleModule.originalPosText2;

        }
        // isAdjusted = !isAdjusted;
    }

    public void closeSub(bool value) {
        // Debug.Log(subtitleModule.text1.transform.position);
        if (value) {
            subtitleModule.text1.gameObject.SetActive(false);
            subtitleModule.text2.gameObject.SetActive(false);

            // text1.transform.position = new Vector3(text1.transform.position.x, centerPos.y, text1.transform.position.z);
            // text2.transform.position = new Vector3(text2.transform.position.x , centerPos.y, text2.transform.position.z);

        } else {
            subtitleModule.text1.gameObject.SetActive(true);
            subtitleModule.text2.gameObject.SetActive(true);
        }
        // isAdjusted = !isAdjusted;
    }


    public void PlayAudio() {
        m_audio_player.Play();
    }

    public void PauseAudio() {
        m_audio_player.Pause();
    }
    public void VolumeDown() {
        m_audio_player.volume = 0.2f;
    }
    public void VolumeUp() {
        m_audio_player.volume = 1.0f;

    }

    public void Mute() {
        m_audio_player.volume = 0.0f;
        
    }

    public void Unmute() {
        m_audio_player.volume = 1.0f;
        
    }
    public void AdjustSound(bool value) {
        if (value) {
            isSoundAdjusted = true;
        } else {
            isSoundAdjusted = false;
            m_audio_player.volume = 1.0f;
        }
    }
}
