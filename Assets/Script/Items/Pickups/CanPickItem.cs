using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 可拾取物品类
/// - 保存场景物品数据与图标，尝试加入物品栏并处理超时销毁。
/// </summary>
public class CanPickItem : MonoBehaviour
{
    #region 配置与运行状态

    [Tooltip("物品贴图渲染器。")]
    [SerializeField] SpriteRenderer icoSprite;

    [Tooltip("当前场景物品对应的物品数据。")]
    [SerializeField] private ItemData itemData;

    [Tooltip("是否允许物品在超时后自动销毁。")]
    public bool isAllowSelfDestroy = true;

    #endregion

    #region 物品配置与拾取流程

    /// <summary>
    /// 记录物品数据，并在图标渲染器可用时更新图标。
    /// </summary>
    /// <param name="data">非空的物品数据，包含待显示图标。</param>
    public void SetItemData(ItemData data)
    {
        itemData = data;                //传递物品数据
        if(icoSprite!= null)
            icoSprite.sprite = data.icon;   //设置贴图
    }

    /// <summary>
    /// 按配置安排随机超时销毁，并在图标未设置时尝试从物品数据补齐。
    /// </summary>
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

    /// <summary>
    /// 尝试将当前物品加入物品栏；成功后播放拾取音效并销毁场景对象。
    /// </summary>
    public void ItemPickUpLogic()
    {
        if (InventoryManager.Instance.TryAddSpecificItem(itemData))
        {
            Destroy(gameObject);
            AudioManager.Instance.PlaySound("pickItem");
        }

    }

    #endregion
}
