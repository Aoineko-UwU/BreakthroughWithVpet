using System.Collections;
using TMPro;
using UnityEngine;

public class Item_Block : MonoBehaviour
{
    [SerializeField] private SpriteRenderer healthBar;  //Ѫ��UI��
    [SerializeField] private float health = 100;        //����ֵ
    [SerializeField] private GameObject particlePrefab; //����Ԥ����
    [SerializeField] private int item_id;               //��ƷID

    private float currentHealth; //��ǰ����ֵ
    private float oringinWidth;  //Ѫ����ʼ���

    private float hurtTimer;    //�����ʱ��
    private float hurtCD = 1f;  //������ʱ��(s)
    private int damage = 2;     //���������˺�
    private bool isAllowTimerWork = true;   //�Ƿ������ʱ��������


    private void Start()
    {
        currentHealth = health; //����ֵ��ʼ��
        hurtTimer = hurtCD;     //��ʱ����ʼ��

        oringinWidth = healthBar.size.x;  // ��¼��ʼ���
        figureCanvas = GameObject.FindGameObjectWithTag("FigureCanvas");
    }

    private void Update()
    {
        if (isAllowTimerWork)
        {
            hurtTimer -= Time.deltaTime;    //��ʱ������
        }

        //Ѫ������
        healthBar.size = new Vector2(oringinWidth * (currentHealth / health), healthBar.size.y);

        BlockHurtSelf();    //�����������������

    }

    //��������&�������
    private void BlockHurtSelf()
    {
        if (hurtTimer <= 0)
        {
            currentHealth -= damage;    //����ֵ����
            hurtTimer = hurtCD;         //CD����
        }

        //�������������
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
            Instantiate(particlePrefab, transform.position, Quaternion.identity);   //��������

            if (item_id != 0)
            {
                if (item_id == 10)
                    AudioManager.Instance.PlaySound("brick_break");
                if (item_id == 11)
                    AudioManager.Instance.PlaySound("log_break");
            }
        }
    }

    //���˺���(�ⲿ����)
    public void GetHurt(float damage)
    {
        float newHealth = currentHealth - damage;  //���˺������ֵ
        //�����˺�����ֵ����0
        if (newHealth <= 0)
            currentHealth = 0;     //����ֵ�̶�Ϊ0
        //������������
        else
            currentHealth = newHealth;

        ShowFigure(damage, true);       //��������
        StartCoroutine(HurtEffect());   //����Ч��
    }

    IEnumerator HurtEffect()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sprite.color = new Color(1f, 0.5f, 0.5f, 1f);
        yield return new WaitForSeconds(0.3f);
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

}
