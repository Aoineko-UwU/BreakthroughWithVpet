using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public List<ItemData> allItemPool;  //���пɳ�ȡ����Ʒ��(�ⲿ����) 

    public List<ItemData> slots = new List<ItemData>();     //��ǰ��Ʒ��
    public int totalSlotCount = 3;                          //��Ʒ��������

    public static InventoryManager Instance;        //�൥��

    private void Awake()
    {
        Instance = this;        //��ʼ���൥��
    }

    private void Start()
    {
        InitValueBasedDifficulty(); //��ʼ����ֵ
    }

    private void Update()
    {
        AddItemTimerSet();      //��ӵ��߼�ʱ��
    }

    private float itemAddTimer;             //������Ӽ�ʱ��
    private float itemAddCD = 5f;           //������Ӽ��


    //�����Ѷȳ�ʼ����ֵ
    private void InitValueBasedDifficulty()
    {
        //��ȡ��Ϸ�ѶȽ���ƥ��
        switch (GameDifficultySystem.Instance.CurrentDifficulty)
        {
            //���Ѷ�
            case GameDifficultyLevel.Easy:
                itemAddCD = 3f;                 
                break;

            //�����Ѷ�
            case GameDifficultyLevel.Normal:
                itemAddCD = 4.5f;
                break;
            
            //�����Ѷ�
            case GameDifficultyLevel.Hard:
                itemAddCD = 6f;
                break;
        }
    }

    //��ӵ��߼�ʱ��
    private void AddItemTimerSet()
    {
        if (!GameManager.Instance.isAllowPlayerControl) return;

        itemAddTimer -= Time.deltaTime;

        //��ʱ�����ʱˢ�²������Ʒ
        if (itemAddTimer <= 0)
        {
            itemAddTimer = itemAddCD;  //ˢ�¼�ʱ��CD
            TryAddRandomItem();
        }
    }

    //����Ϊ��Ʒ����������Ʒ
    private void TryAddRandomItem()
    {
        if (slots.Count >= totalSlotCount) return;      //����Ʒ���������������Ʒ

        int rand = Random.Range(0, allItemPool.Count);  //�����ȡ��Ʒ����ص�ID�����
        slots.Add(allItemPool[rand]);                   //��ӵ���Ʒ��
        RefreshUI();                                    //ˢ��UI
    }


    //��Ʒ��ɾ��������
    public void RemoveAt(int index)
    {
        slots.RemoveAt(index);      //�Ƴ���Ʒ����Ʒ
        RefreshUI();                //ˢ��UI
    }

    //ˢ����Ʒ��UI��ʾ
    public void RefreshUI()
    {
        SlotUI[] slotUI = FindObjectsOfType<SlotUI>();      //��ȡ���е���Ʒ��SlotUI�ű�

        foreach(SlotUI slot in slotUI)
        {
            int i = slot.index;     //��Ʒ��������(�ڼ���)(��һ����Ʒ��indexֵΪ0)

            if(i < slots.Count)     //��Ӧ���ӵ���Ʒ�����̨Slot����ƥ��
            {
                slot.SetItemImage(slots[i]);
            }
            else
            {
                slot.SetItemImage(null);
            }

        }

    }

    //����Ʒ������ض�����(�ⲿ����)
    public bool TryAddSpecificItem(ItemData item)
    {
        if (slots.Count >= totalSlotCount)
        {
            return false;
        }

        slots.Add(item);    //�����Ʒ
        RefreshUI();        //ˢ��UI��ʾ
        return true;
    }

    //��ȡһ���������Ʒ����(�ⲿ����)
    public ItemData GetRandomItem()
    {
        int rand = Random.Range(0, allItemPool.Count);  //�����ȡ��Ʒ����ص�ID�����
        return allItemPool[rand];                       //��ӵ���Ʒ��
    }

}
