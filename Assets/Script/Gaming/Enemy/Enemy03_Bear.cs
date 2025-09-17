using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy03_Bear : MonoBehaviour
{
    [SerializeField] private EnemyHealthSystem healthSystem;    //生命系统脚本
    [SerializeField] private float moveSpeed = 100f;            //移动速度

    private Vector3 pointA;             //巡逻点A
    private Vector3 pointB;             //巡逻点B
    private float pointRange = 8f;      //巡逻点设置范围

    private Rigidbody2D rb;
    private Animator animator;
    private GameObject vpet;
    private float currentSpeed;     //当前移速
    private int moveDir;            //移动方向(-1left| +1 right| 0 idle)
    private float faceDir;          //初始面朝方向

    private bool isWalking = false;                 //是否在移动中
    private bool isAllowAudioPlay = false;          //是否允许音效播放
    private float audioTimer;                       //音效计时器
    [SerializeField] private float audioCD = 0.8f;  //计时器CD

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        vpet = GameObject.FindGameObjectWithTag("Vpet");
    }

    void Start()
    {
        InitValueBasedDifficulty();                 //数值初始化
        moveDir = (Random.value < 0.5f) ? -1 : 1;   //初始化时给个随机方向
        faceDir = transform.localScale.x;           //默认面向方向

        pointA = new Vector3(transform.position.x - pointRange, transform.position.y); //设置PointA
        pointB = new Vector3(transform.position.x + pointRange, transform.position.y); //设置PointB

    }

    //根据难度初始化数值
    private void InitValueBasedDifficulty()
    {
        //获取游戏难度进行匹配(攻击伤害|移动速度|停滞时间修正)
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //简单难度
            case GameDifficultyLevel.Easy:
                attackDamage = 2f;
                moveSpeed = 75f;
                stopTimeFix = 2f;   
                break;

            //正常难度
            case GameDifficultyLevel.Normal:
                attackDamage = 3f;
                moveSpeed = 100f;
                stopTimeFix = 0f;
                break;

            //困难难度
            case GameDifficultyLevel.Hard:
                attackDamage = 4f;
                moveSpeed = 125f;
                stopTimeFix = -2f;
                break;
        }
    }


    void Update()
    {
        //玩家距离超过20m不播放音效
        isAllowAudioPlay = vpet != null &&
            vpet.transform != null &&
            Vector2.Distance(vpet.transform.position, transform.position) <= 20f;

        audioTimer -= Time.deltaTime;       //计时器工作
        if(isAllowStopTimerWork)  randomStopTimer -= Time.deltaTime;

        //处于移动中则更新精灵图翻转
        if (moveDir != 0)
            Flip();

        RandomStopCheck();      //随机停止监测

        float vpetPosX = vpet.transform.position.x;
        float AposX = pointA.x;
        float BposX = pointB.x;

        //若玩家进入巡逻区域，则追逐
        if (vpetPosX > AposX && vpetPosX < BposX)
            ChaseVpet();

        //否则进行巡逻
        else
            Partrol();

        isWalking = currentSpeed != 0f;
        animator.SetBool("isWalk", isWalking);

        if (isWalking && isAllowAudioPlay && audioTimer <= 0f)
        {
            audioTimer = audioCD;
            AudioManager.Instance.PlaySound3D("Enemy_bear_walk", transform.position);
        }
    }

    private float randomStopTimer;              //随机停止计时器
    private float randomStopInterval = 10f;     //随机停止间隔
    private float minStopTime = 3f;             //最低停止时长
    private float maxStopTime = 6f;             //最高停止时长
    private float stopTimeFix = 0f;             //停滞时间修正

    private bool  isStop = false;               //是否暂停中
    private bool isAllowStopTimerWork = true;   //是否允许停止计时器工作

    private void RandomStopCheck()
    {
        if(randomStopTimer <= 0 && !isStop)
        {
            isStop = true;
            randomStopTimer = randomStopInterval;
            moveDir = 0;
            StartCoroutine(StopTime());
        }
    }

    IEnumerator StopTime()
    {
        //等待随机时长
        float rand = Random.Range(minStopTime + stopTimeFix, maxStopTime + stopTimeFix);
        yield return new WaitForSeconds(rand);
        isStop = false;
        moveDir = (Random.value < 0.5f) ? -1 : 1;   //给随机方向
    }

    //追逐逻辑
    private void ChaseVpet()
    {
        //若处于间隔暂停中且距离玩家一定距离，则不触发追逐
        if (isStop && Vector2.Distance(vpet.transform.position, transform.position) > 2f) return;
        else if (isStop)
        {
            isStop = false;
        }
        isAllowStopTimerWork = false;
        moveDir = vpet.transform.position.x > transform.position.x ? 1 : -1;    //根据vpet位置设置移动方向

    }

    //巡逻逻辑
    private void Partrol()
    {
        if (isStop) return;
        isAllowStopTimerWork = true;
        //边界监测(超过巡逻范围则会强制反向移动)
        if (transform.position.x <= pointA.x && moveDir < 0)
        {
            moveDir = 1;  // 到达左边界，向右移动
        }
        else if (transform.position.x >= pointB.x && moveDir > 0)
        {
            moveDir = -1; // 到达右边界，向左移动
        }
    }

    void FixedUpdate()
    {
        if (healthSystem.isDead) return;

        currentSpeed = moveDir * moveSpeed;   //设置移动速度    

        //怪物移动
        if (currentSpeed != 0f)
            rb.velocity = new Vector2(currentSpeed * Time.fixedDeltaTime, rb.velocity.y);
        else
            rb.velocity = new Vector2(0f, rb.velocity.y);
    }


    // 翻转精灵图，根据 moveDir 方向
    private void Flip()
    {
        if (moveDir > 0)
            transform.localScale = new Vector2(faceDir, transform.localScale.y);
        else if (moveDir < 0)
            transform.localScale = new Vector2(-faceDir, transform.localScale.y);
    }

    private float attackDamage = 3f;

    private void OnCollisionStay2D(Collision2D other)
    {
        //攻击玩家
        if (other.collider.CompareTag("Vpet") && !healthSystem.isDead)
        {
            //根据相对位置计算力的方向
            Vector2 force = transform.position.x > vpet.transform.position.x ? Vector2.left : Vector2.right;
            other.gameObject.GetComponent<VpetHealthSystem>().VpetGethurt(attackDamage, force * 400f + Vector2.up * 100f);
        }
    }

}
