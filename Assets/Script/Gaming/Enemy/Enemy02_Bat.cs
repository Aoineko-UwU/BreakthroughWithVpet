using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy02_Bat : MonoBehaviour
{
    private Rigidbody2D rb;

    [SerializeField] private EnemyHealthSystem healthSystem;   //生命系统脚本
    private Vector3 pointA;  //巡逻点1
    private Vector3 pointB;  //巡逻点2
    private float pointRange = 5f;      //巡逻点设置范围

    private float faceDir;              //唯一面向向量值

    private float moveSpeed = 160f;     //移动速度
    private float currentSpeed;         //当前移动速度

    private GameObject vpet;            //桌宠对象

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        vpet = GameObject.FindGameObjectWithTag("Vpet");  //获取桌宠的游戏对象
    }

    private void Start()
    {
        currentSpeed = moveSpeed;
        faceDir = transform.localScale.x;
        pointA = new Vector3(transform.position.x - pointRange, transform.position.y); //设置PointA
        pointB = new Vector3(transform.position.x + pointRange, transform.position.y); //设置PointB

        InitValueBasedDifficulty();         //初始化难度相关数值

    }

    private bool isAllowAudioPlay = true;   //是否允许播放音频

    private void Update()
    {
        //与玩家距离超过20f后禁止播放音效
        isAllowAudioPlay = Vector2.Distance(vpet.transform.position, transform.position) > 20f ? false : true;
        audioTimer -= Time.deltaTime;
        Patrol();
    }

    private void FixedUpdate()
    {
        if (!healthSystem.isDead)
            rb.velocity = new Vector2(currentSpeed * Time.fixedDeltaTime, 0f);
    }

    //根据难度初始化数值
    private void InitValueBasedDifficulty()
    {
        //获取游戏难度进行匹配(攻击伤害|飞行速度)
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //简单难度
            case GameDifficultyLevel.Easy:
                attackDamage = 2f;
                moveSpeed = 120f;
                break;

            //正常难度
            case GameDifficultyLevel.Normal:
                attackDamage = 3f;
                moveSpeed = 160f;
                break;

            //困难难度
            case GameDifficultyLevel.Hard:
                attackDamage = 4f;
                moveSpeed = 200f;
                break;
        }
    }



    private float audioTimer;
    private float audioCD = 0.9f;

    //巡逻方法(Update)
    private void Patrol()
    {
        //若超出了PointA的X轴范围，移动方向改为右侧
        if (transform.position.x < pointA.x)
        {
            currentSpeed = moveSpeed;
            Flip();
        }
        if (transform.position.x > pointB.x)
        {
            currentSpeed = -moveSpeed;
            Flip();
        }

        if(isAllowAudioPlay && audioTimer <= 0)
        {
            audioTimer = audioCD;
            AudioManager.Instance.PlaySound3D("Enemy_bat_wingbeat", transform.position);
        }

    }


    //翻转精灵图(内部引用)
    private void Flip()
    {
        if (currentSpeed > 0)
            transform.localScale = new Vector2(faceDir, transform.localScale.y);
        else
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
            other.gameObject.GetComponent<VpetHealthSystem>().VpetGethurt(attackDamage, force * 300f);
        }
    }

}
