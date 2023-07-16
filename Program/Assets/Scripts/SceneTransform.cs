using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransform : MonoBehaviour
{
    public Transform P0;
    public Transform P1;
    public Transform P2;
    public Transform P3;
    public Transform ChampPos;
    public GameObject c0;
    public GameObject c1;
    public GameObject c2;
    public GameObject c3;
    public static int ChampNum;
    public Camera SceneCam;
    public Transform[] CamPos;
    
    // Start is called before the first frame update
    void Start()
    {
                 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void ChangePosition(int champNum)
    {
        switch (champNum)
        {
            case 0:
                c0.transform.position = ChampPos.position;
                c1.transform.position = P1.position;
                c2.transform.position = P2.position;
                c3.transform.position = P3.position;
                break;
            case 1:
                c1.transform.position = ChampPos.position;
                c0.transform.position = P0.position;
                c2.transform.position = P2.position;
                c3.transform.position = P3.position;
                break;
            case 2:
                c2.transform.position = ChampPos.position;
                c0.transform.position = P0.position;
                c1.transform.position = P1.position;
                c3.transform.position = P3.position;
                break;
            case 3:
                c3.transform.position = ChampPos.position;
                c0.transform.position = P0.position;
                c2.transform.position = P2.position;
                c1.transform.position = P1.position;
                break;
        }
    }
    void CheckTimer()
    {
        if (LevelTimer.remainingTime == 0)
        {
            SceneCam.transform.position = CamPos[1].position;
        }
    }
    
}
