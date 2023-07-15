using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEditor.Progress;
using UnityEditor;
using System.Reflection;

public class PlayerController : MonoBehaviour
{
    private int camResult = -1;
    private Rigidbody playerRigidbody;
    public int healthy;
    private float skillCD = 10f;

    /// <summary>
    /// 玩家移动方向向量
    /// </summary>
    private Vector3 moveDir;
    private Vector2 InputMove;
    public PlayerInput playerInput;
    public bool isOpposite;

    private float InputInteraction = 0f;

    [Header("ChooseCam")]
    public bool HD2D;
    public bool THIRD;
    [Header("MainCam")]
    public Transform tracePoint;
    public float smooth;
    private Vector3 camSpeed;

    [Header("PlayerBaseSpeed")]
    public float baseSpeed;//基础速度

    [Header("PlayerCurSpeed")]
    public float speed;//当前速度

    [Header("PlayerScore")]
    public int score;//分数

    [Header("TotalScore")]
    public int totalScore;//总分

    [Header("HasBell")]
    public bool hasBell;//是否带有铃铛

    [Header("IsBoosted")]
    public bool isBoosted;// 是否处于挨打加速状态

    [Header("IsStunned")]
    public bool isStunned;// 是否处于晕眩状态

    [Header("IsHolding")]
    public bool isHolding;// 是否处于持铃状态

    [Header("BoostDuration")]
    public float boostDuration; // 加速持续时间

    [Header("StunDuration")]
    public float stunDuration;// 晕眩持续时间

    [Header("BellDuration")]
    public float bellDuration;// 铃铛加分间隔时间


    [Header("PlayerAnimator")]
    public Animator animator;//动画机

    [Header("AddForce")]
    public float force;//碰撞传递的力

    [Header("UIHurt")]
    public GameObject UIHurt;//受伤标记
    public bool isInvincible;

    [Header("UIBell")]
    public GameObject UIBell;//铃铛标记

    private void OnEnable()
    {

    }
    void Start()
    {
        camResult = CameraCheck();
        playerRigidbody = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        StartCoroutine(DelaySetPosition());
        baseSpeed = 4;
        healthy = 1;
        score = 0;
        isOpposite = false;
        hasBell = false;
        isBoosted = false;// 是否处于挨打加速状态
        isStunned = false;// 是否处于晕眩状态
        isHolding = false;// 是否处于持铃状态
        boostDuration = 3f; // 加速持续时间
        stunDuration = 5f; // 晕眩持续时间
        bellDuration = 10f; // 铃铛加分间隔时间
        skillCD = 10f;//

        UIBell.SetActive(false);

        if (transform.CompareTag("Normal"))
        {
            speed = baseSpeed;
        }
        else
        {
            speed = 1.2f * baseSpeed;
        }
    }

    void Update()
    {
        PlayerMovement();

        PlayerInteraction();

        //晕眩和加速状态
        if (isStunned)
        {
            speed = 0f;
            stunDuration -= Time.deltaTime;
            if (stunDuration <= 0f)
            {
                // 晕眩时间结束，恢复 鬼的 正常速度
                isStunned = false;
                speed = 1.2f * baseSpeed;

                //****************更新模型为正常的鬼、音效****************
            }
        }
        else if (isBoosted)
        {
            speed = 1.3f * baseSpeed;
            boostDuration -= Time.deltaTime;
            if (boostDuration <= 0f)
            {
                // 加速时间结束，恢复人的正常速度
                isBoosted = false;
                speed = baseSpeed;
            }
        }
        else if (hasBell)
        {
            speed = 0.8f * baseSpeed;
        }
        else if (transform.CompareTag("Ghost"))
        {
            speed = 1.2f * baseSpeed;
        }

        //技能CD
        if (transform.CompareTag("Normal"))
        {
            skillCD = 10f;
        }
        else
        {
            skillCD = Mathf.Max(skillCD-Time.deltaTime,0);
        }

        //持铃加分
        if (hasBell)
        {
            bellDuration -= Time.deltaTime;
            if (bellDuration <= 0f)
            {
                // 铃铛加分
                int addScore = 5 + (int)Math.Floor(totalScore * 0.01);
                AddScore(addScore);
                bellDuration = 10f;
            }
        }

        ////铃铛标识，用于调试
        if (hasBell)
        {
            UIBell.SetActive(true);
        }
        else
        {
            UIBell.SetActive(false);
        }
        ///
        //if (hasBell)
        //{
        //    Vector3 newScale = new Vector3(2, 2, 2);
        //    transform.localScale = newScale;
        //}
        //else
        //{
        //    Vector3 newScale = new Vector3(1, 1, 1);
        //    transform.localScale = newScale;
        //}
    }

