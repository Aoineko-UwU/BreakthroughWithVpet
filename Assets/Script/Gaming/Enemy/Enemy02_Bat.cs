using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy02_Bat : MonoBehaviour
{
    private Rigidbody2D rb;

    [SerializeField] private EnemyHealthSystem healthSystem;   //����ϵͳ�ű�
    private Vector3 pointA;  //Ѳ�ߵ�1
    private Vector3 pointB;  //Ѳ�ߵ�2
    private float pointRange = 5f;      //Ѳ�ߵ����÷�Χ

    private float faceDir;              //Ψһ��������ֵ

    private float moveSpeed = 160f;     //�ƶ��ٶ�
    private float currentSpeed;         //��ǰ�ƶ��ٶ�

    private GameObject vpet;            //�������

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        vpet = GameObject.FindGameObjectWithTag("Vpet");  //��ȡ�������Ϸ����
    }

    private void Start()
    {
        currentSpeed = moveSpeed;
        faceDir = transform.localScale.x;
        pointA = new Vector3(transform.position.x - pointRange, transform.position.y); //����PointA
        pointB = new Vector3(transform.position.x + pointRange, transform.position.y); //����PointB

        InitValueBasedDifficulty();         //��ʼ���Ѷ������ֵ

    }

    private bool isAllowAudioPlay = true;   //�Ƿ���������Ƶ

    private void Update()
    {
        //����Ҿ��볬��20f���ֹ������Ч
        isAllowAudioPlay = Vector2.Distance(vpet.transform.position, transform.position) > 20f ? false : true;
        audioTimer -= Time.deltaTime;
        Patrol();
    }

    private void FixedUpdate()
    {
        if (!healthSystem.isDead)
            rb.velocity = new Vector2(currentSpeed * Time.fixedDeltaTime, 0f);
    }

    //�����Ѷȳ�ʼ����ֵ
    private void InitValueBasedDifficulty()
    {
        //��ȡ��Ϸ�ѶȽ���ƥ��(�����˺�|�����ٶ�)
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //���Ѷ�
            case GameDifficultyLevel.Easy:
                attackDamage = 2f;
                moveSpeed = 120f;
                break;

            //�����Ѷ�
            case GameDifficultyLevel.Normal:
                attackDamage = 3f;
                moveSpeed = 160f;
                break;

            //�����Ѷ�
            case GameDifficultyLevel.Hard:
                attackDamage = 4f;
                moveSpeed = 200f;
                break;
        }
    }



    private float audioTimer;
    private float audioCD = 0.9f;

    //Ѳ�߷���(Update)
    private void Patrol()
    {
        //��������PointA��X�᷶Χ���ƶ������Ϊ�Ҳ�
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


    //��ת����ͼ(�ڲ�����)
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
        //�������
        if (other.collider.CompareTag("Vpet") && !healthSystem.isDead)
        {
            //�������λ�ü������ķ���
            Vector2 force = transform.position.x > vpet.transform.position.x ? Vector2.left : Vector2.right;
            other.gameObject.GetComponent<VpetHealthSystem>().VpetGethurt(attackDamage, force * 300f);
        }
    }

}
