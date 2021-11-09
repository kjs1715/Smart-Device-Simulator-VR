using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateItem : MonoBehaviour
{
    // Start is called before the first frame update

    public int setGreenBall;
    public int setRedBall;

    public int setWhiteBall;

    int greenBall = 0;
    int redBall = 0;

    int whiteBall = 0;

    List<GameObject> containBalls = new List<GameObject>();

    public bool levelComplete = false;

    // Update is called once per frame
    void Update()
    {
        if (redBall == setRedBall && greenBall == setGreenBall && whiteBall == setWhiteBall) {
            SetComplete();
        }

        if (levelComplete) {
            for (int i = 0; i < containBalls.Count; i++) {
                Destroy(containBalls[i]);
            }
            Destroy(gameObject);
        }
        // Debug.Log(redBall + " " + greenBall);
        
    }

    private void OnCollisionEnter(Collision other) {
        if (!containBalls.Contains(other.gameObject)) {
            if (other.gameObject.GetComponent<Renderer>().material.color == Color.red) {
                redBall += 1;
            } else if (other.gameObject.GetComponent<Renderer>().material.color == Color.green) {
                greenBall += 1;
            } else {
                whiteBall += 1;
            }
            containBalls.Add(other.gameObject);
        }
    }

    private void OnCollisionExit(Collision other) {
        if (containBalls.Contains(other.gameObject)) {
            if (other.gameObject.GetComponent<Renderer>().material.color == Color.red) {
                redBall -= 1;
            } else if (other.gameObject.GetComponent<Renderer>().material.color == Color.green) {
                greenBall -= 1;
            } else {
                whiteBall -= 1;
            }
            containBalls.Remove(other.gameObject);
        }
    }

    public void SetComplete() {
        if (!levelComplete)
            levelComplete = true;
    }

    private void OnDestroy() {
        Debug.Log(gameObject.name + " is completed!");
    }
}