    /// <summary>
    /// 用于接收InputAction返回的玩家输入数据
    /// </summary>
    /// <param name="value0"></param>
    public void OnMovement(InputAction.CallbackContext value0)
    {
        if (isOpposite)
        {
            InputMove = -1 * value0.ReadValue<Vector2>();
        }
        else
        {
            InputMove = value0.ReadValue<Vector2>();
        }
    }

    /// <summary>
    /// 玩家移动方法
    /// </summary>
    void PlayerMovement()
    {
        if (GameStart.Instance.GetGameStarter())
        {

            //动画器条件设置和Sprite翻转
            if (playerRigidbody.velocity.x > 0)
            {
                transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
                animator.SetBool("isRunSide", true);
                animator.SetBool("isRunFount", false);

            }
            else if (playerRigidbody.velocity.x < 0)
            {
                transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
                animator.SetBool("isRunSide", true);
                animator.SetBool("isRunFount", false);
            }
            else if (playerRigidbody.velocity.z < 0 && playerRigidbody.velocity.x == 0)
            {
                animator.SetBool("isRunFount", true);
                animator.SetBool("isRunSide", false);
            }
            else if (playerRigidbody.velocity.z > 0 && playerRigidbody.velocity.x == 0)
            {
                animator.SetBool("isRunFount", false);
                animator.SetBool("isRunSide", false);
            }
            else
            {
                animator.SetBool("isRunFount", false);
                animator.SetBool("isRunSide", false);
            }

            //移动方向
            moveDir = (new Vector3(InputMove.x, 0, InputMove.y)).normalized * speed;
            playerRigidbody.velocity = moveDir;

            //模型朝向
            //Vector3 lookDir = transform.position + moveDir;
            //playerModel.transform.LookAt(lookDir);
        }
    }
    
    public void OnInteractive(InputAction.CallbackContext value0)
    {
        InputInteraction = value0.ReadValue<float>();
        //Debug.Log(InputInteraction);
    }

    void PlayerInteraction()
    {
        if(transform.CompareTag("Ghost") && InputInteraction == 1)
        {
            if(skillCD == 0)
            {
                skillCD = 10f;
                GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
                foreach (GameObject go in allGameObjects)
                {
                    if (go.CompareTag("Normal"))
                    {
                        StartCoroutine(go.transform.GetComponent<PlayerController>().IsOpposite());
                    }
                }
            }
            else
            {
                Debug.Log("技能尚未冷却！！");
            }
        }
    }

    //修改分数
    public void AddScore(int addScore)
    {
        score += addScore;

        //总分
        totalScore = 0;
        GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject go in allGameObjects)
        {
            if (go.CompareTag("Normal") || go.CompareTag("Ghost"))
            {
                totalScore += go.transform.GetComponent<PlayerController>().score;
            }
        }
        Debug.Log(totalScore);

