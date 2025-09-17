using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum VpetAvatarState     //桌宠头像状态
{
    healthy,    //0
    normal,     //1
    bad         //2
}

public class VpetHealthSystem : MonoBehaviour
{
    private float vpetHealth = 30f;        //桌宠总生命值
    private float _vpetCurrentHealth;      //桌宠当前生命值

    [SerializeField] private Slider _sliderHealthBar;       //血量条
    [SerializeField] private Image _sliderHealthFill;       //血量条填充图
    [SerializeField] private Image _stateAvatar;            //状态头像
    [SerializeField] private Sprite _healthy;               //健康状态精灵图
    [SerializeField] private Sprite _normal;                //普通状态精灵图
    [SerializeField] private Sprite _bad;                   //危险状态精灵图

    private VpetAvatarState _currentAvatarState;            //当前头像状态
    private VpetAvatarState _newAvatarState;                //新的头像状态

    public bool isVpetDead = false;         //桌宠是否已死亡

    private Rigidbody2D rb;                 //桌宠刚体
    private VpetAction vpetAction;          //桌宠行为脚本

    //生命周期-----------------------------------------------------------------------------------//

    private void Start()
    {
        InitValueBasedDifficulty();                                         //初始化数值
        vpetAction = GetComponent<VpetAction>();                            //获取桌宠行为
        rb = GetComponent<Rigidbody2D>();                                   //获取桌宠刚体
        figureCanvas = GameObject.FindGameObjectWithTag("FigureCanvas");    //获取文字Canvas

        InitHealthBar();                    //初始化血条

    }

    private void Update()
    {
        UpdateHealthBar();         //更新血条
        UpdateAvatarAndColor();    //更新头像状态与血条颜色
    }

    //功能函数-----------------------------------------------------------------------------------//

    //根据难度初始化数值
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

    //血条初始化(Start)
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

    //血条实时更新(Update)
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

    //更新状态头像&血条颜色(Update)
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

    [SerializeField] private SpriteRenderer vpetSpriteRenderer;   //绑定桌宠SpriteRenderer

    public bool isVpetInvincible = false;   //桌宠是否无敌？
    private float invincibleTime = 1f;      //桌宠无敌时间

    //受伤效果
    IEnumerator VpetHurtEffect()
    {
        vpetSpriteRenderer.color = new Color(1f, 0.5f, 0.5f, 1f);
        yield return new WaitForSeconds(0.3f);
        vpetSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);
    }

    //无敌状态设置(协程)
    IEnumerator VpetInvincibleSet()
    {
        isVpetInvincible = true;    //开启无敌
        yield return new WaitForSeconds(invincibleTime);    //无敌时间等待
        isVpetInvincible = false;   //关闭无敌
    }

    //其他方法-----------------------------------------------------------------------------------//

    //使用十六进制数转换颜色值(内部方法调用)
    private Color SetColor(string hexColor)
    {
        Color newColor;
        ColorUtility.TryParseHtmlString(hexColor, out newColor);
        return newColor;
    }

    private float knockBackFactor = 1f; //击退系数

    //设置是否可击退
    public void SetKnockBack(bool isAllowKnockBack)
    {
        if (isAllowKnockBack)
            knockBackFactor = 1f;
        else
            knockBackFactor = 0f;
    }

    //受伤方法(外部调用)
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

    private float recoverMultiplier = 1f;       //生命恢复倍率

    //生命恢复方法(外部调用)
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

    [SerializeField] private GameObject figureTextPrefab;   //数字文本预制体
    private GameObject figureCanvas;                        //FigureCanvas父节点

    //显示UI数字
    private void ShowFigure(float num,bool isRed)
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
}