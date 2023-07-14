using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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
    private Vector2 InputMove;
    PlayerInput playerInput;

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
        playerInput = GetComponent<PlayerInput>();
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
    /// 用于接收InputAction返回的玩家输入数据
    /// </summary>
    /// <param name="value0"></param>
    public void OnMovement(InputAction.CallbackContext value0)
    {
        InputMove = value0.ReadValue<Vector2>();
    }
    /// <summary>
    /// 玩家移动方法
    /// </summary>
    void PlayerMovement()
    {
        //动画器条件设置和Sprite翻转
        if (characterController.velocity.x > 0)
        {
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
            animator.SetBool("isRunSide", true);
            animator.SetBool("isRunFount", false);

        }
        else if (characterController.velocity.x < 0)
        {
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
            animator.SetBool("isRunSide", true);
            animator.SetBool("isRunFount", false);
        }
        else if (characterController.velocity.z<0&&characterController.velocity.x==0)
        {
            animator.SetBool("isRunFount", true);
            animator.SetBool("isRunSide", false);
        }
        else if(characterController.velocity.z>0 && characterController.velocity.x == 0)
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
        characterController.SimpleMove(moveDir);

        //模型朝向
        //Vector3 lookDir = transform.position + moveDir;
        //playerModel.transform.LookAt(lookDir);
        
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
