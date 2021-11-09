using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogTimer : MonoBehaviour
{
    // Start is called before the first frame update

    public Dictionary<string, string> exportDataLog = new Dictionary<string, string>();

    public bool isTicking = false;
    public float time = 0.0f;

    int[] count = new int[2];

    Dictionary<string, string>[] CheckpointTime = new Dictionary<string, string>[2];
    Dictionary<string, string> LogTime = new Dictionary<string, string>();

    void Start()
    {
        for (int i = 0; i < 2; i++) {
            CheckpointTime[i] = new Dictionary<string, string>();
            count[i] = -1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isTicking) {
            time += Time.deltaTime;
        }
    }

    public void StartTime() {
        // LogTime.Clear();
        isTicking = true;
    }
    
    public void StopTime() {
        isTicking = false;
    }

    public void Checkpoint(int index, string prefix = "") {
        StopTime();
        count[index] += 1;
        CheckpointTime[index][prefix + count[index].ToString()] = time.ToString();
        exportDataLog[prefix + count[index].ToString()] = time.ToString();
    
        time = 0.0f;
    }

    public Dictionary<string, string> Exportdic() {
        for (int i = 0; i < 2; i++) {
            foreach (KeyValuePair<string, string> pair in CheckpointTime[i]) {
                LogTime[pair.Key] = pair.Value;
            }
            CheckpointTime[i].Clear();

            count[i] = -1;
        }
        exportDataLog.Clear();
        return LogTime;
    }

    public float GetTime() {
        return time;
    }

    // public override string ToString()
    // {
    //     string str = "";
    //     str = "Total time :" + time + "\n";
    //     for (int i = 0; i < CheckpointTime.Count; i++) {
    //         str += "Check " + i + " :" + CheckpointTime[i] + "\n";
    //     }
    //     return str;
    // }


}
