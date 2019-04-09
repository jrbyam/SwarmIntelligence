using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Diagnostics;
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
    private Stopwatch timer;
    private List<float> times;
    private List<StateVector> birds;


    private float c1 = 1.49445f;
    private float c2 = 1.49445f;
    // private float c1 = 2.0f;
    // private float c2 = 2.0f;
    private float maxVelocity = 20f;
    private int n = 10;
    private float u = 0.9f;  // Unification factor for UPSO

    private List<Vector3> exampleSet = new List<Vector3>(); // Example set for ELPSO
    private int exampleSetSize = 50; // Size of example set for ELPSO

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
                position = position,
                velocity = velocity
            });

            // Initialize gBest with best randomly generated position
            if (Vector3.Distance(goal, position) < Vector3.Distance(goal, StateVector.gBest)) StateVector.gBest = position;
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
        // for (int i = 0; i < flockSize; ++i)
        // {
        //     Profiler.BeginSample("State Vector Loop");
        //     Vector3 position = birds[i].position;
        //     totalX += position.x;
        //     totalY += position.y;
        //     totalZ += position.z;
        //     count++;

        //     // Update personal and global bests
        //     float boidToGoal = Vector3.Distance(goal, position);
        //     if (boidToGoal < Vector3.Distance(goal, birds[i].pBest)) birds[i].pBest = position;
        //     if (boidToGoal < Vector3.Distance(goal, StateVector.gBest)) StateVector.gBest = position;

        //     // Handle PSO here
        //     Vector3 velocity = birds[i].velocity;
        //     float x = velocity.x + (c1 * Random.Range(0f, 1f) * (birds[i].pBest.x - position.x)) + (c2 * Random.Range(0f, 1f) * (StateVector.gBest.x - position.x));
        //     float y = velocity.y + (c1 * Random.Range(0f, 1f) * (birds[i].pBest.y - position.y)) + (c2 * Random.Range(0f, 1f) * (StateVector.gBest.y - position.y));
        //     float z = velocity.z + (c1 * Random.Range(0f, 1f) * (birds[i].pBest.z - position.z)) + (c2 * Random.Range(0f, 1f) * (StateVector.gBest.z - position.z));

        //     Vector3 nearestNeighbor = Vector3.zero;
        //     float nearestNeighborDistance = Mathf.Infinity;
        //     for (int j = 0; j < flockSize; ++j) {
        //         if (i != j) {
        //             float distance = Vector3.Distance(position, birds[j].position);
        //             if (distance < nearestNeighborDistance) {
        //                 nearestNeighborDistance = distance;
        //                 nearestNeighbor = birds[j].position;
        //             }
        //         }
        //     }
        //     if (nearestNeighborDistance < minDistance) {
        //         Vector3 direction = (position - nearestNeighbor).normalized;
        //         birds[i].velocity = direction;
        //     } else {
        //         birds[i].velocity = new Vector3(x, y, z);
        //     }
        //     velocity = birds[i].velocity;

        //     Transform boid = transform.GetChild(i);
        //     Vector3 forward = boid.forward;
        //     Vector3 current = new Vector3(position.x + forward.x, position.y + forward.y, position.z + forward.z);
        //     Vector3 target = new Vector3(position.x + velocity.x, position.y + velocity.y, position.z + velocity.z);
        //     boid.LookAt(Vector3.Slerp(current, target, Time.deltaTime));
        //     forward = boid.forward;

        //     // Always move "forward", direction now facing
        //     float speed = maxVelocity;
        //     // If the bird is pointing down at all, the speed is increased based on how much it's pointing down
        //     if (Vector3.Dot(forward, Vector3.down) > -0.4f)
        //         speed *= 1 + Vector3.Dot(forward, Vector3.down) / 2;
        //     boid.position += forward * speed * Time.deltaTime;
        //     birds[i].position = boid.position;
        //     birds[i].velocity = forward * speed;

        //     if  (Vector3.Distance(birds[i].pBest, goal) < 5)
        //         goal = new Vector3(Random.Range(-50.0f, 50.0f), Random.Range(10.0f, 70.0f), Random.Range(-50.0f, 50.0f));
        //     Profiler.EndSample();
        // }

        // Local version of PSO
        // for (int i = 0; i < flockSize; ++i)
        // {
        //     Profiler.BeginSample("State Vector Loop");
        //     Vector3 position = birds[i].position;
        //     totalX += position.x;
        //     totalY += position.y;
        //     totalZ += position.z;
        //     count++;

        //     // Update personal and local bests
        //     float boidToGoal = Vector3.Distance(goal, position);
        //     if (boidToGoal < Vector3.Distance(goal, birds[i].pBest)) birds[i].pBest = position;
        //     if (boidToGoal < Vector3.Distance(goal, birds[i].lBest)) birds[i].lBest = position;
        //     // If any of my neighbor's lBest is better than mine, update mine
        //     List<int> neighborIdxs = getNeighborhood(i);
        //     for (int j = 0; j < n; ++j) {
        //         if (Vector3.Distance(goal, birds[neighborIdxs[j]].lBest) < Vector3.Distance(goal, birds[i].lBest)) birds[i].lBest = birds[neighborIdxs[j]].lBest;
        //     }

        //     // Handle PSO here
        //     Vector3 velocity = birds[i].velocity;
        //     float x = velocity.x + (c1 * Random.Range(0f, 1f) * (birds[i].pBest.x - position.x)) + (c2 * Random.Range(0f, 1f) * (birds[i].lBest.x - position.x));
        //     float y = velocity.y + (c1 * Random.Range(0f, 1f) * (birds[i].pBest.y - position.y)) + (c2 * Random.Range(0f, 1f) * (birds[i].lBest.y - position.y));
        //     float z = velocity.z + (c1 * Random.Range(0f, 1f) * (birds[i].pBest.z - position.z)) + (c2 * Random.Range(0f, 1f) * (birds[i].lBest.z - position.z));

        //     Vector3 nearestNeighbor = Vector3.zero;
        //     float nearestNeighborDistance = Mathf.Infinity;
        //     for (int j = 0; j < flockSize; ++j) {
        //         if (i != j) {
        //             float distance = Vector3.Distance(position, birds[j].position);
        //             if (distance < nearestNeighborDistance) {
        //                 nearestNeighborDistance = distance;
        //                 nearestNeighbor = birds[j].position;
        //             }
        //         }
        //     }
        //     if (nearestNeighborDistance < minDistance) {
        //         Vector3 direction = (position - nearestNeighbor).normalized;
        //         birds[i].velocity = direction;
        //     } else {
        //         birds[i].velocity = new Vector3(x, y, z);
        //     }
        //     velocity = birds[i].velocity;

        //     Transform boid = transform.GetChild(i);
        //     Vector3 forward = boid.forward;
        //     Vector3 current = new Vector3(position.x + forward.x, position.y + forward.y, position.z + forward.z);
        //     Vector3 target = new Vector3(position.x + velocity.x, position.y + velocity.y, position.z + velocity.z);
        //     boid.LookAt(Vector3.Slerp(current, target, Time.deltaTime));
        //     forward = boid.forward;

        //     // Always move "forward", direction now facing
        //     float speed = maxVelocity;
        //     // If the bird is pointing down at all, the speed is increased based on how much it's pointing down
        //     if (Vector3.Dot(forward, Vector3.down) > -0.4f)
        //         speed *= 1 + Vector3.Dot(forward, Vector3.down) / 2;
        //     boid.position += forward * speed * Time.deltaTime;
        //     birds[i].position = boid.position;
        //     birds[i].velocity = forward * speed;

        //     if  (Vector3.Distance(birds[i].pBest, goal) < 5)
        //         goal = new Vector3(Random.Range(-50.0f, 50.0f), Random.Range(10.0f, 70.0f), Random.Range(-50.0f, 50.0f));
        //     Profiler.EndSample();
        // }

        // UPSO - global and local versions combined
        // for (int i = 0; i < flockSize; ++i)
        // {
        //     Profiler.BeginSample("State Vector Loop");
        //     Vector3 position = birds[i].position;
        //     totalX += position.x;
        //     totalY += position.y;
        //     totalZ += position.z;
        //     count++;

        //     // Update personal and local bests
        //     float boidToGoal = Vector3.Distance(goal, position);
        //     if (boidToGoal < Vector3.Distance(goal, birds[i].pBest)) birds[i].pBest = position;
        //     if (boidToGoal < Vector3.Distance(goal, StateVector.gBest)) StateVector.gBest = position;
        //     if (boidToGoal < Vector3.Distance(goal, birds[i].lBest)) birds[i].lBest = position;
        //     // If any of my neighbor's lBest is better than mine, update mine
        //     List<int> neighborIdxs = getNeighborhood(i);
        //     for (int j = 0; j < n; ++j) {
        //         if (Vector3.Distance(goal, birds[neighborIdxs[j]].lBest) < Vector3.Distance(goal, birds[i].lBest)) birds[i].lBest = birds[neighborIdxs[j]].lBest;
        //     }

        //     // Handle UPSO here
        //     Vector3 velocity = birds[i].velocity;
        //     float lx = velocity.x + (c1 * Random.Range(0f, 1f) * (birds[i].pBest.x - position.x)) + (c2 * Random.Range(0f, 1f) * (birds[i].lBest.x - position.x));
        //     float ly = velocity.y + (c1 * Random.Range(0f, 1f) * (birds[i].pBest.y - position.y)) + (c2 * Random.Range(0f, 1f) * (birds[i].lBest.y - position.y));
        //     float lz = velocity.z + (c1 * Random.Range(0f, 1f) * (birds[i].pBest.z - position.z)) + (c2 * Random.Range(0f, 1f) * (birds[i].lBest.z - position.z));
        //     float gx = velocity.x + (c1 * Random.Range(0f, 1f) * (birds[i].pBest.x - position.x)) + (c2 * Random.Range(0f, 1f) * (StateVector.gBest.x - position.x));
        //     float gy = velocity.y + (c1 * Random.Range(0f, 1f) * (birds[i].pBest.y - position.y)) + (c2 * Random.Range(0f, 1f) * (StateVector.gBest.y - position.y));
        //     float gz = velocity.z + (c1 * Random.Range(0f, 1f) * (birds[i].pBest.z - position.z)) + (c2 * Random.Range(0f, 1f) * (StateVector.gBest.z - position.z));

        //     Vector3 nearestNeighbor = Vector3.zero;
        //     float nearestNeighborDistance = Mathf.Infinity;
        //     for (int j = 0; j < flockSize; ++j) {
        //         if (i != j) {
        //             float distance = Vector3.Distance(position, birds[j].position);
        //             if (distance < nearestNeighborDistance) {
        //                 nearestNeighborDistance = distance;
        //                 nearestNeighbor = birds[j].position;
        //             }
        //         }
        //     }
        //     if (nearestNeighborDistance < minDistance) {
        //         Vector3 direction = (position - nearestNeighbor).normalized;
        //         birds[i].velocity = direction;
        //     } else {
        //         // Combine global and local components using the unification factor
        //         float ux = gx + (1 - u) * lx;
        //         float uy = gy + (1 - u) * ly;
        //         float uz = gz + (1 - u) * lz;
        //         birds[i].velocity = new Vector3(ux, uy, uz);
        //     }
        //     velocity = birds[i].velocity;

        //     Transform boid = transform.GetChild(i);
        //     Vector3 forward = boid.forward;
        //     Vector3 current = new Vector3(position.x + forward.x, position.y + forward.y, position.z + forward.z);
        //     Vector3 target = new Vector3(position.x + velocity.x, position.y + velocity.y, position.z + velocity.z);
        //     boid.LookAt(Vector3.Slerp(current, target, Time.deltaTime));
        //     forward = boid.forward;

        //     // Always move "forward", direction now facing
        //     float speed = maxVelocity;
        //     // If the bird is pointing down at all, the speed is increased based on how much it's pointing down
        //     if (Vector3.Dot(forward, Vector3.down) > -0.4f)
        //         speed *= 1 + Vector3.Dot(forward, Vector3.down) / 2;
        //     boid.position += forward * speed * Time.deltaTime;
        //     birds[i].position = boid.position;
        //     birds[i].velocity = forward * speed;

        //     if  (Vector3.Distance(birds[i].pBest, goal) < 5)
        //         goal = new Vector3(Random.Range(-50.0f, 50.0f), Random.Range(10.0f, 70.0f), Random.Range(-50.0f, 50.0f));
        //     Profiler.EndSample();
        // }

        // CLPSO
        // for (int i = 0; i < flockSize; ++i)
        // {
        //     Profiler.BeginSample("State Vector Loop");
        //     Vector3 position = birds[i].position;
        //     totalX += position.x;
        //     totalY += position.y;
        //     totalZ += position.z;
        //     count++;

        //     // Update personal and local bests
        //     float boidToGoal = Vector3.Distance(goal, position);
        //     if (boidToGoal < Vector3.Distance(goal, birds[i].pBest)) birds[i].pBest = position;

        //     // Handle CLPSO here
        //     Vector3 velocity = birds[i].velocity;
        //     Vector3 otherPBest = birds[Random.Range(0, flockSize)].pBest;
        //     // The learning probability for CLPSO is based on the current index and the flock size
        //     float lp = 0.05f + 0.45f * (Mathf.Exp(10 * 1 / flockSize - 1) - 1) / (Mathf.Exp(10) - 1);
        //     float pBestX = Random.Range(0, 101) <= (1 - lp) * 100 ? otherPBest.x : birds[i].pBest.x;
        //     float pBestY = Random.Range(0, 101) <= (1 - lp) * 100 ? otherPBest.y : birds[i].pBest.y;
        //     float pBestZ = Random.Range(0, 101) <= (1 - lp) * 100 ? otherPBest.z : birds[i].pBest.z;
        //     // Decide based on a learning probability whether or not to use my pBest or the pBest of a random neighbor
        //     float x = velocity.x + (c1 * Random.Range(0f, 1f) * (pBestX - position.x));
        //     float y = velocity.y + (c1 * Random.Range(0f, 1f) * (pBestY - position.y));
        //     float z = velocity.z + (c1 * Random.Range(0f, 1f) * (pBestZ - position.z));

        //     Vector3 nearestNeighbor = Vector3.zero;
        //     float nearestNeighborDistance = Mathf.Infinity;
        //     for (int j = 0; j < flockSize; ++j) {
        //         if (i != j) {
        //             float distance = Vector3.Distance(position, birds[j].position);
        //             if (distance < nearestNeighborDistance) {
        //                 nearestNeighborDistance = distance;
        //                 nearestNeighbor = birds[j].position;
        //             }
        //         }
        //     }
        //     if (nearestNeighborDistance < minDistance) {
        //         Vector3 direction = (position - nearestNeighbor).normalized;
        //         birds[i].velocity = direction;
        //     } else {
        //         birds[i].velocity = new Vector3(x, y, z);
        //     }
        //     velocity = birds[i].velocity;

        //     Transform boid = transform.GetChild(i);
        //     Vector3 forward = boid.forward;
        //     Vector3 current = new Vector3(position.x + forward.x, position.y + forward.y, position.z + forward.z);
        //     Vector3 target = new Vector3(position.x + velocity.x, position.y + velocity.y, position.z + velocity.z);
        //     boid.LookAt(Vector3.Slerp(current, target, Time.deltaTime));
        //     forward = boid.forward;

        //     // Always move "forward", direction now facing
        //     float speed = maxVelocity;
        //     // If the bird is pointing down at all, the speed is increased based on how much it's pointing down
        //     if (Vector3.Dot(forward, Vector3.down) > -0.4f)
        //         speed *= 1 + Vector3.Dot(forward, Vector3.down) / 2;
        //     boid.position += forward * speed * Time.deltaTime;
        //     birds[i].position = boid.position;
        //     birds[i].velocity = forward * speed;

        //     if  (Vector3.Distance(birds[i].pBest, goal) < 5)
        //         goal = new Vector3(Random.Range(-50.0f, 50.0f), Random.Range(10.0f, 70.0f), Random.Range(-50.0f, 50.0f));
        //     Profiler.EndSample();
        // }

        // ELPSO
        for (int i = 0; i < flockSize; ++i)
        {
            Profiler.BeginSample("State Vector Loop");
            Vector3 position = birds[i].position;
            totalX += position.x;
            totalY += position.y;
            totalZ += position.z;
            count++;

            // Update personal and local bests
            float boidToGoal = Vector3.Distance(goal, position);
            if (boidToGoal < Vector3.Distance(goal, birds[i].pBest)) birds[i].pBest = position;
            if (boidToGoal < Vector3.Distance(goal, StateVector.gBest)) StateVector.gBest = position;

            // Update the example set according to ELSPO
            if (exampleSet.Count == 0) exampleSet.Add(StateVector.gBest);
            else if (exampleSet.Count <= exampleSetSize) {
                bool oneBetter = false;
                for (int j = 0; j < exampleSet.Count; ++j) {
                    if (Vector3.Distance(goal, exampleSet[j]) < Vector3.Distance(goal, StateVector.gBest)) {
                        oneBetter = true;
                        break;
                    }
                }
                if (!oneBetter) {
                    if (exampleSet.Count == exampleSetSize) exampleSet.RemoveAt(0); // First in, first out
                    exampleSet.Add(StateVector.gBest);
                }
            }

            // Handle CLPSO here
            Vector3 velocity = birds[i].velocity;
            Vector3 otherPBest = birds[Random.Range(0, flockSize)].pBest;
            Vector3 exampleBest = exampleSet[Random.Range(0, exampleSet.Count)];
            float w = 0.729f;
            float x = w * velocity.x + (c1 * Random.Range(0f, 1f) * (otherPBest.x - position.x)) + (c2 * Random.Range(0f, 1f) * (exampleBest.x - position.x));;
            float y = w * velocity.y + (c1 * Random.Range(0f, 1f) * (otherPBest.y - position.y)) + (c2 * Random.Range(0f, 1f) * (exampleBest.y - position.y));;
            float z = w * velocity.z + (c1 * Random.Range(0f, 1f) * (otherPBest.z - position.z)) + (c2 * Random.Range(0f, 1f) * (exampleBest.z - position.z));;

            Vector3 nearestNeighbor = Vector3.zero;
            float nearestNeighborDistance = Mathf.Infinity;
            for (int j = 0; j < flockSize; ++j) {
                if (i != j) {
                    float distance = Vector3.Distance(position, birds[j].position);
                    if (distance < nearestNeighborDistance) {
                        nearestNeighborDistance = distance;
                        nearestNeighbor = birds[j].position;
                    }
                }
            }
            if (nearestNeighborDistance < minDistance) {
                Vector3 direction = (position - nearestNeighbor).normalized;
                birds[i].velocity = direction;
            } else {
                birds[i].velocity = new Vector3(x, y, z);
            }
            velocity = birds[i].velocity;

            Transform boid = transform.GetChild(i);
            Vector3 forward = boid.forward;
            Vector3 current = new Vector3(position.x + forward.x, position.y + forward.y, position.z + forward.z);
            Vector3 target = new Vector3(position.x + velocity.x, position.y + velocity.y, position.z + velocity.z);
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

            Profiler.EndSample();
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

    private List<int> getNeighborhood(int idx) {
        List<int> result = new List<int>();
        result.Add(idx);
        for (int i = 0; i < n - 1; ++i) {
            idx++;
            if (idx == flockSize) idx = 0;
            result.Add(idx);
        }
        return result;
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
