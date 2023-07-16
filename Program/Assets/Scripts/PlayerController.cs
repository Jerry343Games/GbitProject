using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEditor.Progress;
using UnityEditor;
using System.Reflection;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.StandaloneInputModule;

public class PlayerController : MonoBehaviour
{
    private int camResult = -1;
    private Rigidbody playerRigidbody;
    public int healthy;
    private float skillCD = 20f;

    /// <summary>
    /// 玩家移动方向向量
    /// </summary>
    private Vector3 moveDir;
    private Vector2 InputMove;
    public PlayerInput playerInput;
    public bool isOpposite;

    private float InputInteraction = 0f;
    private float InputDash = 0f;

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

    //[Header("TotalScore")]
    //public int totalScore;//总分

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

    [Header("OpDuration")]
    public float opDuration;// 转向持续时间

    [Header("PlayerAnimator")]
    public Animator animator;//动画机

    [Header("AddForce")]
    public float force;//碰撞传递的力

    [Header("UIHurt")]
    public GameObject UIHurt;//受伤标记
    public bool isInvincible;

    [Header("UIBell")]
    public GameObject UIBell;//铃铛标记

    [Header("CurrentProp")]
    private GameObject curProp; // 当前拾取的道具

    [Header("Characters")]
    public GameObject Girl;
    public GameObject Num;
    public GameObject Fan;
    public GameObject Farmer;
    public GameObject Ghost;
    public GameObject myCharacter;
    [Header("Effect")]
    public GameObject GhostSkillEffect;
    public GameObject DashEffect;

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
        opDuration = 0f;// 转向持续时间
        skillCD = 20f;//
        

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
        Dash();
        PlayerInteraction();
        if (GameStart.Instance.GetGameStarter())
        {
            bellOwner();
        }

