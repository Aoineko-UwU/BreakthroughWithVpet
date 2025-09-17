using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy03_Bear : MonoBehaviour
{
    [SerializeField] private EnemyHealthSystem healthSystem;    //����ϵͳ�ű�
    [SerializeField] private float moveSpeed = 100f;            //�ƶ��ٶ�

    private Vector3 pointA;             //Ѳ�ߵ�A
    private Vector3 pointB;             //Ѳ�ߵ�B
    private float pointRange = 8f;      //Ѳ�ߵ����÷�Χ

    private Rigidbody2D rb;
    private Animator animator;
    private GameObject vpet;
    private float currentSpeed;     //��ǰ����
    private int moveDir;            //�ƶ�����(-1left| +1 right| 0 idle)
    private float faceDir;          //��ʼ�泯����

    private bool isWalking = false;                 //�Ƿ����ƶ���
    private bool isAllowAudioPlay = false;          //�Ƿ�������Ч����
    private float audioTimer;                       //��Ч��ʱ��
    [SerializeField] private float audioCD = 0.8f;  //��ʱ��CD

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        vpet = GameObject.FindGameObjectWithTag("Vpet");
    }

    void Start()
    {
        InitValueBasedDifficulty();                 //��ֵ��ʼ��
        moveDir = (Random.value < 0.5f) ? -1 : 1;   //��ʼ��ʱ�����������
        faceDir = transform.localScale.x;           //Ĭ��������

        pointA = new Vector3(transform.position.x - pointRange, transform.position.y); //����PointA
        pointB = new Vector3(transform.position.x + pointRange, transform.position.y); //����PointB

    }

    //�����Ѷȳ�ʼ����ֵ
    private void InitValueBasedDifficulty()
    {
        //��ȡ��Ϸ�ѶȽ���ƥ��(�����˺�|�ƶ��ٶ�|ͣ��ʱ������)
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //���Ѷ�
            case GameDifficultyLevel.Easy:
                attackDamage = 2f;
                moveSpeed = 75f;
                stopTimeFix = 2f;   
                break;

            //�����Ѷ�
            case GameDifficultyLevel.Normal:
                attackDamage = 3f;
                moveSpeed = 100f;
                stopTimeFix = 0f;
                break;

            //�����Ѷ�
            case GameDifficultyLevel.Hard:
                attackDamage = 4f;
                moveSpeed = 125f;
                stopTimeFix = -2f;
                break;
        }
    }


    void Update()
    {
        //��Ҿ��볬��20m��������Ч
        isAllowAudioPlay = vpet != null &&
            vpet.transform != null &&
            Vector2.Distance(vpet.transform.position, transform.position) <= 20f;

        audioTimer -= Time.deltaTime;       //��ʱ������
        if(isAllowStopTimerWork)  randomStopTimer -= Time.deltaTime;

        //�����ƶ�������¾���ͼ��ת
        if (moveDir != 0)
            Flip();

        RandomStopCheck();      //���ֹͣ���

        float vpetPosX = vpet.transform.position.x;
        float AposX = pointA.x;
        float BposX = pointB.x;

        //����ҽ���Ѳ��������׷��
        if (vpetPosX > AposX && vpetPosX < BposX)
            ChaseVpet();

        //�������Ѳ��
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

    private float randomStopTimer;              //���ֹͣ��ʱ��
    private float randomStopInterval = 10f;     //���ֹͣ���
    private float minStopTime = 3f;             //���ֹͣʱ��
    private float maxStopTime = 6f;             //���ֹͣʱ��
    private float stopTimeFix = 0f;             //ͣ��ʱ������

    private bool  isStop = false;               //�Ƿ���ͣ��
    private bool isAllowStopTimerWork = true;   //�Ƿ�����ֹͣ��ʱ������

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
        //�ȴ����ʱ��
        float rand = Random.Range(minStopTime + stopTimeFix, maxStopTime + stopTimeFix);
        yield return new WaitForSeconds(rand);
        isStop = false;
        moveDir = (Random.value < 0.5f) ? -1 : 1;   //���������
    }

    //׷���߼�
    private void ChaseVpet()
    {
        //�����ڼ����ͣ���Ҿ������һ�����룬�򲻴���׷��
        if (isStop && Vector2.Distance(vpet.transform.position, transform.position) > 2f) return;
        else if (isStop)
        {
            isStop = false;
        }
        isAllowStopTimerWork = false;
        moveDir = vpet.transform.position.x > transform.position.x ? 1 : -1;    //����vpetλ�������ƶ�����

    }

    //Ѳ���߼�
    private void Partrol()
    {
        if (isStop) return;
        isAllowStopTimerWork = true;
        //�߽���(����Ѳ�߷�Χ���ǿ�Ʒ����ƶ�)
        if (transform.position.x <= pointA.x && moveDir < 0)
        {
            moveDir = 1;  // ������߽磬�����ƶ�
        }
        else if (transform.position.x >= pointB.x && moveDir > 0)
        {
            moveDir = -1; // �����ұ߽磬�����ƶ�
        }
    }

    void FixedUpdate()
    {
        if (healthSystem.isDead) return;

        currentSpeed = moveDir * moveSpeed;   //�����ƶ��ٶ�    

        //�����ƶ�
        if (currentSpeed != 0f)
            rb.velocity = new Vector2(currentSpeed * Time.fixedDeltaTime, rb.velocity.y);
        else
            rb.velocity = new Vector2(0f, rb.velocity.y);
    }


    // ��ת����ͼ������ moveDir ����
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
        //�������
        if (other.collider.CompareTag("Vpet") && !healthSystem.isDead)
        {
            //�������λ�ü������ķ���
            Vector2 force = transform.position.x > vpet.transform.position.x ? Vector2.left : Vector2.right;
            other.gameObject.GetComponent<VpetHealthSystem>().VpetGethurt(attackDamage, force * 400f + Vector2.up * 100f);
        }
    }

}
