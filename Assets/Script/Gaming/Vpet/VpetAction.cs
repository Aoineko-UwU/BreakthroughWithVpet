using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public enum VpetState       //����״̬
{
    Idle,           //0
    Walking,        //1
    Fall,           //2
    Climb,          //3
    Eat,            //4
    Sleep,          //5
    Dance,          //6
    Die,            //7
    Win             //8
}

public class VpetAction : MonoBehaviour
{
    //��ȡ����Ķ�����
    [SerializeField] private Animator _animatorVpet;
    [SerializeField] private Animator _animatorVpetHand;
    [SerializeField] private Animator _animatorEatenItem;

    [SerializeField] private GameObject bombPrefab;         //ը��Ԥ����
    [SerializeField] private GameObject winParticle;        //ʤ����������
    [SerializeField] private GameObject speedUpParticle;    //����Buff����
    [SerializeField] private GameObject attackUpParticle;   //�˺�Buff����

    [SerializeField] private GameObject onePunchState;      //һȭ״̬

    private Rigidbody2D rb;             //�������
    private ConstantForce2D force2D;    //����2D������
    private VpetHealthSystem health;    //��������ϵͳ

    private VpetState currentState;  //����״̬

    //��������--------------------------------------------------------------------------------------------//

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();           //��ȡ����
        health = GetComponent<VpetHealthSystem>();  //��ȡ����ϵͳ
        force2D = GetComponent<ConstantForce2D>();  //��ȡ����2D��   
        currentState = VpetState.Idle;      //��ʼ��״̬
        capsuleCollider = GetComponent<CapsuleCollider2D>();    //��ȡ������ײ��
        figureCanvas = GameObject.FindGameObjectWithTag("FigureCanvas");
    }

    private void Start()
    {
        VpetColliderChange();               //��ʼ����ײ��
        InitGroundContactFilter();          //��ʼ������Ӵ�������
        InitAllGroundContactFilter();       //��ʼ��ȫ����Ӵ�������
        InitValueBasedDifficulty();         //��ʼ���Ѷ������ֵ
    }

    private void FixedUpdate()
    {

        if (health.isVpetDead) return;      //����������ִ��
        VpetWalk();     //�����ƶ���Ϊ
        VpetClimb();    //����������Ϊ
        VpetFallHorSpeedSet();  //����Ʈ��ˮƽ��
    }

    private void Update()
    {
        if (health.isVpetDead) return;      //����������ִ��

        VpetFall();         //����Ʈ����Ϊ
        VpetFallCheck();    //����Ʈ����Ϊ���
        VpetDance();        //����������Ϊ
        TimerWork();        //��ʱ������
    }

    //�߶���Ϊ--------------------------------------------------------------------------------------- -----//

    //�����Ѷȳ�ʼ����ֵ(�����˺� | ����Ƶ�� | ����˺�
    private void InitValueBasedDifficulty()
    {
        //��ȡ��Ϸ�ѶȽ���ƥ��
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //���Ѷ�
            case GameDifficultyLevel.Easy:
                vpetAttackDamage = 4f;
                vpetAttackCD = 1.2f;
                spikeDamage = 2f;
                break;

            //�����Ѷ�
            case GameDifficultyLevel.Normal:
                vpetAttackDamage = 3f;
                vpetAttackCD = 1.3f;
                spikeDamage = 3f;
                break;

            //�����Ѷ�
            case GameDifficultyLevel.Hard:
                vpetAttackDamage = 2f;
                vpetAttackCD = 1.4f;
                spikeDamage = 4f;
                break;
        }
    }

    //�߶���Ϊ--------------------------------------------------------------------------------------- -----//

    private float speedUpBuffFix = 1f;      //����Buff��������
    private float _vpetWalkSpeed = 5.5f;    //�����ٶ�
    private float forceMultiplier = 2f;     //�������Ŵ���
    private float walkAudioTimer;
    private float walkAudioCD = 0.62f;

    private void VpetWalk()
    {
        //����״̬Ϊ����ʱִ���ƶ�
        if (currentState == VpetState.Walking)
        {
            //��·��Ч����
            if (walkAudioTimer <= 0)
            {
                walkAudioTimer = walkAudioCD;
                if (isInWater)
                    AudioManager.Instance.PlaySound("swim");
                else
                    AudioManager.Instance.PlaySound("walk");
            }

            _animatorVpet.SetBool("isWalk", true);

            Vector2 avgNormal = GetAverageGroundNormal();                           //ȡƽ������
            Vector2 tangent = new Vector2(avgNormal.y, -avgNormal.x).normalized;    //�������߷���

            // ��� avgNormal ��ʾ����ֱǽ�桱�����߼���ˮƽ������ǿ��������
            if (Mathf.Abs(avgNormal.x) > 0.9f && Mathf.Abs(avgNormal.y) < 0.1f)
            {
                tangent = Vector2.right;
            }

            //��������
            Vector2 gravityForce = rb.mass * Physics2D.gravity;     //�������� G=mg           
            float gAlong = Vector2.Dot(gravityForce, tangent);      //���������߷����ϵķ���            
            Vector2 compensationForce = -gAlong * tangent;          //�����������������

            //��Vpet����
            if (avgNormal.y > 0) 
                rb.AddForce(compensationForce * 0.6f, ForceMode2D.Force);  //����һ����������Vpet������Ӱ��
            //��Vpet����
            else if(avgNormal.y < 0)
                rb.AddForce(compensationForce, ForceMode2D.Force);         //ʩ��ȫ�������������ٶ�


            forceMultiplier = isInWater ? 0.8f : isTouchGround() ? 2f :
                          isGrounded ? 0.6f : 0f;                    //�ƶ�������


            Vector2 desiredVel = tangent * _vpetWalkSpeed;                                   //�ƶ�����������
            Vector2 force = (desiredVel - rb.velocity) * forceMultiplier * speedUpBuffFix;   //�ƶ������ռ���
            rb.AddForce(force, ForceMode2D.Force);
        }
        else
        {
            _animatorVpet.SetBool("isWalk", false);
        }
    }

    [SerializeField] LayerMask groundLayer;                             //������㼶
    private ContactFilter2D groundContactFilter;                        //����Ӵ�������
    private ContactPoint2D[] contactPoints = new ContactPoint2D[10];    //�Ӵ�������

    [SerializeField] LayerMask allGroundLayer;                          //���е����йز㼶
    private ContactFilter2D allGroundContactFilter;                     //ȫ����Ӵ�������
    private ContactPoint2D[] allContactPoints = new ContactPoint2D[1];  //�Ӵ�������

    // ��ȡƽ������
    private Vector2 GetAverageGroundNormal()
    {
        // ��ȡ���нӴ���
        int count = rb.GetContacts(groundContactFilter, contactPoints);
        if (count == 0)
            return Vector2.up; 

        Vector2 sum = Vector2.zero;
        for (int i = 0; i < count; i++)
            sum += contactPoints[i].normal;
        return (sum / count).normalized;
    }

    //�Ƿ��������ײ��Ӵ�
    private bool isTouchGround()
    {
        // ��ȡ���нӴ���
        int count = rb.GetContacts(allGroundContactFilter, allContactPoints);
        if (count > 0)
            return true;
        else
            return false;
    }

    //��ʼ������Ӵ�������
    private void InitGroundContactFilter()
    {
        groundContactFilter = new ContactFilter2D();
        groundContactFilter.SetLayerMask(groundLayer);
        groundContactFilter.useTriggers = false;
    }

    //��ʼ��ȫ����Ӵ�������
    private void InitAllGroundContactFilter()
    {
        allGroundContactFilter = new ContactFilter2D();
        allGroundContactFilter.SetLayerMask(allGroundLayer);
        allGroundContactFilter.useTriggers = false;
    }

    //������Ϊ--------------------------------------------------------------------------------------------//

    private float _vpetClimbSpeed = 7f;                     //�����ٶ�
    private float climbForceMultiplier = 2f;                //�������Ŵ���
    private bool isClimbing = false;                        //�Ƿ�����������

    private float climbAudioTimer;                          //������Ч��ʱ��
    private float climbAudioCD = 0.62f;                     //������Ч���
    private void VpetClimb()
    {
        _animatorVpet.SetBool("isClimbing", isClimbing);    //������״̬ͬ��
        //����״̬Ϊ����ʱִ��
        if (currentState == VpetState.Climb)
        {
            _animatorVpet.ResetTrigger("ClimbEnd");         //����ClimbEnd Trigger

            //����ʱ����������������ִ��һ�ζ���
            if (!isClimbing)
            {
                isClimbing = true;
                rb.velocity = Vector2.zero;
                _animatorVpet.SetTrigger("ClimbStart");
            }

            Vector2 desiredVel = Vector2.up * _vpetClimbSpeed;  //�����ٶȣ���������            
            Vector2 climbForce = (desiredVel - rb.velocity) * climbForceMultiplier; //����������
            rb.AddForce(climbForce, ForceMode2D.Force);         //ʩ����

            //������Ч����(���)
            if (climbAudioTimer <= 0)
            {
                climbAudioTimer = climbAudioCD;
                int rand = Random.Range(1, 5);
                AudioManager.Instance.PlaySound($"Assets/Audio/Vpet/climb/ladder" + rand + ".wav");
            }
        }
        else
        {
            if (isClimbing)
            {
                isClimbing = false;
                _animatorVpet.SetTrigger("ClimbEnd");
            }
        }
    }

    //��ʳ��Ϊ---------------------------------------------------------------------------------------//
    [SerializeField] SpriteRenderer eatenItemSprite;    //��ʳ����Sprite��Ⱦ����
    [SerializeField] GameObject onePunchEffect;         //һȭ״̬����
    public bool isAllowEat = true;                      //�Ƿ������ʳ��

    //�����ʳ
    public void VpetEat(ItemData item)
    {
        if (health.isVpetDead) return;      //����������ִ��

        //�ǿռ��
        if (item != null && isAllowEat)
        {
            if (isFalling) StopFallingLogic();      //������׹��״̬����ֹͣ׹���߼�

            isAllowEat = false;                     //����Ϊ�������ٽ�ʳ
            eatenItemSprite.sprite = item.icon;     //�ı�ʳ�ﾫ��ͼ
            currentState = VpetState.Eat;           //�ı����赱ǰ״̬

            //���Ž�ʳ����
            _animatorVpet.SetTrigger("Eat");    
            _animatorVpetHand.SetTrigger("Eat");
            _animatorEatenItem.SetTrigger("Eat");

            AudioManager.Instance.PlaySound("pickItem");    //������Ч

            //����Э���ӳ�ʱ����ж�ʳ������
            StartCoroutine(StartFoodJudge(item));

            //��������״̬
            if (isClimbing)
                isClimbing = false;

            //����Ʈ��״̬
            if (isFalling)
                isFalling = false;

        }
    }
    IEnumerator StartFoodJudge(ItemData item)
    {
        yield return new WaitForSeconds(0.8f);
        AudioManager.Instance.PlaySound("eating");  //���ųԶ�����Ч
        yield return new WaitForSeconds(1.6f);

        int index = item.itemID;    //��ȡ��ƷID

        //������ƷID�ж�ִ�в�ͬЧ��
        switch (index)
        {
            //��ƻ��
            case 0:
                health.VpetRecover(10f);   //�ظ�����
                currentState = VpetState.Walking;                   //��������״̬
                isAllowEat = true;
                break;
            
            //��˯���
            case 1:
                StartCoroutine(VpetSleep());    //����˯��
                break;

            //����
            case 2:
                isOnePunch = true;              //ǿ����һ�ι���
                onePunchState.SetActive(true);  //����Effect״̬ͼ
                Instantiate(onePunchEffect, transform.position, Quaternion.identity);
                AudioManager.Instance.PlaySound("OnePunchState");
                currentState = VpetState.Walking;
                isAllowEat = true;
                break;

            //����
            case 3:
                int eventIndex = RandomSelector.Instance.EventRandomSelector(1);    //��ȡ������¼�
                switch(eventIndex)
                {
                    //�����ָ��¼�
                    case 1:
                        health.VpetRecover(20f);
                        currentState = VpetState.Walking;
                        break;
                    //˲������
                    case 2:
                        health.VpetGethurt(999f, Vector2.up * 100f);
                        currentState = VpetState.Die;
                        break;
                    //�ƶ�����
                    case 3:
                        if(speedUpBuffCoroutine != null)
                        {
                            StopCoroutine(speedUpBuffCoroutine);
                            speedUpBuffCoroutine = StartCoroutine(Eat_SpeedUp());
                        }
                        else
                            speedUpBuffCoroutine = StartCoroutine(Eat_SpeedUp());

                        currentState = VpetState.Walking;
                        break;
                    //��ͨ�����˺����ӣ�����Ƶ�ʼӿ�
                    case 4:
                        if(AttackUpBuffCoroutine != null)
                        {
                            StopCoroutine(AttackUpBuffCoroutine);
                            AttackUpBuffCoroutine = StartCoroutine(Eat_AttackUp());
                        }
                        else
                            AttackUpBuffCoroutine = StartCoroutine(Eat_AttackUp());

                        currentState = VpetState.Walking;
                        break;
                    //�۳�����
                    case 5:
                        health.VpetGethurt(10f, Vector2.up * 100f);
                        currentState = VpetState.Walking;
                        break;
                    //˲��
                    case 6:
                        Teleport();
                        currentState = VpetState.Walking;
                        break;
                    //˲�䱬ը
                    case 7:
                        var bomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);
                        bomb.GetComponent<Item_Block_bomb>().isInstanctlyExplode = true;
                        currentState = VpetState.Walking;
                        break;
                };

                isAllowEat = true;
                break;

            //����(���˺�)
            case 4:
                if (AttackUpBuffCoroutine != null)
                {
                    StopCoroutine(AttackUpBuffCoroutine);
                    AttackUpBuffCoroutine = StartCoroutine(Eat_AttackUp());
                }
                else
                    AttackUpBuffCoroutine = StartCoroutine(Eat_AttackUp());

                currentState = VpetState.Walking;
                isAllowEat = true;
                break;

            //ѩ��(����)
            case 5:
                if (speedUpBuffCoroutine != null)
                {
                    StopCoroutine(speedUpBuffCoroutine);
                    speedUpBuffCoroutine = StartCoroutine(Eat_SpeedUp());
                }
                else
                    speedUpBuffCoroutine = StartCoroutine(Eat_SpeedUp());
                currentState = VpetState.Walking;
                isAllowEat = true;
                break;


            default:
                Debug.Log("δ֪ʳ������");
                break;
        }

    }

    float speedUpBuffMultiplier = 1.7f;         //����Buff����ļ��ٱ���
    float speedUpBuffDuration = 12f;            //���ٳ���ʱ��
    private Coroutine speedUpBuffCoroutine;     //����BuffЭ��

    IEnumerator Eat_SpeedUp()
    {
        ShowText("�ٶ���������");
        AudioManager.Instance.PlaySound("getBuff");
        speedUpBuffFix = speedUpBuffMultiplier; //���ļ��ٱ���
        var par = Instantiate(speedUpParticle, transform.position, Quaternion.identity, transform); //��������
        Destroy(par, speedUpBuffDuration);  
        //�ȴ�Ч������ʱ��
        yield return new WaitForSeconds(speedUpBuffDuration);

        speedUpBuffFix = 1f;            //�ָ�Ĭ�ϱ���
        speedUpBuffCoroutine = null;    //����Э��
    }

    float attackBuffDamageMultiplier = 2f;      //����Buff����Ĺ�������
    float attackBuffTimeMultiplier = 0.5f;      //����Buff����Ĺ����������
    float attackBuffDuration = 12f;             //����Buff�ĳ���ʱ��
    private Coroutine AttackUpBuffCoroutine;    //����BuffЭ��

    IEnumerator Eat_AttackUp()
    {
        AudioManager.Instance.PlaySound("getBuff");
        ShowText("������������");
        health.SetKnockBack(false);     //���ɻ���״̬
        var par = Instantiate(attackUpParticle, transform.position, Quaternion.identity, transform); //��������
        Destroy(par, speedUpBuffDuration);
        //���ı���
        attackBuffDamageFix = attackBuffDamageMultiplier;
        attackBuffTimeFix = attackBuffTimeMultiplier;
        //�ȴ�Ч������ʱ��
        yield return new WaitForSeconds(attackBuffDuration);
        //�ָ�Ĭ�ϱ���
        attackBuffDamageFix = 1f;
        attackBuffTimeFix = 1f;
        health.SetKnockBack(true);      //�ɻ���״̬

        AttackUpBuffCoroutine = null;   //����Э��
    }


    private float teleportRange = 12f;     // ���˲�Ʒ�Χ
    private int maxSearchAttempts = 10;    // �����Ҵ���

    //˲�Ʒ���Ч��
    private void Teleport()
    {
        Vector2 targetPosition = Vector2.zero;
        bool foundValidPosition = false;

        // ���Բ������ maxSearchAttempts ��
        for (int i = 0; i < maxSearchAttempts; i++)
        {
            // ���ѡ��һ��Ŀ��λ�ã���ָ����Χ�ڣ�
            Vector2 randomDirection = Random.insideUnitCircle * teleportRange;  // ��һ��Բ�η�Χ�����
            targetPosition = (Vector2)transform.position + randomDirection;

            // ���Ŀ��λ���Ƿ���Ч
            if (CanTeleportTo(targetPosition))
            {
                foundValidPosition = true;  // �ҵ�����Ч��Ŀ��λ��
                break;  // �˳�����
            }
        }
        // ����ҵ���Чλ�ã���˲�ƣ�����ԭ��˲��
        if (foundValidPosition)
            transform.position = targetPosition;
        else
            transform.position += Vector3.up * 0.5f;

        AudioManager.Instance.PlaySound("teleport");
    }

    // ���Ŀ��λ���Ƿ���Ч
    private bool CanTeleportTo(Vector2 targetPosition)
    {
        Collider2D hit = Physics2D.OverlapCapsule(targetPosition,capsuleCollider.size*0.2f,capsuleCollider.direction,0f);
        // ���Ŀ��λ������ײ�壬����˲��
        if (hit != null && !hit.CompareTag("Ignore"))         
            return false;

        return true;
    }

    //// ʹ��Gizmo���Ƴ�Ŀ��ص�ļ��뾶
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red; // ��ⷶΧ����ɫ
    //    Gizmos.DrawWireSphere(transform.position, teleportRange);  // ������ɫ��Բ������
    //}


    //˯����Ϊ---------------------------------------------------------------------------------------//

    private float sleepTime = 7.5f;  //ʵ��˯��ʱ��(+2.5s)
    private float sleepAudioTimer;
    private float sleepAudioTimerCD = 2.3f;

    private float sleepRecoverTimer;        //˯�߻ָ���ʱ��
    private float sleepRecoverCD = 1f;      //˯�߻ָ����
    private float sleepRecoverRate = 1f;    //ÿ�������ָ���

    IEnumerator VpetSleep()
    {
        currentState = VpetState.Sleep;
        AudioManager.Instance.PlaySound("startSleep");  //��Ч����
        _animatorVpet.SetTrigger("SleepStart");         //˯�߶�������

        yield return new WaitForSeconds(2.5f);          //�ȴ���������

        VpetColliderChange();                           //�ı���ײ��

        float elapsedTime = 0f;     //����ʱ��
        while (elapsedTime < sleepTime)
        {
            elapsedTime += Time.deltaTime;
            if (sleepAudioTimer <= 0)
            {
                sleepAudioTimer = sleepAudioTimerCD;
                AudioManager.Instance.PlaySound("sleeping");
            }

            if (sleepRecoverTimer <= 0)
            {
                sleepRecoverTimer = sleepRecoverCD;
                health.VpetRecover(sleepRecoverRate);
            }

            yield return null; // �ȴ���һ֡������ѭ����Ծ
        }

        AudioManager.Instance.StopSound("sleeping");
        _animatorVpet.SetTrigger("SleepEnd");           //����������
        yield return new WaitForSeconds(0.9f);          //�ȴ���������

        currentState = VpetState.Walking;               //����Ϊ����״̬
        VpetColliderChange();                           //������ײ��
        isAllowEat = true;                              //�����ʳ
    }

    //Ʈ����Ϊ---------------------------------------------------------------------------------------//

    private bool isGrounded = false;    //�Ƿ����
    private bool isInWater = false;     //�Ƿ���ˮ��

    private bool isAllowFallCheckTimer = false; //�Ƿ�����׹�����ʱ������
    private float fallConfirmTimer;
    private float fallConfirmInterval = 0.2f;   //׹������(�������ʱ�䲻���ڵ������ж�Ϊ׹����)
    private float rayLength = 1.6f;             //���߳���
    private float halfWidth = 0.38f;            //���߰����

    //����׹����Ϊ���(Update)
    private void VpetFallCheck()
    {
        // ����������㣺�С�����
        Vector2 centerOrigin = transform.position;
        Vector2 leftOrigin = centerOrigin + Vector2.left * halfWidth;
        Vector2 rightOrigin = centerOrigin + Vector2.right * halfWidth;
        isGrounded = false;
        isInWater = false; 

        // ���η��������������ߣ�ʹ�� allGroundLayer ����
        foreach (Vector2 origin in new[] { centerOrigin, leftOrigin, rightOrigin })
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, allGroundLayer);
            if (hit.collider != null)
            {
                isGrounded = true;
                // �����ײ���Ƿ���ˮ
                if (hit.collider.CompareTag("Water"))
                    isInWater = true;

                break;
            }
        }

        //Debug.DrawRay(centerOrigin, Vector2.down * rayLength, isGrounded ? Color.green : Color.red);
        //Debug.DrawRay(leftOrigin, Vector2.down * rayLength, isGrounded ? Color.green : Color.red);
        //Debug.DrawRay(rightOrigin, Vector2.down * rayLength, isGrounded ? Color.green : Color.red);

        // �������е�׹��
        if (currentState == VpetState.Walking || currentState == VpetState.Idle)
        {
            //���õ���״̬���
            if (!isAllowFallCheckTimer && !isGrounded)
            {
                fallConfirmTimer = fallConfirmInterval; //����ʱ��
                isAllowFallCheckTimer = true;           //��ʼ���м��
            }
        }
        //�������˵���״̬����ʱ��&&��ʱ���
        if (isAllowFallCheckTimer && fallConfirmTimer <= 0)
        {
            //��ʱ����ʱ�������ڿ���
            if (!isGrounded)
            {
                currentState = VpetState.Fall;      //ȷ��ת��ΪƮ��״̬
                isAllowFallCheckTimer = false;      //ֹͣ��ʱ��ʹ��
            }
            else
                isAllowFallCheckTimer = false;      //ֹͣ��ʱ��ʹ��
        }        
    }
        
    private float flyingForceVer = 7.4f;  //��ֱƮ����
    private bool isFalling = false;     //�Ƿ�����׹��

    private float fallAudioTimer;       //Ʈ����Ч��ʱ��
    private float fallAudioCD = 1f;     //Ʈ����Ч���

    //����׹��(Update)
    private void VpetFall()
    {
        //����״̬Ϊ׹��ʱ����
        if(currentState == VpetState.Fall && !isGrounded && !isGetUpCoroutineWork)
        {
            //ִ��һ��
            if (!isFalling)
            {
                isFalling = true;                               //����Ϊ��������
                _animatorVpet.SetBool("isFalling", isFalling);  //���¶���������
                _animatorVpet.SetTrigger("FallStart");          //����һ�ζ���
                AudioManager.Instance.PlaySound("startFall");   //����һ����Ч
                force2D.force = Vector2.up * flyingForceVer;    //����������
            }

            //Ʈ����Ч����
            if (fallAudioTimer <= 0 && isFalling)
            {
                fallAudioTimer = fallAudioCD;
                AudioManager.Instance.PlaySound("fall");
            }
        }

        //�������
        if(isGrounded && isFalling && currentState == VpetState.Fall && !isGetUpCoroutineWork)
        {
            isAllowEat = false;                         //��ֹ��ʳ
            StopFallingLogic();                         //׹��ֹͣ�߼�       
            StartCoroutine(VpetGetUp());                //�����ӳ�
            AudioManager.Instance.PlaySound("fallen");  //��Ч����
        }
    }

    private float flyingSpeedHor = 4f;             //ˮƽƮ���ٶ�
    private float fallHorForceMultiplier = 2f;     //��������У��ϵ��
    private float negativeVelMultiplier = 0.08f;   //�����ٶ�У��ϵ��

    //����׹��ˮƽ�ٶ�����(FixUpdate)
    private void VpetFallHorSpeedSet()
    {
        if (currentState == VpetState.Fall && !isGrounded && isFalling)
        {
            float vx = rb.velocity.x;               //��ȡX�ᵱǰ�ٶ�          
            float speedDiff = flyingSpeedHor - vx;  //�ٶȲ�

            //�����ٴ��ڲ�ͬ����ʱ��ѡ�ò�ͬϵ��
            float k = vx < 0
                ? fallHorForceMultiplier * negativeVelMultiplier
                : fallHorForceMultiplier;

            // ʩ����
            float forceX = speedDiff * k * speedUpBuffFix;          
            rb.AddForce(Vector2.right * forceX, ForceMode2D.Force);

        }
    }

    bool isGetUpCoroutineWork = false;

    //���������ӳ�
    IEnumerator VpetGetUp()
    {
        isGetUpCoroutineWork = true;
        yield return new WaitForSeconds(2.5f);
        isAllowEat = true;
        currentState = VpetState.Walking;
        isGetUpCoroutineWork = false;
    }

    //׹��ֹͣ�߼�
    private void StopFallingLogic()
    {
        isFalling = false;                              //���Ϊ���ڵ�����
        _animatorVpet.SetBool("isFalling", isFalling);  //���¶���������
        isAllowFallCheckTimer = false;
        force2D.force = Vector2.zero;                 //��ֹ����ʩ��
        AudioManager.Instance.StopSound("fall");      //��ֹ��Ч����
    }

    //������Ϊ---------------------------------------------------------------------------------------//
    private bool isVpetDancing = false;     //�Ƿ���������
    private float vpetDanceRecoverTimer;    //���������ظ���ʱ��
    private float vpetDanceRecoverCD = 1f;  //��ʱ��CD
    private float recoverPerDance = 1f;     //ÿ�λָ���

    private float vpetDanceAttackTimer;     //�����˺���ʱ��
    private float vpetDanceAttackCD = 0.5f; //��ʱ��CD
    private float damagePerDance = 3f;      //ÿ���˺���
    private float damagePerDanceRadius = 2f;//�˺��뾶
    [SerializeField] private LayerMask enemyLayer;  //���˲�

    private void VpetDance()
    {
        if(currentState == VpetState.Dance)
        {
            isAllowEat = false;                 //��ֹ��ʳ
            health.isVpetInvincible = true;     //�޵�Ч��

            if (!isVpetDancing)
            {
                isVpetDancing = true;
                _animatorVpet.SetTrigger("Dance");                  //��������
                AudioManager.Instance.PlayBGM("DanceMusic");        //��������
                StartCoroutine(DanceTime());
            }

            //�����ָ�
            if (vpetDanceRecoverTimer <= 0)
            {
                vpetDanceRecoverTimer = vpetDanceRecoverCD;         
                health.VpetRecover(recoverPerDance);                
            }
            //����˺�
            if(vpetDanceAttackTimer <= 0)
            {
                bool isHurt = false;
                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, damagePerDanceRadius, enemyLayer);

                foreach (Collider2D col in colliders)
                {
                    if (col != null)
                    {
                        col.gameObject.GetComponent<EnemyHealthSystem>().GetHurt(damagePerDance,transform.position,3f);   //����˺�
                        isHurt = true;
                    }
                }
                if (isHurt)
                    vpetDanceAttackTimer = vpetDanceAttackCD;   //����˺���ˢ��CD

            }
        }
    }
    
    IEnumerator DanceTime()
    {
        yield return new WaitForSeconds(13f);
        float duration = 2f;
        float elapsed = 0f;
        float startVolume = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float currentVolume = Mathf.Lerp(startVolume, 0f, t);
            AudioManager.Instance.AdjustBGMVolume(currentVolume);
            yield return null;
        }
        AudioManager.Instance.PauseOrContinueBGM(true); //��ͣBGM
        currentState = VpetState.Walking;               //״̬ת��
        _animatorVpet.SetTrigger("DanceEnd");           //���Ŷ���
        yield return new WaitForSeconds(0.5f);          //���ݵȴ�
        AudioManager.Instance.AdjustBGMVolume(1);       //�ָ�BGM��Դ����
        AudioManager.Instance.PlayBGM("GameMusic");     //������ϷBGM
        isAllowEat = true;                              //�����ʳ
        isVpetDancing = false;                          //�ر�����״̬
        health.isVpetInvincible = false;                //�ر��޵�״̬
    }


    //��ʱ��-----------------------------------------------------------------------------------------//

    //��ʱ������(Update)
    private void TimerWork()
    {
        walkAudioTimer -= Time.deltaTime;
        climbAudioTimer -= Time.deltaTime;
        fallAudioTimer -= Time.deltaTime;
        sleepAudioTimer -= Time.deltaTime;
        vpetAttackTimer -= Time.deltaTime;
        sleepRecoverTimer -= Time.deltaTime;
        vpetDanceAttackTimer -= Time.deltaTime;
        vpetDanceRecoverTimer -= Time.deltaTime;
        if (isAllowFallCheckTimer) fallConfirmTimer -= Time.deltaTime;
    }

    //����״̬-----------------------------------------------------------------------------------------//

    public void VpetStateSet(int state)
    {
        switch (state)
        {
            case 0:
                currentState = VpetState.Idle;
                break;
            case 1:
                currentState = VpetState.Walking;
                break;
            case 2:
                currentState = VpetState.Fall;
                break;
            case 3:
                currentState = VpetState.Climb;
                break;
            case 4:
                currentState = VpetState.Eat;
                break;
            case 5:
                currentState = VpetState.Sleep;
                break;
            case 6:
                currentState = VpetState.Dance;
                break;
            case 7:
                currentState = VpetState.Die;
                break;
            case 8:
                currentState = VpetState.Win;
                break;
            default:
                Debug.Log("δ֪״̬����");
                break;
        }
    }

    //������������---------------------------------------------------------------------------------------//

    public void VpetDead()
    {
        currentState = VpetState.Die;   //����״̬
        StopAllCoroutines();            //ֹͣ��������Э��
        VpetColliderChange();           //�ı���ײ��
        StopFallingLogic();             //����һ��׹��ֹͣ�߼�

        _animatorVpet.SetBool("isClimbing", false);         //ֹͣ����״̬
        AudioManager.Instance.PlaySound("die");             //����������Ч
        _animatorVpet.SetTrigger("Die");                    //���ö���
        GameManager.Instance.VpetDeadHandle();              //֪ͨ������������

    }

    //����ʤ������---------------------------------------------------------------------------------------//
    public void VpetWin()
    {
        currentState = VpetState.Win;   //����״̬
        health.isVpetDead = true;       //��ִֹ����������
        StopAllCoroutines();            //ֹͣ��������Э��
        VpetColliderChange();           //������ײ��
        StopFallingLogic();             //����һ��׹��ֹͣ�߼�

        _animatorVpet.SetBool("isClimbing", false);         //ֹͣ����״̬
        AudioManager.Instance.PlaySound("win");             //���ŵ����յ���Ч
        AudioManager.Instance.PlaySound3D("setRespawnPoint", transform.position);   //��Ч����
        Instantiate(winParticle, transform.position, Quaternion.identity);          //����Ч��
        _animatorVpet.SetTrigger("Win");                    //���ö���
        GameManager.Instance.VpetWinHandle();               //֪ͨ������������

    }

    //������ײ��ı�---------------------------------------------------------------------------------------//

    private CapsuleCollider2D capsuleCollider;

    private void VpetColliderChange()
    {
        //˯�ߺ�����״̬����ײ��
        if(currentState == VpetState.Sleep || currentState == VpetState.Die)
        {
            capsuleCollider.offset = new Vector2(0, -2.67f);            //��ײ��ƫ���� 
            capsuleCollider.size = new Vector2(9f, 3.6f);               //��ײ��ߴ�
            capsuleCollider.direction = CapsuleDirection2D.Horizontal;  //��ײ�䷽��
        }
        //������ײ��
        else
        {
            capsuleCollider.offset = Vector2.zero;                      //��ײ��ƫ����
            capsuleCollider.size = new Vector2(3.67f, 9.2f);            //��ײ��ߴ�
            capsuleCollider.direction = CapsuleDirection2D.Vertical;    //��ײ�䷽��
        }

    }

    //����Ч��---------------------------------------------------------------------------------------//

    [SerializeField] private GameObject TextPrefab;   //�ı�Ԥ����
    private GameObject figureCanvas;                  //FigureCanvas���ڵ�

    //��ʾUI����
    private void ShowText(string text)
    {
        Transform parent = figureCanvas.transform;
        //����TMP�˺�����ʵ��
        GameObject figureText = Instantiate(TextPrefab, transform.position + Vector3.up, Quaternion.identity, parent);
        TextMeshProUGUI tmp = figureText.GetComponent<TextMeshProUGUI>();    //��ȡTMP

        tmp.SetText(text);
        tmp.color = new Color(1f, 1f, 0.4f, 1f);
    }

    //��ײ�¼�---------------------------------------------------------------------------------------//

    private float spikeDamage = 3f;     //����˺�

    private void OnTriggerStay2D(Collider2D other)
    {
        if (health.isVpetDead) return;      //����������ִ��

        //�Ӵ�������ʱ
        if (other.CompareTag("Ladder"))
        {
            if (currentState == VpetState.Dance) return;
            if(currentState == VpetState.Idle || currentState == VpetState.Walking)
                currentState = VpetState.Climb;     //����״̬
            if(currentState == VpetState.Fall)
            {
                StopFallingLogic();                 //ֹͣ׹��
                isAllowEat = true;                  //�����ʳ
                currentState = VpetState.Climb;     //����״̬
     
            }
        }

        if (other.CompareTag("Spike"))
        {
            Vector2 dir = transform.position.y > other.transform.position.y ? Vector2.up : Vector2.down;
            health.VpetGethurt(spikeDamage, dir * 165f);
        }

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (health.isVpetDead) return;      //����������ִ��

        //�뿪����ʱ
        if (other.CompareTag("Ladder") && isClimbing)
        {
            rb.AddForce(Vector2.up * 200f,ForceMode2D.Force);       //ʩ��һ�����ϵ�����
            currentState = VpetState.Walking;     //����״̬
        }
    }

    private float vpetAttackDamage = 3f;      //���蹥���˺�
    private float vpetAttackTimer;            //���蹥����ʱ��
    private float vpetAttackCD = 1.5f;        //���蹥��Ƶ��

    private float attackBuffDamageFix = 1f;   //����Buff�˺�����
    private float attackBuffTimeFix = 1f;     //����Buff����Ƶ������

    private bool isOnePunch = false;          //�Ƿ�һȭ?

    private void OnCollisionStay2D(Collision2D other)
    {
        if (health.isVpetDead) return;      //����������ִ��

        //���������
        if (other.collider.CompareTag("Spike"))
        {
            Vector2 dir = transform.position.y > other.transform.position.y ? Vector2.up + Vector2.right * 0.5f : Vector2.down;
            health.VpetGethurt(spikeDamage, dir * 165f);
        }

        //����������
        if (other.collider.CompareTag("Enemy") && vpetAttackTimer <= 0 && currentState == VpetState.Walking)
        {
            vpetAttackTimer = vpetAttackCD * attackBuffTimeFix;     //����CD����
            var enemyHealth = other.gameObject.GetComponent<EnemyHealthSystem>();

            if (enemyHealth != null)
            {
                //����������״̬
                if (isOnePunch)
                {
                    isOnePunch = false;
                    onePunchState.SetActive(false);  //�ر�Effect״̬ͼ
                    AudioManager.Instance.PlaySound("OnePunch");
                    enemyHealth.GetHurt(vpetAttackDamage * 999 * attackBuffDamageFix, transform.position,25f);
                    CameraShake.Instance.ShakeScreen();
                }
                else
                    enemyHealth.GetHurt(vpetAttackDamage * attackBuffDamageFix, transform.position,5f);

            }
        }

    }
}
