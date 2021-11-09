using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class Logger {
    // Start is called before the first frame update

    // static public string log = "";

    static public Dictionary<string, string> log = new Dictionary<string, string>();

    static public void AddLog(Dictionary<string, string> temp) {
        log.Clear();
        foreach (KeyValuePair<string, string> pair in temp) {
            log.Add(pair.Key, pair.Value);
        }
    }

    static public Dictionary<string, string> GetLog() {
        return log;
    }

}
