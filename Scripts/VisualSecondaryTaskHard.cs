using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Security.Cryptography;

public class VisualSecondaryTaskHard : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] public TMPro.TMP_Text text1;
    [SerializeField] public TMPro.TMP_Text text2;
    [SerializeField] public TMPro.TMP_Text text3;
    [SerializeField] public TMPro.TMP_Text text4;

    TMPro.TMP_Text[] texts;

    bool isStarted = false;
    float visualTime = 5.0f;
    float timeInterval = 5.0f;

    string text = "";

    string[] Alphabet = {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
    int[] char_arr;
    
    int charCount = 0;
    int TextCount = 0;
    int currentActive = -1;

    void Start()
    {
        char_arr = new int[26];
        for (int i = 0; i < 26; i++) {
            char_arr[i] = i;
        }
        char_arr = GetRandomArray(char_arr);

        texts = new TMPro.TMP_Text[4];
        texts[0] = text1;
        texts[1] = text2;
        texts[2] = text3;
        texts[3] = text4;


        ChangeTextTMP();
    }

    // Update is called once per frame
    void Update()
    {
        if (isStarted) {
            visualTime += Time.deltaTime;
            if (visualTime >= timeInterval) {
                text = GenerateRandomAlphanumericString();

                charCount += 1;
                TextCount += 1;

                if (TextCount >= 4) {
                    ChangeTextTMP();
                    TextCount = 0;
                }
                texts[currentActive].text = text;


                if (charCount > 25) {
                    char_arr = GetRandomArray(char_arr);
                    charCount = 0;
                }
                visualTime = 0.0f;
            }
        }

    }

    public void StartTask() {
        isStarted = true;
        text1.text = "";
        text2.text = "";
        text3.text = "";
        text4.text = "";
    }

    public void StopTask() {
        isStarted = false;
        text1.text = "";
        text2.text = "";
        text3.text = "";
        text4.text = "";
    }


    void ChangeTextTMP() {
        currentActive += 1;
        if (currentActive > 3) {
            currentActive = 0;
        }
        text1.text = "";
        text2.text = "";
        text3.text = "";
        text4.text = "";

    }

    public string GenerateRandomAlphanumericString(int length = 13)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    
        var random       = new System.Random();
        var randomString = new string(Enumerable.Repeat(chars, length)
                                                .Select(s => s[random.Next(s.Length)]).ToArray());
        return randomString;
    }

    int[] GetRandomArray(int[] MyList) {
        System.Random random = new System.Random();
        int[] newList = MyList.OrderBy(x => random.Next()).ToArray();
        
        return newList;
    }
}
