//using System.Collections;
//using System.Collections.Generic;
//using System;
//using UnityEngine;
//using UnityEngine.InputSystem;
//public class Controller_Test : MonoBehaviour
//{
//    private CharacterController characterController;
//    private GameObject playerModel;
//    private float horizontal;
//    private float vertical;
//    private int camResult = -1;

//    /// <summary>
//    /// 玩家移动方向向量
//    /// </summary>
//    private Vector3 moveDir;
//    private Vector2 InputMove;
//    PlayerInput playerInput;

//    [Header("ChooseCam")]
//    public bool HD2D;
//    public bool THIRD;
//    [Header("MainCam")]
//    public GameObject mainCamera;
//    public Transform tracePoint;
//    public float smooth;
//    private Vector3 camSpeed;

//    [Header("PlayerSpeed")]
//    public float speed;

//    [Header("PlayerAnimator")]
//    public Animator animator;


//    //从这开始
//    //private bool isHuman = true;//是人or鬼，可以考虑加入CharacterController
//    //private bool hasBell = false; // 铃铛值，默认为false
//    //private bool isAttacked = false; // 是否已被攻击过，即是否受伤
//    //private bool isBoosted = false;// 是否处于挨打加速状态
//    //private bool isStunned = false;// 是否处于晕眩状态
//    //private bool isHolding = false;// 是否处于持铃状态
//    public float boostDuration = 3f; // 加速持续时间
//    public float stunDuration = 10f; // 晕眩持续时间
//    public float bellDuration = 10f; // 铃铛加分间隔时间

//    //velocity
//    //score


//    private void OnTriggerEnter(Collider other)
//    {
//        Rigidbody playerRigidbody = GetComponent<Rigidbody>();

//        if (other.CompareTag("Bell") && playerRigidbody.isHuman)//铃铛，要求是人
//        {
//            playerRigidbody.hasBell = true;
//            moveDir = (new Vector3(0.8f * InputMove.x, 0, 0.8f * InputMove.y)).normalized;
//            playerRigidbody.velocity = moveDir;
//            //****************更新模型为带铃铛、音效****************

//            Destroy(other.gameObject);
//        }
//        else if (other.CompareTag("Coin") && playerRigidbody.isHuman)//金币，仅要求是人
//        {
//            playerRigidbody.score++;
//            BellScoreManager.Instance.SetTotalScore(BellScoreManager.Instance.GetTotalScore() + 1);
//            //****************特效、音效****************

//            Destroy(other.gameObject);
//        }
//        else if (other.CompareTag("") && !playerRigidbody.hasBell && playerRigidbody.isHuman)//其他道具，必须是人，且没有触碰铃铛
//        {

//        }
//        else if (other.CompareTag("Player") && playerRigidbody.isHuman && !other.GetComponent<Rigidbody>().isHuman)//****************人碰到鬼,先写碰撞，后面改成攻击****************
//        {
//            //****************受伤动画、音效****************

//            //人未受伤
//            if (!playerRigidbody.isAttacked)
//            {
//                //结算分数
//                if (playerRigidbody.hasBell)
//                {
//                    int addScore = 6 + (int)Math.Floor(BellScoreManager.Instance.GetTotalScore() * 0.1);
//                    other.GetComponent<Rigidbody>().score += addScore;
//                    BellScoreManager.Instance.SetTotalScore(BellScoreManager.Instance.GetTotalScore() + addScore);

//                    //****************随机转移铃铛****************

//                }
//                else
//                {
//                    int addScore = 4 + (int)Math.Floor(BellScoreManager.Instance.GetTotalScore() * 0.05);
//                    other.GetComponent<Rigidbody>().score += addScore;
//                    BellScoreManager.Instance.SetTotalScore(BellScoreManager.Instance.GetTotalScore() + addScore);
//                }
//                //结算状态
//                playerRigidbody.isAttacked = true;
//                boostDuration = 3f;
//                playerRigidbody.isBoosted = true;

//            }
//            else
//            {
//                //结算分数
//                if (playerRigidbody.hasBell)
//                {
//                    int addScore = 10 + (int)Math.Floor(BellScoreManager.Instance.GetTotalScore() * 0.2);
//                    other.GetComponent<Rigidbody>().score += addScore;
//                    BellScoreManager.Instance.SetTotalScore(BellScoreManager.Instance.GetTotalScore() + addScore);

//                    //****************随机转移铃铛****************

//                }
//                else
//                {
//                    int addScore = 6 + (int)Math.Floor(BellScoreManager.Instance.GetTotalScore() * 0.1);
//                    other.GetComponent<Rigidbody>().score += addScore;
//                    BellScoreManager.Instance.SetTotalScore(BellScoreManager.Instance.GetTotalScore() + addScore);
//                }
//                //结算人的状态
//                playerRigidbody.isAttacked = false;
//                stunDuration = 10f;
//                playerRigidbody.isStunned = true;
//                playerRigidbody.isHuman = false;
//                //****************更新模型为被晕眩的鬼、音效****************

//                //****************场上负标记清零****************
//                GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
//                foreach (GameObject go in allGameObjects)
//                {
//                    if (go.CompareTag("Player") && go.GetComponent<Rigidbody>().isHuman && go.GetComponent<Rigidbody>().isAttacked)
//                    {
//                        go.GetComponent<Rigidbody>().isAttacked = false;
//                    }
//                }

