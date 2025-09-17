using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private GameObject Enemy;       //ˢ�µĹ���
    private GameObject currentEnemy;                 //��ǰ����
    private Transform parent;

    private GameObject vpet;
    private bool isAllowSpawn = true;       //�Ƿ��������ɣ�
    private bool isEnemySpawned = false;    //�����Ƿ��Ѿ�����

    private void Awake()
    {
        vpet = GameObject.FindGameObjectWithTag("Vpet");  //��ȡ�������Ϸ����
    }

    private void Start()
    {
        parent = gameObject.transform;
        InitValueBasedDifficulty();     //��ʼ����ֵ
    }

    private void Update()
    {
        isAllowSpawn = Vector2.Distance(vpet.transform.position, transform.position) < 30f ? true : false;

        CheckToSpawnEnemy();
    }

    //�����Ѷȳ�ʼ����ֵ
    private void InitValueBasedDifficulty()
    {
        //��ȡ��Ϸ�ѶȽ���ƥ��
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //���Ѷ�
            case GameDifficultyLevel.Easy:
                spawnTimeFix = 5f;
                break;

            //�����Ѷ�
            case GameDifficultyLevel.Normal:
                spawnTimeFix = 0f;
                break;

            //�����Ѷ�
            case GameDifficultyLevel.Hard:
                spawnTimeFix = -5f;
                break;
        }
    }

    [SerializeField] private float minSpawnRange = -3f;         //��С������Χ
    [SerializeField] private float maxSpawnRange = 3f;          //���������Χ
    [SerializeField] private float minRespawnTime = 15f;        //�����������ʱ��
    [SerializeField] private float maxRespawnTime = 30f;        //���������ʱ��
    private float spawnTimeFix = 0f;    //����ʱ������

    private float respawnTimer;     //������ʱ��
    bool isRespawn = false;         //�Ƿ����������

    private void CheckToSpawnEnemy()
    {
        if (!isAllowSpawn) return;  //�������������򷵻�

        if(!isEnemySpawned) respawnTimer -= Time.deltaTime;     //����δ����ʱ��ʱ������

        //�����ﻹδ���е�һ������
        if (!isEnemySpawned && !isRespawn)        
        {
            SpawnEnemy();           //���ɹ���
            isEnemySpawned = true;  //������
        }
        //�����������ɲ�������
        else if(isEnemySpawned && currentEnemy == null)
        {
            isEnemySpawned = false; //δ����
            isRespawn = true;       //��������
            float randSpawnTime = Random.Range(minRespawnTime + spawnTimeFix , maxRespawnTime + spawnTimeFix); //��ȡ�������ʱ��
            respawnTimer = randSpawnTime;   //��������ʱ��
        }
        //������������ʱ���� && ���������׶� && ��δ����
        if(respawnTimer <=0 && isRespawn && !isEnemySpawned)
        {
            SpawnEnemy();           //���ɹ���
            isEnemySpawned = true;  //������
        }
    }

    //��������
    private void SpawnEnemy()
    {
        float rand = Random.Range(minSpawnRange, maxSpawnRange);                            //��ȡ���������Χ
        Vector2 randPos = new Vector2(transform.position.x + rand, transform.position.y);   //�趨����λ��
        currentEnemy = Instantiate(Enemy, randPos, Quaternion.identity, parent);            //����
        currentEnemy.GetComponent<EnemyHealthSystem>().SetParentSpawnPoint(this);           //���ö���������㸸��
    }

    //�ӳ���������״̬(�ⲿ����)
    public void DelayResetRespawnState()
    {
        Invoke("ResetRespawn", 0.5f);
    }

    private void ResetRespawn()
    {
        isRespawn = false;     //��������״̬
        isEnemySpawned = false;
    }

}
