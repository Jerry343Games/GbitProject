using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.UI;
using DG.Tweening;

public class LevelTimer : MonoBehaviour
{
    public AudioClip bgmClip; // ±≥æ∞“Ù¿÷µƒAudioClip
    public AudioSource bgmAudioSource = new AudioSource();
    public static LevelTimer Instance { get; private set; } = new LevelTimer();

    public static float remainingTime = 120f;

    public static int curLevel = 1;

    public static bool isNight = false;

    public Transform camera1;
    public Transform camera2;
    public Transform pos1;
    public Transform pos2;
    public Transform pos3;
    public Transform pos4;
    public Transform pos5;
    public Transform pos6;
    public Transform pos7;
    public Transform pos8;

    public Transform cameraend;
    public Transform champion;

    public Image Clock;
    public Image Black;
    
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
        Black.DOFade(0, 0.5f);
    }

    public void PlayBGM()
    {
        bgmAudioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        Clock.fillAmount = remainingTime / 120f;
        if (GameStart.Instance.GetGameStarter())
        {
            remainingTime = Math.Max(0f, remainingTime - Time.deltaTime);
            if(remainingTime < 60f)
            {
                isNight = true;
                GoldGenerate.Instance.SetCoin(4);
            }
            if(remainingTime == 0)
            {

                curLevel++;
                remainingTime = 120f;
                isNight = false;
                GoldGenerate.Instance.SetCoin(2);

                if (curLevel == 2)
                {
                    Black.DOFade(1, 0.5f);
                    StartCoroutine(Level2());
                }
                else if(curLevel == 3)
                {
                    Black.DOFade(1, 0.5f);
                    StartCoroutine(Level3());
                }
                else
                {
                    Black.DOFade(1, 0.5f);
                    StartCoroutine(LevelEnd());
                }
            }
        }
    }

    IEnumerator Level2()
    {
        GameStart.Instance.SetGameStarterFalse();
        yield return new WaitForSeconds(3f);
        Black.DOFade(0, 0.25f);
        GameStart.Instance.SetGameStarterTrue();
        Camera.main.transform.GetComponent<CameraShake>().SetShakePos(camera1.position);
        Camera.main.transform.position = camera1.position;
        Debug.Log(Camera.main.transform.position.x + " " + Camera.main.transform.position.y + " " + Camera.main.transform.position.z);

        int cur = 0;
        GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject go in allGameObjects)
        {
            if (go.CompareTag("Normal") || go.CompareTag("Ghost"))
            {
                if (cur == 0)
                {
                    go.transform.position = pos1.position;
                    cur++;
                }
                else if (cur == 1)
                {
                    go.transform.position = pos2.position;
                    cur++;
                }
                else if (cur == 2)
                {
                    go.transform.position = pos3.position;
                    cur++;
                }
                else if (cur == 3)
                {
                    go.transform.position = pos4.position;
                    cur++;
                }
            }
        }
    }

    IEnumerator Level3()
    {
        GameStart.Instance.SetGameStarterFalse();
        yield return new WaitForSeconds(3f);
        Black.DOFade(0, 0.25f);
        GameStart.Instance.SetGameStarterTrue();
        Camera.main.transform.GetComponent<CameraShake>().SetShakePos(camera2.position);
        Camera.main.transform.position = camera2.position;
        Debug.Log(Camera.main.transform.position.x + " " + Camera.main.transform.position.y + " " + Camera.main.transform.position.z);

        int cur = 0;
        GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject go in allGameObjects)
        {
            if (go.CompareTag("Normal") || go.CompareTag("Ghost"))
            {
                if (cur == 0)
                {
                    go.transform.position = pos5.position;
                    cur++;
                }
                else if (cur == 1)
                {
                    go.transform.position = pos6.position;
                    cur++;
                }
                else if (cur == 2)
                {
                    go.transform.position = pos7.position;
                    cur++;
                }
                else if (cur == 3)
                {
                    go.transform.position = pos8.position;
                    cur++;
                }
            }
        }
    }

    IEnumerator LevelEnd()
    {
        GameStart.Instance.SetGameStarterFalse();
        yield return new WaitForSeconds(3f);
        Black.DOFade(0, 0.25f);

        int owner = 0;
        int ownerScore = -999;
        GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject go in allGameObjects)
        {
            if (go.CompareTag("Normal"))
            {
                if (go.GetComponent<PlayerController>().score > ownerScore)
                {
                    //go.transform.GetComponent<PlayerController>().hasBell = true;
                    //go.transform.GetComponent<PlayerController>().UIBell.SetActive(true);
                    ownerScore = go.GetComponent<PlayerController>().score;
                    owner = go.transform.GetComponent<PlayerController>().playerInput.playerIndex;
                }
            }
        }

        Camera.main.transform.GetComponent<CameraShake>().SetShakePos(cameraend.position);
        Camera.main.transform.position = cameraend.position;
        Debug.Log(Camera.main.transform.position.x + " " + Camera.main.transform.position.y + " " + Camera.main.transform.position.z);

        allGameObjects = GameObject.FindObjectsOfType<GameObject>();
        if (owner == 0)
        {
            foreach (GameObject go in allGameObjects)
            {
                if (go.name == "CharacterGirl")
                {
                    go.transform.position = champion.position;
                    break;
                }
            }
        }
        else if (owner == 1)
        {
            foreach (GameObject go in allGameObjects)
            {
                if (go.name == "CharacterNum")
                {
                    go.transform.position = champion.position;
                    break;
                }
            }
        }
        else if (owner == 2)
        {
            foreach (GameObject go in allGameObjects)
            {
                if (go.name == "CharacterFarmer")
                {
                    go.transform.position = champion.position;
                    break;
                }
            }
        }
        else if (owner == 3)
        {
            foreach (GameObject go in allGameObjects)
            {
                if (go.name == "CharacterHelsing")
                {
                    go.transform.position = champion.position;
                    break;
                }
            }
        }
    }
}
