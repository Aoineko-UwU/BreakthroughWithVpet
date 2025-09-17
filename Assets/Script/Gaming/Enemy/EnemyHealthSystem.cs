using System.Collections;
using TMPro;
using DG.Tweening;
using UnityEngine;

public class EnemyHealthSystem : MonoBehaviour
{
    [SerializeField] private SpriteRenderer healthBar;  //Ѫ��UI��
    [SerializeField] public float health = 30;          //����ֵ
    [SerializeField] private GameObject particlePrefab; //����Ԥ����

    private float currentHealth;    //��ǰ����ֵ
    private float oringinWidth;     //Ѫ����ʼ���
    private SpriteRenderer sprite;  //���ﾫ����Ⱦ��
    private Rigidbody2D rb;

    public bool isDead = false;     //�Ƿ��Ѿ�����

    private GameObject vpet;

    private void Start()
    {
        vpet = GameObject.FindGameObjectWithTag("Vpet");                    //��ȡ�������Ϸ����
        sprite = GetComponent<SpriteRenderer>();                            //��ȡ������Ⱦ
        rb = GetComponent<Rigidbody2D>();                                   //��ȡ����
        figureCanvas = GameObject.FindGameObjectWithTag("FigureCanvas");    //��ȡ����Canvas��

        InitValueBasedDifficulty();         //��ʼ����ֵ

        InitHealth();   //��ʼ������

    }

    private void Update()
    {
        if (isDead)
            sprite.color = sprite.color = new Color(1f, 0, 0, 1f);      //���ľ�����ɫ
        else
        {
            //Ѫ������
            healthBar.size = new Vector2(oringinWidth * (currentHealth / totalHealth), healthBar.size.y);
            SelfDestroy();  //�Դݻ��߼�
            HealthCheck();  //�������
        }
    }


    //�����Ѷȳ�ʼ����ֵ
    private void InitValueBasedDifficulty()
    {
        //��ȡ��Ϸ�ѶȽ���ƥ��(������������)
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //���Ѷ�
            case GameDifficultyLevel.Easy:
                healthMultiplier = 0.75f;
                break;

            //�����Ѷ�
            case GameDifficultyLevel.Normal:
                healthMultiplier = 1;
                break;

            //�����Ѷ�
            case GameDifficultyLevel.Hard:
                healthMultiplier = 1.25f;
                break;
        }
    }

    private float healthMultiplier = 1f;    //��������
    private float totalHealth;
    private void InitHealth()
    {
        oringinWidth = healthBar.size.x;  //��¼��ʼ���
        totalHealth = Mathf.Ceil(health * healthMultiplier);//����ֵ��ʼ��
        currentHealth = totalHealth;
    }

    //�������
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


    //���˺���(�ⲿ����)
    public void GetHurt(float damage , Vector3 pos, float force)
    {
        if (isDead) return;

        float newHealth = currentHealth - damage;  //���˺������ֵ
        //�����˺�����ֵ����0
        if (newHealth <= 0)
            currentHealth = 0;     //����ֵ�̶�Ϊ0
        //������������
        else
            currentHealth = newHealth;

        ShowFigure(damage, true);       //��������
        StartCoroutine(HurtEffect());   //����Ч��
        AudioManager.Instance.PlaySound3D("Enemy_getHurt", transform.position);

        Vector3 dir = (transform.position - pos).normalized;  //���㷽������
        Vector2 pushForce = dir * force;                      //����������������ˣ��õ�����
        rb.AddForce(pushForce, ForceMode2D.Impulse);          //������ʩ�ӵ�������
    }

    //����Ч��
    IEnumerator HurtEffect()
    {
        sprite.color = new Color(1f, 0.5f, 0.5f, 1f);
        yield return new WaitForSeconds(0.3f);
        sprite.color = new Color(1f, 1f, 1f, 1f);
    }

    //����Ч��
    IEnumerator Dead()
    {
        GameObject vpet = GameObject.FindGameObjectWithTag("Vpet");
        if(vpet.transform.position.x>transform.position.x)
            transform.DORotate(Vector3.forward * 180, 0.5f);              //��ת����
        else
            transform.DORotate(Vector3.forward * -180, 0.5f);              //��ת����

        yield return new WaitForSeconds(1f);                        //1s������
        Destroy(gameObject);
        if (particlePrefab != null)
            Instantiate(particlePrefab, transform.position, Quaternion.identity);   //��������
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

    //��Ҿ����Զ������(Update)
    bool isAllowStartDestroyTimer = false;      //�Ƿ��������Դݻټ�ʱ��
    float destroyTimer;                         //�Դݻټ�ʱ��
    float destroyInterval = 5f;                 //�Դݻټ��

    float destroyDistance = 30f;                //����Ҿ����Զ�Ժ���������Դݻ٣�
    private SpawnPoint parentSpawnPoint;        //�������ű�
    private void SelfDestroy()
    {
        //�Ի���������
        isAllowStartDestroyTimer = Vector2.Distance(vpet.transform.position, transform.position) > destroyDistance ? true : false;

        if (isAllowStartDestroyTimer)
            destroyTimer -= Time.deltaTime;     //������������ʼ��ʱ
        else
            destroyTimer = destroyInterval;     //�������ôݻ�ʱ��

        //�ݻټ�ʱ������ݻٱ���Ϸ����
        if (destroyTimer <= 0)
        {
            if (parentSpawnPoint != null)
                parentSpawnPoint.DelayResetRespawnState();    //��������״̬

            Destroy(gameObject);
        }
            
    }

    public void SetParentSpawnPoint(SpawnPoint spawnPoint)
    {
        parentSpawnPoint = spawnPoint;
    }
}
