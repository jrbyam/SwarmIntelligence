using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Diagnostics;


public class Flock : MonoBehaviour
{
    public Vector3 flockCenter;
    public int flockSize;
    public List<GameObject> boidTemplates;
    private float minDistance = 1f;
    public Vector3 goal;
    public Transform sphere;
    public bool colors;
    private Stopwatch timer;
    private List<float> times;
    private List<StateVector> birds;


    private float c1 = 2.0f;
    private float c2 = 2.0f;
    private float maxVelocity = 20f;

    // Start is called before the first frame update
    void Start()
    {
        birds = new List<StateVector>();
        for (int i = 0; i < flockSize; ++i) {
            Vector3 position = new Vector3(Random.Range(0.0f, 10.0f), Random.Range(0.0f, 10.0f), Random.Range(0.0f, 10.0f));
            Quaternion rotation = Random.rotation;
            GameObject boid = Instantiate(boidTemplates[colors ? Random.Range(1, boidTemplates.Count) : 0], position, rotation);
            if (boid.transform.localEulerAngles.x > 45f && boid.transform.localEulerAngles.x <= 180f)
                boid.transform.localEulerAngles = new Vector3(45f, boid.transform.localEulerAngles.y, boid.transform.localEulerAngles.z);
            if (boid.transform.localEulerAngles.x < 315f && boid.transform.localEulerAngles.x > 180f)
                boid.transform.localEulerAngles = new Vector3(315f, boid.transform.localEulerAngles.y, boid.transform.localEulerAngles.z);
            boid.transform.SetParent(transform);

            Vector3 velocity = new Vector3(Random.Range(0.0f, 10.0f), Random.Range(10.0f, 20.0f), Random.Range(0.0f, 10.0f));

            // Comment this out for state vector implementation
            // TestBird bird = boid.AddComponent<TestBird>();
            // bird.v = velocity;

            // Comment this out for non-state-vector implementation
            birds.Add(new StateVector {
                position = boid.transform.position,
                velocity = velocity
            });
        }


        timer = new Stopwatch();
        times = new List<float>();
    }

