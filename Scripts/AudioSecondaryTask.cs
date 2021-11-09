using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Security.Cryptography;

public class AudioSecondaryTask : MonoBehaviour
{
    // Start is called before the first frame update

    AudioSource audioSource;

    bool isStarted = false;
    float audioTime = 5.0f;
    float timeInterval = 5.0f;

    // 1-10
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

    ArrayList numClips;
    int[] numClips_arr;

    int clipCount = 0;



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

        numClips_arr = new int[10];
        for (int i = 0; i < 10; i++) {
            numClips_arr[i] = i;
        }
        numClips_arr = GetRandomArray(numClips_arr);

    }

    // Update is called once per frame
    void Update()
    {
        if (isStarted) {
            audioTime += Time.deltaTime;
            if (audioTime >= timeInterval) {
                audioSource.clip = (AudioClip) numClips[numClips_arr[clipCount]];
                audioSource.Play();
                clipCount += 1;

                if (clipCount > 9) {
                    numClips_arr = GetRandomArray(numClips_arr);
                    clipCount = 0;
                }
                audioTime = 0.0f;
            }
        }

    }

    public void StartTask() {
        isStarted = true;
        audioSource.Stop();
    }

    public void StopTask() {
        isStarted = false;
        audioSource.Stop();
    }

    int[] GetRandomArray(int[] MyList) {
        System.Random random = new System.Random();
        int[] newList = MyList.OrderBy(x => random.Next()).ToArray();
        
        return newList;
    }
}
