using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid
{
    public static Vector3 gBest { get; set; }
    public static List<Vector3> exampleSet = new List<Vector3>(); // Example set for ELPSO
    public Vector3 pBest { get; set; }
    public Vector3 lBest { get; set; }
    public Vector3 position { get; set; }
    public Vector3 velocity { get; set; }
    public Vector3 nearestNeighbor { get; set; }
    public List<int> neighborIdxs { get; set; }
    public Vector3 randomOtherPBest { get; set; }

    public Boid() {
        gBest = Vector3.zero;
        pBest = Vector3.zero;
        lBest = Vector3.zero;
        neighborIdxs = new List<int>();
    }

    public void Action(Transform bird) {

        // Update personal, global and local bests
        float boidToGoal = Vector3.Distance(Flock.goal, position);
        if (boidToGoal < Vector3.Distance(Flock.goal, pBest)) pBest = position;
        if (boidToGoal < Vector3.Distance(Flock.goal, Boid.gBest)) Boid.gBest = position;
        if (boidToGoal < Vector3.Distance(Flock.goal, lBest)) lBest = position;

        // Vector3 nearestNeighbor = Vector3.zero;
        // float nearestNeighborDistance = float.MaxValue;
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

        if (Vector3.Distance(position, nearestNeighbor) < Flock.minDistance) {
            Vector3 direction = (position - nearestNeighbor).normalized;
            velocity = direction;
        } else {
            switch (SceneController.algorithm) {
                case 0: // Global version of PSO
                    float x = velocity.x + (SceneController.c1 * Random.Range(0f, 1f) * (pBest.x - position.x)) + (SceneController.c2 * Random.Range(0f, 1f) * (Boid.gBest.x - position.x));
                    float y = velocity.y + (SceneController.c1 * Random.Range(0f, 1f) * (pBest.y - position.y)) + (SceneController.c2 * Random.Range(0f, 1f) * (Boid.gBest.y - position.y));
                    float z = velocity.z + (SceneController.c1 * Random.Range(0f, 1f) * (pBest.z - position.z)) + (SceneController.c2 * Random.Range(0f, 1f) * (Boid.gBest.z - position.z));
                    velocity = new Vector3(x, y, z);
                    break;
                case 1: // Local version of PSO
                    x = velocity.x + (SceneController.c1 * Random.Range(0f, 1f) * (pBest.x - position.x)) + (SceneController.c2 * Random.Range(0f, 1f) * (lBest.x - position.x));
                    y = velocity.y + (SceneController.c1 * Random.Range(0f, 1f) * (pBest.y - position.y)) + (SceneController.c2 * Random.Range(0f, 1f) * (lBest.y - position.y));
                    z = velocity.z + (SceneController.c1 * Random.Range(0f, 1f) * (pBest.z - position.z)) + (SceneController.c2 * Random.Range(0f, 1f) * (lBest.z - position.z));
                    velocity = new Vector3(x, y, z);
                    break;
                case 2: // UPSO
                    float lx = velocity.x + (SceneController.c1 * Random.Range(0f, 1f) * (pBest.x - position.x)) + (SceneController.c2 * Random.Range(0f, 1f) * (lBest.x - position.x));
                    float ly = velocity.y + (SceneController.c1 * Random.Range(0f, 1f) * (pBest.y - position.y)) + (SceneController.c2 * Random.Range(0f, 1f) * (lBest.y - position.y));
                    float lz = velocity.z + (SceneController.c1 * Random.Range(0f, 1f) * (pBest.z - position.z)) + (SceneController.c2 * Random.Range(0f, 1f) * (lBest.z - position.z));
                    float gx = velocity.x + (SceneController.c1 * Random.Range(0f, 1f) * (pBest.x - position.x)) + (SceneController.c2 * Random.Range(0f, 1f) * (Boid.gBest.x - position.x));
                    float gy = velocity.y + (SceneController.c1 * Random.Range(0f, 1f) * (pBest.y - position.y)) + (SceneController.c2 * Random.Range(0f, 1f) * (Boid.gBest.y - position.y));
                    float gz = velocity.z + (SceneController.c1 * Random.Range(0f, 1f) * (pBest.z - position.z)) + (SceneController.c2 * Random.Range(0f, 1f) * (Boid.gBest.z - position.z));
                    // Combine global and local components using the unification factor
                    Vector3 global = new Vector3(gx, gy, gz) * SceneController.u;
                    Vector3 local = new Vector3(lx, ly, lz) * (1 - SceneController.u);
                    velocity = global + local;
                    break;
                case 3: // CLPSO
                    // The learning probability for CLPSO is based on the current index and the flock size
                    float lp = 0.05f + 0.45f * (Mathf.Exp(10 * 1 / SceneController.flockSize - 1) - 1) / (Mathf.Exp(10) - 1);
                    float pBestX = Random.Range(0, 101) <= (1 - lp) * 100 ? randomOtherPBest.x : pBest.x;
                    float pBestY = Random.Range(0, 101) <= (1 - lp) * 100 ? randomOtherPBest.y : pBest.y;
                    float pBestZ = Random.Range(0, 101) <= (1 - lp) * 100 ? randomOtherPBest.z : pBest.z;
                    // Decide based on a learning probability whether or not to use my pBest or the pBest of a random neighbor
                    x = velocity.x + (SceneController.c1 * Random.Range(0f, 1f) * (pBestX - position.x));
                    y = velocity.y + (SceneController.c1 * Random.Range(0f, 1f) * (pBestY - position.y));
                    z = velocity.z + (SceneController.c1 * Random.Range(0f, 1f) * (pBestZ - position.z));
                    velocity = new Vector3(x, y, z);
                    break;
                case 4: // ELPSO
                    // Update the example set according to ELSPO
                    if (exampleSet.Count == 0) exampleSet.Add(Boid.gBest);
                    else if (exampleSet.Count <= SceneController.exampleSetSize) {
                        bool oneBetter = false;
                        for (int j = 0; j < exampleSet.Count; ++j) {
                            if (Vector3.Distance(Flock.goal, exampleSet[j]) < Vector3.Distance(Flock.goal, Boid.gBest)) {
                                oneBetter = true;
                                break;
                            }
                        }
                        if (!oneBetter) {
                            if (exampleSet.Count == SceneController.exampleSetSize) exampleSet.RemoveAt(0); // First in, first out
                            exampleSet.Add(Boid.gBest);
                        }
                    }
                    Vector3 exampleBest = exampleSet[Random.Range(0, exampleSet.Count)];
                    x = SceneController.w * velocity.x + (SceneController.c1 * Random.Range(0f, 1f) * (randomOtherPBest.x - position.x)) + (SceneController.c2 * Random.Range(0f, 1f) * (exampleBest.x - position.x));;
                    y = SceneController.w * velocity.y + (SceneController.c1 * Random.Range(0f, 1f) * (randomOtherPBest.y - position.y)) + (SceneController.c2 * Random.Range(0f, 1f) * (exampleBest.y - position.y));;
                    z = SceneController.w * velocity.z + (SceneController.c1 * Random.Range(0f, 1f) * (randomOtherPBest.z - position.z)) + (SceneController.c2 * Random.Range(0f, 1f) * (exampleBest.z - position.z));;
                    velocity = new Vector3(x, y, z);
                    break;
            }
        }

        Vector3 forward = bird.forward;
        Vector3 current = new Vector3(position.x + forward.x, position.y + forward.y, position.z + forward.z);
        Vector3 target = new Vector3(position.x + velocity.x, position.y + velocity.y, position.z + velocity.z);
        bird.LookAt(Vector3.Slerp(current, target, Time.deltaTime));
        forward = bird.forward;

        // Always move "forward", direction now facing
        float speed = SceneController.maxVelocity;
        // If the bird is pointing down at all, the speed is increased based on how much it's pointing down
        if (Vector3.Dot(forward, Vector3.down) > -0.4f)
            speed *= 1 + Vector3.Dot(forward, Vector3.down) / 2;
        bird.position += forward * speed * Time.deltaTime;
        position = bird.position;
        velocity = forward * speed;

        if  (Vector3.Distance(pBest, Flock.goal) < 5)
            Flock.goal = new Vector3(Random.Range(-50.0f, 50.0f), Random.Range(10.0f, 70.0f), Random.Range(-50.0f, 50.0f));
    }
}
