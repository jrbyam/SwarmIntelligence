using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform flock;

    private bool top = true;

    // Update is called once per frame
    void Update()
    {
        Vector3 center = flock.GetComponent<Flock>().flockCenter;
        //transform.position = new Vector3(center.x + (top ? 0f : 50), center.y + (top ? 50f : 0f), center.z);//new Vector3(centerX + 20, centerY + 5, centerZ + 20);
        transform.LookAt(center);

        if (Input.GetKeyDown(KeyCode.C)) {
            top = !top;
        }
    }
}