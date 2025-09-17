using System.Collections;
using TMPro;
using UnityEngine;

public class Item_Block : MonoBehaviour
{
    [SerializeField] private SpriteRenderer healthBar;  //血条UI绑定
    [SerializeField] private float health = 100;        //生命值
    [SerializeField] private GameObject particlePrefab; //粒子预制体
    [SerializeField] private int item_id;               //物品ID

    private float currentHealth; //当前生命值
    private float oringinWidth;  //血条初始宽度

    private float hurtTimer;    //自损计时器
    private float hurtCD = 1f;  //自损间隔时长(s)
    private int damage = 2;     //单次自损伤害
    private bool isAllowTimerWork = true;   //是否允许计时器工作？


    private void Start()
    {
        currentHealth = health; //生命值初始化
        hurtTimer = hurtCD;     //计时器初始化

        oringinWidth = healthBar.size.x;  // 记录初始宽度
        figureCanvas = GameObject.FindGameObjectWithTag("FigureCanvas");
    }

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

    //方块自损&生命监测
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
            Instantiate(particlePrefab, transform.position, Quaternion.identity);   //生成粒子

            if (item_id != 0)
            {
                if (item_id == 10)
                    AudioManager.Instance.PlaySound("brick_break");
                if (item_id == 11)
                    AudioManager.Instance.PlaySound("log_break");
            }
        }
    }

    //受伤函数(外部调用)
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

    IEnumerator HurtEffect()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sprite.color = new Color(1f, 0.5f, 0.5f, 1f);
        yield return new WaitForSeconds(0.3f);
        sprite.color = new Color(1f, 1f, 1f, 1f);
    }


    [SerializeField] private GameObject figureTextPrefab;   //数字文本预制体
    private GameObject figureCanvas;                        //FigureCanvas父节点

    //显示UI数字
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

}
