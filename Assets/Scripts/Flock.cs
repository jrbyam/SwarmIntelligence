﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Diagnostics;
using UnityEngine.Profiling;

public class Flock : MonoBehaviour
{
    public PlayButton playButton;
    public Vector3 flockCenter;
    public List<GameObject> boidTemplates;
    private float minDistance = 1f;
    public Vector3 goal;
    public Transform sphere;
    public bool colors;
    private Stopwatch timer;
    private List<float> times;
    private KdTree<StateVector> birds;

    private List<Vector3> exampleSet = new List<Vector3>(); // Example set for ELPSO

    // Start is called before the first frame update
    void Start()
    {
        ResetFlock();

        timer = new Stopwatch();
        times = new List<float>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale > 0f) {
            // Get center of flock
            float totalX = 0f;
            float totalY = 0f;
            float totalZ = 0f;
            float count = 0;
            timer.Start();

            birds.UpdatePositions(); // Tree positions must be updated
            switch (SceneController.algorithm) {
                case 0:
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
                    //     float speed = SceneController.maxVelocity;
                    //     // If the bird is pointing down at all, the speed is increased based on how much it's pointing down
                    //     if (Vector3.Dot(forward, Vector3.down) > -0.4f)
                    //         speed *= 1 + Vector3.Dot(forward, Vector3.down) / 2;
                    //     boid.position += forward * speed * Time.deltaTime;
                    //     bird.v = forward * speed;

                    //     if  (Vector3.Distance(bird.pBest, goal) < 5)
                    //         goal = new Vector3(Random.Range(-50.0f, 50.0f), Random.Range(10.0f, 70.0f), Random.Range(-50.0f, 50.0f));
                    // }

                    // Base PSO with state vector
                    for (int i = 0; i < SceneController.flockSize; ++i)
                    {
                        // Profiler.BeginSample("State Vector Loop");
                        StateVector bird = birds[i];
                        Vector3 position = bird.position;
                        totalX += position.x;
                        totalY += position.y;
                        totalZ += position.z;
                        count++;

                        // Update personal and global bests
                        float boidToGoal = Vector3.Distance(goal, position);
                        if (boidToGoal < Vector3.Distance(goal, bird.pBest)) birds[i].pBest = position;
                        if (boidToGoal < Vector3.Distance(goal, StateVector.gBest)) StateVector.gBest = position;

                        bird = birds[i];
                        // Handle PSO here
                        Vector3 velocity = bird.velocity;
                        float x = velocity.x + (SceneController.c1 * Random.Range(0f, 1f) * (bird.pBest.x - position.x)) + (SceneController.c2 * Random.Range(0f, 1f) * (StateVector.gBest.x - position.x));
                        float y = velocity.y + (SceneController.c1 * Random.Range(0f, 1f) * (bird.pBest.y - position.y)) + (SceneController.c2 * Random.Range(0f, 1f) * (StateVector.gBest.y - position.y));
                        float z = velocity.z + (SceneController.c1 * Random.Range(0f, 1f) * (bird.pBest.z - position.z)) + (SceneController.c2 * Random.Range(0f, 1f) * (StateVector.gBest.z - position.z));

                        Vector3 nearestNeighbor = Vector3.zero;
                        float nearestNeighborDistance = float.MaxValue;
                        // int birdCount = birds.Count;
                        // for (int j = 0; j < birdCount; ++j) {
                        //     if (i != j) {
                        //         float distance = Vector3.Distance(position, birds[j].position);
                        //         if (distance < nearestNeighborDistance) {
                        //             nearestNeighborDistance = distance;
                        //             nearestNeighbor = birds[j].position;
                        //         }
                        //     }
                        // }

                        nearestNeighbor = birds.FindClosest(bird.position).position;
                        nearestNeighborDistance = Vector3.Distance(bird.position, nearestNeighbor);
                        if (nearestNeighborDistance < minDistance) {
                            Vector3 direction = (position - nearestNeighbor).normalized;
                            birds[i].velocity = direction;
                        } else {
                            birds[i].velocity = new Vector3(x, y, z);
                        }
                        bird = birds[i];
                        velocity = bird.velocity;

                        Transform boid = transform.GetChild(i);
                        Vector3 forward = boid.forward;
                        Vector3 current = new Vector3(position.x + forward.x, position.y + forward.y, position.z + forward.z);
                        Vector3 target = new Vector3(position.x + velocity.x, position.y + velocity.y, position.z + velocity.z);
                        boid.LookAt(Vector3.Slerp(current, target, Time.deltaTime));
                        forward = boid.forward;

                        // Always move "forward", direction now facing
                        float speed = SceneController.maxVelocity;
                        // If the bird is pointing down at all, the speed is increased based on how much it's pointing down
                        if (Vector3.Dot(forward, Vector3.down) > -0.4f)
                            speed *= 1 + Vector3.Dot(forward, Vector3.down) / 2;
                        boid.position += forward * speed * Time.deltaTime;
                        birds[i].position = boid.position;
                        birds[i].velocity = forward * speed;

                        if  (Vector3.Distance(bird.pBest, goal) < 5)
                            goal = new Vector3(Random.Range(-50.0f, 50.0f), Random.Range(10.0f, 70.0f), Random.Range(-50.0f, 50.0f));
                        // Profiler.EndSample();
                    }
                    break;

                case 1:
                    // Local version of PSO
                    for (int i = 0; i < SceneController.flockSize; ++i)
                    {
                        // Profiler.BeginSample("State Vector Loop");
                        StateVector bird = birds[i];
                        Vector3 position = bird.position;
                        totalX += position.x;
                        totalY += position.y;
                        totalZ += position.z;
                        count++;

                        // Update personal and local bests
                        float boidToGoal = Vector3.Distance(goal, position);
                        if (boidToGoal < Vector3.Distance(goal, bird.pBest)) birds[i].pBest = position;
                        if (boidToGoal < Vector3.Distance(goal, bird.lBest)) birds[i].lBest = position;

                        bird = birds[i];
                        // If any of my neighbor's lBest is better than mine, update mine
                        List<int> neighborIdxs = getNeighborhood(i);
                        for (int j = 0; j < SceneController.n; ++j) {
                            if (Vector3.Distance(goal, birds[neighborIdxs[j]].lBest) < Vector3.Distance(goal, birds[i].lBest)) birds[i].lBest = birds[neighborIdxs[j]].lBest;
                        }
                        bird = birds[i];

                        // Handle PSO here
                        Vector3 velocity = bird.velocity;
                        float x = velocity.x + (SceneController.c1 * Random.Range(0f, 1f) * (bird.pBest.x - position.x)) + (SceneController.c2 * Random.Range(0f, 1f) * (bird.lBest.x - position.x));
                        float y = velocity.y + (SceneController.c1 * Random.Range(0f, 1f) * (bird.pBest.y - position.y)) + (SceneController.c2 * Random.Range(0f, 1f) * (bird.lBest.y - position.y));
                        float z = velocity.z + (SceneController.c1 * Random.Range(0f, 1f) * (bird.pBest.z - position.z)) + (SceneController.c2 * Random.Range(0f, 1f) * (bird.lBest.z - position.z));

                        Vector3 nearestNeighbor = Vector3.zero;
                        float nearestNeighborDistance = Mathf.Infinity;
                        // int birdCount = birds.Count;
                        // for (int j = 0; j < birdCount; ++j) {
                        //     if (i != j) {
                        //         float distance = Vector3.Distance(position, birds[j].position);
                        //         if (distance < nearestNeighborDistance) {
                        //             nearestNeighborDistance = distance;
                        //             nearestNeighbor = birds[j].position;
                        //         }
                        //     }
                        // }

                        nearestNeighbor = birds.FindClosest(bird.position).position;
                        nearestNeighborDistance = Vector3.Distance(bird.position, nearestNeighbor);
                        if (nearestNeighborDistance < minDistance) {
                            Vector3 direction = (position - nearestNeighbor).normalized;
                            birds[i].velocity = direction;
                        } else {
                            birds[i].velocity = new Vector3(x, y, z);
                        }
                        bird = birds[i];
                        velocity = bird.velocity;

                        Transform boid = transform.GetChild(i);
                        Vector3 forward = boid.forward;
                        Vector3 current = new Vector3(position.x + forward.x, position.y + forward.y, position.z + forward.z);
                        Vector3 target = new Vector3(position.x + velocity.x, position.y + velocity.y, position.z + velocity.z);
                        boid.LookAt(Vector3.Slerp(current, target, Time.deltaTime));
                        forward = boid.forward;

                        // Always move "forward", direction now facing
                        float speed = SceneController.maxVelocity;
                        // If the bird is pointing down at all, the speed is increased based on how much it's pointing down
                        if (Vector3.Dot(forward, Vector3.down) > -0.4f)
                            speed *= 1 + Vector3.Dot(forward, Vector3.down) / 2;
                        boid.position += forward * speed * Time.deltaTime;
                        birds[i].position = boid.position;
                        birds[i].velocity = forward * speed;

                        if  (Vector3.Distance(bird.pBest, goal) < 5)
                            goal = new Vector3(Random.Range(-50.0f, 50.0f), Random.Range(10.0f, 70.0f), Random.Range(-50.0f, 50.0f));
                        // Profiler.EndSample();
                    }
                    break;

                case 2:
                    // UPSO - global and local versions combined
                    for (int i = 0; i < SceneController.flockSize; ++i)
                    {
                        // Profiler.BeginSample("State Vector Loop");
                        StateVector bird = birds[i];
                        Vector3 position = bird.position;
                        totalX += position.x;
                        totalY += position.y;
                        totalZ += position.z;
                        count++;

                        // Update personal and local bests
                        float boidToGoal = Vector3.Distance(goal, position);
                        if (boidToGoal < Vector3.Distance(goal, bird.pBest)) birds[i].pBest = position;
                        if (boidToGoal < Vector3.Distance(goal, StateVector.gBest)) StateVector.gBest = position;
                        if (boidToGoal < Vector3.Distance(goal, bird.lBest)) birds[i].lBest = position;

                        bird = birds[i];
                        // If any of my neighbor's lBest is better than mine, update mine
                        List<int> neighborIdxs = getNeighborhood(i);
                        for (int j = 0; j < SceneController.n; ++j) {
                            if (Vector3.Distance(goal, birds[neighborIdxs[j]].lBest) < Vector3.Distance(goal, birds[i].lBest)) birds[i].lBest = birds[neighborIdxs[j]].lBest;
                        }
                        bird = birds[i];

                        // Handle UPSO here
                        Vector3 velocity = bird.velocity;
                        float lx = velocity.x + (SceneController.c1 * Random.Range(0f, 1f) * (bird.pBest.x - position.x)) + (SceneController.c2 * Random.Range(0f, 1f) * (bird.lBest.x - position.x));
                        float ly = velocity.y + (SceneController.c1 * Random.Range(0f, 1f) * (bird.pBest.y - position.y)) + (SceneController.c2 * Random.Range(0f, 1f) * (bird.lBest.y - position.y));
                        float lz = velocity.z + (SceneController.c1 * Random.Range(0f, 1f) * (bird.pBest.z - position.z)) + (SceneController.c2 * Random.Range(0f, 1f) * (bird.lBest.z - position.z));
                        float gx = velocity.x + (SceneController.c1 * Random.Range(0f, 1f) * (bird.pBest.x - position.x)) + (SceneController.c2 * Random.Range(0f, 1f) * (StateVector.gBest.x - position.x));
                        float gy = velocity.y + (SceneController.c1 * Random.Range(0f, 1f) * (bird.pBest.y - position.y)) + (SceneController.c2 * Random.Range(0f, 1f) * (StateVector.gBest.y - position.y));
                        float gz = velocity.z + (SceneController.c1 * Random.Range(0f, 1f) * (bird.pBest.z - position.z)) + (SceneController.c2 * Random.Range(0f, 1f) * (StateVector.gBest.z - position.z));

                        Vector3 nearestNeighbor = Vector3.zero;
                        float nearestNeighborDistance = Mathf.Infinity;
                        // int birdCount = birds.Count;
                        // for (int j = 0; j < birdCount; ++j) {
                        //     if (i != j) {
                        //         float distance = Vector3.Distance(position, birds[j].position);
                        //         if (distance < nearestNeighborDistance) {
                        //             nearestNeighborDistance = distance;
                        //             nearestNeighbor = birds[j].position;
                        //         }
                        //     }
                        // }

                        nearestNeighbor = birds.FindClosest(bird.position).position;
                        nearestNeighborDistance = Vector3.Distance(bird.position, nearestNeighbor);
                        if (nearestNeighborDistance < minDistance) {
                            Vector3 direction = (position - nearestNeighbor).normalized;
                            birds[i].velocity = direction;
                        } else {
                            // Combine global and local components using the unification factor
                            Vector3 global = new Vector3(gx, gy, gz) * SceneController.u;
                            Vector3 local = new Vector3(lx, ly, lz) * (1 - SceneController.u);
                            birds[i].velocity = global + local;
                        }
                        bird = birds[i];
                        velocity = bird.velocity;

                        Transform boid = transform.GetChild(i);
                        Vector3 forward = boid.forward;
                        Vector3 current = new Vector3(position.x + forward.x, position.y + forward.y, position.z + forward.z);
                        Vector3 target = new Vector3(position.x + velocity.x, position.y + velocity.y, position.z + velocity.z);
                        boid.LookAt(Vector3.Slerp(current, target, Time.deltaTime));
                        forward = boid.forward;

                        // Always move "forward", direction now facing
                        float speed = SceneController.maxVelocity;
                        // If the bird is pointing down at all, the speed is increased based on how much it's pointing down
                        if (Vector3.Dot(forward, Vector3.down) > -0.4f)
                            speed *= 1 + Vector3.Dot(forward, Vector3.down) / 2;
                        boid.position += forward * speed * Time.deltaTime;
                        birds[i].position = boid.position;
                        birds[i].velocity = forward * speed;

                        if  (Vector3.Distance(bird.pBest, goal) < 5)
                            goal = new Vector3(Random.Range(-50.0f, 50.0f), Random.Range(10.0f, 70.0f), Random.Range(-50.0f, 50.0f));
                        // Profiler.EndSample();
                    }
                    break;

                case 3:
                    // CLPSO
                    for (int i = 0; i < SceneController.flockSize; ++i)
                    {
                        // Profiler.BeginSample("State Vector Loop");
                        StateVector bird = birds[i];
                        Vector3 position = bird.position;
                        totalX += position.x;
                        totalY += position.y;
                        totalZ += position.z;
                        count++;

                        // Update personal and local bests
                        float boidToGoal = Vector3.Distance(goal, position);
                        if (boidToGoal < Vector3.Distance(goal, bird.pBest)) birds[i].pBest = position;

                        bird = birds[i];
                        // Handle CLPSO here
                        Vector3 velocity = bird.velocity;
                        Vector3 otherPBest = birds[Random.Range(0, SceneController.flockSize)].pBest;
                        // The learning probability for CLPSO is based on the current index and the flock size
                        float lp = 0.05f + 0.45f * (Mathf.Exp(10 * 1 / SceneController.flockSize - 1) - 1) / (Mathf.Exp(10) - 1);
                        float pBestX = Random.Range(0, 101) <= (1 - lp) * 100 ? otherPBest.x : bird.pBest.x;
                        float pBestY = Random.Range(0, 101) <= (1 - lp) * 100 ? otherPBest.y : bird.pBest.y;
                        float pBestZ = Random.Range(0, 101) <= (1 - lp) * 100 ? otherPBest.z : bird.pBest.z;
                        // Decide based on a learning probability whether or not to use my pBest or the pBest of a random neighbor
                        float x = velocity.x + (SceneController.c1 * Random.Range(0f, 1f) * (pBestX - position.x));
                        float y = velocity.y + (SceneController.c1 * Random.Range(0f, 1f) * (pBestY - position.y));
                        float z = velocity.z + (SceneController.c1 * Random.Range(0f, 1f) * (pBestZ - position.z));

                        Vector3 nearestNeighbor = Vector3.zero;
                        float nearestNeighborDistance = Mathf.Infinity;
                        // int birdCount = birds.Count;
                        // for (int j = 0; j < birdCount; ++j) {
                        //     if (i != j) {
                        //         float distance = Vector3.Distance(position, birds[j].position);
                        //         if (distance < nearestNeighborDistance) {
                        //             nearestNeighborDistance = distance;
                        //             nearestNeighbor = birds[j].position;
                        //         }
                        //     }
                        // }

                        nearestNeighbor = birds.FindClosest(bird.position).position;
                        nearestNeighborDistance = Vector3.Distance(bird.position, nearestNeighbor);
                        if (nearestNeighborDistance < minDistance) {
                            Vector3 direction = (position - nearestNeighbor).normalized;
                            birds[i].velocity = direction;
                        } else {
                            birds[i].velocity = new Vector3(x, y, z);
                        }
                        bird = birds[i];
                        velocity = bird.velocity;

                        Transform boid = transform.GetChild(i);
                        Vector3 forward = boid.forward;
                        Vector3 current = new Vector3(position.x + forward.x, position.y + forward.y, position.z + forward.z);
                        Vector3 target = new Vector3(position.x + velocity.x, position.y + velocity.y, position.z + velocity.z);
                        boid.LookAt(Vector3.Slerp(current, target, Time.deltaTime));
                        forward = boid.forward;

                        // Always move "forward", direction now facing
                        float speed = SceneController.maxVelocity;
                        // If the bird is pointing down at all, the speed is increased based on how much it's pointing down
                        if (Vector3.Dot(forward, Vector3.down) > -0.4f)
                            speed *= 1 + Vector3.Dot(forward, Vector3.down) / 2;
                        boid.position += forward * speed * Time.deltaTime;
                        birds[i].position = boid.position;
                        birds[i].velocity = forward * speed;

                        if  (Vector3.Distance(bird.pBest, goal) < 5)
                            goal = new Vector3(Random.Range(-50.0f, 50.0f), Random.Range(10.0f, 70.0f), Random.Range(-50.0f, 50.0f));
                        // Profiler.EndSample();
                    }
                    break;

                case 4:
                    // ELPSO
                    for (int i = 0; i < SceneController.flockSize; ++i)
                    {
                        // Profiler.BeginSample("State Vector Loop");
                        StateVector bird = birds[i];
                        Vector3 position = bird.position;
                        totalX += position.x;
                        totalY += position.y;
                        totalZ += position.z;
                        count++;

                        // Update personal and local bests
                        float boidToGoal = Vector3.Distance(goal, position);
                        if (boidToGoal < Vector3.Distance(goal, bird.pBest)) birds[i].pBest = position;
                        if (boidToGoal < Vector3.Distance(goal, StateVector.gBest)) StateVector.gBest = position;

                        // Update the example set according to ELSPO
                        if (exampleSet.Count == 0) exampleSet.Add(StateVector.gBest);
                        else if (exampleSet.Count <= SceneController.exampleSetSize) {
                            bool oneBetter = false;
                            for (int j = 0; j < exampleSet.Count; ++j) {
                                if (Vector3.Distance(goal, exampleSet[j]) < Vector3.Distance(goal, StateVector.gBest)) {
                                    oneBetter = true;
                                    break;
                                }
                            }
                            if (!oneBetter) {
                                if (exampleSet.Count == SceneController.exampleSetSize) exampleSet.RemoveAt(0); // First in, first out
                                exampleSet.Add(StateVector.gBest);
                            }
                        }

                        bird = birds[i];
                        // Handle ELPSO here
                        Vector3 velocity = bird.velocity;
                        Vector3 otherPBest = birds[Random.Range(0, SceneController.flockSize)].pBest;
                        Vector3 exampleBest = exampleSet[Random.Range(0, exampleSet.Count)];
                        float x = SceneController.w * velocity.x + (SceneController.c1 * Random.Range(0f, 1f) * (otherPBest.x - position.x)) + (SceneController.c2 * Random.Range(0f, 1f) * (exampleBest.x - position.x));;
                        float y = SceneController.w * velocity.y + (SceneController.c1 * Random.Range(0f, 1f) * (otherPBest.y - position.y)) + (SceneController.c2 * Random.Range(0f, 1f) * (exampleBest.y - position.y));;
                        float z = SceneController.w * velocity.z + (SceneController.c1 * Random.Range(0f, 1f) * (otherPBest.z - position.z)) + (SceneController.c2 * Random.Range(0f, 1f) * (exampleBest.z - position.z));;

                        Vector3 nearestNeighbor = Vector3.zero;
                        float nearestNeighborDistance = Mathf.Infinity;
                        // int birdCount = birds.Count;
                        // for (int j = 0; j < birdCount; ++j) {
                        //     if (i != j) {
                        //         float distance = Vector3.Distance(position, birds[j].position);
                        //         if (distance < nearestNeighborDistance) {
                        //             nearestNeighborDistance = distance;
                        //             nearestNeighbor = birds[j].position;
                        //         }
                        //     }
                        // }

                        nearestNeighbor = birds.FindClosest(bird.position).position;
                        nearestNeighborDistance = Vector3.Distance(bird.position, nearestNeighbor);
                        if (nearestNeighborDistance < minDistance) {
                            Vector3 direction = (position - nearestNeighbor).normalized;
                            birds[i].velocity = direction;
                        } else {
                            birds[i].velocity = new Vector3(x, y, z);
                        }
                        bird = birds[i];
                        velocity = bird.velocity;

                        Transform boid = transform.GetChild(i);
                        Vector3 forward = boid.forward;
                        Vector3 current = new Vector3(position.x + forward.x, position.y + forward.y, position.z + forward.z);
                        Vector3 target = new Vector3(position.x + velocity.x, position.y + velocity.y, position.z + velocity.z);
                        boid.LookAt(Vector3.Slerp(current, target, Time.deltaTime));
                        forward = boid.forward;

                        // Always move "forward", direction now facing
                        float speed = SceneController.maxVelocity;
                        // If the bird is pointing down at all, the speed is increased based on how much it's pointing down
                        if (Vector3.Dot(forward, Vector3.down) > -0.4f)
                            speed *= 1 + Vector3.Dot(forward, Vector3.down) / 2;
                        boid.position += forward * speed * Time.deltaTime;
                        birds[i].position = boid.position;
                        birds[i].velocity = forward * speed;

                        if  (Vector3.Distance(bird.pBest, goal) < 5)
                            goal = new Vector3(Random.Range(-50.0f, 50.0f), Random.Range(10.0f, 70.0f), Random.Range(-50.0f, 50.0f));

                        // Profiler.EndSample();
                    }
                    break;
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
    }

    public void ResetFlock() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        birds = new KdTree<StateVector>();
        for (int i = 0; i < SceneController.flockSize; ++i) {
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
        playButton.lastFlockSize = SceneController.flockSize;
        Time.timeScale = 1f;
    }

    private IEnumerator newGoal() {
        goal = new Vector3(Random.Range(-100.0f, 100.0f), Random.Range(10.0f, 70.0f), Random.Range(-100.0f, 100.0f));
        yield return new WaitForSeconds(15);
        StartCoroutine(newGoal());
    }

    private List<int> getNeighborhood(int idx) {
        List<int> result = new List<int>();
        result.Add(idx);
        for (int i = 0; i < SceneController.n - 1; ++i) {
            idx++;
            if (idx == SceneController.flockSize) idx = 0;
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
