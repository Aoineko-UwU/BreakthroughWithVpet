using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy01_Frog : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    private float jumpForceY = 5f; //��ֱ��Ծ��
    private float jumpForceX = 5f; //ˮƽ��Ծ��

    [SerializeField] private EnemyHealthSystem healthSystem;   //����ϵͳ�ű�
    private Vector3 pointA;  //Ѳ�ߵ�1
    private Vector3 pointB;  //Ѳ�ߵ�2
    private float pointRange = 5f;      //Ѳ�ߵ����÷�Χ

    private bool isVpetInRange = false;    //�����Ƿ��ڹ�����Χ��
    private bool isAttacking = true;
    private float faceDir;                 //Ψһ��������ֵ

    private GameObject vpet;               //�������

    //�������ں���--------------------------------------------------------------------------------------//

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        vpet = GameObject.FindGameObjectWithTag("Vpet");  //��ȡ�������Ϸ����
    }

    private void Start()
    {
        faceDir = transform.localScale.x;
        pointA = new Vector3(transform.position.x - pointRange, transform.position.y); //����PointA
        pointB = new Vector3(transform.position.x + pointRange, transform.position.y); //����PointB

        InitValueBasedDifficulty();     //��ʼ���Ѷ������ֵ
        jumpCD = Random.Range(1f, 2f);  //�����ԾCD

    }

    private void Update()
    {
        if (healthSystem.isDead) return;    //����������ִ��

        CheckIsGrounded();      //��ؼ��
        CheckFall();            //׹����
        CheckHasFallGround();   //�Ƿ�����ؼ��
        CheckVpetEnter();       //��������Ƿ���빥����Χ
        FrogIdleAudio();        //���ܴ�����Ч

        if (isVpetInRange)      //����ڹ�����Χ��ִ��׷����Ϊ
            ChaseVpet();
        else
            Patrol();           //����ִ��Ѳ����Ϊ

        if (isAllowTimerWork)
        {
            jumpTimer -= Time.deltaTime;
            frogYellTimer -= Time.deltaTime;
        }


        //����Ҿ��볬��20f���ֹ������Ч
        isAllowAudioPlay = Vector2.Distance(vpet.transform.position, transform.position) > 20f ? false : true;  
    }


    //���ܺ���-----------------------------------------------------------------------------------------//

    //�����Ѷȳ�ʼ����ֵ
    private void InitValueBasedDifficulty()
    {
        //��ȡ��Ϸ�ѶȽ���ƥ��(�˺�)
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //���Ѷ�
            case GameDifficultyLevel.Easy:
                attackDamage = 2f;
                break;

            //�����Ѷ�
            case GameDifficultyLevel.Normal:
                attackDamage = 3f;
                break;

            //�����Ѷ�
            case GameDifficultyLevel.Hard:
                attackDamage = 4f;
                break;
        }
    }

    private float jumpTimer;           //��Ծ��ʱ��
    private float jumpCD;              //��ԾCD
    private bool isAllowTimerWork = true;
    Vector2 jumpDir = Vector2.right;

    private bool isAllowAudioPlay = true;   //�Ƿ���������Ƶ

    //Ѳ�߷���(Update)
    private void Patrol()
    {
        //��������PointA��X�᷶Χ�����������Ϊ�Ҳ�
        if (transform.position.x < pointA.x)
            jumpDir = Vector2.right;

        if (transform.position.x > pointB.x)
            jumpDir = Vector2.left;

        if (jumpTimer <= 0)
            Jump(jumpDir);

    }

    //׷������(Update)
    private void ChaseVpet()
    {
        if (vpet == null) return;

        //������������ҵ����λ��
        float pos = transform.position.x - vpet.transform.position.x;
        //��������������
        if (pos > 0)
            jumpDir = Vector2.left;
        else
            jumpDir = Vector2.right;

        if (jumpTimer <= 0)
            Jump(jumpDir);
    }

    //��Ծ����(�ڲ�����)
    private void Jump(Vector2 jumpDir)
    {
        if (!isGrounded) return;    //���ڵ���ʱ������Ծ
        isAttacking = true;         //������
        isAllowTimerWork = false;   //�ڼ��ֹ��ʱ
        isAllowCheckFall = true;    //���������ؼ��
        jumpTimer = jumpCD;         //CD����

        rb.velocity = new Vector2(rb.velocity.x, jumpForceY);  //����y���ٶ�   
        rb.AddForce(jumpDir * jumpForceX, ForceMode2D.Impulse); //�����Ծ��
        Flip(); //���г������

        animator.SetTrigger("jump");        //��������
        
        if (isAllowAudioPlay)
            AudioManager.Instance.PlaySound3D("Enemy_frog_jump", transform.position);    //������Ч
    }   

    //��������Ƿ���뷶Χ(Update)
    private void CheckVpetEnter()
    {
        //�������X������pointA��pointB����
        if (vpet.transform.position.x > pointA.x &&
           vpet.transform.position.x < pointB.x)
        {
            isVpetInRange = true;
        }
        else isVpetInRange = false;
    }

    private bool isGrounded = true;     //�Ƿ����

    private float rayLength = 0.1f;                 //���߳���
    private float halfWidth = 0.46f;                //���߰����
    [SerializeField] private LayerMask Layer;       //����&&���˲�
    private void CheckIsGrounded()
    {
        // ����������㣺�С�����
        Vector2 centerOrigin = transform.position + Vector3.down *0.3f;
        Vector2 leftOrigin = centerOrigin + Vector2.left * halfWidth;
        Vector2 rightOrigin = centerOrigin + Vector2.right * halfWidth;
        isGrounded = false;

        // ���η��������������ߣ�ʹ�� groundLayer ����
        foreach (Vector2 origin in new[] { centerOrigin, leftOrigin, rightOrigin })
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, Layer);
            if (hit.collider != null)
            {
                isGrounded = true;
                break;
            }
        }
        animator.SetBool("isGround", isGrounded);   //�붯����ͬ��
    }   

    private bool isAllowCheckFall = false;  //�Ƿ�������׹��

    //׹����(Update)
    private void CheckFall()
    {
        //��������
        if (isAllowCheckFall)
        {
            //��y���ٶȵ���һ����ֵ�򴥷�׹�䶯��
            if (rb.velocity.y < 0.1)
            {
                isAllowCheckFall = false;
                animator.SetTrigger("fall");
                hasFallGround = false;
            }
        }
    }

    private bool hasFallGround = true;

    //�Ƿ�����ؼ��(�ڲ�����)
    private void CheckHasFallGround()
    {
        if (!hasFallGround)
        {
            //����ʱ�����
            if (isGrounded)
            {
                hasFallGround = true;       //�����
                isAllowTimerWork = true;    //�����ʱ������
                isAttacking = false;        //�رչ���״̬
            }
        }
    }

    //��ת����ͼ(�ڲ�����)
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

    private float attackDamage = 3f;        //�����˺�

    //��ײ��Ϊ
    private void OnCollisionStay2D(Collision2D other)
    {
        //�������
        if (other.collider.CompareTag("Vpet") && isAttacking && !healthSystem.isDead)
        {
            //�������λ�ü������ķ���
            Vector2 force = transform.position.x > vpet.transform.position.x ? Vector2.left : Vector2.right;        
            other.gameObject.GetComponent<VpetHealthSystem>().VpetGethurt(attackDamage, force * 200f);
        }
    }
}
