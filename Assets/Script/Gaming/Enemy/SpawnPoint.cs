using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private GameObject Enemy;       //刷新的怪物
    private GameObject currentEnemy;                 //当前怪物
    private Transform parent;

    private GameObject vpet;
    private bool isAllowSpawn = true;       //是否允许生成？
    private bool isEnemySpawned = false;    //怪物是否已经生成

    private void Awake()
    {
        vpet = GameObject.FindGameObjectWithTag("Vpet");  //获取桌宠的游戏对象
    }

    private void Start()
    {
        parent = gameObject.transform;
        InitValueBasedDifficulty();     //初始化数值
    }

    private void Update()
    {
        isAllowSpawn = Vector2.Distance(vpet.transform.position, transform.position) < 30f ? true : false;

        CheckToSpawnEnemy();
    }

    //根据难度初始化数值
    private void InitValueBasedDifficulty()
    {
        //获取游戏难度进行匹配
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //简单难度
            case GameDifficultyLevel.Easy:
                spawnTimeFix = 5f;
                break;

            //正常难度
            case GameDifficultyLevel.Normal:
                spawnTimeFix = 0f;
                break;

            //困难难度
            case GameDifficultyLevel.Hard:
                spawnTimeFix = -5f;
                break;
        }
    }

    [SerializeField] private float minSpawnRange = -3f;         //最小重生范围
    [SerializeField] private float maxSpawnRange = 3f;          //最大重生范围
    [SerializeField] private float minRespawnTime = 15f;        //怪物最短重生时间
    [SerializeField] private float maxRespawnTime = 30f;        //怪物最长重生时间
    private float spawnTimeFix = 0f;    //重生时间修正

    private float respawnTimer;     //重生计时器
    bool isRespawn = false;         //是否进入了重生

    private void CheckToSpawnEnemy()
    {
        if (!isAllowSpawn) return;  //若不允许重生则返回

        if(!isEnemySpawned) respawnTimer -= Time.deltaTime;     //怪物未生成时计时器运行

        //若怪物还未进行第一次生成
        if (!isEnemySpawned && !isRespawn)        
        {
            SpawnEnemy();           //生成怪物
            isEnemySpawned = true;  //已生成
        }
        //若怪物已生成并已死亡
        else if(isEnemySpawned && currentEnemy == null)
        {
            isEnemySpawned = false; //未生成
            isRespawn = true;       //进入重生
            float randSpawnTime = Random.Range(minRespawnTime + spawnTimeFix , maxRespawnTime + spawnTimeFix); //获取随机重生时间
            respawnTimer = randSpawnTime;   //设置重生时间
        }
        //若怪物重生计时结束 && 处于重生阶段 && 还未生成
        if(respawnTimer <=0 && isRespawn && !isEnemySpawned)
        {
            SpawnEnemy();           //生成怪物
            isEnemySpawned = true;  //已生成
        }
    }

    //怪物生成
    private void SpawnEnemy()
    {
        float rand = Random.Range(minSpawnRange, maxSpawnRange);                            //获取随机重生范围
        Vector2 randPos = new Vector2(transform.position.x + rand, transform.position.y);   //设定重生位置
        currentEnemy = Instantiate(Enemy, randPos, Quaternion.identity, parent);            //生成
        currentEnemy.GetComponent<EnemyHealthSystem>().SetParentSpawnPoint(this);           //设置对象的重生点父类
    }

    //延迟重置重生状态(外部调用)
    public void DelayResetRespawnState()
    {
        Invoke("ResetRespawn", 0.5f);
    }

    private void ResetRespawn()
    {
        isRespawn = false;     //重置重生状态
        isEnemySpawned = false;
    }

}
