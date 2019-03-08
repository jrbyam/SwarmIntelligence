using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    public Vector3 flockCenter;
    public int flockSize;
    public GameObject boidTemplate;
    private float minDistance = 1f;
    public Vector3 goal;
    public Transform sphere;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < flockSize; ++i) {
            Vector3 position = new Vector3(Random.Range(0.0f, 10.0f), Random.Range(0.0f, 10.0f), Random.Range(0.0f, 10.0f));
            Quaternion rotation = Random.rotation;
            GameObject boid = Instantiate(boidTemplate, position, rotation);
            if (boid.transform.localEulerAngles.x > 45f && boid.transform.localEulerAngles.x <= 180f)
                boid.transform.localEulerAngles = new Vector3(45f, boid.transform.localEulerAngles.y, boid.transform.localEulerAngles.z);
            if (boid.transform.localEulerAngles.x < 315f && boid.transform.localEulerAngles.x > 180f)
                boid.transform.localEulerAngles = new Vector3(315f, boid.transform.localEulerAngles.y, boid.transform.localEulerAngles.z);
            boid.transform.SetParent(transform);
            TestBird bird = boid.AddComponent<TestBird>();
            bird.v = new Vector3(Random.Range(0.0f, 10.0f), Random.Range(10.0f, 20.0f), Random.Range(0.0f, 10.0f));
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Get center of flock
        float totalX = 0f;
        float totalY = 0f;
        float totalZ = 0f;
        float count = 0;
        foreach (Transform boid in transform)
        {
             totalX += boid.position.x;
             totalY += boid.position.y;
             totalZ += boid.position.z;
             count++;
        }
        float centerX = totalX / count;
        float centerY = totalY / count;
        float centerZ = totalZ / count;

        flockCenter = new Vector3(centerX, centerY, centerZ);
        sphere.position = goal;
    }

    private IEnumerator newGoal() {
        goal = new Vector3(Random.Range(-50.0f, 50.0f), Random.Range(10.0f, 70.0f), Random.Range(-50.0f, 50.0f));
        yield return new WaitForSeconds(15);
        StartCoroutine(newGoal());
    }

}
