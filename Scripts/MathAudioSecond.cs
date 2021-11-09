using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Security.Cryptography;

public class MathAudioSecond : MonoBehaviour
{

    AudioSource audioSource;
    // Start is called before the first frame update



    // 1-24
    [SerializeField] public AudioClip numClip1;
    [SerializeField] public AudioClip numClip2;
    [SerializeField] public AudioClip numClip3;
    [SerializeField] public AudioClip numClip4;
    [SerializeField] public AudioClip numClip5;
    [SerializeField] public AudioClip numClip6;
    [SerializeField] public AudioClip numClip7;
    [SerializeField] public AudioClip numClip8;
    [SerializeField] public AudioClip numClip9;
    [SerializeField] public AudioClip numClip10;
    [SerializeField] public AudioClip numClip11;
    [SerializeField] public AudioClip numClip12;
    [SerializeField] public AudioClip numClip13;
    [SerializeField] public AudioClip numClip14;
    [SerializeField] public AudioClip numClip15;
    [SerializeField] public AudioClip numClip16;
    [SerializeField] public AudioClip numClip17;
    [SerializeField] public AudioClip numClip18;
    [SerializeField] public AudioClip numClip19;
    [SerializeField] public AudioClip numClip20;
    [SerializeField] public AudioClip numClip21;
    [SerializeField] public AudioClip numClip22;
    [SerializeField] public AudioClip numClip23;
    [SerializeField] public AudioClip numClip24;


    public LogTimer logTimer;

    ArrayList numClips;
    int[] numClips_arr;

    int clipCount = 0;

    int currentAudio = 0;
    int currentOrder = 0;

    public bool startTask = false;
    public bool checkTime = false;
    public bool exportDic = false;

    public bool changeOrder = false;

    
    int[] numClips_arr1 = {0, 2, 3, 1};
    int[] numClips_arr2 = {3, 2, 0, 1};
    int[] numClips_arr3 = {1, 0, 2, 3};

    // 0231
    // 3201
    // 1023

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        numClips = new ArrayList();
        
        numClips.Add(numClip1);
        numClips.Add(numClip2);
        numClips.Add(numClip3);
        numClips.Add(numClip4);
        numClips.Add(numClip5);
        numClips.Add(numClip6);
        numClips.Add(numClip7);
        numClips.Add(numClip8);
        numClips.Add(numClip9);
        numClips.Add(numClip10);
        numClips.Add(numClip11);
        numClips.Add(numClip12);
        numClips.Add(numClip13);
        numClips.Add(numClip14);
        numClips.Add(numClip15);
        numClips.Add(numClip16);
        numClips.Add(numClip17);
        numClips.Add(numClip18);
        numClips.Add(numClip19);
        numClips.Add(numClip20);
        numClips.Add(numClip21);
        numClips.Add(numClip22);
        numClips.Add(numClip23);
        numClips.Add(numClip24);
        // numClips.Add(numClip5);

        numClips_arr = new int[25];
        for (int i = 0; i < 24; i++) {
            numClips_arr[i] = i;
        }
        numClips_arr = GetRandomArray(numClips_arr);
    }

    // Update is called once per frame
    void Update()
    {
        if (startTask) {
            StartTask();
        }

        if (checkTime) {
            CheckTime();
        }

        if (exportDic) {
            ExportLog();
        }


        // 

    }

    public void StartTask() {
        // TODO: modify number
        if (currentAudio > 23) {
            numClips_arr = GetRandomArray(numClips_arr);
            currentAudio = 0;
        }

        if (logTimer.isTicking) {
            return ;
        }

        StartCoroutine(startAudio());
        
    }


    public void CheckTime() {
        logTimer.Checkpoint(0);
        audioSource.Pause();

        currentAudio += 1;
        checkTime = false;
    }

    public void ExportLog() {
        if (exportDic) {
            Logger.AddLog(logTimer.Exportdic());
            currentAudio = 0;
            numClips_arr = GetRandomArray(numClips_arr);
            exportDic = false;
        }

    }

    int[] GetRandomArray(int[] MyList) {
        System.Random random = new System.Random();
        int[] newList = MyList.OrderBy(x => random.Next()).ToArray();
        
        return newList;
    }

    IEnumerator startAudio() {
        logTimer.StartTime();
        yield return new WaitForSeconds(7);
        audioSource.clip = (AudioClip) numClips[numClips_arr[currentAudio]];
        // audioSource.clip = numClip1;
        audioSource.Play();

        // startTask = false;

        // start to record time
        
    }
    public void Clear() {
        logTimer.StopTime();
        logTimer.time = 0.0f;
    }
}
