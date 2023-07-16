using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldGenerate : MonoBehaviour
{
    public static GoldGenerate Instance { get; private set; } = new GoldGenerate();

    public GameObject coinPrefab;

    public Vector2 areaSizeX;
    public Vector2 areaSizeY;

    public Vector2 areaSizeX1;
    public Vector2 areaSizeY1;

    public Vector2 areaSizeX2;
    public Vector2 areaSizeY2;

    public Vector2 areaSizeX22;
    public Vector2 areaSizeY22;

    public Vector2 areaSizeX3;
    public Vector2 areaSizeY3;

    public Vector2 areaSizeX33;
    public Vector2 areaSizeY33;

    public static int coinCount = 2;

    public float duaration = 10f;
    private float curDuaration = 10f;

    void Start()
    {

    }

    void Update()
    {
        if (GameStart.Instance.GetGameStarter())
        {
            curDuaration -= Time.deltaTime;
            if (curDuaration <= 0)
            {
                curDuaration = duaration;
                GenerateCoins();
            }
        }
    }

    public void SetCoin(int num)
    {
        coinCount = num;
    }

    private void GenerateCoins()
    {
        for (int i = 0; i < coinCount; i++)
        {
            float randomX = Random.Range(areaSizeX.x, areaSizeX.y);
            float randomZ = Random.Range(areaSizeY.x, areaSizeY.y);
            Vector3 spawnPosition = new Vector3(randomX, -1.83f, randomZ);
            GameObject coin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
            StartCoroutine(DelayMyselfInvis(coin));
        }
        for (int i = 0; i < coinCount; i++)
        {
            float randomX = Random.Range(areaSizeX1.x, areaSizeX1.y);
            float randomZ = Random.Range(areaSizeY1.x, areaSizeY1.y);
            Vector3 spawnPosition = new Vector3(randomX, -1.83f, randomZ);
            GameObject coin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
            StartCoroutine(DelayMyselfInvis(coin));
        }
        for (int i = 0; i < coinCount; i++)
        {
            float randomX = Random.Range(areaSizeX2.x, areaSizeX2.y);
            float randomZ = Random.Range(areaSizeY2.x, areaSizeY2.y);
            Vector3 spawnPosition = new Vector3(randomX, -1.83f, randomZ);
            GameObject coin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
            StartCoroutine(DelayMyselfInvis(coin));
        }
        for (int i = 0; i < coinCount; i++)
        {
            float randomX = Random.Range(areaSizeX22.x, areaSizeX22.y);
            float randomZ = Random.Range(areaSizeY22.x, areaSizeY22.y);
            Vector3 spawnPosition = new Vector3(randomX, -1.83f, randomZ);
            GameObject coin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
            StartCoroutine(DelayMyselfInvis(coin));
        }
        for (int i = 0; i < coinCount; i++)
        {
            float randomX = Random.Range(areaSizeX3.x, areaSizeX3.y);
            float randomZ = Random.Range(areaSizeY3.x, areaSizeY3.y);
            Vector3 spawnPosition = new Vector3(randomX, -1.83f, randomZ);
            GameObject coin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
            StartCoroutine(DelayMyselfInvis(coin));
        }
        for (int i = 0; i < coinCount; i++)
        {
            float randomX = Random.Range(areaSizeX33.x, areaSizeX33.y);
            float randomZ = Random.Range(areaSizeY33.x, areaSizeY33.y);
            Vector3 spawnPosition = new Vector3(randomX, -1.83f, randomZ);
            GameObject coin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
            StartCoroutine(DelayMyselfInvis(coin));
        }
    }
    IEnumerator DelayMyselfInvis(GameObject coin)
    {
        yield return new WaitForSeconds(10f);
        if (coin != null)
        {
            Destroy(coin);
        }
    }
}
