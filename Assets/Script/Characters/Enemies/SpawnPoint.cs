using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人生成点类
/// - 在桌宠附近生成敌人，并按难度调整敌人离场后的重生等待。
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    #region 配置与运行状态

    [Tooltip("刷新的怪物。")]
    [SerializeField] private GameObject Enemy;

    /// <summary>当前怪物。</summary>
    private GameObject currentEnemy;

    /// <summary>生成的敌人实例所使用的父节点。</summary>
    private Transform parent;

    /// <summary>用于交互或距离判断的桌宠对象。</summary>
    private GameObject vpet;

    /// <summary>是否允许生成。</summary>
    private bool isAllowSpawn = true;

    /// <summary>怪物是否已经生成。</summary>
    private bool isEnemySpawned = false;

    #endregion

    #region 初始化与范围更新

    /// <summary>
    /// 缓存桌宠对象，用于判断是否启用附近生成。
    /// </summary>
    private void Awake()
    {
        vpet = GameObject.FindGameObjectWithTag("Vpet");  //获取桌宠的游戏对象
    }

    /// <summary>
    /// 将自身设为敌人实例的父节点，并初始化难度对应的重生时间修正。
    /// </summary>
    private void Start()
    {
        parent = gameObject.transform;
        InitValueBasedDifficulty();     //初始化数值
    }

    /// <summary>
    /// 按桌宠距离更新生成许可，并推进生成与重生等待流程。
    /// </summary>
    private void Update()
    {
        isAllowSpawn = Vector2.Distance(vpet.transform.position, transform.position) < 30f ? true : false;

        CheckToSpawnEnemy();
    }

    #endregion

    #region 难度配置与敌人生成

    /// <summary>
    /// 根据当前难度设置重生等待时间的增减量。
    /// </summary>
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

    [Tooltip("生成位置相对出生点的最小水平偏移。")]
    [SerializeField] private float minSpawnRange = -3f;

    [Tooltip("生成位置相对出生点的最大水平偏移。")]
    [SerializeField] private float maxSpawnRange = 3f;

    [Tooltip("应用难度修正前的最短重生等待时间，单位为秒。")]
    [SerializeField] private float minRespawnTime = 15f;

    [Tooltip("应用难度修正前的最长重生等待时间，单位为秒。")]
    [SerializeField] private float maxRespawnTime = 30f;

    /// <summary>重生时间修正。</summary>
    private float spawnTimeFix = 0f;

    /// <summary>重生计时器。</summary>
    private float respawnTimer;

    /// <summary>是否进入了重生。</summary>
    bool isRespawn = false;

    /// <summary>
    /// 在生成范围内处理首次生成、敌人消失后的随机等待以及计时结束后的重生。
    /// </summary>
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

    /// <summary>
    /// 在配置的水平偏移范围内生成敌人，并将自身登记为该敌人的出生点。
    /// </summary>
    private void SpawnEnemy()
    {
        float rand = Random.Range(minSpawnRange, maxSpawnRange);                            //获取随机重生范围
        Vector2 randPos = new Vector2(transform.position.x + rand, transform.position.y);   //设定重生位置
        currentEnemy = Instantiate(Enemy, randPos, Quaternion.identity, parent);            //生成
        currentEnemy.GetComponent<EnemyHealthSystem>().SetParentSpawnPoint(this);           //设置对象的重生点父类
    }

    #endregion

    #region 离场重置

    /// <summary>
    /// 延迟半秒清除生成与重生标记，供敌人远距离自动销毁时重新开放首次生成流程。
    /// </summary>
    public void DelayResetRespawnState()
    {
        Invoke("ResetRespawn", 0.5f);
    }

    /// <summary>
    /// 清除已生成及重生阶段标记，使下次允许生成时可重新创建敌人。
    /// </summary>
    private void ResetRespawn()
    {
        isRespawn = false;     //重置重生状态
        isEnemySpawned = false;
    }

    #endregion
}
