using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBird : Boid
{
    // gBest and pBest are the positions with the shortest distance to the goal point
    public static Vector3 gBest = Vector3.zero;
    private Vector3 pBest = Vector3.zero;
    private float c1 = 2f;
    private float c2 = 2f;

    // Start is called before the first frame update
    void Start()
    {
        this.maxVelocity = 20f;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 goal = transform.parent.GetComponent<Flock>().goal;
        // Update personal and global bests
        if (Vector3.Distance(goal, transform.position) < Vector3.Distance(goal, pBest)) pBest = transform.position;
        if (Vector3.Distance(goal, transform.position) < Vector3.Distance(goal, gBest)) gBest = transform.position;

        // Handle PSO here
        float x = this.v.x + (c1 * Random.Range(0f, 1f) * (pBest.x - transform.position.x)) + (c2 * Random.Range(0f, 1f) * (gBest.x - transform.position.x));
        float y = this.v.y + (c1 * Random.Range(0f, 1f) * (pBest.y - transform.position.y)) + (c2 * Random.Range(0f, 1f) * (gBest.y - transform.position.y));
        float z = this.v.z + (c1 * Random.Range(0f, 1f) * (pBest.z - transform.position.z)) + (c2 * Random.Range(0f, 1f) * (gBest.z - transform.position.z));
        
        this.v = new Vector3(x, y, z);

        Vector3 nearestNeighbor = Vector3.zero;
        float nearestNeighborDistance = Mathf.Infinity;
        foreach (Transform other in transform.parent.transform) {
            if (transform != other) {
                float distance = Vector3.Distance(transform.position, other.position);
                if (distance < nearestNeighborDistance) {
                    nearestNeighborDistance = distance;
                    nearestNeighbor = other.position;
                }
            }
        }
        if (nearestNeighborDistance < 1f) {
            Vector3 direction = (transform.position - nearestNeighbor).normalized;
            this.v = direction;
        }

        Vector3 current = new Vector3(transform.position.x + transform.forward.x, transform.position.y + transform.forward.y, transform.position.z + transform.forward.z);
        Vector3 target = new Vector3(transform.position.x + this.v.x, transform.position.y + this.v.y, transform.position.z + this.v.z);
        transform.LookAt(Vector3.Slerp(current, target, Time.deltaTime));

        // Temporary key controls to get correct flight model
        // if (Input.GetKey(KeyCode.A)) {
        //     transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y - 1f, transform.localEulerAngles.z);
        // }
        // if (Input.GetKey(KeyCode.D)) {
        //     transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y + 1f, transform.localEulerAngles.z);
        // }
        // if (Input.GetKey(KeyCode.S)) {
        //     transform.localEulerAngles = new Vector3(transform.localEulerAngles.x - 1f, transform.localEulerAngles.y, transform.localEulerAngles.z);
        // }
        // if (Input.GetKey(KeyCode.W)) {
        //     transform.localEulerAngles = new Vector3(transform.localEulerAngles.x + 1f, transform.localEulerAngles.y, transform.localEulerAngles.z);
        // }

        if  (Vector3.Distance(pBest, goal) < 5)
            transform.parent.GetComponent<Flock>().goal = new Vector3(Random.Range(-100.0f, 100.0f), Random.Range(10.0f, 70.0f), Random.Range(-100.0f, 100.0f));

        // Always move "forward", direction now facing
        float speed = this.maxVelocity;
        // If the bird is pointing down at all, the speed is increased based on how much it's pointing down
        if (Vector3.Dot(transform.forward, Vector3.down) > -0.4f)
            speed *= 1 + Vector3.Dot(transform.forward, Vector3.down) / 2;
        transform.position += transform.forward * speed * Time.deltaTime;
        this.v = transform.forward * speed;
    }

    private IEnumerator LevelOut() {
        Quaternion target = Quaternion.Euler(new Vector3(0f, transform.localEulerAngles.y, 0f));
        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime);
        yield return null;
        StartCoroutine(LevelOut());
    }

}
