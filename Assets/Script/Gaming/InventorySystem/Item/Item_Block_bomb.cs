using System.Collections;
using TMPro;
using UnityEngine;

public class Item_Block_bomb : MonoBehaviour
{
    [SerializeField] private SpriteRenderer healthBar;  //血条UI绑定
    [SerializeField] private float health = 5;          //生命值
    [SerializeField] private GameObject particlePrefab; //粒子预制体

    private float currentHealth; //当前生命值
    private float oringinWidth;  //血条初始宽度

    private float hurtTimer;    //自损计时器
    private float hurtCD = 1f;  //自损间隔时长(s)
    private int damage = 1;     //单次自损伤害


    private void Start()
    {
        figureCanvas = GameObject.FindGameObjectWithTag("FigureCanvas");    //获取伤害数字Canvas
        currentHealth = health;           //生命值初始化
        hurtTimer = hurtCD;               //计时器初始化
        oringinWidth = healthBar.size.x;  //记录初始宽度
        StartCoroutine(SelfHurtEffect()); //开启自损

        AudioManager.Instance.PlaySound("bomb_fuse");
    }

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

    //自损&生命监测
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

    public bool isInstanctlyExplode = false;    //是否瞬间爆炸
    private float explosionRadius = 2.5f;       //爆炸半径
    private float explosionForce = 11f;         //爆炸力
    private float explosionDamageToBlock = 999f;     //对建筑爆炸伤害
    private float explosionDamageToEnemy = 40f;      //对敌人爆炸伤害

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

    //受伤函数(外部调用)
    public void GetHurt(float damage)
    {
        currentHealth -= damage;        //扣除血量
        ShowFigure(damage, true);
        StartCoroutine(HurtEffect());   //受伤效果
    }

    IEnumerator HurtEffect()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sprite.color = new Color(1f, 0.5f, 0.5f, 1f);
        yield return new WaitForSeconds(0.3f);
        sprite.color = new Color(1f, 1f, 1f, 1f);
    }

    IEnumerator SelfHurtEffect()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sprite.color = new Color(0.6f, 0.2f, 0f, 1f);
        yield return new WaitForSeconds(0.5f);
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

    //碰到怪物直接爆炸
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Enemy"))
        {
            currentHealth = 0;
        }
    }
}
