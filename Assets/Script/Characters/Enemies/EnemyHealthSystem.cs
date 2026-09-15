using System.Collections;
using TMPro;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 敌人生命系统类
/// - 管理敌人生命、受击与死亡表现，并在远离桌宠时自动销毁。
/// </summary>
public class EnemyHealthSystem : MonoBehaviour
{
    #region 配置与运行状态

    [Tooltip("血条UI绑定。")]
    [SerializeField] private SpriteRenderer healthBar;

    [Tooltip("生命值。")]
    [SerializeField] public float health = 30;

    [Tooltip("粒子预制体。")]
    [SerializeField] private GameObject particlePrefab;

    /// <summary>当前生命值。</summary>
    private float currentHealth;

    /// <summary>血条初始宽度。</summary>
    private float oringinWidth;

    /// <summary>当前对象的精灵渲染器。</summary>
    private SpriteRenderer sprite;

    /// <summary>当前对象的二维刚体组件。</summary>
    private Rigidbody2D rb;

    [Tooltip("是否已经死亡。")]
    public bool isDead = false;

    /// <summary>用于交互或距离判断的桌宠对象。</summary>
    private GameObject vpet;

    #endregion

    #region 初始化与状态更新

    /// <summary>
    /// 缓存桌宠、刚体、精灵和飘字画布，并初始化难度生命倍率与血条。
    /// </summary>
    private void Start()
    {
        vpet = GameObject.FindGameObjectWithTag("Vpet");                    //获取桌宠的游戏对象
        sprite = GetComponent<SpriteRenderer>();                            //获取精灵渲染
        rb = GetComponent<Rigidbody2D>();                                   //获取刚体
        figureCanvas = GameObject.FindGameObjectWithTag("FigureCanvas");    //获取数字Canvas层

        InitValueBasedDifficulty();         //初始化数值

        InitHealth();   //初始化生命

    }

    /// <summary>
    /// 存活时更新血条、离场销毁及死亡判定；死亡后维持红色显示。
    /// </summary>
    private void Update()
    {
        if (isDead)
            sprite.color = sprite.color = new Color(1f, 0, 0, 1f);      //更改精灵颜色
        else
        {
            //血条更新
            healthBar.size = new Vector2(oringinWidth * (currentHealth / totalHealth), healthBar.size.y);
            SelfDestroy();  //自摧毁逻辑
            HealthCheck();  //生命监测
        }
    }

    #endregion

    #region 生命初始化与受击死亡

