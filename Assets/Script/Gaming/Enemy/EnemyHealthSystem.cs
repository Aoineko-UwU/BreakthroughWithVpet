using System.Collections;
using TMPro;
using DG.Tweening;
using UnityEngine;

public class EnemyHealthSystem : MonoBehaviour
{
    [SerializeField] private SpriteRenderer healthBar;  //血条UI绑定
    [SerializeField] public float health = 30;          //生命值
    [SerializeField] private GameObject particlePrefab; //粒子预制体

    private float currentHealth;    //当前生命值
    private float oringinWidth;     //血条初始宽度
    private SpriteRenderer sprite;  //怪物精灵渲染器
    private Rigidbody2D rb;

    public bool isDead = false;     //是否已经死亡

    private GameObject vpet;

    private void Start()
    {
        vpet = GameObject.FindGameObjectWithTag("Vpet");                    //获取桌宠的游戏对象
        sprite = GetComponent<SpriteRenderer>();                            //获取精灵渲染
        rb = GetComponent<Rigidbody2D>();                                   //获取刚体
        figureCanvas = GameObject.FindGameObjectWithTag("FigureCanvas");    //获取数字Canvas层

        InitValueBasedDifficulty();         //初始化数值

        InitHealth();   //初始化生命

    }

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


    //根据难度初始化数值
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

    private float healthMultiplier = 1f;    //生命倍率
    private float totalHealth;
    private void InitHealth()
    {
        oringinWidth = healthBar.size.x;  //记录初始宽度
        totalHealth = Mathf.Ceil(health * healthMultiplier);//生命值初始化
        currentHealth = totalHealth;
    }

    //生命监测
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


    //受伤函数(外部调用)
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

    //受伤效果
    IEnumerator HurtEffect()
    {
        sprite.color = new Color(1f, 0.5f, 0.5f, 1f);
        yield return new WaitForSeconds(0.3f);
        sprite.color = new Color(1f, 1f, 1f, 1f);
    }

    //死亡效果
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
            Instantiate(particlePrefab, transform.position, Quaternion.identity);   //生成粒子
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

    //玩家距离过远自销毁(Update)
    bool isAllowStartDestroyTimer = false;      //是否允许激活自摧毁计时器
    float destroyTimer;                         //自摧毁计时器
    float destroyInterval = 5f;                 //自摧毁间隔

    float destroyDistance = 30f;                //与玩家距离多远以后允许进行自摧毁？
    private SpawnPoint parentSpawnPoint;        //父重生脚本
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

    public void SetParentSpawnPoint(SpawnPoint spawnPoint)
    {
        parentSpawnPoint = spawnPoint;
    }
}
