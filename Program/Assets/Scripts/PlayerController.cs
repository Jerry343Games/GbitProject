using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
    private int camResult = -1;
    private Rigidbody playerRigidbody;
    public int healthy;

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
    public Transform tracePoint;
    public float smooth;
    private Vector3 camSpeed;

    [Header("PlayerSpeed")]
    public float speed;//速度

    [Header("PlayerAnimator")]
    public Animator animator;//动画机

    [Header("AddForce")]
    public float force;//碰撞传递的力

    [Header("UIHurt")]
    public GameObject UIHurt;//受伤标记
    public  bool isInvincible;

    private void OnEnable()
    {

    }
    void Start()
    {
        camResult= CameraCheck();
        playerRigidbody=GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        StartCoroutine(DelaySetPosition());
        healthy = 1;
    }

    void Update()
    {
        PlayerMovement();
        //MainCameraFollow(tracePoint,mainCamera);

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
        else if (playerRigidbody.velocity.z<0&&playerRigidbody.velocity.x==0)
        {
            animator.SetBool("isRunFount", true);
            animator.SetBool("isRunSide", false);
        }
        else if(playerRigidbody.velocity.z>0 && playerRigidbody.velocity.x == 0)
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

    private void OnCollisionEnter(Collision collision)
    {
        //如果该玩家是正常且被鬼碰撞
        if (transform.CompareTag("Normal") && collision.transform.CompareTag("Ghost"))
        {
            Debug.Log(collision.transform.tag + collision.transform.GetComponent<PlayerController>().healthy);
            //自己血量大于0时
            if (healthy > 0&&!isInvincible)
            {
                UIHurt.SetActive(true);
                collision.transform.GetComponent<PlayerController>().UIHurt.SetActive(false);
                healthy--;
                collision.transform.GetComponent<PlayerController>().isInvincible = true;
                collision.transform.GetComponent<PlayerController>().SetInvisFalse();
                isInvincible = true;
                StartCoroutine(DelayMyselfInvis());
                playerRigidbody.AddForce(new Vector3((transform.position - collision.transform.position).x, 0.2f, (collision.transform.position - transform.position).z) * force, ForceMode.Impulse);
                
            }
            //血量为0时
            if (healthy <= 0 && !isInvincible)
            {
                //互换Tag和加满生命值
                UIHurt.SetActive(false);
                collision.transform.GetComponent<PlayerController>().UIHurt.SetActive(false);
                isInvincible = true;
                StartCoroutine(DelayMyselfInvis());
                collision.transform.GetComponent<PlayerController>().isInvincible = true;
                collision.transform.GetComponent<PlayerController>().SetInvisFalse();
                transform.tag = "Ghost";
                collision.transform.tag = "Normal";
                healthy = 1;
                collision.transform.GetComponent<PlayerController>().healthy = 1;
            }
        }
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
