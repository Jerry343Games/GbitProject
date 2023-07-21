using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public static GameStart Instance { get; private set; } = new GameStart();

    public static bool gameStarter = false;
    public static int neededPlayer = 4;
    public static int curPlayer = 0;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public int GetCurPlayer()
    {
        return curPlayer;
    }

    public bool GetGameStarter()
    {
        return gameStarter;
    }
    public void SetGameStarterFalse()
    {
        gameStarter = false;
        curPlayer = 0;
    }
    public void SetGameStarterTrue()
    {
        gameStarter = true;
        curPlayer = 4;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Normal"))
        {
            curPlayer++;
            Debug.Log("当前人数:"+curPlayer);
            if (curPlayer == neededPlayer)
            {
                gameStarter = true;
                // 从0到3之间选择一个数
                int selectedNumber = Random.Range(0, 4);

                // 剩下的数
                int remainingNumber = -1;

                // 根据选中的数，选择剩下的数
                if (selectedNumber == 0)
                {
                    remainingNumber = Random.Range(1, 4);
                }
                else if (selectedNumber == 1)
                {
                    remainingNumber = Random.Range(0, 1);
                }
                else if (selectedNumber == 2)
                {
                    remainingNumber = Random.Range(0, 2);
                }
                else if (selectedNumber == 3)
                {
                    remainingNumber = Random.Range(0, 3);
                }

                GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
                foreach (GameObject go in allGameObjects)
                {
                    if (go.CompareTag("Normal") && go.transform.GetComponent<PlayerController>().playerInput.playerIndex == selectedNumber)
                    {
                        go.transform.GetComponent<PlayerController>().tag = "Ghost";
                        go.transform.GetComponent<PlayerController>().myCharacter.SetActive(false);
                        go.transform.GetComponent<PlayerController>().Ghost.SetActive(true);
                        go.transform.GetComponent<PlayerController>().skillCD = 0f;
                    }
                    if(go.CompareTag("Normal") && go.transform.GetComponent<PlayerController>().playerInput.playerIndex == remainingNumber)
                    {
                        go.transform.GetComponent<PlayerController>().hasBell = true;
                        go.transform.GetComponent<PlayerController>().UIBell.SetActive(true);
                    }
                }
            }
            Destroy(gameObject);
        }
    }
}

