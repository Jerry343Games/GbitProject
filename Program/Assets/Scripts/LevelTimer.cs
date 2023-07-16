using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

public class LevelTimer : MonoBehaviour
{
    public AudioClip bgmClip; // ±≥æ∞“Ù¿÷µƒAudioClip
    public AudioSource bgmAudioSource = new AudioSource();
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
        bgmAudioSource = gameObject.AddComponent<AudioSource>();
        bgmAudioSource.clip = bgmClip;
        bgmAudioSource.volume = 0.4f;
        bgmAudioSource.loop = true;
        PlayBGM();
    }

    public void PlayBGM()
    {
        bgmAudioSource.Play();
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
                //πÿø®Ω· ¯
                Debug.Log("πÿø®Ω· ¯£°");
                GameStart.Instance.SetGameStarterFalse();

                curLevel++;
                remainingTime = 300f;
                isNight = false;


            }
        }
    }
}