        //计分板
        int index = playerInput.playerIndex;
        GameObject targetObject;
        Text textComponent;
        switch (index)
        {
            case 0:
                targetObject = GameObject.Find("Text0");
                textComponent = targetObject.GetComponent<Text>();
                if (textComponent != null)
                {
                    textComponent.text = score.ToString();
                }
                break;
            case 1:
                targetObject = GameObject.Find("Text1");
                textComponent = targetObject.GetComponent<Text>();
                if (textComponent != null)
                {
                    textComponent.text = score.ToString();
                }
                break;
            case 2:
                targetObject = GameObject.Find("Text2");
                textComponent = targetObject.GetComponent<Text>();
                if (textComponent != null)
                {
                    textComponent.text = score.ToString();
                }
                break;
            case 3:
                targetObject = GameObject.Find("Text3");
                textComponent = targetObject.GetComponent<Text>();
                if (textComponent != null)
                {
                    textComponent.text = score.ToString();
                }
                break;
            default:
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //如果该玩家是正常且被鬼碰撞
        if (transform.CompareTag("Normal") && collision.transform.CompareTag("Ghost") && !collision.transform.GetComponent<PlayerController>().isStunned)
        {
            //镜头晃动
            GameObject mainCameraObj = GameObject.Find("Main Camera");
            mainCameraObj.GetComponent<CameraShake>().Shake(0.5f, 0.1f);

            Debug.Log(collision.transform.tag + collision.transform.GetComponent<PlayerController>().healthy);
            //自己血量大于0时
            if (healthy > 0 && !isInvincible)
            {
                UIHurt.SetActive(true);
                collision.transform.GetComponent<PlayerController>().UIHurt.SetActive(false);
                healthy--;
                collision.transform.GetComponent<PlayerController>().isInvincible = true;
                collision.transform.GetComponent<PlayerController>().SetInvisFalse();
                isInvincible = true;
                StartCoroutine(DelayMyselfInvis());
                playerRigidbody.AddForce(new Vector3((transform.position - collision.transform.position).x, 0.2f, (collision.transform.position - transform.position).z) * force, ForceMode.Impulse);

                //加速
                boostDuration = 3f;
                isBoosted = true;

                if (hasBell)
                {
                    //删除头上的铃铛
                    UIBell.SetActive(false);

                    //算分
                    int addScore = 6 + (int)Math.Floor(0.1 * totalScore);
                    collision.transform.GetComponent<PlayerController>().AddScore(addScore);
                    totalScore = collision.transform.GetComponent<PlayerController>().totalScore;

                    //随机转移铃铛
                    hasBell = false;
                    bellDuration = 10f;
                    List<int> list = new List<int>();
                    int index = playerInput.playerIndex;
                    GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
                    foreach (GameObject go in allGameObjects)
                    {
                        if (go.CompareTag("Normal") && go.transform.GetComponent<PlayerController>().playerInput.playerIndex != index)
                        {
                            list.Add(go.transform.GetComponent<PlayerController>().playerInput.playerIndex);
                        }
                    }
                    int randomIndex = UnityEngine.Random.Range(0, list.Count);
                    int bellAdder = list[randomIndex];
                    foreach (GameObject go in allGameObjects)
                    {
                        if (go.CompareTag("Normal") && go.transform.GetComponent<PlayerController>().playerInput.playerIndex == bellAdder)
                        {
                            go.transform.GetComponent<PlayerController>().hasBell = true;
                            go.transform.GetComponent<PlayerController>().UIBell.SetActive(true);
                            break;
                        }
                    }
                }
                else
                {
                    int addScore = 4 + (int)Math.Floor(0.05 * totalScore);
                    collision.transform.GetComponent<PlayerController>().AddScore(addScore);
                    totalScore = collision.transform.GetComponent<PlayerController>().totalScore;
                }
            }
            //血量为0时
            else if (healthy <= 0 && !isInvincible)
            {
                GameObject[] allGameObjects;
                if (hasBell)
                {
                    //删除头上的铃铛
                    UIBell.SetActive(false);

                    //算分
                    int addScore = 10 + (int)Math.Floor(0.2 * totalScore);
                    collision.transform.GetComponent<PlayerController>().AddScore(addScore);
                    totalScore = collision.transform.GetComponent<PlayerController>().totalScore;

                    //随机转移铃铛
                    hasBell = false;
                    bellDuration = 10f;
                    List<int> list = new List<int>();
                    int index = playerInput.playerIndex;
                    allGameObjects = GameObject.FindObjectsOfType<GameObject>();
                    foreach (GameObject go in allGameObjects)
                    {
                        if (go.CompareTag("Normal") && go.transform.GetComponent<PlayerController>().playerInput.playerIndex != index)
                        {
                            list.Add(go.transform.GetComponent<PlayerController>().playerInput.playerIndex);
                        }
                    }
                    int randomIndex = UnityEngine.Random.Range(0, list.Count);
                    int bellAdder = list[randomIndex];
                    foreach (GameObject go in allGameObjects)
                    {
                        if (go.CompareTag("Normal") && go.transform.GetComponent<PlayerController>().playerInput.playerIndex == bellAdder)
                        {
                            go.transform.GetComponent<PlayerController>().hasBell = true;
                            go.transform.GetComponent<PlayerController>().UIBell.SetActive(true);
                            break;
                        }
                    }
                }
                else
                {
                    int addScore = 6 + (int)Math.Floor(0.1 * totalScore);
                    collision.transform.GetComponent<PlayerController>().AddScore(addScore);
                    totalScore = collision.transform.GetComponent<PlayerController>().totalScore;
                }

                //********对方，鬼变成人*********

                //晕眩
                stunDuration = 5f;
                isStunned = true;
                playerRigidbody.isKinematic = true;

                //自己，人变成鬼
                string prefabPath = "Assets/Effect/ChannelPink.prefab";
                GameObject channelPinkPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                Vector3 currentPosition = transform.position;
                Quaternion currentRotation = transform.rotation;
                GameObject instantiatedPrefab = Instantiate(channelPinkPrefab, currentPosition, currentRotation);
                Destroy(instantiatedPrefab, 4f);
                StartCoroutine(EffectBoom());

                //场上负标记清零
                allGameObjects = GameObject.FindObjectsOfType<GameObject>();
                foreach (GameObject go in allGameObjects)
                {
                    if (go.CompareTag("Normal"))
                    {
                        go.GetComponent<PlayerController>().healthy = 1;
                        go.GetComponent<PlayerController>().UIHurt.SetActive(false);
                    }
                }

                //互换Tag和加满生命值
                UIHurt.SetActive(false);
                collision.transform.GetComponent<PlayerController>().UIHurt.SetActive(false);
                //isInvincible = true;
                //StartCoroutine(DelayMyselfInvis());
                collision.transform.GetComponent<PlayerController>().isInvincible = true;
                collision.transform.GetComponent<PlayerController>().SetInvisFalse();
                transform.tag = "Ghost";
                collision.transform.tag = "Normal";
                healthy = 1;
                collision.transform.GetComponent<PlayerController>().healthy = 1;
                collision.transform.GetComponent<PlayerController>().speed = baseSpeed;

            }
        }
        //如果玩家正常且被金币碰撞
        else if (transform.CompareTag("Normal") && collision.transform.CompareTag("Coin"))
        {
            AddScore(1);
            //*****特效、音效*****

            Destroy(collision.gameObject);
        }
        //如果玩家正常且没有铃铛,碰撞道具
        /*else if (transform.CompareTag("Normal") && !hasBell && collision.transform.CompareTag(""))
        {

        }*/

    }
    IEnumerator EffectBoom()
    {
        yield return new WaitForSeconds(4f);
        //镜头晃动
        GameObject mainCameraObj = GameObject.Find("Main Camera");
        mainCameraObj.GetComponent<CameraShake>().Shake(0.5f, 0.1f);

        //恢复刚体
        playerRigidbody.isKinematic = false;

        string prefabPath = "Assets/Effect/GhostExplosion.prefab";
        GameObject GhostExplosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Vector3 currentPosition = transform.position;
        Quaternion currentRotation = transform.rotation;
        GameObject instantiatedPrefab = Instantiate(GhostExplosionPrefab, currentPosition, currentRotation);
        Destroy(instantiatedPrefab, 1f);
    }
    IEnumerator IsOpposite()
    {
        isOpposite = true;
        yield return new WaitForSeconds(5f);
        isOpposite = false;
    }
    IEnumerator DelayMyselfInvis()
    {
        yield return new WaitForSeconds(1f);
        isInvincible = false;
    }
    IEnumerator DelaySetPosition()
    {
        yield return new WaitForSeconds(0.1f);
        SetStartPosition();
    }
    public void SetInvisFalse()
    {
        StartCoroutine(DelayMyselfInvis());
    }
    
    /// <summary>
    /// 设置初始位置
    /// </summary>
    private void SetStartPosition()
    {

        if (playerInput.playerIndex == 0)
        {
            transform.position = Reload.P0;
        }
        if (playerInput.playerIndex == 1)
        {
            transform.position = Reload.P1;
        }
        if (playerInput.playerIndex == 2)
        {
            transform.position = Reload.P2;
        }
        if (playerInput.playerIndex == 3)
        {
            transform.position = Reload.P3;
        }
    }

    /// <summary>
    /// 检查相机选择是否正确
    /// </summary>
    /// <returns></returns>
    int CameraCheck()
    {
        //HD2D
        if (!THIRD && HD2D)
        {
            Debug.Log("启用HD2D模式相机");
            return 0;
        }
        //第三人称
        else if (THIRD && !HD2D)
        {
            Debug.Log("启用3D模式相机");
            return 1;
        }
        else if (!THIRD && !HD2D)
        {
            Debug.LogWarning("请选择相机模式");
            return -1;
        }
        else
        {
            Debug.LogWarning("相机模式不可同时选中");
            return -1;
        }
    }

}
