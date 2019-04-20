using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Diagnostics;
using UnityEngine.Profiling;

public class Flock : MonoBehaviour
{
    public static Vector3 goal = new Vector3(50f, 50f, 0f);
    public static float minDistance = 1f;
    public PlayButton playButton;
    public Vector3 flockCenter;
    public List<GameObject> boidTemplates;
    public Transform sphere;
    public bool colors;
    private Stopwatch timer;
    private List<float> times;
    private KdTree<Boid> birds;

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
            for (int i = 0; i < SceneController.flockSize; ++i)
            {
                // Profiler.BeginSample("State Vector Loop");
                Boid bird = birds[i];
                Vector3 position = bird.position;
                totalX += position.x;
                totalY += position.y;
                totalZ += position.z;
                count++;

                // Sense
                bird.nearestNeighbor = birds.FindClosest(position).position;
                bird.randomOtherPBest = birds[Random.Range(0, SceneController.flockSize)].pBest; // For CLPSO and ELPSO

                // For Local PSO and UPSO, check neighbors for LBEST values
                if (SceneController.algorithm == 1 || SceneController.algorithm == 2) {
                    // If any of my neighbor's lBest is better than mine, update mine
                    for (int j = 0; j < SceneController.n; ++j) {
                        if (Vector3.Distance(goal, birds[bird.neighborIdxs[j]].lBest) < Vector3.Distance(goal, birds[i].lBest)) birds[i].lBest = birds[bird.neighborIdxs[j]].lBest;
                    }
                }

                // Act
                bird.Action(transform.GetChild(i));

                // Profiler.EndSample();
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

        birds = new KdTree<Boid>();
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
            Boid stateVector = new Boid {
                position = position,
                velocity = velocity
            };

            // Initialize neighborhood
            int idx = i;
            stateVector.neighborIdxs.Add(idx);
            for (int j = 0; j < SceneController.n - 1; ++j) {
                idx++;
                if (idx == SceneController.flockSize) idx = 0;
                stateVector.neighborIdxs.Add(idx);
            }

            birds.Add(stateVector);

            // Initialize gBest with best randomly generated position
            if (Vector3.Distance(Flock.goal, position) < Vector3.Distance(Flock.goal, Boid.gBest)) Boid.gBest = position;
        }
        playButton.lastFlockSize = SceneController.flockSize;
        Time.timeScale = 1f;
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
