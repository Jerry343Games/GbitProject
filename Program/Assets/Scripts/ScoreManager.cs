using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private int totalScore = 0;
    private float lastIncreaseTime = 0f;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public int GetTotalScore()
    {
        return totalScore;
    }

    public void SetTotalScore(int curScore)
    {
        totalScore = curScore;
    }
}
