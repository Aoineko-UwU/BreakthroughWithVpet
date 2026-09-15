using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 可放置方块类
/// - 管理方块生命、自损、受击反馈与破坏表现。
/// </summary>
public class Item_Block : MonoBehaviour
{
    #region 配置与运行状态

    [Tooltip("血条UI绑定。")]
    [SerializeField] private SpriteRenderer healthBar;

    [Tooltip("生命值。")]
    [SerializeField] private float health = 100;

    [Tooltip("粒子预制体。")]
    [SerializeField] private GameObject particlePrefab;

    [Tooltip("物品ID。")]
    [SerializeField] private int item_id;

    /// <summary>当前生命值。</summary>
    private float currentHealth;

    /// <summary>血条初始宽度。</summary>
    private float oringinWidth;

    /// <summary>自损计时器。</summary>
    private float hurtTimer;

    /// <summary>自损间隔时长(s)。</summary>
    private float hurtCD = 1f;

    /// <summary>单次自损伤害。</summary>
    private int damage = 2;

    /// <summary>是否允许递减跳跃及叫声计时器。</summary>
    private bool isAllowTimerWork = true;

    #endregion

    #region 初始化与自损更新

    /// <summary>
    /// 初始化生命、自损计时、血条基准宽度和飘字画布。
    /// </summary>
    private void Start()
    {
        currentHealth = health; //生命值初始化
        hurtTimer = hurtCD;     //计时器初始化

        oringinWidth = healthBar.size.x;  // 记录初始宽度
        figureCanvas = GameObject.FindGameObjectWithTag("FigureCanvas");
    }

    /// <summary>
    /// 在计时允许时递减自损计时器，更新血条并检查自损及破坏。
    /// </summary>
    private void Update()
    {
        if (isAllowTimerWork)
        {
            hurtTimer -= Time.deltaTime;    //计时器工作
        }

        //血条更新
        healthBar.size = new Vector2(oringinWidth * (currentHealth / health), healthBar.size.y);

        BlockHurtSelf();    //方块自损与生命监测

    }

    #endregion

    #region 生命操作与受击表现

    /// <summary>
    /// 按间隔扣除方块生命，归零后销毁并播放粒子及按物品编号选择的破坏音效。
    /// </summary>
    private void BlockHurtSelf()
    {
        if (hurtTimer <= 0)
        {
            currentHealth -= damage;    //生命值减少
            hurtTimer = hurtCD;         //CD重置
        }

        //生命归零后销毁
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
            ObjectPoolManager.GetOrCreate().Spawn(particlePrefab, transform.position, Quaternion.identity); //生成粒子

            if (item_id != 0)
            {
                if (item_id == 10)
                    AudioManager.Instance.PlaySound("brick_break");
                if (item_id == 11)
                    AudioManager.Instance.PlaySound("log_break");
            }
        }
    }

    /// <summary>
    /// 扣除生命且最低保留为零，并播放受击飘字和变色反馈。
    /// </summary>
    /// <param name="damage">本次扣除的生命值。</param>
    public void GetHurt(float damage)
    {
        float newHealth = currentHealth - damage;  //受伤后的生命值
        //若受伤后生命值低于0
        if (newHealth <= 0)
            currentHealth = 0;     //生命值固定为0
        //否则正常受伤
        else
            currentHealth = newHealth;

        ShowFigure(damage, true);       //受伤数字
        StartCoroutine(HurtEffect());   //受伤效果
    }

    /// <summary>
    /// 短暂将方块精灵染红，随后恢复白色。
    /// </summary>
    /// <returns>控制受击变色时长的协程迭代器。</returns>
    IEnumerator HurtEffect()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sprite.color = new Color(1f, 0.5f, 0.5f, 1f);
        yield return new WaitForSeconds(0.3f);
        sprite.color = new Color(1f, 1f, 1f, 1f);
    }

    #endregion

    #region 数值飘字

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

    #endregion
}
