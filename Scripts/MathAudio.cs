using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Security.Cryptography;

public class MathAudio : MonoBehaviour
{

    AudioSource audioSource;
    // Start is called before the first frame update



    // 1-24
    [SerializeField] public AudioClip numClip1;
    [SerializeField] public AudioClip numClip2;
    [SerializeField] public AudioClip numClip3;
    [SerializeField] public AudioClip numClip4;
    [SerializeField] public AudioClip numClip5;


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
        // numClips.Add(numClip5);

        numClips_arr = numClips_arr1;
        // for (int i = 0; i < 4; i++) {
        //     numClips_arr[i] = i;
        // }
        // numClips_arr = GetRandomArray(numClips_arr);
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
        if (changeOrder) {
            if (currentOrder == 0) {
                numClips_arr = numClips_arr1;
            }
            if (currentOrder == 1) {
                numClips_arr = numClips_arr2;
            }
            if (currentOrder == 2) {
                numClips_arr = numClips_arr3;
            }
            currentOrder += 1;
            if (currentOrder > 2) {
                currentOrder = 0;
            }

            changeOrder = false;
        }
    }

    public void StartTask() {
        // TODO: modify number
        // if (currentAudio > 5) {
        //     // random sort, make same color could not appear twice at first
        //     int last = numClips_arr[0];
        //     numClips_arr = GetRandomArray(numClips_arr);
        //     while (last == numClips_arr[0]) {
        //         numClips_arr = GetRandomArray(numClips_arr);
        //     }
        // }
        if (currentAudio > 3) {
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
        yield return new WaitForSeconds(6);
        audioSource.clip = (AudioClip) numClips[numClips_arr[currentAudio]];
        // audioSource.clip = numClip1;
        audioSource.Play();

        // startTask = false;

        // start to record time
        logTimer.StartTime();
    }
    public void Clear() {
        logTimer.StopTime();
        logTimer.time = 0.0f;
        startTask = false;
        checkTime = false;

    }
}
