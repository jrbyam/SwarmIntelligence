using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform flock;

    private bool top = false;

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale > 0f) {
            Vector3 center = flock.GetComponent<Flock>().flockCenter;
            if (top) transform.position = new Vector3(center.x, center.y + 50f, center.z);
            else {
                if (transform.position.y > 36f) transform.position = new Vector3(0f, 35f, 0f);
            }
            transform.LookAt(center);
            if (Input.GetKeyDown(KeyCode.C)) {
                top = !top;
            }
        }
    }
}