    /// <summary>
    /// 根据当前难度设置敌人初始生命倍率。
    /// </summary>
    private void InitValueBasedDifficulty()
    {
        //获取游戏难度进行匹配(怪物生命倍率)
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //简单难度
            case GameDifficultyLevel.Easy:
                healthMultiplier = 0.75f;
                break;

            //正常难度
            case GameDifficultyLevel.Normal:
                healthMultiplier = 1;
                break;

            //困难难度
            case GameDifficultyLevel.Hard:
                healthMultiplier = 1.25f;
                break;
        }
    }

    /// <summary>生命倍率。</summary>
    private float healthMultiplier = 1f;

    /// <summary>应用难度倍率并向上取整后的最大生命值。</summary>
    private float totalHealth;

    /// <summary>
    /// 记录血条宽度，将基础生命乘以难度倍率并向上取整，作为最大与当前生命。
    /// </summary>
    private void InitHealth()
    {
        oringinWidth = healthBar.size.x;  //记录初始宽度
        totalHealth = Mathf.Ceil(health * healthMultiplier);//生命值初始化
        currentHealth = totalHealth;
    }

    /// <summary>
    /// 生命归零且尚未标记死亡时，启动死亡表现并标记为死亡。
    /// </summary>
    private void HealthCheck()
    {
        if (currentHealth <= 0 && !isDead)
        {
            StartCoroutine(Dead());
            StopCoroutine(HurtEffect());
            AudioManager.Instance.PlaySound3D("Enemy_die", transform.position);
            isDead = true;
        }
    }

    /// <summary>
    /// 扣除生命、播放受击反馈，并施加远离伤害来源的冲量；死亡后忽略请求。
    /// </summary>
    /// <param name="damage">本次扣除的生命值。</param>
    /// <param name="pos">伤害来源的世界坐标，用于计算击退方向。</param>
    /// <param name="force">沿远离来源方向施加的击退冲量大小。</param>
    public void GetHurt(float damage , Vector3 pos, float force)
    {
        if (isDead) return;

        float newHealth = currentHealth - damage;  //受伤后的生命值
        //若受伤后生命值低于0
        if (newHealth <= 0)
            currentHealth = 0;     //生命值固定为0
        //否则正常受伤
        else
            currentHealth = newHealth;

        ShowFigure(damage, true);       //受伤数字
        StartCoroutine(HurtEffect());   //受伤效果
        AudioManager.Instance.PlaySound3D("Enemy_getHurt", transform.position);

        Vector3 dir = (transform.position - pos).normalized;  //计算方向向量
        Vector2 pushForce = dir * force;                      //将方向向量与力相乘，得到推力
        rb.AddForce(pushForce, ForceMode2D.Impulse);          //将推力施加到刚体上
    }

    /// <summary>
    /// 短暂将精灵染红，再恢复为白色。
    /// </summary>
    /// <returns>控制受击变色等待的协程迭代器。</returns>
    IEnumerator HurtEffect()
    {
        sprite.color = new Color(1f, 0.5f, 0.5f, 1f);
        yield return new WaitForSeconds(0.3f);
        sprite.color = new Color(1f, 1f, 1f, 1f);
    }

    /// <summary>
    /// 朝桌宠相对方向播放翻转动画，延迟销毁敌人并生成配置的死亡粒子。
    /// </summary>
    /// <returns>控制死亡延迟的协程迭代器。</returns>
    IEnumerator Dead()
    {
        GameObject vpet = GameObject.FindGameObjectWithTag("Vpet");
        if(vpet.transform.position.x>transform.position.x)
            transform.DORotate(Vector3.forward * 180, 0.5f);              //旋转动画
        else
            transform.DORotate(Vector3.forward * -180, 0.5f);              //旋转动画

        yield return new WaitForSeconds(1f);                        //1s后销毁
        Destroy(gameObject);
        if (particlePrefab != null)
            ObjectPoolManager.GetOrCreate().Spawn(particlePrefab, transform.position, Quaternion.identity); //生成粒子
    }

    #endregion

    #region 飘字与远距离离场

    [Tooltip("数字文本预制体。")]
    [SerializeField] private GameObject figureTextPrefab;

    /// <summary>FigureCanvas父节点。</summary>
    private GameObject figureCanvas;

    /// <summary>
    /// 在飘字画布下生成数值文本，并按伤害或恢复类型设置颜色。
    /// </summary>
    /// <param name="num">要显示的数值。</param>
    /// <param name="isRed">为 true 时使用受伤红色，为 false 时使用恢复绿色。</param>
    private void ShowFigure(float num, bool isRed)
    {
        Transform parent = figureCanvas.transform;
        // 优先从对象池获取飘字，预制体或对象池不可用时保留直接实例化兜底。
        ObjectPoolManager pool = ObjectPoolManager.GetOrCreate();
        GameObject figureText = pool.Spawn(figureTextPrefab, transform.position + Vector3.up, Quaternion.identity, parent);
        TextMeshProUGUI tmp = figureText.GetComponent<TextMeshProUGUI>();    //获取TMP

        tmp.SetText(num.ToString());

        //设置文本颜色
        if (isRed)
            tmp.color = new Color(1, 0.4f, 0.4f, 1);
        else
            tmp.color = new Color(0.4f, 1, 0.5f, 1);
    }

    /// <summary>是否允许激活自摧毁计时器。</summary>
    bool isAllowStartDestroyTimer = false;

    /// <summary>自摧毁计时器。</summary>
    float destroyTimer;

    /// <summary>自摧毁间隔。</summary>
    float destroyInterval = 5f;

    /// <summary>与玩家距离多远以后允许进行自摧毁。</summary>
    float destroyDistance = 30f;

    /// <summary>父重生脚本。</summary>
    private SpawnPoint parentSpawnPoint;

    /// <summary>
    /// 连续远离桌宠达到等待时长后销毁自身，并请求出生点延迟重置生成标记。
    /// </summary>
    private void SelfDestroy()
    {
        //自毁允许条件
        isAllowStartDestroyTimer = Vector2.Distance(vpet.transform.position, transform.position) > destroyDistance ? true : false;

        if (isAllowStartDestroyTimer)
            destroyTimer -= Time.deltaTime;     //若条件允许则开始计时
        else
            destroyTimer = destroyInterval;     //否则重置摧毁时间

        //摧毁计时结束后摧毁本游戏对象
        if (destroyTimer <= 0)
        {
            if (parentSpawnPoint != null)
                parentSpawnPoint.DelayResetRespawnState();    //重置重生状态

            Destroy(gameObject);
        }

    }

    /// <summary>
    /// 记录生成此敌人的出生点，供远距离自动离场时重置生成状态。
    /// </summary>
    /// <param name="spawnPoint">所属的敌人出生点；允许为空，此时不发出重置请求。</param>
    public void SetParentSpawnPoint(SpawnPoint spawnPoint)
    {
        parentSpawnPoint = spawnPoint;
    }

    #endregion
}