//                //结算鬼的状态
//                other.GetComponent<Rigidbody>().isHuman = true;
//                moveDir = (new Vector3(InputMove.x, 0, InputMove.y)).normalized;
//                other.GetComponent<Rigidbody>().velocity = moveDir;
//                //****************更新模型为人、音效**************** 如何标识不同的人？用id？

//            }
//        }
//    }

//    void Start()
//    {
//        playerInput = GetComponent<PlayerInput>();
//        characterController = GetComponent<CharacterController>();
//        playerModel = GameObject.Find("PlayerModel");
//        camResult = CameraCheck();
//    }

//    void Update()
//    {
//        PlayerMovement();
//        MainCameraFollow(tracePoint, mainCamera);

//        Rigidbody playerRigidbody = GetComponent<Rigidbody>();

//        //加速和晕眩状态
//        if (playerRigidbody.isBoosted)
//        {
//            moveDir = (new Vector3(1.3f * InputMove.x, 0, 1.3f * InputMove.y)).normalized;
//            playerRigidbody.velocity = moveDir;
//            boostDuration -= Time.deltaTime;
//            if (boostDuration <= 0f)
//            {
//                // 加速时间结束，恢复人的正常速度
//                playerRigidbody.isBoosted = false;
//                moveDir = (new Vector3(InputMove.x, 0, InputMove.y)).normalized;
//                playerRigidbody.velocity = moveDir;
//            }
//        }
//        else if (playerRigidbody.isStunned)
//        {
//            moveDir = (new Vector3(0, 0, 0)).normalized;
//            playerRigidbody.velocity = moveDir;
//            stunDuration -= Time.deltaTime;
//            if (stunDuration <= 0f)
//            {
//                // 晕眩时间结束，恢复 鬼的 正常速度
//                playerRigidbody.isStunned = false;
//                moveDir = (new Vector3(1.2f * InputMove.x, 0, 1.2f * InputMove.y)).normalized;
//                playerRigidbody.velocity = moveDir;

//                //****************更新模型为正常的鬼、音效****************
//            }
//        }

//        //持铃加分
//        if (playerRigidbody.hasBell)
//        {
//            bellDuration -= Time.deltaTime;
//            if (bellDuration <= 0f)
//            {
//                // 铃铛加分
//                int addScore = 5 + (int)Math.Floor(BellScoreManager.Instance.GetTotalScore() * 0.01);
//                playerRigidbody.score += addScore;
//                BellScoreManager.Instance.SetTotalScore(BellScoreManager.Instance.GetTotalScore() + addScore);
//                bellDuration = 10f;
//            }
//        }
//    }

//    /// <summary>
//    /// 用于接收InputAction返回的玩家输入数据
//    /// </summary>
//    /// <param name="value0"></param>
//    public void OnMovement(InputAction.CallbackContext value0)
//    {
//        InputMove = value0.ReadValue<Vector2>();
//    }
//    /// <summary>
//    /// 玩家移动方法
//    /// </summary>
//    void PlayerMovement()
//    {
//        //动画器条件设置和Sprite翻转
//        if (characterController.velocity.x > 0)
//        {
//            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
//            animator.SetBool("isRunSide", true);
//            animator.SetBool("isRunFount", false);

//        }
//        else if (characterController.velocity.x < 0)
//        {
//            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
//            animator.SetBool("isRunSide", true);
//            animator.SetBool("isRunFount", false);
//        }
//        else if (characterController.velocity.z < 0 && characterController.velocity.x == 0)
//        {
//            animator.SetBool("isRunFount", true);
//            animator.SetBool("isRunSide", false);
//        }
//        else if (characterController.velocity.z > 0 && characterController.velocity.x == 0)
//        {
//            animator.SetBool("isRunFount", false);
//            animator.SetBool("isRunSide", false);
//        }
//        else
//        {
//            animator.SetBool("isRunFount", false);
//            animator.SetBool("isRunSide", false);
//        }

//        //移动方向
//        moveDir = (new Vector3(InputMove.x, 0, InputMove.y)).normalized * speed;
//        characterController.SimpleMove(moveDir);

//        //模型朝向
//        //Vector3 lookDir = transform.position + moveDir;
//        //playerModel.transform.LookAt(lookDir);

//    }

//    /// <summary>
//    /// 检查相机选择是否正确
//    /// </summary>
//    /// <returns></returns>
//    int CameraCheck()
//    {
//        //HD2D
//        if (!THIRD && HD2D)
//        {
//            Debug.Log("启用HD2D模式相机");
//            return 0;
//        }
//        //第三人称
//        else if (THIRD && !HD2D)
//        {
//            Debug.Log("启用3D模式相机");
//            return 1;
//        }
//        else if (!THIRD && !HD2D)
//        {
//            Debug.LogWarning("请选择相机模式");
//            return -1;
//        }
//        else
//        {
//            Debug.LogWarning("相机模式不可同时选中");
//            return -1;
//        }
//    }

//    /// <summary>
//    /// 相机跟踪方法
//    /// </summary>
//    void MainCameraFollow(Transform target, GameObject mainCamera)
//    {
//        //HD2D
//        if (camResult == 0)
//        {
//            mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, target.transform.position, ref camSpeed, smooth);
//        }
//    }

//}
