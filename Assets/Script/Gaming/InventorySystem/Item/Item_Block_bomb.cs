using System.Collections;
using TMPro;
using UnityEngine;

public class Item_Block_bomb : MonoBehaviour
{
    [SerializeField] private SpriteRenderer healthBar;  //Ѫ��UI��
    [SerializeField] private float health = 5;          //����ֵ
    [SerializeField] private GameObject particlePrefab; //����Ԥ����

    private float currentHealth; //��ǰ����ֵ
    private float oringinWidth;  //Ѫ����ʼ���

    private float hurtTimer;    //�����ʱ��
    private float hurtCD = 1f;  //������ʱ��(s)
    private int damage = 1;     //���������˺�


    private void Start()
    {
        figureCanvas = GameObject.FindGameObjectWithTag("FigureCanvas");    //��ȡ�˺�����Canvas
        currentHealth = health;           //����ֵ��ʼ��
        hurtTimer = hurtCD;               //��ʱ����ʼ��
        oringinWidth = healthBar.size.x;  //��¼��ʼ���
        StartCoroutine(SelfHurtEffect()); //��������

        AudioManager.Instance.PlaySound("bomb_fuse");
    }

    private void Update()
    {
        hurtTimer -= Time.deltaTime;    //��ʱ������
        if (isInstanctlyExplode)
        {
            isInstanctlyExplode = false;
            Explode();
        }

        //Ѫ������
        healthBar.size = new Vector2(oringinWidth * (currentHealth / health), healthBar.size.y);

        BlockHurtSelf();    //�����������������

    }

    //����&�������
    private void BlockHurtSelf()
    {
        if (hurtTimer <= 0)
        {
            currentHealth -= damage;    //����ֵ����
            StartCoroutine(SelfHurtEffect());
            hurtTimer = hurtCD;         //CD����
        }

        //�������������
        if (currentHealth <= 0)
        {            
            Explode();      //��ը
        }
    }

    public bool isInstanctlyExplode = false;    //�Ƿ�˲�䱬ը
    private float explosionRadius = 2.5f;       //��ը�뾶
    private float explosionForce = 11f;         //��ը��
    private float explosionDamageToBlock = 999f;     //�Խ�����ը�˺�
    private float explosionDamageToEnemy = 40f;      //�Ե��˱�ը�˺�

    private void Explode()
    {
        CameraShake.Instance.ShakeScreen();     //��Ļ�ζ�
        AudioManager.Instance.PlaySound("bomb_explode");                        //������Ч
        Instantiate(particlePrefab, transform.position, Quaternion.identity);   //��������
        // ��ⱬը��Χ�ڵ���������
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D collider in colliders)
        {
            // ���㱬ը���ķ���
            Vector2 direction = collider.transform.position - transform.position;   //����
            float distance = direction.magnitude;                                   //���

            // ����ʩ�ӵı�ը��������Խ����ʩ�ӵ���Խ��
            float forceMagnitude = Mathf.Lerp(explosionForce, 0, distance / explosionRadius);
            Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                // ʩ�ӱ�ը��
                rb.AddForce(direction.normalized * forceMagnitude * rb.mass, ForceMode2D.Impulse);
            }

            //����˺�
            if (collider.GetComponent<Item_Block>() != null)
                collider.GetComponent<Item_Block>().GetHurt(explosionDamageToBlock);
            if(collider.GetComponent<Item_Block_bomb>()!=null)
                collider.GetComponent<Item_Block_bomb>().GetHurt(explosionDamageToBlock);
            if(collider.GetComponent<EnemyHealthSystem>()!=null)
                collider.GetComponent<EnemyHealthSystem>().GetHurt(explosionDamageToEnemy, transform.position,25f);
        }

        // ����ը������
        Destroy(gameObject);
    }

    //���˺���(�ⲿ����)
    public void GetHurt(float damage)
    {
        currentHealth -= damage;        //�۳�Ѫ��
        ShowFigure(damage, true);
        StartCoroutine(HurtEffect());   //����Ч��
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

    
    [SerializeField] private GameObject figureTextPrefab;   //�����ı�Ԥ����
    private GameObject figureCanvas;                        //FigureCanvas���ڵ�

    //��ʾUI����
    private void ShowFigure(float num, bool isRed)
    {
        Transform parent = figureCanvas.transform;
        //����TMP�˺�����ʵ��
        GameObject figureText = Instantiate(figureTextPrefab, transform.position + Vector3.up, Quaternion.identity, parent);
        TextMeshProUGUI tmp = figureText.GetComponent<TextMeshProUGUI>();    //��ȡTMP

        tmp.SetText(num.ToString());

        //�����ı���ɫ
        if (isRed)
            tmp.color = new Color(1, 0.4f, 0.4f, 1);
        else
            tmp.color = new Color(0.4f, 1, 0.5f, 1);

    }

    //��������ֱ�ӱ�ը
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Enemy"))
        {
            currentHealth = 0;
        }
    }
}
