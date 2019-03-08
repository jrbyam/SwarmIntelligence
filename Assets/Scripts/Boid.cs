using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    public float maxVelocity;
    public float minVelocity;
    float maxAcceleration;
    float maxPitch; // Stored as a positive number, restricted in positive and negative directions
    float maxRoll;
    float maxYaw;

    public Vector3 v = Vector3.zero;
    public Vector3 a = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // if (transform.position.y < 10) {
        //     LevelOut();
        // }
    }

    private void LevelOut() {
        Quaternion target = Quaternion.Euler(new Vector3(0f, transform.localEulerAngles.y, 0f));
        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime);
    }
}