        if (isOpposite)
        {
            opDuration -= Time.deltaTime;
            if (opDuration <= 0f)
            {
                isOpposite = false;
            }
        }

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
            speed = baseSpeed;
        }
        else if (transform.CompareTag("Ghost"))
        {
            speed = 1.2f * baseSpeed;
        }

        //技能CD
        if (transform.CompareTag("Normal"))
        {
            skillCD = 20f;
        }
        else
        {
            skillCD = Mathf.Max(skillCD-Time.deltaTime,0);
        }
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
                animator.SetBool("LeftWalk", true);
                animator.SetBool("FontWalk", false);
                animator.SetBool("BackWalk", false);
            }
            else if (playerRigidbody.velocity.x < 0)
            {
                transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
                animator.SetBool("LeftWalk", true);
                animator.SetBool("FontWalk", false);
                animator.SetBool("BackWalk", false);
            }
            else if (playerRigidbody.velocity.z < 0 && playerRigidbody.velocity.x == 0)
            {
                animator.SetBool("BackWalk", false);
                animator.SetBool("FontWalk", true);
                animator.SetBool("LeftWalk", false);
            }
            else if (playerRigidbody.velocity.z > 0 && playerRigidbody.velocity.x == 0)
            {
                animator.SetBool("FontWalk", false);
                animator.SetBool("LeftWalk", false);
                animator.SetBool("BackWalk", true);
            }
            else
            {
                animator.SetBool("FontWalk", false);
                animator.SetBool("LeftWalk", false);
                animator.SetBool("BackWalk", false);
            }

            //移动方向
            moveDir = (new Vector3(InputMove.x, 0, InputMove.y)).normalized ;
            playerRigidbody.velocity = moveDir * speed;

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

    public void OnDash(InputAction.CallbackContext value0)
    {
        InputDash = value0.ReadValue<float>();
    }

    void PlayerInteraction()
    {
        if(transform.CompareTag("Ghost") && InputInteraction == 1)
        {
            if(skillCD == 0)
            {
                Instantiate(GhostSkillEffect, transform.position,Quaternion.Euler(90,0,0));
                skillCD = 20f;
                GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
                foreach (GameObject go in allGameObjects)
                {
                    if (go.CompareTag("Normal"))
                    {
                        go.transform.GetComponent<PlayerController>().IsOpposite();
                    }
                }
            }
            else
            {
                Debug.Log("技能尚未冷却！！！");
            }
        }
        else if (transform.CompareTag("Normal") && InputInteraction == 1)
        {
            if(curProp != null)
            {
                // 解除道具与玩家的关联
                curProp.transform.SetParent(null);

                // 激活道具的刚体，并施加力使其飞出去
                Rigidbody itemRigidbody = curProp.GetComponent<Rigidbody>();
                curProp.GetComponent<PropController>().isWeapon = true;
                if (itemRigidbody != null)
                {
                    itemRigidbody.isKinematic = false;
                    //Vector3 force = new Vector3(moveDir.x, moveDir.y, 0);
                    //Debug.Log(transform.forward.x + " " + transform.forward.y + " " + transform.forward.z);
                    Debug.Log(moveDir.x + " " + moveDir.z);
                    //itemRigidbody.AddForce(transform.forward * 100, ForceMode.Impulse);

                    Vector3 force = new Vector3(moveDir.x, 0, moveDir.z);
                    itemRigidbody.AddForce(force * 10, ForceMode.Impulse);
                }

                curProp = null;
            }
            else
            {
                Debug.Log("你尚未持有道具！！！");
            }
        }
    }

    //判定铃铛所属
    public void bellOwner()
    {
        int owner = 0;
        int ownerScore = -999;
        GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject go in allGameObjects)
        {
            if (go.CompareTag("Ghost"))
            {
                //删除头上的铃铛
                go.transform.GetComponent<PlayerController>().hasBell = false;
                go.transform.GetComponent<PlayerController>().UIBell.SetActive(false);
            }
            else if (go.CompareTag("Normal"))
            {
                //删除头上的铃铛
                go.transform.GetComponent<PlayerController>().hasBell = false;
                go.transform.GetComponent<PlayerController>().UIBell.SetActive(false);

                if (go.GetComponent<PlayerController>().score > ownerScore)
                {
                    go.transform.GetComponent<PlayerController>().hasBell = true;
                    go.transform.GetComponent<PlayerController>().UIBell.SetActive(true);
                    ownerScore = go.GetComponent<PlayerController>().score;
                    owner = go.transform.GetComponent<PlayerController>().playerInput.playerIndex;
                }
            }
        }
        //有道具则删除
        allGameObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject go in allGameObjects)
        {
            if((go.CompareTag("Ghost") || go.CompareTag("Normal")) && go.transform.GetComponent<PlayerController>().playerInput.playerIndex == owner)
            {
                if(go.transform.GetComponent<PlayerController>().curProp != null)
                {
                    Destroy(go.transform.GetComponent<PlayerController>().curProp);
                    go.transform.GetComponent<PlayerController>().curProp = null;
                }
            }
        }
    }

    //修改分数
    public void AddScore(int addScore)
    {
        score += addScore;

        //总分
        //totalScore = 0;
        //GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
        //foreach (GameObject go in allGameObjects)
        //{
        //    if (go.CompareTag("Normal") || go.CompareTag("Ghost"))
        //    {
        //        totalScore += go.transform.GetComponent<PlayerController>().score;
        //    }
        //}
        //Debug.Log(totalScore);

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

    private void Dash()
    {
        if (transform.CompareTag("Normal")&&InputDash==1)
        {
            playerRigidbody.AddForce(moveDir*10, ForceMode.Impulse);
            Quaternion rotation = Quaternion.LookRotation(moveDir);
            Vector3 v3 = new Vector3(rotation.eulerAngles. x, rotation.eulerAngles. y, rotation.eulerAngles. z);
            Instantiate(DashEffect, transform.position, Quaternion.Euler(180, v3.y, 0),transform.parent); //Quaternion.Euler(rotation.eulerAngles.x,rotation.eulerAngles.y,rotation.eulerAngles.z);
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
                    //UIBell.SetActive(false);

                    //算分
                    //int addScore = 60 + (int)Math.Floor(0.1 * totalScore);
                    int theft = (int)Math.Floor(0.1 * score);
                    int addScore = 60 + theft;
                    collision.transform.GetComponent<PlayerController>().AddScore(addScore);
                    AddScore(-theft);
                    //totalScore = collision.transform.GetComponent<PlayerController>().totalScore;

                    //随机转移铃铛
                    //hasBell = false;
                    //List<int> list = new List<int>();
                    //int index = playerInput.playerIndex;
                    //GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
                    //foreach (GameObject go in allGameObjects)
                    //{
                    //    if (go.CompareTag("Normal") && go.transform.GetComponent<PlayerController>().playerInput.playerIndex != index)
                    //    {
                    //        list.Add(go.transform.GetComponent<PlayerController>().playerInput.playerIndex);
                    //    }
                    //}
                    //int randomIndex = UnityEngine.Random.Range(0, list.Count);
                    //int bellAdder = list[randomIndex];
                    //foreach (GameObject go in allGameObjects)
                    //{
                    //    if (go.CompareTag("Normal") && go.transform.GetComponent<PlayerController>().playerInput.playerIndex == bellAdder)
                    //    {
                    //        go.transform.GetComponent<PlayerController>().hasBell = true;
                    //        go.transform.GetComponent<PlayerController>().UIBell.SetActive(true);
                    //        break;
                    //    }
                    //}
                }
                else
                {
                    int theft = (int)Math.Floor(0.05 * score);
                    int addScore = 40 + theft;
                    collision.transform.GetComponent<PlayerController>().AddScore(addScore);
                    AddScore(-theft);
                    //int addScore = 4 + (int)Math.Floor(0.05 * totalScore);
                    //totalScore = collision.transform.GetComponent<PlayerController>().totalScore;
                }
            }
            //血量为0时
            else if (healthy <= 0 && !isInvincible)
            {
                GameObject[] allGameObjects;
                if (hasBell)
                {
                    //删除头上的铃铛
                    //UIBell.SetActive(false);

                    //算分
                    int theft = (int)Math.Floor(0.15 * score);
                    int addScore = 100 + theft;
                    collision.transform.GetComponent<PlayerController>().AddScore(addScore);
                    AddScore(-theft);
                    //int addScore = 10 + (int)Math.Floor(0.2 * totalScore);
                    //collision.transform.GetComponent<PlayerController>().AddScore(addScore);
                    //totalScore = collision.transform.GetComponent<PlayerController>().totalScore;

                    //随机转移铃铛
                    //hasBell = false;
                    //List<int> list = new List<int>();
                    //int index = playerInput.playerIndex;
                    //allGameObjects = GameObject.FindObjectsOfType<GameObject>();
                    //foreach (GameObject go in allGameObjects)
                    //{
                    //    if (go.CompareTag("Normal") && go.transform.GetComponent<PlayerController>().playerInput.playerIndex != index)
                    //    {
                    //        list.Add(go.transform.GetComponent<PlayerController>().playerInput.playerIndex);
                    //    }
                    //}
                    //int randomIndex = UnityEngine.Random.Range(0, list.Count);
                    //int bellAdder = list[randomIndex];
                    //foreach (GameObject go in allGameObjects)
                    //{
                    //    if (go.CompareTag("Normal") && go.transform.GetComponent<PlayerController>().playerInput.playerIndex == bellAdder)
                    //    {
                    //        go.transform.GetComponent<PlayerController>().hasBell = true;
                    //        go.transform.GetComponent<PlayerController>().UIBell.SetActive(true);
                    //        break;
                    //    }
                    //}
                }
                else
                {
                    int theft = (int)Math.Floor(0.1 * score);
                    int addScore = 60 + theft;
                    collision.transform.GetComponent<PlayerController>().AddScore(addScore);
                    AddScore(-theft);
                    //int addScore = 6 + (int)Math.Floor(0.1 * totalScore);
                    //collision.transform.GetComponent<PlayerController>().AddScore(addScore);
                    //totalScore = collision.transform.GetComponent<PlayerController>().totalScore;
                }

                //********对方，鬼变成人*********
                collision.transform.GetComponent<PlayerController>().Ghost.SetActive(false);
                collision.transform.GetComponent<PlayerController>().myCharacter.SetActive(true);

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
                myCharacter.SetActive(false);
                Ghost.SetActive(true);
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
        else if (transform.CompareTag("Normal") && !hasBell && collision.transform.CompareTag("Prop"))
        {
            // 找到道具，拾取道具并将其与玩家关联
            curProp = collision.gameObject;
            curProp.transform.SetParent(transform);
            Vector3 pickupOffset = new Vector3(0f, 0f, 1f);
            curProp.transform.localPosition = pickupOffset;

            // 停用道具的刚体，使其跟随玩家移动
            Rigidbody itemRigidbody = curProp.GetComponent<Rigidbody>();
            if (itemRigidbody != null)
            {
                itemRigidbody.isKinematic = true;
            }

        }

    }


    IEnumerator EffectBoom()
    {
        yield return new WaitForSeconds(4f);
        //镜头晃动
        GameObject mainCameraObj = GameObject.Find("Main Camera");
        mainCameraObj.GetComponent<CameraShake>().Shake(0.3f, 0.1f);

        //恢复刚体
        playerRigidbody.isKinematic = false;

        string prefabPath = "Assets/Effect/GhostExplosion.prefab";
        GameObject GhostExplosionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Vector3 currentPosition = transform.position;
        Quaternion currentRotation = transform.rotation;
        GameObject instantiatedPrefab = Instantiate(GhostExplosionPrefab, currentPosition, currentRotation);
        Destroy(instantiatedPrefab, 1f);
    }
    public void IsOpposite()
    {
        isOpposite = true;
        opDuration = 5;
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
        animator = myCharacter.GetComponent<Animator>();
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
            Girl.SetActive(true);
            myCharacter = Girl;
        }
        if (playerInput.playerIndex == 1)
        {
            transform.position = Reload.P1;
            Num.SetActive(true);
            myCharacter = Num;
        }
        if (playerInput.playerIndex == 2)
        {
            transform.position = Reload.P2;
            Farmer.SetActive(true);
            myCharacter = Farmer;
        }
        if (playerInput.playerIndex == 3)
        {
            transform.position = Reload.P3;
            Fan.SetActive(true);
            myCharacter = Fan;
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
