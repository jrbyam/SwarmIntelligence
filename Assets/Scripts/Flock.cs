using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.Profiling;

public class Flock : MonoBehaviour
{
    public Vector3 flockCenter;
    public int flockSize;
    public List<GameObject> boidTemplates;
    private float minDistance = 1f;
    public Vector3 goal;
    public Transform sphere;
    public bool colors;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < flockSize; ++i) {
            Vector3 position = new Vector3(Random.Range(0.0f, 10.0f), Random.Range(0.0f, 10.0f), Random.Range(0.0f, 10.0f));
            Quaternion rotation = Random.rotation;
            GameObject boid = Instantiate(boidTemplates[colors ? Random.Range(1, boidTemplates.Count) : 0], position, rotation);
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
        Profiler.BeginSample("Boid Control Block");
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

            TestBird bird = boid.GetComponent<TestBird>();

            // Update personal and global bests
            if (Vector3.Distance(goal, boid.position) < Vector3.Distance(goal, bird.pBest)) bird.pBest = boid.position;
            if (Vector3.Distance(goal, boid.position) < Vector3.Distance(goal, TestBird.gBest)) TestBird.gBest = boid.position;

            // Handle PSO here
            float x = bird.v.x + (bird.c1 * Random.Range(0f, 1f) * (bird.pBest.x - boid.position.x)) + (bird.c2 * Random.Range(0f, 1f) * (TestBird.gBest.x - boid.position.x));
            float y = bird.v.y + (bird.c1 * Random.Range(0f, 1f) * (bird.pBest.y - boid.position.y)) + (bird.c2 * Random.Range(0f, 1f) * (TestBird.gBest.y - boid.position.y));
            float z = bird.v.z + (bird.c1 * Random.Range(0f, 1f) * (bird.pBest.z - boid.position.z)) + (bird.c2 * Random.Range(0f, 1f) * (TestBird.gBest.z - boid.position.z));

            bird.v = new Vector3(x, y, z);

            Vector3 nearestNeighbor = Vector3.zero;
            float nearestNeighborDistance = Mathf.Infinity;
            foreach (Transform other in transform) {
                if (boid != other) {
                    float distance = Vector3.Distance(boid.position, other.position);
                    if (distance < nearestNeighborDistance) {
                        nearestNeighborDistance = distance;
                        nearestNeighbor = other.position;
                    }
                }
            }
            if (nearestNeighborDistance < 1f) {
                Vector3 direction = (boid.position - nearestNeighbor).normalized;
                bird.v = direction;
            }

            Vector3 current = new Vector3(boid.position.x + boid.forward.x, boid.position.y + boid.forward.y, boid.position.z + boid.forward.z);
            Vector3 target = new Vector3(boid.position.x + bird.v.x, boid.position.y + bird.v.y, boid.position.z + bird.v.z);
            boid.LookAt(Vector3.Slerp(current, target, Time.deltaTime));

            // Always move "forward", direction now facing
            float speed = bird.maxVelocity;
            // If the bird is pointing down at all, the speed is increased based on how much it's pointing down
            if (Vector3.Dot(boid.forward, Vector3.down) > -0.4f)
                speed *= 1 + Vector3.Dot(boid.forward, Vector3.down) / 2;
            boid.position += boid.forward * speed * Time.deltaTime;
            bird.v = boid.forward * speed;

            if  (Vector3.Distance(bird.pBest, goal) < 5)
                goal = new Vector3(Random.Range(-50.0f, 50.0f), Random.Range(10.0f, 70.0f), Random.Range(-50.0f, 50.0f));
        }

        float centerX = totalX / count;
        float centerY = totalY / count;
        float centerZ = totalZ / count;

        flockCenter = new Vector3(centerX, centerY, centerZ);

        sphere.position = goal;

        if (Input.GetKey(KeyCode.Tab))
            goal = new Vector3(Random.Range(-100.0f, 100.0f), Random.Range(10.0f, 70.0f), Random.Range(-100.0f, 100.0f));
        Profiler.EndSample();
    }

    private IEnumerator newGoal() {
        goal = new Vector3(Random.Range(-100.0f, 100.0f), Random.Range(10.0f, 70.0f), Random.Range(-100.0f, 100.0f));
        yield return new WaitForSeconds(15);
        StartCoroutine(newGoal());
    }

}
