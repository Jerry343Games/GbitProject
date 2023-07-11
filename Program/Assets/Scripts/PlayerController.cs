using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private GameObject playerModel;
    private float horizontal;
    private float vertical;
    private int camResult = -1;

    /// <summary>
    /// 玩家移动方向向量
    /// </summary>
    private Vector3 moveDir;

    [Header("ChooseCam")]
    public bool HD2D;
    public bool THIRD;
    [Header("MainCam")]
    public GameObject mainCamera;
    public Transform tracePoint;
    public float smooth;
    private Vector3 camSpeed;

    [Header("PlayerSpeed")]
    public float speed;

    [Header("PlayerAnimator")]
    public Animator animator;
   
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerModel = GameObject.Find("PlayerModel");
        camResult= CameraCheck();
    }

    void Update()
    {
        PlayerMovement();
        MainCameraFollow(tracePoint,mainCamera);
    }

    /// <summary>
    /// 玩家移动方法
    /// </summary>
    void PlayerMovement()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        //动画条件设置和Sprite翻转
        if (characterController.velocity.x > 0)
        {
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
            animator.SetBool("isRun", true);
        }
        else if (characterController.velocity.x < 0)
        {
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
            animator.SetBool("isRun", true);
        }else if (characterController.velocity.z!=0)
        {
            animator.SetBool("isRun", true);
        }
        else
        {
            animator.SetBool("isRun", false);
        }

        //移动方向
        moveDir = (new Vector3(horizontal, 0, vertical)).normalized * speed;
        characterController.SimpleMove(moveDir);
        if (!HD2D)
        {
            //模型朝向
            Vector3 lookDir = transform.position + moveDir;
            playerModel.transform.LookAt(lookDir);
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

    /// <summary>
    /// 相机跟踪方法
    /// </summary>
    void MainCameraFollow(Transform target,GameObject mainCamera)
    {
        //HD2D
        if (camResult==0)
        {
            mainCamera.transform.position= Vector3.SmoothDamp(mainCamera.transform.position,target.transform.position,ref camSpeed ,smooth);
        }    
    }


    
}
