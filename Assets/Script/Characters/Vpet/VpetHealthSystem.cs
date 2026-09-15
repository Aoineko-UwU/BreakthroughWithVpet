using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 桌宠头像状态枚举
/// - 区分健康、普通和危险生命区间对应的头像显示状态。
/// </summary>
public enum VpetAvatarState
{
    /// <summary>健康头像状态，生命比例不低于 70%。</summary>
    healthy,

    /// <summary>普通头像状态，生命比例不低于 30% 且低于 70%。</summary>
    normal,

    /// <summary>危险头像状态，生命比例低于 30%。</summary>
    bad
}

/// <summary>
/// 桌宠生命系统类
/// - 管理生命、恢复、受击无敌与击退，并更新生命条、头像及飘字。
/// </summary>
public class VpetHealthSystem : MonoBehaviour
{
    #region 配置与运行状态

    /// <summary>桌宠总生命值。</summary>
    private float vpetHealth = 30f;

    /// <summary>桌宠当前生命值。</summary>
    private float _vpetCurrentHealth;

    [Tooltip("血量条。")]
    [SerializeField] private Slider _sliderHealthBar;

    [Tooltip("血量条填充图。")]
    [SerializeField] private Image _sliderHealthFill;

    [Tooltip("状态头像。")]
    [SerializeField] private Image _stateAvatar;

    [Tooltip("健康状态精灵图。")]
    [SerializeField] private Sprite _healthy;

    [Tooltip("普通状态精灵图。")]
    [SerializeField] private Sprite _normal;

    [Tooltip("危险状态精灵图。")]
    [SerializeField] private Sprite _bad;

    /// <summary>当前头像状态。</summary>
    private VpetAvatarState _currentAvatarState;

    /// <summary>新的头像状态。</summary>
    private VpetAvatarState _newAvatarState;

    [Tooltip("桌宠是否停止常规行为；死亡流程设置，胜利流程也用此标记停用更新。")]
    public bool isVpetDead = false;

    /// <summary>当前对象的二维刚体组件。</summary>
    private Rigidbody2D rb;

    /// <summary>桌宠行为脚本。</summary>
    private VpetAction vpetAction;

    #endregion

    #region 初始化与界面更新

    /// <summary>
    /// 初始化难度属性，缓存行为、刚体与飘字画布，然后初始化生命条及头像。
    /// </summary>
    private void Start()
    {
        InitValueBasedDifficulty();                                         //初始化数值
        vpetAction = GetComponent<VpetAction>();                            //获取桌宠行为
        rb = GetComponent<Rigidbody2D>();                                   //获取桌宠刚体
        figureCanvas = GameObject.FindGameObjectWithTag("FigureCanvas");    //获取文字Canvas

        InitHealthBar();                    //初始化血条

    }

    /// <summary>
    /// 同步当前生命值到血条，并在生命区间改变时更新头像和填充颜色。
    /// </summary>
    private void Update()
    {
        UpdateHealthBar();         //更新血条
        UpdateAvatarAndColor();    //更新头像状态与血条颜色
    }

    #endregion

    #region 难度与生命界面

