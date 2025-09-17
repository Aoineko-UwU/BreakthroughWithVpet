using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public enum VpetState       //桌宠状态
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
    //获取桌宠的动画器
    [SerializeField] private Animator _animatorVpet;
    [SerializeField] private Animator _animatorVpetHand;
    [SerializeField] private Animator _animatorEatenItem;

    [SerializeField] private GameObject bombPrefab;         //炸弹预制体
    [SerializeField] private GameObject winParticle;        //胜利触发粒子
    [SerializeField] private GameObject speedUpParticle;    //加速Buff粒子
    [SerializeField] private GameObject attackUpParticle;   //伤害Buff粒子

    [SerializeField] private GameObject onePunchState;      //一拳状态

    private Rigidbody2D rb;             //桌宠刚体
    private ConstantForce2D force2D;    //桌宠2D持续力
    private VpetHealthSystem health;    //桌宠生命系统

    private VpetState currentState;  //桌宠状态

    //生命周期--------------------------------------------------------------------------------------------//

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();           //获取刚体
        health = GetComponent<VpetHealthSystem>();  //获取生命系统
        force2D = GetComponent<ConstantForce2D>();  //获取持续2D力   
        currentState = VpetState.Idle;      //初始化状态
        capsuleCollider = GetComponent<CapsuleCollider2D>();    //获取胶囊碰撞箱
        figureCanvas = GameObject.FindGameObjectWithTag("FigureCanvas");
    }

    private void Start()
    {
        VpetColliderChange();               //初始化碰撞箱
        InitGroundContactFilter();          //初始化地面接触过滤器
        InitAllGroundContactFilter();       //初始化全地面接触过滤器
        InitValueBasedDifficulty();         //初始化难度相关数值
    }

    private void FixedUpdate()
    {

        if (health.isVpetDead) return;      //桌宠死亡则不执行
        VpetWalk();     //桌宠移动行为
        VpetClimb();    //桌宠攀爬行为
        VpetFallHorSpeedSet();  //桌宠飘飞水平力
    }

    private void Update()
    {
        if (health.isVpetDead) return;      //桌宠死亡则不执行

        VpetFall();         //桌宠飘飞行为
        VpetFallCheck();    //桌宠飘飞行为监测
        VpetDance();        //桌宠跳舞行为
        TimerWork();        //计时器工作
    }

    //走动行为--------------------------------------------------------------------------------------- -----//

    //根据难度初始化数值(攻击伤害 | 攻击频率 | 尖刺伤害
    private void InitValueBasedDifficulty()
    {
        //获取游戏难度进行匹配
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //简单难度
            case GameDifficultyLevel.Easy:
                vpetAttackDamage = 4f;
                vpetAttackCD = 1.2f;
                spikeDamage = 2f;
                break;

            //正常难度
            case GameDifficultyLevel.Normal:
                vpetAttackDamage = 3f;
                vpetAttackCD = 1.3f;
                spikeDamage = 3f;
                break;

            //困难难度
            case GameDifficultyLevel.Hard:
                vpetAttackDamage = 2f;
                vpetAttackCD = 1.4f;
                spikeDamage = 4f;
                break;
        }
    }

    //走动行为--------------------------------------------------------------------------------------- -----//

    private float speedUpBuffFix = 1f;      //加速Buff移速修正
    private float _vpetWalkSpeed = 5.5f;    //行走速度
    private float forceMultiplier = 2f;     //行走力放大倍率
    private float walkAudioTimer;
    private float walkAudioCD = 0.62f;

    private void VpetWalk()
    {
        //桌宠状态为行走时执行移动
        if (currentState == VpetState.Walking)
        {
            //走路音效播放
            if (walkAudioTimer <= 0)
            {
                walkAudioTimer = walkAudioCD;
                if (isInWater)
                    AudioManager.Instance.PlaySound("swim");
                else
                    AudioManager.Instance.PlaySound("walk");
            }

            _animatorVpet.SetBool("isWalk", true);

            Vector2 avgNormal = GetAverageGroundNormal();                           //取平均法线
            Vector2 tangent = new Vector2(avgNormal.y, -avgNormal.x).normalized;    //计算切线方向

            // 如果 avgNormal 表示“垂直墙面”（法线几乎水平），就强制往右走
            if (Mathf.Abs(avgNormal.x) > 0.9f && Mathf.Abs(avgNormal.y) < 0.1f)
            {
                tangent = Vector2.right;
            }

            //重力补偿
            Vector2 gravityForce = rb.mass * Physics2D.gravity;     //计算重力 G=mg           
            float gAlong = Vector2.Dot(gravityForce, tangent);      //重力在切线方向上的分量            
            Vector2 compensationForce = -gAlong * tangent;          //补偿力：反方向抵消

            //若Vpet上坡
            if (avgNormal.y > 0) 
                rb.AddForce(compensationForce * 0.6f, ForceMode2D.Force);  //保留一部分重力给Vpet带来的影响
            //若Vpet下坡
            else if(avgNormal.y < 0)
                rb.AddForce(compensationForce, ForceMode2D.Force);         //施加全力抵消重力加速度


            forceMultiplier = isInWater ? 0.8f : isTouchGround() ? 2f :
                          isGrounded ? 0.6f : 0f;                    //移动力倍率


            Vector2 desiredVel = tangent * _vpetWalkSpeed;                                   //推动沿切线匀速
            Vector2 force = (desiredVel - rb.velocity) * forceMultiplier * speedUpBuffFix;   //移动力最终计算
            rb.AddForce(force, ForceMode2D.Force);
        }
        else
        {
            _animatorVpet.SetBool("isWalk", false);
        }
    }

    [SerializeField] LayerMask groundLayer;                             //仅地面层级
    private ContactFilter2D groundContactFilter;                        //地面接触过滤器
    private ContactPoint2D[] contactPoints = new ContactPoint2D[10];    //接触点数组

    [SerializeField] LayerMask allGroundLayer;                          //所有地面有关层级
    private ContactFilter2D allGroundContactFilter;                     //全地面接触过滤器
    private ContactPoint2D[] allContactPoints = new ContactPoint2D[1];  //接触点数组

    // 获取平均法线
    private Vector2 GetAverageGroundNormal()
    {
        // 获取所有接触点
        int count = rb.GetContacts(groundContactFilter, contactPoints);
        if (count == 0)
            return Vector2.up; 

        Vector2 sum = Vector2.zero;
        for (int i = 0; i < count; i++)
            sum += contactPoints[i].normal;
        return (sum / count).normalized;
    }

    //是否与地面碰撞箱接触
    private bool isTouchGround()
    {
        // 获取所有接触点
        int count = rb.GetContacts(allGroundContactFilter, allContactPoints);
        if (count > 0)
            return true;
        else
            return false;
    }

    //初始化地面接触过滤器
    private void InitGroundContactFilter()
    {
        groundContactFilter = new ContactFilter2D();
        groundContactFilter.SetLayerMask(groundLayer);
        groundContactFilter.useTriggers = false;
    }

    //初始化全地面接触过滤器
    private void InitAllGroundContactFilter()
    {
        allGroundContactFilter = new ContactFilter2D();
        allGroundContactFilter.SetLayerMask(allGroundLayer);
        allGroundContactFilter.useTriggers = false;
    }

    //攀爬行为--------------------------------------------------------------------------------------------//

    private float _vpetClimbSpeed = 7f;                     //攀爬速度
    private float climbForceMultiplier = 2f;                //攀爬力放大倍率
    private bool isClimbing = false;                        //是否正在攀爬？

    private float climbAudioTimer;                          //攀爬音效计时器
    private float climbAudioCD = 0.62f;                     //攀爬音效间隔
    private void VpetClimb()
    {
        _animatorVpet.SetBool("isClimbing", isClimbing);    //动画机状态同步
        //桌宠状态为攀爬时执行
        if (currentState == VpetState.Climb)
        {
            _animatorVpet.ResetTrigger("ClimbEnd");         //重置ClimbEnd Trigger

            //若此时不是正在攀爬，则执行一次动画
            if (!isClimbing)
            {
                isClimbing = true;
                rb.velocity = Vector2.zero;
                _animatorVpet.SetTrigger("ClimbStart");
            }

            Vector2 desiredVel = Vector2.up * _vpetClimbSpeed;  //期望速度：向上匀速            
            Vector2 climbForce = (desiredVel - rb.velocity) * climbForceMultiplier; //计算驱动力
            rb.AddForce(climbForce, ForceMode2D.Force);         //施加力

            //攀爬音效播放(随机)
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

    //进食行为---------------------------------------------------------------------------------------//
    [SerializeField] SpriteRenderer eatenItemSprite;    //进食道具Sprite渲染器绑定
    [SerializeField] GameObject onePunchEffect;         //一拳状态粒子
    public bool isAllowEat = true;                      //是否允许进食？

    //桌宠进食
    public void VpetEat(ItemData item)
    {
        if (health.isVpetDead) return;      //桌宠死亡则不执行

        //非空检查
        if (item != null && isAllowEat)
        {
            if (isFalling) StopFallingLogic();      //若处于坠落状态，则停止坠落逻辑

            isAllowEat = false;                     //更改为不允许再进食
            eatenItemSprite.sprite = item.icon;     //改变食物精灵图
            currentState = VpetState.Eat;           //改变桌宠当前状态

            //播放进食动画
            _animatorVpet.SetTrigger("Eat");    
            _animatorVpetHand.SetTrigger("Eat");
            _animatorEatenItem.SetTrigger("Eat");

            AudioManager.Instance.PlaySound("pickItem");    //播放音效

            //启用协程延迟时间后判断食物类型
            StartCoroutine(StartFoodJudge(item));

            //重置攀爬状态
            if (isClimbing)
                isClimbing = false;

            //重置飘飞状态
            if (isFalling)
                isFalling = false;

        }
    }
    IEnumerator StartFoodJudge(ItemData item)
    {
        yield return new WaitForSeconds(0.8f);
        AudioManager.Instance.PlaySound("eating");  //播放吃东西音效
        yield return new WaitForSeconds(1.6f);

        int index = item.itemID;    //获取物品ID

        //根据物品ID判断执行不同效果
        switch (index)
        {
            //金苹果
            case 0:
                health.VpetRecover(10f);   //回复生命
                currentState = VpetState.Walking;                   //更新桌宠状态
                isAllowEat = true;
                break;
            
            //昏睡红茶
            case 1:
                StartCoroutine(VpetSleep());    //桌宠睡眠
                break;

            //肾宝
            case 2:
                isOnePunch = true;              //强化下一次攻击
                onePunchState.SetActive(true);  //设置Effect状态图
                Instantiate(onePunchEffect, transform.position, Quaternion.identity);
                AudioManager.Instance.PlaySound("OnePunchState");
                currentState = VpetState.Walking;
                isAllowEat = true;
                break;

            //地球
            case 3:
                int eventIndex = RandomSelector.Instance.EventRandomSelector(1);    //获取随机的事件
                switch(eventIndex)
                {
                    //生命恢复事件
                    case 1:
                        health.VpetRecover(20f);
                        currentState = VpetState.Walking;
                        break;
                    //瞬间死亡
                    case 2:
                        health.VpetGethurt(999f, Vector2.up * 100f);
                        currentState = VpetState.Die;
                        break;
                    //移动加速
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
                    //普通攻击伤害增加，攻击频率加快
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
                    //扣除生命
                    case 5:
                        health.VpetGethurt(10f, Vector2.up * 100f);
                        currentState = VpetState.Walking;
                        break;
                    //瞬移
                    case 6:
                        Teleport();
                        currentState = VpetState.Walking;
                        break;
                    //瞬间爆炸
                    case 7:
                        var bomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);
                        bomb.GetComponent<Item_Block_bomb>().isInstanctlyExplode = true;
                        currentState = VpetState.Walking;
                        break;
                };

                isAllowEat = true;
                break;

            //可乐(加伤害)
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

            //雪碧(加速)
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
                Debug.Log("未知食物类型");
                break;
        }

    }

    float speedUpBuffMultiplier = 1.7f;         //加速Buff给予的加速倍率
    float speedUpBuffDuration = 12f;            //加速持续时间
    private Coroutine speedUpBuffCoroutine;     //加速Buff协程

    IEnumerator Eat_SpeedUp()
    {
        ShowText("速度提升↑↑");
        AudioManager.Instance.PlaySound("getBuff");
        speedUpBuffFix = speedUpBuffMultiplier; //更改加速倍率
        var par = Instantiate(speedUpParticle, transform.position, Quaternion.identity, transform); //粒子生成
        Destroy(par, speedUpBuffDuration);  
        //等待效果持续时间
        yield return new WaitForSeconds(speedUpBuffDuration);

        speedUpBuffFix = 1f;            //恢复默认倍率
        speedUpBuffCoroutine = null;    //清理本协程
    }

    float attackBuffDamageMultiplier = 2f;      //攻击Buff给予的攻击倍率
    float attackBuffTimeMultiplier = 0.5f;      //攻击Buff给予的攻击间隔倍率
    float attackBuffDuration = 12f;             //攻击Buff的持续时间
    private Coroutine AttackUpBuffCoroutine;    //攻击Buff协程

    IEnumerator Eat_AttackUp()
    {
        AudioManager.Instance.PlaySound("getBuff");
        ShowText("攻击提升↑↑");
        health.SetKnockBack(false);     //不可击退状态
        var par = Instantiate(attackUpParticle, transform.position, Quaternion.identity, transform); //粒子生成
        Destroy(par, speedUpBuffDuration);
        //更改倍率
        attackBuffDamageFix = attackBuffDamageMultiplier;
        attackBuffTimeFix = attackBuffTimeMultiplier;
        //等待效果持续时间
        yield return new WaitForSeconds(attackBuffDuration);
        //恢复默认倍率
        attackBuffDamageFix = 1f;
        attackBuffTimeFix = 1f;
        health.SetKnockBack(true);      //可击退状态

        AttackUpBuffCoroutine = null;   //清理本协程
    }


    private float teleportRange = 12f;     // 最大瞬移范围
    private int maxSearchAttempts = 10;    // 最大查找次数

    //瞬移方法效果
    private void Teleport()
    {
        Vector2 targetPosition = Vector2.zero;
        bool foundValidPosition = false;

        // 尝试查找最多 maxSearchAttempts 次
        for (int i = 0; i < maxSearchAttempts; i++)
        {
            // 随机选择一个目标位置（在指定范围内）
            Vector2 randomDirection = Random.insideUnitCircle * teleportRange;  // 在一个圆形范围内随机
            targetPosition = (Vector2)transform.position + randomDirection;

            // 检查目标位置是否有效
            if (CanTeleportTo(targetPosition))
            {
                foundValidPosition = true;  // 找到了有效的目标位置
                break;  // 退出查找
            }
        }
        // 如果找到有效位置，则瞬移，否则原地瞬移
        if (foundValidPosition)
            transform.position = targetPosition;
        else
            transform.position += Vector3.up * 0.5f;

        AudioManager.Instance.PlaySound("teleport");
    }

    // 检查目标位置是否有效
    private bool CanTeleportTo(Vector2 targetPosition)
    {
        Collider2D hit = Physics2D.OverlapCapsule(targetPosition,capsuleCollider.size*0.2f,capsuleCollider.direction,0f);
        // 如果目标位置有碰撞体，则不能瞬移
        if (hit != null && !hit.CompareTag("Ignore"))         
            return false;

        return true;
    }

    //// 使用Gizmo绘制出目标地点的监测半径
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red; // 监测范围的颜色
    //    Gizmos.DrawWireSphere(transform.position, teleportRange);  // 绘制绿色的圆形区域
    //}


    //睡觉行为---------------------------------------------------------------------------------------//

    private float sleepTime = 7.5f;  //实际睡眠时间(+2.5s)
    private float sleepAudioTimer;
    private float sleepAudioTimerCD = 2.3f;

    private float sleepRecoverTimer;        //睡眠恢复计时器
    private float sleepRecoverCD = 1f;      //睡眠恢复间隔
    private float sleepRecoverRate = 1f;    //每次生命恢复量

    IEnumerator VpetSleep()
    {
        currentState = VpetState.Sleep;
        AudioManager.Instance.PlaySound("startSleep");  //音效播放
        _animatorVpet.SetTrigger("SleepStart");         //睡眠动作播放

        yield return new WaitForSeconds(2.5f);          //等待动作播放

        VpetColliderChange();                           //改变碰撞箱

        float elapsedTime = 0f;     //经过时间
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

            yield return null; // 等待下一帧，保持循环活跃
        }

        AudioManager.Instance.StopSound("sleeping");
        _animatorVpet.SetTrigger("SleepEnd");           //起身动作播放
        yield return new WaitForSeconds(0.9f);          //等待动作播放

        currentState = VpetState.Walking;               //更改为行走状态
        VpetColliderChange();                           //更改碰撞箱
        isAllowEat = true;                              //允许进食
    }

    //飘飞行为---------------------------------------------------------------------------------------//

    private bool isGrounded = false;    //是否落地
    private bool isInWater = false;     //是否处于水中

    private bool isAllowFallCheckTimer = false; //是否允许坠落监测计时器工作
    private float fallConfirmTimer;
    private float fallConfirmInterval = 0.2f;   //坠落监测间隔(超过这个时间不处于地面则判定为坠落中)
    private float rayLength = 1.6f;             //射线长度
    private float halfWidth = 0.38f;            //射线半宽间隔

    //桌宠坠落行为监测(Update)
    private void VpetFallCheck()
    {
        // 三个射线起点：中、左、右
        Vector2 centerOrigin = transform.position;
        Vector2 leftOrigin = centerOrigin + Vector2.left * halfWidth;
        Vector2 rightOrigin = centerOrigin + Vector2.right * halfWidth;
        isGrounded = false;
        isInWater = false; 

        // 依次发射三条向下射线，使用 allGroundLayer 过滤
        foreach (Vector2 origin in new[] { centerOrigin, leftOrigin, rightOrigin })
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, allGroundLayer);
            if (hit.collider != null)
            {
                isGrounded = true;
                // 检查碰撞体是否是水
                if (hit.collider.CompareTag("Water"))
                    isInWater = true;

                break;
            }
        }

        //Debug.DrawRay(centerOrigin, Vector2.down * rayLength, isGrounded ? Color.green : Color.red);
        //Debug.DrawRay(leftOrigin, Vector2.down * rayLength, isGrounded ? Color.green : Color.red);
        //Debug.DrawRay(rightOrigin, Vector2.down * rayLength, isGrounded ? Color.green : Color.red);

        // 从行走切到坠落
        if (currentState == VpetState.Walking || currentState == VpetState.Idle)
        {
            //启用掉落状态监测
            if (!isAllowFallCheckTimer && !isGrounded)
            {
                fallConfirmTimer = fallConfirmInterval; //赋予时间
                isAllowFallCheckTimer = true;           //开始进行监测
            }
        }
        //若启用了掉落状态监测计时器&&计时完成
        if (isAllowFallCheckTimer && fallConfirmTimer <= 0)
        {
            //计时结束时若还处在空中
            if (!isGrounded)
            {
                currentState = VpetState.Fall;      //确认转换为飘飞状态
                isAllowFallCheckTimer = false;      //停止计时器使用
            }
            else
                isAllowFallCheckTimer = false;      //停止计时器使用
        }        
    }
        
    private float flyingForceVer = 7.4f;  //垂直飘飞力
    private bool isFalling = false;     //是否正在坠落

    private float fallAudioTimer;       //飘飞音效计时器
    private float fallAudioCD = 1f;     //飘飞音效间隔

    //桌宠坠落(Update)
    private void VpetFall()
    {
        //桌宠状态为坠落时触发
        if(currentState == VpetState.Fall && !isGrounded && !isGetUpCoroutineWork)
        {
            //执行一次
            if (!isFalling)
            {
                isFalling = true;                               //设置为正在下落
                _animatorVpet.SetBool("isFalling", isFalling);  //更新动画器参数
                _animatorVpet.SetTrigger("FallStart");          //播放一次动画
                AudioManager.Instance.PlaySound("startFall");   //播放一次音效
                force2D.force = Vector2.up * flyingForceVer;    //设置悬浮力
            }

            //飘飞音效播放
            if (fallAudioTimer <= 0 && isFalling)
            {
                fallAudioTimer = fallAudioCD;
                AudioManager.Instance.PlaySound("fall");
            }
        }

        //若已落地
        if(isGrounded && isFalling && currentState == VpetState.Fall && !isGetUpCoroutineWork)
        {
            isAllowEat = false;                         //禁止进食
            StopFallingLogic();                         //坠落停止逻辑       
            StartCoroutine(VpetGetUp());                //起身延迟
            AudioManager.Instance.PlaySound("fallen");  //音效播放
        }
    }

    private float flyingSpeedHor = 4f;             //水平飘飞速度
    private float fallHorForceMultiplier = 2f;     //正常向右校正系数
    private float negativeVelMultiplier = 0.08f;   //负向速度校正系数

    //桌宠坠落水平速度设置(FixUpdate)
    private void VpetFallHorSpeedSet()
    {
        if (currentState == VpetState.Fall && !isGrounded && isFalling)
        {
            float vx = rb.velocity.x;               //获取X轴当前速度          
            float speedDiff = flyingSpeedHor - vx;  //速度差

            //当移速处于不同方向时，选用不同系数
            float k = vx < 0
                ? fallHorForceMultiplier * negativeVelMultiplier
                : fallHorForceMultiplier;

            // 施加力
            float forceX = speedDiff * k * speedUpBuffFix;          
            rb.AddForce(Vector2.right * forceX, ForceMode2D.Force);

        }
    }

    bool isGetUpCoroutineWork = false;

    //桌宠起身延迟
    IEnumerator VpetGetUp()
    {
        isGetUpCoroutineWork = true;
        yield return new WaitForSeconds(2.5f);
        isAllowEat = true;
        currentState = VpetState.Walking;
        isGetUpCoroutineWork = false;
    }

    //坠落停止逻辑
    private void StopFallingLogic()
    {
        isFalling = false;                              //变更为不在掉落中
        _animatorVpet.SetBool("isFalling", isFalling);  //更新动画器参数
        isAllowFallCheckTimer = false;
        force2D.force = Vector2.zero;                 //终止力的施加
        AudioManager.Instance.StopSound("fall");      //终止音效播放
    }

    //跳舞行为---------------------------------------------------------------------------------------//
    private bool isVpetDancing = false;     //是否正在跳舞
    private float vpetDanceRecoverTimer;    //跳舞生命回复计时器
    private float vpetDanceRecoverCD = 1f;  //计时器CD
    private float recoverPerDance = 1f;     //每次恢复量

    private float vpetDanceAttackTimer;     //跳舞伤害计时器
    private float vpetDanceAttackCD = 0.5f; //计时器CD
    private float damagePerDance = 3f;      //每次伤害量
    private float damagePerDanceRadius = 2f;//伤害半径
    [SerializeField] private LayerMask enemyLayer;  //敌人层

    private void VpetDance()
    {
        if(currentState == VpetState.Dance)
        {
            isAllowEat = false;                 //禁止进食
            health.isVpetInvincible = true;     //无敌效果

            if (!isVpetDancing)
            {
                isVpetDancing = true;
                _animatorVpet.SetTrigger("Dance");                  //触发动作
                AudioManager.Instance.PlayBGM("DanceMusic");        //播放音乐
                StartCoroutine(DanceTime());
            }

            //生命恢复
            if (vpetDanceRecoverTimer <= 0)
            {
                vpetDanceRecoverTimer = vpetDanceRecoverCD;         
                health.VpetRecover(recoverPerDance);                
            }
            //造成伤害
            if(vpetDanceAttackTimer <= 0)
            {
                bool isHurt = false;
                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, damagePerDanceRadius, enemyLayer);

                foreach (Collider2D col in colliders)
                {
                    if (col != null)
                    {
                        col.gameObject.GetComponent<EnemyHealthSystem>().GetHurt(damagePerDance,transform.position,3f);   //造成伤害
                        isHurt = true;
                    }
                }
                if (isHurt)
                    vpetDanceAttackTimer = vpetDanceAttackCD;   //造成伤害则刷新CD

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
        AudioManager.Instance.PauseOrContinueBGM(true); //暂停BGM
        currentState = VpetState.Walking;               //状态转变
        _animatorVpet.SetTrigger("DanceEnd");           //播放动画
        yield return new WaitForSeconds(0.5f);          //短暂等待
        AudioManager.Instance.AdjustBGMVolume(1);       //恢复BGM音源音量
        AudioManager.Instance.PlayBGM("GameMusic");     //播放游戏BGM
        isAllowEat = true;                              //允许进食
        isVpetDancing = false;                          //关闭跳舞状态
        health.isVpetInvincible = false;                //关闭无敌状态
    }


    //计时器-----------------------------------------------------------------------------------------//

    //计时器工作(Update)
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

    //设置状态-----------------------------------------------------------------------------------------//

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
                Debug.Log("未知状态设置");
                break;
        }
    }

    //桌宠死亡处理---------------------------------------------------------------------------------------//

    public void VpetDead()
    {
        currentState = VpetState.Die;   //更改状态
        StopAllCoroutines();            //停止其他所有协程
        VpetColliderChange();           //改变碰撞箱
        StopFallingLogic();             //进行一次坠落停止逻辑

        _animatorVpet.SetBool("isClimbing", false);         //停止攀爬状态
        AudioManager.Instance.PlaySound("die");             //播放死亡音效
        _animatorVpet.SetTrigger("Die");                    //设置动画
        GameManager.Instance.VpetDeadHandle();              //通知进行死亡处理

    }

    //桌宠胜利处理---------------------------------------------------------------------------------------//
    public void VpetWin()
    {
        currentState = VpetState.Win;   //更改状态
        health.isVpetDead = true;       //防止执行其他操作
        StopAllCoroutines();            //停止其他所有协程
        VpetColliderChange();           //更新碰撞箱
        StopFallingLogic();             //进行一次坠落停止逻辑

        _animatorVpet.SetBool("isClimbing", false);         //停止攀爬状态
        AudioManager.Instance.PlaySound("win");             //播放到达终点音效
        AudioManager.Instance.PlaySound3D("setRespawnPoint", transform.position);   //音效播放
        Instantiate(winParticle, transform.position, Quaternion.identity);          //粒子效果
        _animatorVpet.SetTrigger("Win");                    //设置动画
        GameManager.Instance.VpetWinHandle();               //通知进行死亡处理

    }

    //桌宠碰撞箱改变---------------------------------------------------------------------------------------//

    private CapsuleCollider2D capsuleCollider;

    private void VpetColliderChange()
    {
        //睡眠和死亡状态的碰撞箱
        if(currentState == VpetState.Sleep || currentState == VpetState.Die)
        {
            capsuleCollider.offset = new Vector2(0, -2.67f);            //碰撞箱偏移量 
            capsuleCollider.size = new Vector2(9f, 3.6f);               //碰撞箱尺寸
            capsuleCollider.direction = CapsuleDirection2D.Horizontal;  //碰撞箱方向
        }
        //常规碰撞箱
        else
        {
            capsuleCollider.offset = Vector2.zero;                      //碰撞箱偏移量
            capsuleCollider.size = new Vector2(3.67f, 9.2f);            //碰撞箱尺寸
            capsuleCollider.direction = CapsuleDirection2D.Vertical;    //碰撞箱方向
        }

    }

    //文字效果---------------------------------------------------------------------------------------//

    [SerializeField] private GameObject TextPrefab;   //文本预制体
    private GameObject figureCanvas;                  //FigureCanvas父节点

    //显示UI数字
    private void ShowText(string text)
    {
        Transform parent = figureCanvas.transform;
        //创建TMP伤害数字实例
        GameObject figureText = Instantiate(TextPrefab, transform.position + Vector3.up, Quaternion.identity, parent);
        TextMeshProUGUI tmp = figureText.GetComponent<TextMeshProUGUI>();    //获取TMP

        tmp.SetText(text);
        tmp.color = new Color(1f, 1f, 0.4f, 1f);
    }

    //碰撞事件---------------------------------------------------------------------------------------//

    private float spikeDamage = 3f;     //尖刺伤害

    private void OnTriggerStay2D(Collider2D other)
    {
        if (health.isVpetDead) return;      //桌宠死亡则不执行

        //接触到梯子时
        if (other.CompareTag("Ladder"))
        {
            if (currentState == VpetState.Dance) return;
            if(currentState == VpetState.Idle || currentState == VpetState.Walking)
                currentState = VpetState.Climb;     //更改状态
            if(currentState == VpetState.Fall)
            {
                StopFallingLogic();                 //停止坠落
                isAllowEat = true;                  //允许进食
                currentState = VpetState.Climb;     //更改状态
     
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
        if (health.isVpetDead) return;      //桌宠死亡则不执行

        //离开梯子时
        if (other.CompareTag("Ladder") && isClimbing)
        {
            rb.AddForce(Vector2.up * 200f,ForceMode2D.Force);       //施加一个向上的推力
            currentState = VpetState.Walking;     //更改状态
        }
    }

    private float vpetAttackDamage = 3f;      //桌宠攻击伤害
    private float vpetAttackTimer;            //桌宠攻击计时器
    private float vpetAttackCD = 1.5f;        //桌宠攻击频率

    private float attackBuffDamageFix = 1f;   //攻击Buff伤害修正
    private float attackBuffTimeFix = 1f;     //攻击Buff攻击频率修正

    private bool isOnePunch = false;          //是否一拳?

    private void OnCollisionStay2D(Collision2D other)
    {
        if (health.isVpetDead) return;      //桌宠死亡则不执行

        //若碰到尖刺
        if (other.collider.CompareTag("Spike"))
        {
            Vector2 dir = transform.position.y > other.transform.position.y ? Vector2.up + Vector2.right * 0.5f : Vector2.down;
            health.VpetGethurt(spikeDamage, dir * 165f);
        }

        //若碰到敌人
        if (other.collider.CompareTag("Enemy") && vpetAttackTimer <= 0 && currentState == VpetState.Walking)
        {
            vpetAttackTimer = vpetAttackCD * attackBuffTimeFix;     //攻击CD重置
            var enemyHealth = other.gameObject.GetComponent<EnemyHealthSystem>();

            if (enemyHealth != null)
            {
                //若处于肾宝状态
                if (isOnePunch)
                {
                    isOnePunch = false;
                    onePunchState.SetActive(false);  //关闭Effect状态图
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
