using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy01_Frog : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    private float jumpForceY = 5f; //垂直跳跃力
    private float jumpForceX = 5f; //水平跳跃力

    [SerializeField] private EnemyHealthSystem healthSystem;   //生命系统脚本
    private Vector3 pointA;  //巡逻点1
    private Vector3 pointB;  //巡逻点2
    private float pointRange = 5f;      //巡逻点设置范围

    private bool isVpetInRange = false;    //桌宠是否在攻击范围内
    private bool isAttacking = true;
    private float faceDir;                 //唯一面向向量值

    private GameObject vpet;               //桌宠对象

    //生命周期函数--------------------------------------------------------------------------------------//

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        vpet = GameObject.FindGameObjectWithTag("Vpet");  //获取桌宠的游戏对象
    }

    private void Start()
    {
        faceDir = transform.localScale.x;
        pointA = new Vector3(transform.position.x - pointRange, transform.position.y); //设置PointA
        pointB = new Vector3(transform.position.x + pointRange, transform.position.y); //设置PointB

        InitValueBasedDifficulty();     //初始化难度相关数值
        jumpCD = Random.Range(1f, 2f);  //随机跳跃CD

    }

    private void Update()
    {
        if (healthSystem.isDead) return;    //若已死亡则不执行

        CheckIsGrounded();      //落地监测
        CheckFall();            //坠落监测
        CheckHasFallGround();   //是否已落地监测
        CheckVpetEnter();       //监测桌宠是否进入攻击范围
        FrogIdleAudio();        //青蛙待机音效

        if (isVpetInRange)      //玩家在攻击范围内执行追逐行为
            ChaseVpet();
        else
            Patrol();           //否则执行巡逻行为

        if (isAllowTimerWork)
        {
            jumpTimer -= Time.deltaTime;
            frogYellTimer -= Time.deltaTime;
        }


        //与玩家距离超过20f后禁止播放音效
        isAllowAudioPlay = Vector2.Distance(vpet.transform.position, transform.position) > 20f ? false : true;  
    }


    //功能函数-----------------------------------------------------------------------------------------//

    //根据难度初始化数值
    private void InitValueBasedDifficulty()
    {
        //获取游戏难度进行匹配(伤害)
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //简单难度
            case GameDifficultyLevel.Easy:
                attackDamage = 2f;
                break;

            //正常难度
            case GameDifficultyLevel.Normal:
                attackDamage = 3f;
                break;

            //困难难度
            case GameDifficultyLevel.Hard:
                attackDamage = 4f;
                break;
        }
    }

    private float jumpTimer;           //跳跃计时器
    private float jumpCD;              //跳跃CD
    private bool isAllowTimerWork = true;
    Vector2 jumpDir = Vector2.right;

    private bool isAllowAudioPlay = true;   //是否允许播放音频

    //巡逻方法(Update)
    private void Patrol()
    {
        //若超出了PointA的X轴范围，力方向更改为右侧
        if (transform.position.x < pointA.x)
            jumpDir = Vector2.right;

        if (transform.position.x > pointB.x)
            jumpDir = Vector2.left;

        if (jumpTimer <= 0)
            Jump(jumpDir);

    }

    //追击方法(Update)
    private void ChaseVpet()
    {
        if (vpet == null) return;

        //计算青蛙与玩家的相对位置
        float pos = transform.position.x - vpet.transform.position.x;
        //若玩家在青蛙左侧
        if (pos > 0)
            jumpDir = Vector2.left;
        else
            jumpDir = Vector2.right;

        if (jumpTimer <= 0)
            Jump(jumpDir);
    }

    //跳跃方法(内部引用)
    private void Jump(Vector2 jumpDir)
    {
        if (!isGrounded) return;    //处于地面时才能跳跃
        isAttacking = true;         //攻击中
        isAllowTimerWork = false;   //期间禁止计时
        isAllowCheckFall = true;    //允许进行落地监测
        jumpTimer = jumpCD;         //CD重置

        rb.velocity = new Vector2(rb.velocity.x, jumpForceY);  //赋予y向速度   
        rb.AddForce(jumpDir * jumpForceX, ForceMode2D.Impulse); //添加跳跃力
        Flip(); //进行朝向矫正

        animator.SetTrigger("jump");        //动画播放
        
        if (isAllowAudioPlay)
            AudioManager.Instance.PlaySound3D("Enemy_frog_jump", transform.position);    //播放音效
    }   

    //监测桌宠是否进入范围(Update)
    private void CheckVpetEnter()
    {
        //若桌宠的X坐标在pointA与pointB区间
        if (vpet.transform.position.x > pointA.x &&
           vpet.transform.position.x < pointB.x)
        {
            isVpetInRange = true;
        }
        else isVpetInRange = false;
    }

    private bool isGrounded = true;     //是否落地

    private float rayLength = 0.1f;                 //射线长度
    private float halfWidth = 0.46f;                //射线半宽间隔
    [SerializeField] private LayerMask Layer;       //地面&&敌人层
    private void CheckIsGrounded()
    {
        // 三个射线起点：中、左、右
        Vector2 centerOrigin = transform.position + Vector3.down *0.3f;
        Vector2 leftOrigin = centerOrigin + Vector2.left * halfWidth;
        Vector2 rightOrigin = centerOrigin + Vector2.right * halfWidth;
        isGrounded = false;

        // 依次发射三条向下射线，使用 groundLayer 过滤
        foreach (Vector2 origin in new[] { centerOrigin, leftOrigin, rightOrigin })
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, Layer);
            if (hit.collider != null)
            {
                isGrounded = true;
                break;
            }
        }
        animator.SetBool("isGround", isGrounded);   //与动画器同步
    }   

    private bool isAllowCheckFall = false;  //是否允许检测坠落

    //坠落监测(Update)
    private void CheckFall()
    {
        //若允许监测
        if (isAllowCheckFall)
        {
            //当y向速度低于一定阈值则触发坠落动作
            if (rb.velocity.y < 0.1)
            {
                isAllowCheckFall = false;
                animator.SetTrigger("fall");
                hasFallGround = false;
            }
        }
    }

    private bool hasFallGround = true;

    //是否已落地监测(内部引用)
    private void CheckHasFallGround()
    {
        if (!hasFallGround)
        {
            //若此时已落地
            if (isGrounded)
            {
                hasFallGround = true;       //已落地
                isAllowTimerWork = true;    //允许计时器工作
                isAttacking = false;        //关闭攻击状态
            }
        }
    }

    //翻转精灵图(内部引用)
    private void Flip()
    {
        if(jumpDir == Vector2.right)
             transform.localScale = new Vector2(-faceDir, transform.localScale.y);
        else
            transform.localScale = new Vector2(faceDir, transform.localScale.y);
    }

    private float frogYellTimer;
    private float frogYellCD = 1.3f;

    private void FrogIdleAudio()
    {
        if (!isAllowAudioPlay) return;

        if (jumpTimer > 0 && jumpTimer < jumpCD - 0.5f)
        {
            if (frogYellTimer < 0)
            {
                frogYellTimer = frogYellCD;
                AudioManager.Instance.PlaySound3D("Enemy_frog_idle",transform.position);
            }
        }

    }

    private float attackDamage = 3f;        //攻击伤害

    //碰撞行为
    private void OnCollisionStay2D(Collision2D other)
    {
        //攻击玩家
        if (other.collider.CompareTag("Vpet") && isAttacking && !healthSystem.isDead)
        {
            //根据相对位置计算力的方向
            Vector2 force = transform.position.x > vpet.transform.position.x ? Vector2.left : Vector2.right;        
            other.gameObject.GetComponent<VpetHealthSystem>().VpetGethurt(attackDamage, force * 200f);
        }
    }
}
