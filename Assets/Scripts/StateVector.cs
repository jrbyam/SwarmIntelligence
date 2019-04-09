using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateVector
{
    public static Vector3 gBest { get; set; }
    public Vector3 pBest { get; set; }
    public Vector3 lBest { get; set; }
    public Vector3 position { get; set; }
    public Vector3 velocity { get; set; }

    public StateVector() {
        gBest = Vector3.zero;
        pBest = Vector3.zero;
        lBest = Vector3.zero;
    }
}
