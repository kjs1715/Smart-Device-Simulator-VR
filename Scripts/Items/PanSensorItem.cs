using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// a little bit different with other tasks
public class PanSensorItem : SensorItem
{
    // Start is called before the first frame update

    int successCount = 0;
    ArrayList foods;
    ArrayList foodsCounted;
    Timer timer;
    void Awake()
    {
        foods = new ArrayList();
        foodsCounted = new ArrayList();
        timer = gameObject.GetComponent<Timer>();
    }

    // Update is called once per frame
    void Update()
    {

    }

//     private void OnCollisionEnter(Collision other) {
//         if (other.gameObject.tag == "Foods") {
//             if (!foods.Contains(other.gameObject) && other.gameObject.GetComponent<Renderer>().material.color == Color.white) {
//                 foods.Add(other.gameObject);
//                 timer.ResetTimer();
//                 timer.CountStart();
//                 Debug.Log("Start boiling!");
//             }

//         }
//     }

//     private void OnCollisionExit(Collision other) {
//         if (other.gameObject.tag == "Foods") {
//             if (!foodsCounted.Contains(other.gameObject) && other.gameObject.GetComponent<Renderer>().material.color == Color.green) {
//                 successCount += 1;
//                 foodsCounted.Add(other.gameObject);
//                 Debug.Log(successCount);
//             }
//         }
//     }
}
