using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneCone : MonoBehaviour
{
    private Rigidbody2D rb;
    private GameObject vpet;
    [SerializeField]private GameObject breakParticle;   //��������

    private void Awake()
    {
        vpet = GameObject.FindGameObjectWithTag("Vpet");  //��ȡ�������Ϸ����
        rb = GetComponent<Rigidbody2D>();                 //��ȡ����
    }

    private void Start()
    {
        InitValueBasedDifficulty();     //��ʼ����ֵ
    }

    private void Update()
    {
        CheckVpetArrive();
    }

    //�����Ѷȳ�ʼ����ֵ
    private void InitValueBasedDifficulty()
    {
        //��ȡ��Ϸ�ѶȽ���ƥ��(�������˺�|�Խ����˺�)
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //���Ѷ�
            case GameDifficultyLevel.Easy:
                damageToVpet = 3f;
                damageToBlock = 6f;
                break;

            //�����Ѷ�
            case GameDifficultyLevel.Normal:
                damageToVpet = 4f;
                damageToBlock = 8f;
                break;

            //�����Ѷ�
            case GameDifficultyLevel.Hard:
                damageToVpet = 5f;
                damageToBlock = 10f;
                break;
        }
    }

    public bool isFalling = false;
    private bool isStartCheck = false;
    private float startCheckTime = 0.3f;
    private float damageToVpet = 4f;
    private float damageToBlock = 8f;

    private void CheckVpetArrive()
    {
        if (isFalling) return;
        Vector2 vpetV2PosX = new Vector2(vpet.transform.position.x, transform.position.y);
        if (Vector2.Distance(vpetV2PosX, transform.position) < 2f)
        {
            isFalling = true;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;    //���Y������
            Invoke("StartCheck", startCheckTime);
        }
    }

    public void StartCheck()
    {
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;    //���Y������
        isStartCheck = true;
        Destroy(gameObject, 5f);
        rb.AddForce(Vector2.up * 0.1f);     //�������
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isStartCheck) return;

        if(other.CompareTag("Ground") || other.CompareTag("Vpet"))
        {
            if (other.CompareTag("Vpet"))
            {
                other.GetComponent<VpetHealthSystem>().VpetGethurt(damageToVpet, (Vector2.left + Vector2.up) * 40f);
            }
            if (other.CompareTag("Ground") && other.GetComponent<Item_Block>() != null)
                other.GetComponent<Item_Block>().GetHurt(damageToBlock);

            AudioManager.Instance.PlaySound3D("stoneBreak", transform.position);    //������Ч
            Instantiate(breakParticle, transform.position, Quaternion.identity);    //������������
            Destroy(gameObject);    //����
        }
    }

}
