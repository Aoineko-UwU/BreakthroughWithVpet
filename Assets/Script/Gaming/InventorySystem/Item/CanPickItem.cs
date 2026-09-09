using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 表示可被桌宠拾取的场景物品，并负责将其写入物品栏。
/// </summary>
public class CanPickItem : MonoBehaviour
{
    [Tooltip("物品贴图渲染器")]
    [SerializeField] SpriteRenderer icoSprite;      //物品贴图渲染器
    [Tooltip("物品数据")]
    [SerializeField] private ItemData itemData;     //物品数据
    [Tooltip("是否允许物品在超时后自动销毁。")]
    public bool isAllowSelfDestroy = true;

    /// <summary>设置该场景物品对应的物品栏数据。</summary>
    public void SetItemData(ItemData data)
    {
        itemData = data;                //传递物品数据
        if(icoSprite!= null)
            icoSprite.sprite = data.icon;   //设置贴图
    }

    private void Start()
    {
        if (isAllowSelfDestroy)
        {
            //随机时刻后自动摧毁
            float randDestroyTime = Random.Range(7f, 8f);
            Destroy(gameObject, randDestroyTime);
        }

        if (icoSprite.sprite == null && itemData != null)
        {
            icoSprite.sprite = itemData.icon;
        }
    }

    /// <summary>尝试将物品加入物品栏，成功后销毁场景物品。</summary>
    public void ItemPickUpLogic()
    {
        if (InventoryManager.Instance.TryAddSpecificItem(itemData))
        {
            Destroy(gameObject);
            AudioManager.Instance.PlaySound("pickItem");
        }

    }
}