    // Update is called once per frame
    void Update()
    {
        
        // Get center of flock
        float totalX = 0f;
        float totalY = 0f;
        float totalZ = 0f;
        float count = 0;
        timer.Start();

        // Base PSO without state vector
        // foreach (Transform boid in transform)
        // {
        //     Vector3 boidPosition = boid.position;
        //     totalX += boidPosition.x;
        //     totalY += boidPosition.y;
        //     totalZ += boidPosition.z;
        //     count++;

        //     TestBird bird = boid.GetComponent<TestBird>();

        //     // Update personal and global bests
        //     float boidToGoal = Vector3.Distance(goal, boidPosition);
        //     if (boidToGoal < Vector3.Distance(goal, bird.pBest)) bird.pBest = boidPosition;
        //     if (boidToGoal < Vector3.Distance(goal, TestBird.gBest)) TestBird.gBest = boidPosition;

        //     // Handle PSO here
        //     float x = bird.v.x + (bird.c1 * Random.Range(0f, 1f) * (bird.pBest.x - boidPosition.x)) + (bird.c2 * Random.Range(0f, 1f) * (TestBird.gBest.x - boidPosition.x));
        //     float y = bird.v.y + (bird.c1 * Random.Range(0f, 1f) * (bird.pBest.y - boidPosition.y)) + (bird.c2 * Random.Range(0f, 1f) * (TestBird.gBest.y - boidPosition.y));
        //     float z = bird.v.z + (bird.c1 * Random.Range(0f, 1f) * (bird.pBest.z - boidPosition.z)) + (bird.c2 * Random.Range(0f, 1f) * (TestBird.gBest.z - boidPosition.z));

        //     bird.v = new Vector3(x, y, z);

        //     Vector3 nearestNeighbor = Vector3.zero;
        //     float nearestNeighborDistance = Mathf.Infinity;
        //     foreach (Transform other in transform) {
        //         if (boid != other) {
        //             float distance = Vector3.Distance(boidPosition, other.position);
        //             if (distance < nearestNeighborDistance) {
        //                 nearestNeighborDistance = distance;
        //                 nearestNeighbor = other.position;
        //             }
        //         }
        //     }
        //     if (nearestNeighborDistance < 1f) {
        //         Vector3 direction = (boidPosition - nearestNeighbor).normalized;
        //         bird.v = direction;
        //     }

        //     Vector3 forward = boid.forward;
        //     Vector3 current = new Vector3(boidPosition.x + forward.x, boidPosition.y + forward.y, boidPosition.z + forward.z);
        //     Vector3 target = new Vector3(boidPosition.x + bird.v.x, boidPosition.y + bird.v.y, boidPosition.z + bird.v.z);
        //     boid.LookAt(Vector3.Slerp(current, target, Time.deltaTime));
        //     forward = boid.forward;

        //     // Always move "forward", direction now facing
        //     float speed = bird.maxVelocity;
        //     // If the bird is pointing down at all, the speed is increased based on how much it's pointing down
        //     if (Vector3.Dot(forward, Vector3.down) > -0.4f)
        //         speed *= 1 + Vector3.Dot(forward, Vector3.down) / 2;
        //     boid.position += forward * speed * Time.deltaTime;
        //     bird.v = forward * speed;

        //     if  (Vector3.Distance(bird.pBest, goal) < 5)
        //         goal = new Vector3(Random.Range(-50.0f, 50.0f), Random.Range(10.0f, 70.0f), Random.Range(-50.0f, 50.0f));
        // }

        // Base PSO with state vector
        for (int i = 0; i < birds.Count; ++i)
        {
            totalX += birds[i].position.x;
            totalY += birds[i].position.y;
            totalZ += birds[i].position.z;
            count++;

            // Update personal and global bests
            float boidToGoal = Vector3.Distance(goal, birds[i].position);
            if (boidToGoal < Vector3.Distance(goal, birds[i].pBest)) birds[i].pBest = birds[i].position;
            if (boidToGoal < Vector3.Distance(goal, StateVector.gBest)) StateVector.gBest = birds[i].position;

            // Handle PSO here
            float x = birds[i].velocity.x + (c1 * Random.Range(0f, 1f) * (birds[i].pBest.x - birds[i].position.x)) + (c2 * Random.Range(0f, 1f) * (StateVector.gBest.x - birds[i].position.x));
            float y = birds[i].velocity.y + (c1 * Random.Range(0f, 1f) * (birds[i].pBest.y - birds[i].position.y)) + (c2 * Random.Range(0f, 1f) * (StateVector.gBest.y - birds[i].position.y));
            float z = birds[i].velocity.z + (c1 * Random.Range(0f, 1f) * (birds[i].pBest.z - birds[i].position.z)) + (c2 * Random.Range(0f, 1f) * (StateVector.gBest.z - birds[i].position.z));

            birds[i].velocity = new Vector3(x, y, z);

            Vector3 nearestNeighbor = Vector3.zero;
            float nearestNeighborDistance = Mathf.Infinity;
            for (int j = 0; j < birds.Count; ++j) {
                if (i != j) {
                    float distance = Vector3.Distance(birds[i].position, birds[j].position);
                    if (distance < nearestNeighborDistance) {
                        nearestNeighborDistance = distance;
                        nearestNeighbor = birds[j].position;
                    }
                }
            }
            if (nearestNeighborDistance < 1f) {
                Vector3 direction = (birds[i].position - nearestNeighbor).normalized;
                birds[i].velocity = direction;
            }

            Transform boid = transform.GetChild(i);
            Vector3 forward = boid.forward;
            Vector3 current = new Vector3(birds[i].position.x + forward.x, birds[i].position.y + forward.y, birds[i].position.z + forward.z);
            Vector3 target = new Vector3(birds[i].position.x + birds[i].velocity.x, birds[i].position.y + birds[i].velocity.y, birds[i].position.z + birds[i].velocity.z);
            boid.LookAt(Vector3.Slerp(current, target, Time.deltaTime));
            forward = boid.forward;

            // Always move "forward", direction now facing
            float speed = maxVelocity;
            // If the bird is pointing down at all, the speed is increased based on how much it's pointing down
            if (Vector3.Dot(forward, Vector3.down) > -0.4f)
                speed *= 1 + Vector3.Dot(forward, Vector3.down) / 2;
            boid.position += forward * speed * Time.deltaTime;
            birds[i].position = boid.position;
            birds[i].velocity = forward * speed;

            if  (Vector3.Distance(birds[i].pBest, goal) < 5)
                goal = new Vector3(Random.Range(-50.0f, 50.0f), Random.Range(10.0f, 70.0f), Random.Range(-50.0f, 50.0f));
        }
        timer.Stop();
        times.Add(timer.ElapsedMilliseconds);
        timer.Reset();

        float centerX = totalX / count;
        float centerY = totalY / count;
        float centerZ = totalZ / count;

        flockCenter = new Vector3(centerX, centerY, centerZ);

        sphere.position = goal;

        if (Input.GetKey(KeyCode.Tab))
            goal = new Vector3(Random.Range(-100.0f, 100.0f), Random.Range(10.0f, 70.0f), Random.Range(-100.0f, 100.0f));
    }

    private IEnumerator newGoal() {
        goal = new Vector3(Random.Range(-100.0f, 100.0f), Random.Range(10.0f, 70.0f), Random.Range(-100.0f, 100.0f));
        yield return new WaitForSeconds(15);
        StartCoroutine(newGoal());
    }

    void OnApplicationQuit()
    {
        float sum = 0f;
        for (int i = 0; i < times.Count; ++i) {
            sum += times[i];
        }
        UnityEngine.Debug.Log("Average time per frame: " + (sum / times.Count) + " ms");
    }

}
