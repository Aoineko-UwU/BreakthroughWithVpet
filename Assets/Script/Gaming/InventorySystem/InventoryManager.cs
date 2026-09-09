using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : Singleton<InventoryManager>
{
    public List<ItemData> allItemPool;  //所有可抽取的物品池(外部挂载) 

    public List<ItemData> slots = new List<ItemData>();     //当前物品栏
    public int totalSlotCount = 3;                          //物品格子总数

    private SlotUI[] slotUIs;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this)
            return;

        slotUIs = FindObjectsOfType<SlotUI>();
    }

    private void Start()
    {
        InitValueBasedDifficulty(); //初始化数值
    }

    private void Update()
    {
        AddItemTimerSet();      //添加道具计时器
    }

    private float itemAddTimer;             //道具添加计时器
    private float itemAddCD = 5f;           //道具添加间隔


    //根据难度初始化数值
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

    //添加道具计时器
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

    //尝试为物品栏添加随机物品
    private void TryAddRandomItem()
    {
        if (slots.Count >= totalSlotCount) return;      //若物品栏已满则不添加新物品

        int rand = Random.Range(0, allItemPool.Count);  //随机获取物品池相关的ID随机数
        slots.Add(allItemPool[rand]);                   //添加到物品栏
        RefreshUI();                                    //刷新UI
    }


    //物品栏删除并排序
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

    //刷新物品栏UI显示
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

    //给物品栏添加特定物体(外部调用)
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

    //获取一个随机的物品数据(外部调用)
    public ItemData GetRandomItem()
    {
        int rand = Random.Range(0, allItemPool.Count);  //随机获取物品池相关的ID随机数
        return allItemPool[rand];                       //添加到物品栏
    }

}
