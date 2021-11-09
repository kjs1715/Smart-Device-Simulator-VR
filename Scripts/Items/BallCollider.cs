using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCollider : MonoBehaviour
{
    // Start is called before the first frame update

    public TMPro.TMP_Text text;
    private void OnCollisionEnter(Collision other) {
        Debug.Log(other.gameObject.name);
        text.text = other.gameObject.name;
        if (other.gameObject.name == "") {

        }
    }
}
