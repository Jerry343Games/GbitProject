using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LevelTimer : MonoBehaviour
{
    public static LevelTimer Instance { get; private set; } = new LevelTimer();

    public static float remainingTime = 300f;

    public static int curLevel = 1;

    public static bool isNight = false;

    public float GetCurTime()
    {
        return remainingTime;
    }

    public int GetCurLevel()
    {
        return curLevel;
    }

    public bool GetIsNight()
    {
        return isNight;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameStart.Instance.GetGameStarter())
        {
            remainingTime = Math.Max(0f, remainingTime - Time.deltaTime);
            if(remainingTime < 30f)
            {

            }
            if(remainingTime == 0)
            {
                //关卡结束
                Debug.Log("关卡结束！");
                GameStart.Instance.SetGameStarterFalse();

                curLevel++;
                remainingTime = 300f;
                isNight = false;


            }
        }
    }
}
