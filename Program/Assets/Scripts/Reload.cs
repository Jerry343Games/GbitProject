using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class Reload:MonoBehaviour
{
    public Transform R0;
    public Transform R1;
    public Transform R2;
    public Transform R3;

    public static Vector3 P0;
    public static Vector3 P1;
    public static Vector3 P2;
    public static Vector3 P3;


    private void OnEnable()
    {
        P0 = R0.position;
        P1 = R1.position;
        P2 = R2.position;
        P3 = R3.position;

    }
    //private void Start()
    //{
    //    P0 = R0.position;
    //    P1 = R1.position;
    //    P2 = R2.position;
    //    P3 = R3.position;
    //    Debug.Log(P0 + P1 + P2 + P3);
    //}
}
