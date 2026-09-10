using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物品栏管理类
/// - 管理道具池和物品列表，按难度定时补充物品并刷新格子图标。
/// </summary>
public class InventoryManager : Singleton<InventoryManager>
{
    #region 配置与运行状态

    [Tooltip("随机抽取物品的数据池；随机获取前必须配置至少一个条目。")]
    public List<ItemData> allItemPool;

    [Tooltip("当前物品栏条目列表。")]
    public List<ItemData> slots = new List<ItemData>();

    [Tooltip("物品栏允许容纳的条目上限。")]
    public int totalSlotCount = 3;

    /// <summary>场景中的物品栏格子缓存。</summary>
    private SlotUI[] slotUIs;

    #endregion

    #region 生命周期

    /// <summary>
    /// 注册场景单例；仅有效实例缓存场景中的物品栏格子。
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if (Instance != this)
            return;

        slotUIs = FindObjectsOfType<SlotUI>();
    }

    /// <summary>
    /// 初始化当前难度对应的物品补充间隔。
    /// </summary>
    private void Start()
    {
        InitValueBasedDifficulty(); //初始化数值
    }

    /// <summary>
    /// 推进物品补充计时。
    /// </summary>
    private void Update()
    {
        AddItemTimerSet();      //添加道具计时器
    }

    #endregion

    #region 随机补充与难度配置

    /// <summary>道具添加计时器。</summary>
    private float itemAddTimer;

    /// <summary>道具添加间隔。</summary>
    private float itemAddCD = 5f;

    /// <summary>
    /// 根据难度设置自动补充物品的间隔。
    /// </summary>
    private void InitValueBasedDifficulty()
    {
        //获取游戏难度进行匹配
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //简单难度
            case GameDifficultyLevel.Easy:
                itemAddCD = 3f;
                break;

            //正常难度
            case GameDifficultyLevel.Normal:
                itemAddCD = 4.5f;
                break;

            //困难难度
            case GameDifficultyLevel.Hard:
                itemAddCD = 6f;
                break;
        }
    }

    /// <summary>
    /// 允许玩家操作时递减补充计时器，到期后重置间隔并尝试添加随机物品。
    /// </summary>
    private void AddItemTimerSet()
    {
        if (!GameManager.Instance.isAllowPlayerControl) return;

        itemAddTimer -= Time.deltaTime;

        //计时器完成时刷新并添加物品
        if (itemAddTimer <= 0)
        {
            itemAddTimer = itemAddCD;  //刷新计时器CD
            TryAddRandomItem();
        }
    }

    /// <summary>
    /// 物品栏未满时从物品池随机追加一个条目并刷新图标；要求物品池非空。
    /// </summary>
    private void TryAddRandomItem()
    {
        if (slots.Count >= totalSlotCount) return;      //若物品栏已满则不添加新物品

        int rand = Random.Range(0, allItemPool.Count);  //随机获取物品池相关的ID随机数
        slots.Add(allItemPool[rand]);                   //添加到物品栏
        RefreshUI();                                    //刷新UI
    }

    #endregion

    #region 物品操作与界面同步

    /// <summary>
    /// 移除有效索引处的物品并刷新图标；越界时输出警告。
    /// </summary>
    /// <param name="index">待移除物品在列表中的零基索引。</param>
    public void RemoveAt(int index)
    {
        if (index < 0 || index >= slots.Count)
        {
            Debug.LogWarning($"尝试移除无效的物品栏索引：{index}");
            return;
        }

        slots.RemoveAt(index);      //移除物品栏物品
        RefreshUI();                //刷新UI
    }

    /// <summary>
    /// 按格子索引同步物品图标，超出物品数量的格子隐藏图标；不修改选中框。
    /// </summary>
    public void RefreshUI()
    {
        foreach(SlotUI slot in slotUIs)
        {
            int i = slot.index;     //物品栏的索引(第几格)(第一格物品栏index值为0)

            if(i < slots.Count)     //对应格子的物品栏与后台Slot进行匹配
            {
                slot.SetItemImage(slots[i]);
            }
            else
            {
                slot.SetItemImage(null);
            }

        }

    }

    /// <summary>
    /// 物品栏有空位时追加指定条目并刷新图标，不在此处校验物品是否为空。
    /// </summary>
    /// <param name="item">待追加的物品数据。</param>
    /// <returns>有空位且完成追加时为 true，物品栏已满时为 false。</returns>
    public bool TryAddSpecificItem(ItemData item)
    {
        if (slots.Count >= totalSlotCount)
        {
            return false;
        }

        slots.Add(item);    //添加物品
        RefreshUI();        //刷新UI显示
        return true;
    }

    /// <summary>
    /// 从物品池随机读取一个条目，不修改物品栏；要求物品池非空。
    /// </summary>
    /// <returns>抽中的物品数据引用。</returns>
    public ItemData GetRandomItem()
    {
        int rand = Random.Range(0, allItemPool.Count);  //随机获取物品池相关的ID随机数
        return allItemPool[rand];                       //添加到物品栏
    }

    #endregion
}