    /// <summary>
    /// 根据难度设置最大生命、受击无敌时长和恢复倍率。
    /// </summary>
    private void InitValueBasedDifficulty()
    {
        //获取游戏难度进行匹配(桌宠生命值|无敌时间|生命恢复倍率)
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //简单难度
            case GameDifficultyLevel.Easy:
                vpetHealth = 40f;
                invincibleTime = 1.25f;
                recoverMultiplier = 1.5f;
                break;

            //正常难度
            case GameDifficultyLevel.Normal:
                vpetHealth = 30f;
                invincibleTime = 1f;
                recoverMultiplier = 1f;
                break;

            //困难难度
            case GameDifficultyLevel.Hard:
                vpetHealth = 20f;
                invincibleTime = 0.85f;
                recoverMultiplier = 0.5f;
                break;
        }
    }

    /// <summary>
    /// 填满当前生命与血条，并初始化健康头像及填充颜色。
    /// </summary>
    private void InitHealthBar()
    {
        _vpetCurrentHealth = vpetHealth;                //更新当前生命值
        _sliderHealthBar.maxValue = vpetHealth;         //血条Value最大值更改为Vpet生命值
        _sliderHealthBar.value = _vpetCurrentHealth;    //将Vpet当前生命值更新到血条Value
        _sliderHealthFill.color = SetColor("#83ff58");  //初始化血条颜色

        //初始化桌宠头像状态
        _currentAvatarState = VpetAvatarState.healthy;  //初始为健康状态
        _newAvatarState = _currentAvatarState;          //保持初始的状态一致性
        _stateAvatar.sprite = _healthy;                 //初始化状态头像
    }

    /// <summary>
    /// 生命数值变化时更新滑条，并按生命比例计算待显示的头像状态。
    /// </summary>
    private void UpdateHealthBar()
    {
        //为空检查&&仅在currentHealth值发生改变时执行
        if (_sliderHealthBar && _vpetCurrentHealth != _sliderHealthBar.value)
        {
            //血量在0及以上时正常更新UI
            if (_vpetCurrentHealth >= 0)
                _sliderHealthBar.value = _vpetCurrentHealth;    //更新血条的Value
            //血量在0以下时默认设置Value为0
            else
                _sliderHealthBar.value = 0;

            //计算血量百分比阈值(**[0%~30%),[30%~70%),[70%~N%)**)
            float healthPercent = _vpetCurrentHealth / vpetHealth;  //当前生命百分比
            if (healthPercent >= 0.7f)
                _newAvatarState = VpetAvatarState.healthy;      //状态为健康
            else if (healthPercent >= 0.3f)
                _newAvatarState = VpetAvatarState.normal;       //状态为普通
            else
                _newAvatarState = VpetAvatarState.bad;          //状态为危险
        }
    }

    /// <summary>
    /// 头像状态发生变化时，切换头像精灵与对应的血条颜色。
    /// </summary>
    private void UpdateAvatarAndColor()
    {
        //状态发生改变时执行
        if(_currentAvatarState != _newAvatarState)
        {
            _currentAvatarState = _newAvatarState;      //更新状态

            switch (_currentAvatarState)
            {
                //健康状态时
                case VpetAvatarState.healthy:
                    _stateAvatar.sprite = _healthy;
                    _sliderHealthFill.color = SetColor("#83ff58");   //绿色血条
                    break;

                //普通状态时
                case VpetAvatarState.normal:
                    _stateAvatar.sprite = _normal;
                    _sliderHealthFill.color = SetColor("#fff958");   //黄色血条
                    break;

                //危险状态时
                case VpetAvatarState.bad:
                    _stateAvatar.sprite = _bad;
                    _sliderHealthFill.color = SetColor("#ff7058");   //红色血条
                    break;

                default:
                    Debug.Log("未知状态");
                    break;
            }
        }
    }

    #endregion

    #region 受击无敌与生命操作

    [Tooltip("绑定桌宠SpriteRenderer。")]
    [SerializeField] private SpriteRenderer vpetSpriteRenderer;

    [Tooltip("桌宠是否无敌。")]
    public bool isVpetInvincible = false;

    /// <summary>桌宠无敌时间。</summary>
    private float invincibleTime = 1f;

    /// <summary>
    /// 短暂将桌宠精灵染红，随后恢复白色。
    /// </summary>
    /// <returns>控制受击变色时长的协程迭代器。</returns>
    IEnumerator VpetHurtEffect()
    {
        vpetSpriteRenderer.color = new Color(1f, 0.5f, 0.5f, 1f);
        yield return new WaitForSeconds(0.3f);
        vpetSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);
    }

    /// <summary>
    /// 开启受击无敌，并在配置时长后关闭。
    /// </summary>
    /// <returns>控制无敌等待时间的协程迭代器。</returns>
    IEnumerator VpetInvincibleSet()
    {
        isVpetInvincible = true;    //开启无敌
        yield return new WaitForSeconds(invincibleTime);    //无敌时间等待
        isVpetInvincible = false;   //关闭无敌
    }

    /// <summary>
    /// 解析 HTML 格式颜色字符串。
    /// </summary>
    /// <param name="hexColor">要解析的颜色文本，例如 #83ff58。</param>
    /// <returns>解析输出的颜色；解析失败时保留 Unity 接口给出的输出值。</returns>
    private Color SetColor(string hexColor)
    {
        Color newColor;
        ColorUtility.TryParseHtmlString(hexColor, out newColor);
        return newColor;
    }

    /// <summary>击退系数。</summary>
    private float knockBackFactor = 1f;

    /// <summary>
    /// 通过击退系数控制后续受伤请求是否施加击退力；受伤入口仍会清零速度。
    /// </summary>
    /// <param name="isAllowKnockBack">为 true 时使用完整击退系数，为 false 时将系数设为零。</param>
    public void SetKnockBack(bool isAllowKnockBack)
    {
        if (isAllowKnockBack)
            knockBackFactor = 1f;
        else
            knockBackFactor = 0f;
    }

    /// <summary>
    /// 在非无敌且存活时扣除生命，处理死亡通知、击退、受击反馈及短暂无敌。
    /// </summary>
    /// <param name="damage">本次扣除的生命值。</param>
    /// <param name="force">受击施力向量，会乘以当前击退系数；调用前会清零刚体速度。</param>
    public void VpetGethurt(float damage,Vector2 force)
    {
        if (isVpetInvincible || isVpetDead) return;

        float newHealth = _vpetCurrentHealth - damage;  //受伤后的生命值
        //若受伤后生命值低于0
        if (newHealth <= 0)
        {
            _vpetCurrentHealth = 0;     //生命值固定为0
            isVpetDead = true;          //桌宠死亡
            vpetAction.VpetDead();      //执行死亡行为
        }

        //否则正常受伤
        else
            _vpetCurrentHealth = newHealth;

        rb.velocity = Vector2.zero;
        rb.AddForce(force * knockBackFactor);

        AudioManager.Instance.PlaySound("getHurt");  //播放音效

        StartCoroutine(VpetHurtEffect());       //受伤效果
        StartCoroutine(VpetInvincibleSet());    //受伤无敌状态设置
        ShowFigure(damage, true);
    }

    /// <summary>生命恢复倍率。</summary>
    private float recoverMultiplier = 1f;

    /// <summary>
    /// 存活时按难度倍率计算并向上取整恢复量，恢复不超过最大生命。
    /// </summary>
    /// <param name="recoverHealth">应用难度倍率前的基础恢复量。</param>
    public void VpetRecover(float recoverHealth)
    {
        float recover = Mathf.Ceil(recoverHealth * recoverMultiplier);

        if (isVpetDead) return;

        float newHealth = _vpetCurrentHealth + recover;   //恢复生命后的生命值
        //若恢复后生命值高于最大生命值
        if (newHealth >= vpetHealth)
            _vpetCurrentHealth = vpetHealth;    //仅恢复到最大生命
        //否则正常恢复生命
        else
            _vpetCurrentHealth = newHealth;     //恢复生命

        ShowFigure(recover, false);   //恢复数字
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
    private void ShowFigure(float num,bool isRed)
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
