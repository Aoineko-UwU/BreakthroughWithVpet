using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 炸弹方块类
/// - 管理炸弹自损引信、触发爆炸、范围推力和对方块及敌人的伤害。
/// </summary>
public class Item_Block_bomb : MonoBehaviour
{
    #region 配置与运行状态

    [Tooltip("血条UI绑定。")]
    [SerializeField] private SpriteRenderer healthBar;

    [Tooltip("生命值。")]
    [SerializeField] private float health = 5;

    [Tooltip("粒子预制体。")]
    [SerializeField] private GameObject particlePrefab;

    /// <summary>当前生命值。</summary>
    private float currentHealth;

    /// <summary>血条初始宽度。</summary>
    private float oringinWidth;

    /// <summary>自损计时器。</summary>
    private float hurtTimer;

    /// <summary>自损间隔时长(s)。</summary>
    private float hurtCD = 1f;

    /// <summary>单次自损伤害。</summary>
    private int damage = 1;

    #endregion

    #region 引信初始化与更新

    /// <summary>
    /// 初始化生命、自损计时、血条与飘字画布，启动闪烁反馈及引信音效。
    /// </summary>
    private void Start()
    {
        figureCanvas = GameObject.FindGameObjectWithTag("FigureCanvas");    //获取伤害数字Canvas
        currentHealth = health;           //生命值初始化
        hurtTimer = hurtCD;               //计时器初始化
        oringinWidth = healthBar.size.x;  //记录初始宽度
        StartCoroutine(SelfHurtEffect()); //开启自损

        AudioManager.Instance.PlaySound("bomb_fuse");
    }

    /// <summary>
    /// 更新引信计时，响应立即爆炸标记，再更新血条及自损判定。
    /// </summary>
    private void Update()
    {
        hurtTimer -= Time.deltaTime;    //计时器工作
        if (isInstanctlyExplode)
        {
            isInstanctlyExplode = false;
            Explode();
        }

        //血条更新
        healthBar.size = new Vector2(oringinWidth * (currentHealth / health), healthBar.size.y);

        BlockHurtSelf();    //方块自损与生命监测

    }

    #endregion

    #region 自损与爆炸

    /// <summary>
    /// 按间隔扣除生命并闪烁，生命归零时触发爆炸。
    /// </summary>
    private void BlockHurtSelf()
    {
        if (hurtTimer <= 0)
        {
            currentHealth -= damage;    //生命值减少
            StartCoroutine(SelfHurtEffect());
            hurtTimer = hurtCD;         //CD重置
        }

        //生命归零后销毁
        if (currentHealth <= 0)
        {
            Explode();      //爆炸
        }
    }

    [Tooltip("是否瞬间爆炸。")]
    public bool isInstanctlyExplode = false;

    /// <summary>爆炸半径。</summary>
    private float explosionRadius = 2.5f;

    /// <summary>爆炸力。</summary>
    private float explosionForce = 11f;

    /// <summary>对建筑爆炸伤害。</summary>
    private float explosionDamageToBlock = 999f;

    /// <summary>对敌人爆炸伤害。</summary>
    private float explosionDamageToEnemy = 40f;

    /// <summary>
    /// 播放爆炸表现，对范围内刚体施加距离衰减冲量，并对方块及敌人请求伤害后销毁自身。
    /// </summary>
    private void Explode()
    {
        CameraShake.Instance.ShakeScreen();     //屏幕晃动
        AudioManager.Instance.PlaySound("bomb_explode");                        //播放音效
        Instantiate(particlePrefab, transform.position, Quaternion.identity);   //生成粒子
        // 检测爆炸范围内的所有物体
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D collider in colliders)
        {
            // 计算爆炸力的方向
            Vector2 direction = collider.transform.position - transform.position;   //方向
            float distance = direction.magnitude;                                   //规格化

            // 计算施加的爆炸力，距离越近，施加的力越大
            float forceMagnitude = Mathf.Lerp(explosionForce, 0, distance / explosionRadius);
            Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                // 施加爆炸力
                rb.AddForce(direction.normalized * forceMagnitude * rb.mass, ForceMode2D.Impulse);
            }

            //造成伤害
            if (collider.GetComponent<Item_Block>() != null)
                collider.GetComponent<Item_Block>().GetHurt(explosionDamageToBlock);
            if(collider.GetComponent<Item_Block_bomb>()!=null)
                collider.GetComponent<Item_Block_bomb>().GetHurt(explosionDamageToBlock);
            if(collider.GetComponent<EnemyHealthSystem>()!=null)
                collider.GetComponent<EnemyHealthSystem>().GetHurt(explosionDamageToEnemy, transform.position,25f);
        }

        // 销毁炸弹对象
        Destroy(gameObject);
    }

    #endregion

    #region 受击反馈与接触引爆

    /// <summary>
    /// 扣除炸弹生命并显示受击反馈；爆炸判定由更新流程处理。
    /// </summary>
    /// <param name="damage">本次扣除的生命值。</param>
    public void GetHurt(float damage)
    {
        currentHealth -= damage;        //扣除血量
        ShowFigure(damage, true);
        StartCoroutine(HurtEffect());   //受伤效果
    }

    /// <summary>
    /// 短暂将炸弹精灵染红，随后恢复白色。
    /// </summary>
    /// <returns>控制受击变色时长的协程迭代器。</returns>
    IEnumerator HurtEffect()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sprite.color = new Color(1f, 0.5f, 0.5f, 1f);
        yield return new WaitForSeconds(0.3f);
        sprite.color = new Color(1f, 1f, 1f, 1f);
    }

    /// <summary>
    /// 短暂将炸弹精灵染为引信警示色，随后恢复白色。
    /// </summary>
    /// <returns>控制引信闪烁时长的协程迭代器。</returns>
    IEnumerator SelfHurtEffect()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sprite.color = new Color(0.6f, 0.2f, 0f, 1f);
        yield return new WaitForSeconds(0.5f);
        sprite.color = new Color(1f, 1f, 1f, 1f);
    }

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
        //创建TMP伤害数字实例
        GameObject figureText = Instantiate(figureTextPrefab, transform.position + Vector3.up, Quaternion.identity, parent);
        TextMeshProUGUI tmp = figureText.GetComponent<TextMeshProUGUI>();    //获取TMP

        tmp.SetText(num.ToString());

        //设置文本颜色
        if (isRed)
            tmp.color = new Color(1, 0.4f, 0.4f, 1);
        else
            tmp.color = new Color(0.4f, 1, 0.5f, 1);

    }

    /// <summary>
    /// 首次接触敌人时将生命清零，由后续生命检查触发爆炸。
    /// </summary>
    /// <param name="other">首次接触的碰撞信息。</param>
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Enemy"))
        {
            currentHealth = 0;
        }
    }

    #endregion
}
